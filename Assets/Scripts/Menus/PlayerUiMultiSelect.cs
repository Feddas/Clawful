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

        /// <summary> Navigation will be toggled when the player submits or retracts a selection. </summary>
        private InputActionReference navigate;

        /// <summary> Support up to 4 players. These pivots ensure cursor images don't overlap with one another. </summary>
        private Vector2[] cursorPivot = new Vector2[4] {
        new Vector2(1, 0),
        new Vector2(0, 0),
        new Vector2(1, 1),
        new Vector2(0, 1) };

        private PlayerInput playerInput
        {
            get
            {
                return _playerInput ??= GetComponent<PlayerInput>();
            }
        }
        private PlayerInput _playerInput;

        private MultiplayerEventSystem eventSystem
        {
            get
            {
                return _eventSystem ??= GetComponent<MultiplayerEventSystem>();
            }
        }
        private MultiplayerEventSystem _eventSystem;

        private InputSystemUIInputModule uiInput
        {
            get
            {
                return _uiInput ??= GetComponent<InputSystemUIInputModule>();
            }
        }
        private InputSystemUIInputModule _uiInput;

        /// <summary> cache playerInput.currentControlScheme to handle playerInput being destroyed before this. </summary>
        private string controlScheme;

        public void Start()
        {
            // customize cursor icon for each player
            Cursor.sprite = Hover;
            Cursor.color = Random.ColorHSV(0, 1, 1, 1, .8f, .8f);
            var cursorRect = Cursor.GetComponent<RectTransform>();
            cursorRect.pivot = cursorPivot[playerInput.playerIndex];

            // Cache uiInput.move so that it can be temporarily nulled
            // alternative: remove this device from the Navigate InputAction. Shawn couldn't get that to work https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html#choosing-which-devices-to-use / https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_bindingMask
            navigate = uiInput.move;
        }

        public void OnEnable()
        {
            controlScheme ??= playerInput.currentControlScheme;
            UiSelectedOnEnable.ActiveEventSystems.Add(controlScheme, eventSystem);
            this.IsGroupSelect = UiSelectedOnEnable.ActiveInstance.IsGroupSelect;
        }

        public void OnDisable()
        {
            UiSelectedOnEnable.ActiveEventSystems.Remove(controlScheme);
        }

        /// <summary> Occurs when player has toggled their choice.
        /// <see cref="PlayerInput"/> component invokes this function via UnityEvent </summary>
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
            {
                return;
            }

            StartCoroutine(FrameAfterSubmit());
        }

        /// <summary> Occurs when player has moved, navigating the UI.
        /// <see cref="PlayerInput"/> component invokes this function via UnityEvent </summary>
        public void OnNavigate(InputAction.CallbackContext context)//InputValue value)
        {
            if (context.phase != InputActionPhase.Performed)
            {
                return;
            }

            StartCoroutine(FrameAfterNavigate());
        }

        /// <summary> When OnSubmit is called UiSelectedOnEnable.ActiveInstance could be calling OnEnable the same frame. After a frame, UiSelectedOnEnable.ActiveInstance will be enabled. </summary>
        private IEnumerator FrameAfterSubmit()
        {
            yield return null;

            // update IsGroupSelect and visual cursor
            bool lastGroupSelect = IsGroupSelect;
            IsGroupSelect = UiSelectedOnEnable.ActiveInstance.IsGroupSelect; // Set by UiSelectedOnEnable.ActiveInstance's OnEnable
            yield return FrameAfterNavigate(); // update cursor position

            // toggle lock of this players EventSystem to the current UI
            if (false == lastGroupSelect) // don't toggle anything
            {
                yield break;
            }
            if (uiInput.move == null)
            {
                uiInput.move = navigate;
                Cursor.sprite = Hover;
            }
            else
            {
                uiInput.move = null;
                Cursor.sprite = Select;
            }
        }

        /// <summary> When OnNavigate is called EventSystem.CurrentSelectedGameObject hasn't been updated yet. After a frame, it will be updated. </summary>
        private IEnumerator FrameAfterNavigate()
        {
            yield return null;

            // Debug.Log(this.name + " selected " + eventSystem.currentSelectedGameObject.name + ". location" + eventSystem.currentSelectedGameObject.transform.position);
            Cursor.transform.SetParent(eventSystem.currentSelectedGameObject.transform, worldPositionStays: false);
        }
    }
}
