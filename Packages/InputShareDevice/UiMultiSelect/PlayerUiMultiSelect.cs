using ShareDevice;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ShareDevice
{
    [RequireComponent(typeof(InputSystemUIInputModule))]
    [RequireComponent(typeof(MultiplayerEventSystem))]
    public class PlayerUiMultiSelect : MonoBehaviour
    {
        [Header("Allows multiple players to interact with UI.\n"
              + "Toggling UI navigation depending on if the player\n"
              + "has locked or submitted a choice. It also positions\n" +
                "and colors each players cursor.")]
        [Space(16f)]

        [Tooltip("If true, wait for all active eventsystems to complete selection.")]
        public bool IsGroupSelect = true;

        [Tooltip("Where the cursor icon for this player is stored.")]
        [SerializeField]
        private Image Cursor;

        [Tooltip("Icon indicating that the player has not yet submitted a choice.")]
        [SerializeField]
        private Sprite Hover;

        [Tooltip("Icon indicating that the player has submitted a choice.")]
        [SerializeField]
        private Sprite Select;

        [Tooltip("Raise new Claw character should be shown. AKA, What to do when the player has changed their individual selection amidst a group selection. bool payload is true when locked, false when unlocked")]
        [SerializeField]
        private UnityEngine.Events.UnityEvent<bool> OnSelectionChanged;

        [Header("readonly")]
        [Tooltip("UI panel with UiSelectedOnEnable.IsGroupSelect is shown and this player has locked their selection.")]
        [SerializeField]
        private bool cursorLocked;

        /// <summary> Navigation will be toggled when the player submits or retracts a selection. </summary>
        private InputActionReference navigate;

        /// <summary> clone of Cursor sent to active UI. cursorClone will be destroyed when the UI is destroyed. </summary>
        private Image cursorClone;

        /// <summary> Support up to 4 players. These pivots ensure cursor images don't overlap with one another. </summary>
        private Vector2[] cursorPivot = new Vector2[4] {
            new Vector2(1, 0),
            new Vector2(0, 0),
            new Vector2(1, 1),
            new Vector2(0, 1) };

        public PlayerInput PlayerInput
        {
            get
            {
                return _playerInput ??= GetComponent<PlayerInput>();
            }
        }
        private PlayerInput _playerInput;

        public MultiplayerEventSystem EventSystem
        {
            get
            {
                return _eventSystem ??= GetComponent<MultiplayerEventSystem>();
            }
        }
        private MultiplayerEventSystem _eventSystem;

        public InputSystemUIInputModule UiInput
        {
            get
            {
                return _uiInput ??= GetComponent<InputSystemUIInputModule>();
            }
        }
        private InputSystemUIInputModule _uiInput;

        /// <summary> Uniquely identifies players using either multiple devices (gamepads) or multiple players having controlschemes on a device (2 on 1 keyboard). This value needs to be cached to handle playerInput being destroyed before this component. </summary>
        private string playerId;
        private int frameCreated;

        public void Awake()
        {
            // customize cursor icon for each player
            Cursor.sprite = Hover;
            Cursor.color = Random.ColorHSV(0, 1, 1, 1, .8f, .8f);
            var cursorRect = Cursor.GetComponent<RectTransform>();
            cursorRect.pivot = cursorPivot[PlayerInput.playerIndex];

            // If UiSelectedOnEnable has ActiveInstance show cursor
            ShowCursor();

            // Cache uiInput.move so that it can be temporarily nulled
            // alternative: remove this device from the Navigate InputAction. Shawn couldn't get that to work https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html#choosing-which-devices-to-use / https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_bindingMask
            navigate = UiInput.move;
            frameCreated = Time.frameCount;
        }

        public void OnEnable()
        {
            playerId ??= PlayerInput.devices[0].name + ":" + PlayerInput.currentControlScheme;
            Players.Manage.Add(playerId, this);
        }

        public void OnDisable()
        {
            Players.Manage.Remove(playerId);
        }

        /// <summary> Sets whether or not this player can perform an action to leave the game. </summary>
        public void SetCanLeaveGame(bool canLeave, InputActionReference mapToLeave)
        {
            var actionToLeave = PlayerInput.actions[mapToLeave.name];
            if (canLeave)
            {
                actionToLeave.Enable();
            }
            else
            {
                actionToLeave.Disable();
            }
        }

        /// <summary> Required <see cref="PlayerInput"/> component invokes this function via UnityEvent when LeaveGame action is triggered. </summary>
        public void OnLeaveGame(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // force unlock
                if (cursorLocked)
                {
                    cursorLocked = false;
                    NotifyLockChanged();
                }

                // remove players objects
                if (cursorClone != null)
                {
                    Destroy(cursorClone.gameObject);
                }
                Destroy(gameObject);
                // TODO: check if "PlayerInputManager" can (or should) be re-enabled so that a new player can rejoin
            }
        }

        /// <summary> Occurs when player has toggled their choice.
        /// <see cref="PlayerInput"/> component invokes this function via UnityEvent </summary>
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed
                || frameCreated == Time.frameCount) // ignore submit actions triggered in the same frame player instantiated from joining.
            {
                return;
            }

            StartCoroutine(FrameAfterSubmit(this.IsGroupSelect)); // cache this panels.IsGroupSelect to handle UiSelectedOnEnable.ActiveInstance's OnEnable overwriting this.IsGroupSelect with new panel.
        }

        /// <summary> Occurs when player has moved, navigating the UI.
        /// <see cref="PlayerInput"/> component invokes this function via UnityEvent </summary>
        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
            {
                return;
            }

            StartCoroutine(FrameAfterNavigate());
        }

        /// <summary> Set if this players EventSystem allows them to move off of the current UI element </summary>
        public void SetCursorLock(bool isLocked)
        {
            // set cursor properties
            cursorLocked = isLocked;
            if (navigate != null) // else, this.Awake() hasn't been called yet
            {
                UiInput.move = isLocked ? null : navigate;
            }

            // set cursor visuals
            if (cursorClone == null)
            {
                ShowCursor();
            }
            cursorClone.sprite = isLocked ? Select : Hover;
        }

        /// <summary> When OnSubmit is called UiSelectedOnEnable.ActiveInstance could be calling OnEnable the same frame. After a frame, UiSelectedOnEnable.ActiveInstance will be enabled. </summary>
        /// <param name="groupSelectBeforeSubmit"> Status of <seealso cref="IsGroupSelect"/> when the submit occurred. <seealso cref="IsGroupSelect"/> can change every time a new UiSelectedOnEnable panel is opened.  </param>
        private IEnumerator FrameAfterSubmit(bool groupSelectBeforeSubmit)
        {
            yield return null; // ensure it's FrameAfterSubmit

            ShowCursor(); // incase submit action left the cursor on a previous UI panel

            if (false == groupSelectBeforeSubmit // Not in "IsGroupSelect" mode. don't toggle anything
                || cursorClone == null)  // in invalid state. likely no UiSelectedOnEnable.ActiveInstance
            {
                yield break; // not in "IsGroupSelect" mode
            }

            // In "IsGroupSelect" mode
            SetCursorLock(UiInput.move != null); // use UiInput.move to determine what state to toggle into

            NotifyLockChanged();
        }

        private void NotifyLockChanged()
        {
            // update the players individual selection
            OnSelectionChanged?.Invoke(cursorLocked);

            // update the groups selection. check if this cursorLock changes everyone in the group being locked (AKA all ready).
            UiSelectedOnEnable.ActiveInstance.OnPlayerLockChanged(cursorLocked);
        }

        /// <summary> When OnNavigate is called EventSystem.CurrentSelectedGameObject hasn't been updated yet. After a frame, it will be updated. </summary>
        private IEnumerator FrameAfterNavigate()
        {
            yield return null; // ensure it's FrameAfterNavigate

            // Debug.Log(this.name + " selected " + eventSystem.currentSelectedGameObject.name + ". location" + eventSystem.currentSelectedGameObject.transform.position);

            ShowCursor();
        }

        /// <summary> Moves the cursor from any previous panel to the UI element that is currently in focus. </summary>
        private void ShowCursor()
        {
            if (cursorClone != null)
            {
                cursorClone.transform.SetParent(EventSystem.currentSelectedGameObject.transform, worldPositionStays: false);
            }
            else if (UiSelectedOnEnable.ActiveInstance != null) // player was created this frame and should be joined to an already existing UI panel
            {                                                   // or cursorClone was destroyed by changing scenes
                if (EventSystem.currentSelectedGameObject == null)
                {
                    EventSystem.SetSelectedGameObject(UiSelectedOnEnable.ActiveInstance.GetFirstSelected());
                }
                cursorClone = Instantiate<Image>(Cursor, EventSystem.currentSelectedGameObject.transform, worldPositionStays: false);
            }
        }
    }
}
