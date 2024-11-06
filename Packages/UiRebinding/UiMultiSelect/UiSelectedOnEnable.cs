using ShareDevice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EventSystem = UnityEngine.EventSystems.EventSystem;

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
                    throw new System.Exception("There are no active instances of UiSelectedOnEnable on any gameobjects. PlayerUiMultiSelect needs an active UiSelectedOnEnable on a canvas to support multiple players selecting UI.");
                }
                return activeInstance;
            }
        }
        private static UiSelectedOnEnable activeInstance;

        /// <summary> If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone. </summary>
        public bool IsGroupSelect { get { return isGroupSelect; } }

        [Tooltip("UI element to have focus when this gameobject is activated. Also tells the UI nav system where to start from.")]
        [SerializeField]
        private Selectable FirstSelected;

        [Tooltip("If true, wait for all active eventsystems to lock a selection. If false, first eventsytem that selects a submission activates it for everyone.")]
        [SerializeField]
        private bool isGroupSelect;

        private void OnEnable()
        {
            activeInstance = this;
            FirstSelected.Select();
        }

        private void Update()
        {
            ForceSelection(EventSystem.current);
        }

        private void ForceSelection(EventSystem eventSystem)
        {
            if (eventSystem == null)
            {
                return;
            }

            // Each EventSystem should always have a UI element selected.
            if (eventSystem.currentSelectedGameObject == null)
            {
                FirstSelected.Select();
            }
        }
    }
}
