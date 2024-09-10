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
}
