using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ShareDevice
{
    /// <summary> Singleton to manage all players </summary>
    public class Players
    {
        public static Players Manage
        {
            get
            {
                return instance ??= new Players();
            }
        }
        private static Players instance;

        /// <summary> key is playerInput.currentControlScheme. </summary>
        private Dictionary<string, PlayerUiMultiSelect> Active { get; set; } = new Dictionary<string, PlayerUiMultiSelect>();

        public IEnumerable<PlayerUiMultiSelect> UiMultiSelectors
        {
            get { return Active.Values; }
        }

        public IEnumerable<UnityEngine.InputSystem.UI.MultiplayerEventSystem> EventSystems
        {
            get { return Active.Values.Select(p => p.EventSystem); }
        }

        public IEnumerable<UnityEngine.InputSystem.UI.InputSystemUIInputModule> UiInputModules
        {
            get { return Active.Values.Select(p => p.UiInput); }
        }

        public PlayerUiMultiSelect this[string key]
        {
            get
            {
                return Active[key];
            }
        }

        public void Add(string key, PlayerUiMultiSelect value)
        {
            Active.Add(key, value);
        }

        public void Remove(string key)
        {
            Active.Remove(key);
        }

        /// <summary> Setup after a new UiSelectedOnEnable.cs panel has opened </summary>
        //public void OnUiPanelOpened(bool isGroupSelect)
        //{
        //    foreach (var player in Active.Values)
        //    {
        //        player.IsGroupSelect = isGroupSelect;
        //    }
        //}

        /// <summary> Cleanup after a UiSelectedOnEnable.cs panel has closed </summary>
        public void OnUiPanelClosed()
        {
            foreach (var player in Active.Values)
            {
                player.IsGroupSelect = false;
            }
        }
    }
}