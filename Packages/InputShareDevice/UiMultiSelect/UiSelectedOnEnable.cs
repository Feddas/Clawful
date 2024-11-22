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

        public static void SetUiInteractable(bool isUiInteractable)
        {
            foreach (var uiInputModule in Players.Manage.UiInputModules)
            {
                uiInputModule.enabled = isUiInteractable;
            }
        }

        /// <summary> If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone. </summary>
        public bool IsGroupSelect { get { return isGroupSelect; } }

        [Tooltip("UI element to have focus when this gameobject is activated. Also tells the UI nav system where to start from.")]
        [SerializeField]
        private Selectable FirstSelected;

        [Tooltip("If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone.")]
        [SerializeField]
        private bool isGroupSelect;

        [Tooltip("Raised when all players have locked in a selection. payload is true when group selection is locked. payload is false when group selection is broken.")]
        [SerializeField]
        private UnityEngine.Events.UnityEvent<bool> OnGroupSelect;

        /// <summary> Invoke this when all active eventsystems have a selection newly locked or broken. </summary>
        /// <param name="isLocked"> true when group selection is locked. false when group selection is broken. </param>
        public void InvokeGroupSelect(bool isLocked)
        {
            // Debug.Log($"{Time.frameCount} {(isLocked ? "everyone locked" : "lock broken")}. count {ShareDevice.LockedSelections.Instance.LockCount}.");
            OnGroupSelect.Invoke(isLocked);
        }

        private void OnEnable()
        {
            activeInstance = this;
            SetUiInteractable(true);
            ForceSelection();
            // TODO Players.Manage.OnUiPanelOpened(IsGroupSelect);   REMOVE PlayerUiMultiSelect.cs OnEnable()'s this.IsGroupSelect = UiSelectedOnEnable.ActiveInstance.IsGroupSelect;
        }

        private void OnDisable()
        {
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
