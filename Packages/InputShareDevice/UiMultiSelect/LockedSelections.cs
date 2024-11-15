using InputShareDevice;

namespace ShareDevice
{
    /// <summary> Singleton to track if all players in the group have locked a selection.
    /// LockSelection required for UiSelecedOnEnable's IsGroupSelection </summary>
    public class LockedSelections
    {
        public static LockedSelections Instance
        {
            get
            {
                return instance ??= new LockedSelections();
            }
        }
        private static LockedSelections instance;

        /// <summary> Total number of players. To raise a GroupSelect, each player will need to lock in a selection. </summary> 
        public int PlayerCount { get; set; }

        /// <summary> how many players have locked in a selection </summary>
        public int LockCount { get; private set; } = 0;

        private bool wasLocked;

        /// <summary> Determines if all players in the group have locked their selection.
        /// Subscribes to an individual player locking their selection.  </summary>
        /// <returns> if all players are locked </returns>
        public bool OnPlayerLockChanged(bool isLocked)
        {
            LockCount += isLocked ? 1 : -1;

            // check if all players are locked
            if (LockCount == PlayerCount)
            {
                // raise/return result
                wasLocked = true;
                UiSelectedOnEnable.ActiveInstance.InvokeGroupSelect(wasLocked);
            }
            else if (wasLocked)
            {
                wasLocked = false;
                UiSelectedOnEnable.ActiveInstance.InvokeGroupSelect(wasLocked);
            }

            return wasLocked;
        }
    }
}
