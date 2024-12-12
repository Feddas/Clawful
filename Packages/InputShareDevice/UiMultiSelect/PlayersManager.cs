using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

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

        public IEnumerable<PlayerInput> PlayerInputs
        {
            get { return Active.Values.Select(p => p.PlayerInput); }
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

        public void Add(string key, PlayerUiMultiSelect player)
        {
            Active.Add(key, player);

            /// late add player to already opened <seealso cref="OnUiPanelOpened(bool)"/>
            if (UiSelectedOnEnable.ActiveInstance != null)
            {
                SetupPlayer(player, UiSelectedOnEnable.ActiveInstance.IsGroupSelect);
            }
        }

        public void Remove(string key)
        {
            Active.Remove(key);
        }

        /// <summary> Setup a newly opened UiSelectedOnEnable.cs panel for already existing players </summary>
        public void OnUiPanelOpened(bool isGroupSelect)
        {
            foreach (var player in Active.Values)
            {
                SetupPlayer(player, isGroupSelect);
            }
        }

        /// <summary> Cleanup after a UiSelectedOnEnable.cs panel has closed </summary>
        public void OnUiPanelClosed()
        {
            foreach (var player in Active.Values)
            {
                player.IsGroupSelect = false;
            }
        }

        private void SetupPlayer(PlayerUiMultiSelect player, bool isGroupSelect)
        {
            player.IsGroupSelect = isGroupSelect;
            player.SetCursorLock(false);
        }
    }
}
