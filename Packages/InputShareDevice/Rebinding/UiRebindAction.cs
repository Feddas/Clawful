using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes

namespace ShareDevice
{
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// combined with Sasquatch B https://youtu.be/qXbjyzBlduY & Samyam https://youtu.be/csqVa2Vimao
    /// </summary>
    public class UiRebindAction : MonoBehaviour
    {
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        private InputActionReference actionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        public int PlayerIndex
        {
            get
            {
                return m_PlayerIndex;
            }
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string bindingId
        {
            get => m_BindingId;
            set
            {
                if (m_BindingId != value)
                {
                    m_BindingId = value;
                    UpdateBindingDisplay();
                }
            }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TMPro.TextMeshProUGUI actionLabel
        {
            get => m_ActionLabel;
            set
            {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TMPro.TextMeshProUGUI bindingText
        {
            get => m_BindingText;
            set
            {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary> Button to be deactivated when control is bound to its default key. </summary>
        public GameObject resetButton
        {
            get => m_ResetButton;
            set
            {
                m_ResetButton = value;
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// Optional text label that will be updated with prompt for user input
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindOverlay"/>
        private TMPro.TextMeshProUGUI rebindPrompt
        {
            get
            {
                return m_RebindText ??= rebindOverlay?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }
        }
        private TMPro.TextMeshProUGUI m_RebindText;

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindPrompt"/>
        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = GetAction();
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(m_BindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var hasOverrides = false;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get action
            InputAction action = GetAction();

            // Get display string from action.
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
                    hasOverrides = action.bindings[bindingIndex].hasOverrides
                        || bindingsInComposite(action, bindingIndex).Any(b => b.hasOverrides);
                }
            }

            // Set on UI.
            if (m_BindingText != null)
                m_BindingText.text = displayString;

            if (resetButton != null)
                resetButton?.SetActive(hasOverrides);

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        /// <returns> Action being rebound based on if <seealso cref="UseBindingAssetNotPlayerIndex"/> is set </returns>
        public InputAction GetAction()
        {
            return GetAction(actionReference);
        }

        /// <param name="mapToAction"> Action this will be mapped or copied into </param>
        public InputAction GetAction(InputActionReference mapToAction)
        {
            if (UseBindingAssetNotPlayerIndex || PlayerInput.all.Count <= PlayerIndex)
            {
                return mapToAction?.action;
            }
            else
            {
                return PlayerInput.all[PlayerIndex].actions[mapToAction.name];
            }
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (m_AllowDuplicateBindings)
            {
                ResetAllowingDuplicates(action, bindingIndex);
            }
            else
            {
                ResetNoDuplicates(action, bindingIndex);
            }

            UpdateBindingDisplay();
            m_RebindStopEvent?.Invoke(this, null);
        }

        public static void ResetAllowingDuplicates(InputAction action, int bindingIndex)
        {
            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
        }

        private void ResetNoDuplicates(InputAction action, int bindingIndex)
        {
            InputBinding newBinding = action.bindings[bindingIndex];
            string oldOverridePath = newBinding.overridePath;

            action.RemoveBindingOverride(bindingIndex);

            foreach (var otherAction in action.actionMap.actions)
            {
                if (otherAction == action)
                {
                    continue;
                }

                for (global::System.Int32 i = 0; i < otherAction.bindings.Count; i++)
                {
                    InputBinding binding = otherAction.bindings[i];
                    if (binding.overridePath == newBinding.path)
                    {
                        otherAction.ApplyBindingOverride(i, oldOverridePath);
                    }
                }
            }
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
            }

            // disable the action before use so it can be rebound
            action.Disable();

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Gamepad>/Start")
                .WithControlsExcluding("<Keyboard>/p")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(
                    operation =>
                    {
                        action.Enable();
                        m_RebindStopEvent?.Invoke(this, operation);
                        m_RebindOverlay?.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        action.Enable();
                        m_RebindOverlay?.SetActive(false);
                        m_RebindStopEvent?.Invoke(this, operation);

                        if (false == m_AllowDuplicateBindings)
                        {
                            if (CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                            {
                                action.RemoveBindingOverride(bindingIndex);
                                CleanUp();
                                PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                                return;
                            }
                        }

                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            // Bring up rebind overlay, if we have one.
            m_RebindOverlay?.SetActive(true);
            if (rebindPrompt != null)
            {
                var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                rebindPrompt.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (m_RebindOverlay == null && rebindPrompt == null && m_RebindStartEvent == null && m_BindingText != null)
                m_BindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }

        private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                if (binding.action == newBinding.action)
                {
                    continue;
                }

                if (binding.effectivePath == newBinding.effectivePath)
                {
                    Debug.Log("Duplicate binding found: " + newBinding.effectivePath);
                    return true;
                }
            }

            if (allCompositeParts)
            {
                for (int i = 1; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.overridePath)
                    {
                        Debug.Log("Duplicate binding found: " + newBinding.effectivePath);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary> If binding at <paramref name="bindingIndex"/> is a composite, return all bindings that belong to that composite.
        /// TODO: Make this an extension method for action.bindings </summary>
        private IEnumerable<InputBinding> bindingsInComposite(InputAction action, int bindingIndex)
        {
            if (false == action.bindings[bindingIndex].isComposite)
                yield break;

            for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                yield return action.bindings[i];
        }

        protected void OnEnable()
        {
            if (m_InputDeviceIcons != null)
            {
                m_InputDeviceIcons.Initialize(this);
            }
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<UiRebindAction>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;

            // Use PlayerIndex (Not BindingAsset). overwrites m_binding using PlayerIndex
            if (false == UseBindingAssetNotPlayerIndex && PlayerInput.all.Count > m_PlayerIndex)
            {
                var playersAction = PlayerInput.all[m_PlayerIndex].actions[actionReference.name];
                var bindingIndex = playersAction.GetBindingIndexForControl(playersAction.controls[0]);
                if (playersAction.bindings[bindingIndex].isPartOfComposite) bindingIndex--; // workaround for bug in Unity's code. composite indices are 1 too big
                bindingId = playersAction.bindings[bindingIndex].id.ToString();
            }
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < s_RebindActionUIs.Count; ++i)
            {
                var component = s_RebindActionUIs[i];
                var referencedAction = component.actionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference m_Action;

        [Tooltip("How the binding should be located, either by binding asset Id or with the index of a playerInput at runtime")]
        [SerializeField]
        public bool UseBindingAssetNotPlayerIndex = true;

        [Tooltip("Which player to rebind controls based off a player. Player is found at runtime, in PlayerInput.all[]. index is based off order player joined.")]
        [SerializeField]
        private int m_PlayerIndex;

        [SerializeField]
        private string m_BindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("When true, a single button can be assigned to multiple actions.")]
        [SerializeField]
        private bool m_AllowDuplicateBindings = true;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
            + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TMPro.TextMeshProUGUI m_ActionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TMPro.TextMeshProUGUI m_BindingText;

        [Tooltip("Button to be deactivated when control is bound to its default key.")]
        [SerializeField]
        private GameObject m_ResetButton;

        [Tooltip("Maps formatted binding string to an image icon.")]
        [SerializeField]
        private InputDeviceIcons m_InputDeviceIcons;

        [Tooltip("Optional UI that will be shown while a rebind is in progress. First TMProText found on it will be used to dislay rebind status.")]
        [SerializeField]
        private GameObject m_RebindOverlay;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<UiRebindAction> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
#if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
#endif

        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = GetAction();
                m_ActionLabel.text = action != null ? action.name : string.Empty;
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<UiRebindAction, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<UiRebindAction, InputActionRebindingExtensions.RebindingOperation>
        {
        }
    }
}
