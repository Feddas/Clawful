using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ShareDevice
{
    /// <summary>
    /// <see cref="PlayerInputManager"/> doesn't support multiple players on a single device.
    /// This class replaces <see cref="PlayerInputManager"/>, adding support for multiple players on a single device.
    /// https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/PlayerInputManager.cs
    ///
    /// This class solves https://discussions.unity.com/t/2-players-1-keyboard-custom-playerinputmanager-to-handle-sharing-a-device-with-multiple-players/1535785
    ///
    /// 2 players on one keyboard options:
    /// first comment on https://www.youtube.com/watch?v=g_s0y5yFxYg
    /// with 2 maps https://discussions.unity.com/t/how-to-use-the-new-inputsystem-for-2-players-on-one-keyboard/800856
    /// unity staff suggestion is actuating a device to spawns all players schemed for that device https://discussions.unity.com/t/2-players-on-same-input-device/762490/4
    /// keyboard splitter author, from post above, does InputSystem overview https://www.youtube.com/playlist?list=PL_2UsN9hUYBs4-u5rIQoQV8b7r7wz93GG
    /// </summary>
    public class PlayerShareDeviceManager : MonoBehaviour
    {
        [Header("Allows multiple control schemes, on the same device,\n"
              + "to join. ActionToJoin must specify control schemes.\n"
              + "SpawnPositionsAvailable sets max number of players.\n"
              + "Players can drop by holding down their bound join\n"
              + "button. They can rejoin using any join button.")]
        [Space(16f)]

        [Tooltip("Buttons available to press for a player to join. Each control scheme should only have one button for this action. Composite actions will not work.")]
        [SerializeField]
        private InputActionReference actionToJoin;

        [Tooltip("Buttons available to press for a player to leave. Each control scheme should only have one button for this action. Composite actions will not work.")]
        [SerializeField]
        private InputActionReference actionToLeave;

        [Tooltip("PlayerInput prefab that will be instantiated on join.")]
        [SerializeField]
        private GameObject playerPrefab;

        [Tooltip("The max number of players that can join and their positioning when instantiated.")]
        [SerializeField]
        private Transform[] spawnPositionsAvailable;

        /// <summary> What control schemes have already joined. They're not allowed to join again. </summary>
        private List<string> schemesJoined = new List<string>();

        // We want to remove the event listener we install through InputSystem.onAnyButtonPress
        // after we're done so remember it here.
        private IDisposable eventListener;

        /// <summary> Initial, full list, of all controls. Key is a button path, Value is scheme that button is paired with.
        /// <see cref="availableSchemes"/> & <see cref="intialSchemes"/> are needed because PlayerInput.Instantiate(PlayerPrefab...) causes <see cref="actionToJoin"/> to be paired to a single control on a device. Collapsing all previous controls it had to only the matching control.</summary>
        private Dictionary<InputControl, string> availableSchemes;
        private Dictionary<InputControl, string> intialSchemes = new Dictionary<InputControl, string>();

        /// <summary> <bindingId, effectiveButtonPath> Cache of every actionToJoin button's effective path. Used to determine if rebinding occurred. </summary>
        private Dictionary<Guid, string> currentBindingPaths;

        /// <summary> Subscribes to UnityEvent PlayerInputManager.NotifyPlayerLeft </summary>
        private UnityAction<PlayerInput> actionPlayerLeft;

        private void Start()
        {
            // Ensure PlayerInputManager's PlayerLeftEvent is subscribed to our OnPlayerLeft
            PlayerInputManager playerInputManager = PlayerInputManager.instance;
            if (playerInputManager == null)
            {
                playerInputManager = gameObject.AddComponent<PlayerInputManager>();
                playerInputManager.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
            }
            else if (playerInputManager.notificationBehavior != PlayerNotifications.InvokeUnityEvents)
            {
                throw new NotImplementedException("Dropping players currently supports only PlayerInputManager using PlayerNotifications.InvokeUnityEvents.");
            }
            actionPlayerLeft = new UnityAction<PlayerInput>(OnPlayerLeft);
#if UNITY_EDITOR
            UnityEventTools.AddPersistentListener(playerInputManager.playerLeftEvent, actionPlayerLeft);
#else
            playerInputManager.playerLeftEvent.AddListener(actionPlayerLeft);
#endif

            InputSystem.onActionChange += OnBindingChange;

            LoadExistingPlayers();
        }

        private void OnDestroy()
        {
            PlayerInputManager.instance?.playerLeftEvent.RemoveListener(actionPlayerLeft);
            InputSystem.onActionChange -= OnBindingChange;
        }

        private void OnEnable()
        {
            currentBindingPaths = actionToJoin.action.bindings.ToDictionary(b => b.id, b => b.effectivePath);

            // cache the intial, full list, of all controls.
            intialSchemes = buildControlSchemePairs();

            // shallow copy for modifying
            availableSchemes = intialSchemes.ToDictionary(d => d.Key, d => d.Value);

            // listen for any button press https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_onAnyButtonPress
            eventListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
        }

        private void OnDisable()
        {
            eventListener.Dispose();
        }

        /// <summary> Similiar to the <see cref="PlayerInputManager"/> function that raises OnPlayerJoined. This one still needs to check if the button belongs to a new player. </summary>
        void OnAnyButtonPressed(InputControl button)
        {
            if (false == availableSchemes.ContainsKey(button))
            {
                // Debug.Log(button.path + " doesnt have a scheme. Or its already been added.");
                return;
            }
            else
            {
                addPlayer(thatPressed: button);
            }
        }

        /// <summary> Sets whether or not all players are allowed to perform an action to leave the game. Invoked by UnityEvents, such as after a countdown sequence completes. </summary>
        public void SetCanPlayersLeave(bool canLeave)
        {
            foreach (PlayerUiMultiSelect player in Players.Manage.UiMultiSelectors)
            {
                player.SetCanLeaveGame(canLeave, actionToLeave);
            }
        }

        /// <summary> This must be called from <see cref="PlayerInputManager"/> due to <see cref="PlayerInput"/> having a hard reference in OnDisable `PlayerInputManager.instance?.NotifyPlayerLeft(this);`
        /// https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/PlayerInput.cs#L1746
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeft(PlayerInput player)
        {
            if (atMaxPlayers()) // right before freeing up a player spot, then renable listening for a new player to join
            {
                eventListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
            }

            // Debug.Log(player.name + " left. scheme " + player.currentControlScheme + " control" + player.actions[actionToJoin.name].controls[0].path);
            availableSchemes.Add(player.actions[actionToJoin.name].controls[0], player.currentControlScheme);

            // mark their spawn position as freed
            var spawnPlayer = player.GetComponent<PlayerInputRespawn>();
            spawnPositionsAvailable[spawnPlayer.Respawn.Index] = spawnPlayer.Respawn.Transform;
            Debug.Log("Player Left " + player.name + " freed up spawn " + spawnPlayer.name);
        }

        private void addPlayer(InputControl thatPressed)
        {
            var scheme = availableSchemes[thatPressed];
            var playerInput = PlayerInput.Instantiate(playerPrefab, controlScheme: scheme, pairWithDevice: thatPressed.device);

            // If the player did not end up with a valid input setup, unjoin the player.
            if (playerInput.hasMissingRequiredDevices)
            {
                Destroy(playerInput);
                return;
            }

            // determine index of first available spawn position
            var spawnPosition = trackPlayer(thatPressed);

            // setup player
            playerInput.gameObject.name = "Player" + playerInput.playerIndex + playerInput.currentControlScheme;
            var playerRespawn = playerInput.GetComponent<PlayerInputRespawn>();
            if (playerRespawn == null)
            {
                playerRespawn = gameObject.AddComponent<PlayerInputRespawn>();
            }
            playerRespawn.SetPosition(spawnPosition);
            Debug.Log("Player Joined " + playerInput.name + " using spawn " + spawnPosition.Transform.name);

            // DebugPlayersConnected();
        }

        /// <summary>
        /// <see cref="availableSchemes"/> 's indexing key is InputControl. Therefore, it needs to be rebuilt if the controls changed.
        /// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_onActionChange
        /// </summary>
        private void OnBindingChange(object obj, InputActionChange change)
        {
            var referencedAction = actionToJoin?.action;
            if (change != InputActionChange.BoundControlsChanged // limit to only binding completed
                || referencedAction == null)                     // actionToJoin must be in a valid state. Maybe this being null should throw an exception
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            if (referencedAction.actionMap?.asset == actionAsset)
            {
                updateRebindDependencies(referencedAction);
            }
        }

        /// <summary> If a rebind occurred, <seealso cref="availableSchemes"/> needs to be updated.</summary>
        private void updateRebindDependencies(InputAction referencedAction)
        {
            // check if rebind occurred
            bool isRebindDetected = referencedAction.bindings.Any(b => currentBindingPaths[b.id] != b.effectivePath);
            if (false == isRebindDetected)
            {
                return;
            }

            // update availableSchemes
            foreach (InputBinding bind in referencedAction.bindings)
            {
                if (currentBindingPaths[bind.id] != bind.effectivePath)
                {
                    Debug.Log($"{Time.frameCount} Rebinding {currentBindingPaths[bind.id]} changed to {bind.effectivePath}");

                    // remove the old
                    var oldControl = intialSchemes.Where(d => d.Value == bind.groups).FirstOrDefault().Key;
                    availableSchemes.Remove(oldControl);

                    // add the new
                    var referencedDevice = referencedAction.actionMap.actions[0].controls[0].device;
                    var controlButtonPath = bind.effectivePath.Split('/').Last();
                    var newControl = referencedDevice.GetChildControl(controlButtonPath);
                    availableSchemes.Add(newControl, bind.groups);
                    currentBindingPaths[bind.id] = bind.effectivePath;
                }
            }
        }

        /// <summary> Account for already existing players. i.e. players with DontDestroyOnLoad active </summary>
        private void LoadExistingPlayers()
        {
            foreach (PlayerUiMultiSelect player in Players.Manage.UiMultiSelectors)
            {
                var newSpawn = trackPlayer(player.PlayerInput.actions[actionToJoin.name].controls[0]);
                player.SetCanLeaveGame(true, actionToLeave);

                // assume old spawn position gameobject was destroyed. Overwrite with spawnPositionsAvailable result.
                player.GetComponent<PlayerInputRespawn>().SetPosition(newSpawn);
            }
        }

        /// <returns> preferred spawn location </returns>
        private TransformAtIndex trackPlayer(InputControl playerControl)
        {
            // determine index of first available spawn position
            var spawnPosition = spawnPositionsAvailable
                .Select((value, index) => new TransformAtIndex(index, value))
                .Where(pair => pair.Transform != null)
                .FirstOrDefault();
            spawnPositionsAvailable[spawnPosition.Index] = null;

            // disable listening for a new player on the same control scheme
            availableSchemes.Remove(playerControl);
            if (atMaxPlayers())
            {
                // there are no players left to join, unsubscribe to anyKey events
                eventListener.Dispose();
            }
            return spawnPosition;
        }

        // commented out code below saved incase buildControlSchemePairs()'s `actionToJoin.action.controls` Needs to be replaced with `FindSourceActionMap(actionToJoin).controls`
        /// <summary> Converts a reference to the root instance. https://discussions.unity.com/t/rebinding-with-input-system-not-applying-changes/941167/2?clickref=1101lzQCBLBB&utm_source=partnerize&utm_medium=affiliate&utm_campaign=unity_affiliate </summary>
        //private InputAction FindSourceActionMap(InputActionReference actionReference)
        //{
        //    foreach (var map in actionToJoin.action.actionMap.asset.actionMaps)
        //    {
        //        foreach (var action in map.actions)
        //        {
        //            if (action.id == actionReference.action.id)
        //            {
        //                return action;
        //            }
        //        }
        //    }

        //    Debug.LogError(actionToJoin.action.actionMap.asset.name + " doesn't contain " + actionReference.action.name);
        //    return null;
        //}

        /// <returns> Pairs of all controls and schemes related to <see cref="actionToJoin"/></returns>
        private Dictionary<InputControl, string> buildControlSchemePairs()
        {
            var result = new Dictionary<InputControl, string>();
            foreach (var control in actionToJoin.action.controls)
            {
                // "binding.group" contains the name of the scheme. In case
                // the binding is assigned to more than one, it's a semicolon-
                // separate list (like "Keyboard;Gamepad").
                // ref https://discussions.unity.com/t/any-way-to-get-the-control-scheme-for-an-input-through-inputaction-callbackcontext/811484/5
                // https://www.reddit.com/r/Unity3D/comments/vgn4iw/new_input_system_how_to_use_same_devices_across/
                var scheme = actionToJoin.action.GetBindingForControl(control).Value.groups;
                result.Add(control, scheme);
            }
            return result;
        }

        /// <returns> true if there are no more spawn positions or devices available for a new player to join </returns>
        private bool atMaxPlayers()
        {
            return spawnPositionsAvailable.All(p => p == null) || availableSchemes.Count == 0;
        }

        /// <summary> print all players and the devices they're paired to</summary>
        private void DebugPlayersConnected()
        {
            var allPairings = PlayerInput.all.Select(p =>
                p.name
                + "-device(" + p.user.pairedDevices.Count + "):" + (p.user.pairedDevices.Count > 0 ? p.user.pairedDevices[0].name : "na")
                + "-" + p.currentControlScheme);
            Debug.Log(Time.frameCount + ": " + string.Join(" | ", allPairings));
        }
    }

    public class TransformAtIndex
    {
        /// <summary> Points to the index into an array that this Transform was copied from. </summary>
        public int Index { get; set; }
        public Transform Transform { get; set; }
        public TransformAtIndex(int index, Transform transform)
        {
            Index = index;
            Transform = transform;
        }
    }
}
