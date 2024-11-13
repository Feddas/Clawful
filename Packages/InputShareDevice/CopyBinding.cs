using UnityEngine;
using UnityEngine.InputSystem;

namespace InputShareDevice
{
    public class CopyBinding : MonoBehaviour
    {
        [Tooltip("Reference to action that is to have its binding overridden to match a source action. Both actions bindings must match and be in the same order.")]
        [SerializeField]
        private InputActionReference intoAction;

        /// <summary>
        /// Allows a single interactive rebinding to update multiple actions that are bound to the same control.
        /// Copies the binding path from <paramref name="sourceAction"/> to <see cref="intoAction"/>.
        /// </summary>
        /// <param name="sourceAction"> action to copy binding path from. </param>
        /// <param name="rebindingOperation"> used as a flag. When null it's assumed this is being called from sourceAction being ResetToDefault. </param>
        public void FromRebind(UiRebindAction sourceAction, InputActionRebindingExtensions.RebindingOperation rebindingOperation)
        {
            if (intoAction == null
                || intoAction.action == sourceAction.actionReference.action)
            {
                return;
            }

            var bindingId = new System.Guid(sourceAction.bindingId);
            var bindingIndex = sourceAction.actionReference.action.bindings.IndexOf(x => x.id == bindingId);
            if (rebindingOperation == null) // should (hopefully) only happen when a reset to defaults was done
            {
                UiRebindAction.ResetAllowingDuplicates(intoAction, bindingIndex); // duplicates have to be allowed, because copying is making a duplicate.
                return;
            }

            // copy binding path from sourceAction
            InputBinding sourceBinding = sourceAction.actionReference.action.bindings[bindingIndex];
            string targetOverridePath = sourceBinding.overridePath;
            intoAction.action.Disable();
            intoAction.action.ApplyBindingOverride(bindingIndex, targetOverridePath);
            intoAction.action.Enable();
        }
    }
}
