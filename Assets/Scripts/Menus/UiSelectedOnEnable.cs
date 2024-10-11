using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiSelectedOnEnable : MonoBehaviour
{
    [Tooltip("UI element to have focus when this gameobject is activated. Also tells the UI nav system where to start from.")]
    [SerializeField]
    private Selectable FirstSelected;

    private void OnEnable()
    {
        FirstSelected.Select();
    }

    private void Update()
    {
        // When a mouse pointer selects null UI it disables a gamepads ability to navigate UI. Fix by reverting to FirstSelected if gamepad is actuated (ref https://youtu.be/lclDl-NGUMg?t=3290 )
        if (UnityEngine.InputSystem.Gamepad.current != null
            && UnityEngine.InputSystem.Gamepad.current.wasUpdatedThisFrame
            && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            FirstSelected.Select();
        }
    }
}
