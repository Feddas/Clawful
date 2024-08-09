using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject characterMenu;
    public GameObject characterMenu2;
    public EventSystem eventSystem;
    public GameManager gm;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeToCharacterSelect()
    {
        startMenu.SetActive(false);
        GameObject newChar = Instantiate(characterMenu);
        GameObject newChar2 = Instantiate(characterMenu2);

        //eventSystem.firstSelectedGameObject = characterMenu.transform.Find("1").gameObject;
        //characterMenu.transform.Find("1").GetComponent<Button>().Select();
    }
}
