using UnityEngine;

namespace ShareDevice
{
    /// <summary> Singleton to track if all players in the group have locked a selection.
    /// LockSelection required for UiSelecedOnEnable's IsGroupSelection </summary>
    [System.Serializable]
    public class LockedSelections
    {
        /// <summary> If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone. </summary>
        public bool IsGroupSelect { get { return isGroupSelect; } }

        [Tooltip("If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone.")]
        [SerializeField]
        private bool isGroupSelect;

        [Tooltip("If 0, uses active player count. Otherwise, waits for players to join until minimum player/group count is met.")]
        [SerializeField]
        private int minGroupSize = 0;

        [Tooltip("Raised when all players have locked in a selection. payload is true when group selection is locked. payload is false when group selection is broken.")]
        [SerializeField]
        private UnityEngine.Events.UnityEvent<bool> OnGroupSelect;

        /// <summary> Total number of players required to satisfy GroupSelect. To raise a GroupSelect, each player will need to lock in a selection. </summary> 
        private int RequiredPlayerCount
        {
            get { return Mathf.Max(UnityEngine.InputSystem.PlayerInput.all.Count, minGroupSize); }
        }

        /// <summary> how many players have locked in a selection </summary>
        private int LockCount { get; set; } = 0;

        private bool wasLocked;

        /// <summary> Determines if all players in the group have locked their selection.
        /// Subscribes to an individual player locking their selection.  </summary>
        /// <returns> if all players are locked </returns>
        public bool OnPlayerLockChanged(bool isLocked)
        {
            LockCount += isLocked ? 1 : -1;

            // check if all players are locked
            if (LockCount == RequiredPlayerCount)
            {
                // raise/return result
                wasLocked = true;
                InvokeGroupSelect(wasLocked);
            }
            else if (wasLocked)
            {
                wasLocked = false;
                InvokeGroupSelect(wasLocked);
            }

            return wasLocked;
        }

        public void ResetLockCount()
        {
            LockCount = 0;
        }

        /// <summary> Invoke this when all active eventsystems have a selection newly locked or broken. </summary>
        /// <param name="isLocked"> true when group selection is locked. false when group selection is broken. </param>
        public void InvokeGroupSelect(bool isLocked)
        {
            // Debug.Log($"{Time.frameCount} {(isLocked ? "everyone locked" : "lock broken")}. count {ShareDevice.LockedSelections.Instance.LockCount}.");
            OnGroupSelect.Invoke(isLocked);
        }
    }
}
