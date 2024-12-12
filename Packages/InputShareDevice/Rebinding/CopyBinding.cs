using UnityEngine;
using UnityEngine.InputSystem;

namespace ShareDevice
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
            // Guard clause: intoAction is valid and not already copied
            if (intoAction == null
                || intoAction.action == sourceAction.GetAction())
            {
                return;
            }

            // Guard clause: duplicates must be allowed
            var bindingId = new System.Guid(sourceAction.bindingId);
            var bindingIndex = sourceAction.GetAction().bindings.IndexOf(x => x.id == bindingId);
            if (rebindingOperation == null) // should (hopefully) only happen when a reset to defaults was done
            {
                UiRebindAction.ResetAllowingDuplicates(intoAction, bindingIndex); // duplicates have to be allowed, because copying is making a duplicate.
                return;
            }

            // Copy binding path from sourceAction into targetAction
            var targetAction = sourceAction.GetAction(intoAction);
            var bindings = sourceAction.GetAction().bindings;
            targetAction.Disable();
            if (bindings[bindingIndex].isComposite) // copy overrides from part bindings.
            {
                for (var i = bindingIndex + 1; i < bindings.Count && bindings[i].isPartOfComposite && bindings[i].hasOverrides; ++i)
                {
                    targetAction.ApplyBindingOverride(i, bindings[i].overridePath);
                }
            }
            else // copy overrides from single binding, for a button.
            {
                string targetOverridePath = bindings[bindingIndex].overridePath;
                targetAction.ApplyBindingOverride(bindingIndex, targetOverridePath);
            }
            targetAction.Enable();
        }
    }
}
