using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterSelector : MonoBehaviour
{
    public bool isPlayerOne = true;

    public List<Button> buttons;
    public Button firstSelected;
    public MenuController mc;
    public Transform modelLocOne;
    public Transform modelLocTwo;
    public GameObject currentModel;

    private InputAction cancelAction;
    public InputDevice device;

    // Start is called before the first frame update
    void Start()
    {
        mc = FindAnyObjectByType<MenuController>();
        mc.PlayerJoin(this);
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        device = playerInput.devices[0];
        cancelAction = playerInput.actions["Cancel"];
        cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        cancelAction.performed -= OnCancel;
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        if (currentModel != null)
            Deselect();
    }

    public void BecomPlayerTwo()
    {
        isPlayerOne = false;
        foreach (Button b in buttons)
        {
            ColorBlock cb = b.colors;
            cb.selectedColor = Color.green;
            b.colors = cb;
            RectTransform rt = b.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(55, 55);
        }
    }

    public void PickCharacter(GameObject character)
    {
        mc.SelectCharacter(this, character, device.deviceId);
    }

    public void DisplayCharacter(GameObject model)
    {
        if (isPlayerOne)
        {
            if (currentModel != null)
                Destroy(currentModel);

            GameObject go = Instantiate(model, modelLocOne);
            currentModel = go;
        }
        else
        {
            if (currentModel != null)
                Destroy(currentModel);

            GameObject go = Instantiate(model, modelLocTwo);
            currentModel = go;
        }
    }

    public void Deselect()
    {
        if(currentModel != null)
        {
            mc.RemoveCharacter(this);
            Destroy(currentModel);
        }
    }
}
