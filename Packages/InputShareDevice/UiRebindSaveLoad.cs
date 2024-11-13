using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputShareDevice
{
    /// <summary> copied from https://www.youtube.com/watch?v=csqVa2Vimao&t=1970s </summary>
    public class UiRebindSaveLoad : MonoBehaviour
    {
        /// <summary> PlayerPrefs key used to store rebinds </summary>
        public static readonly string Key = "rebinds";

        public InputActionAsset actions;

        public void OnEnable()
        {
            Load();
        }

        public void Save()
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(Key, rebinds);
        }

        public void Load()
        {
            var rebinds = PlayerPrefs.GetString(Key);
            if (false == string.IsNullOrEmpty(rebinds))
            {
                actions.LoadBindingOverridesFromJson(rebinds);
            }
        }

        public void SaveThenDeactivate()
        {
            Save();
            gameObject.SetActive(false);
        }
    }
}
