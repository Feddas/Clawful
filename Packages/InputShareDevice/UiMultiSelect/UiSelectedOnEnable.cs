using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ShareDevice
{
    public class UiSelectedOnEnable : MonoBehaviour
    {
        /// <summary> Most recent enabled instance of UiSelectedOnEnable </summary>
        public static UiSelectedOnEnable ActiveInstance
        {
            get
            {
                if (activeInstance == null)
                {
                    SetUiInteractable(false);
                    Debug.LogWarning("Players ability to interact with UI has been disabled. There are no active instances of UiSelectedOnEnable on any gameobjects. PlayerUiMultiSelect needs an active UiSelectedOnEnable on a canvas to support multiple players selecting UI.");
                }
                return activeInstance;
            }
        }
        private static UiSelectedOnEnable activeInstance;

        /// <summary> Change all players ability to interact with UI. Does this by setting enabled of all InputSystemUIInputModules. </summary>
        public static void SetUiInteractable(bool isUiInteractable)
        {
            foreach (var uiInputModule in Players.Manage.UiInputModules)
            {
                uiInputModule.enabled = isUiInteractable;
            }
        }

        /// <summary> If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone. </summary>
        public bool IsGroupSelect { get { return groupSelection.IsGroupSelect; } }

        [Tooltip("UI element to have focus when this gameobject is activated. Also tells the UI nav system where to start from.")]
        [SerializeField]
        private Selectable FirstSelected;

        [Tooltip("Decisions around all players having to lock in a selection.")]
        [SerializeField]
        private LockedSelections groupSelection;

        /// <summary> Determines if all players in the group have locked their selection.
        /// Subscribes to an individual player locking their selection.  </summary>
        /// <returns> if all players are locked </returns>
        public bool OnPlayerLockChanged(bool isLocked)
        {
            return groupSelection.OnPlayerLockChanged(isLocked);
        }

        public GameObject GetFirstSelected()
        {
            return FirstSelected.gameObject;
        }

        private void OnEnable()
        {
            activeInstance = this;
            SetUiInteractable(true);
            ForceSelection();
            Players.Manage.OnUiPanelOpened(IsGroupSelect);
        }

        private void OnDisable()
        {
            groupSelection.ResetLockCount(); // otherwise bad values when this panel is re-enabled
            Players.Manage.OnUiPanelClosed();
        }

        private void Update()
        {
            ForceSelection();
        }

        private void ForceSelection()
        {
            // Each EventSystem should always have a UI element selected. Otherwise navigation can't work.
            foreach (var eventSystem in Players.Manage.EventSystems
                .Where(d => d.currentSelectedGameObject == null || false == d.currentSelectedGameObject.activeInHierarchy))
            {
                eventSystem.SetSelectedGameObject(FirstSelected.gameObject);
            }
        }
    }
}
