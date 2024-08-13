using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public bool isPlayerOne = true;

    public List<Button> buttons;
    public Button firstSelected;
    public MenuController mc;
    public Transform modelLocOne;
    public Transform modelLocTwo;
    public GameObject currentModel;

    // Start is called before the first frame update
    void Start()
    {
        mc = FindAnyObjectByType<MenuController>();
        mc.PlayerJoin(this);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        mc.SelectCharacter(this, character);
        if (isPlayerOne)
        {
            if (currentModel != null)
                Destroy(currentModel);

            GameObject go = Instantiate(character, modelLocOne);
            currentModel = go;
        }
        else
        {
            if (currentModel != null)
                Destroy(currentModel);

            GameObject go = Instantiate(character, modelLocTwo);
            currentModel = go;
        }
    }
}
