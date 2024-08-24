using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Timeline.Actions;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject selectMenu;
    public GameObject characterMenu;
    private bool hasPlayerOne;
    public EventSystem eventSystem;
    public GameManager gm;
    public PlayerInputManager pm;

    public GameObject playerOneCharacter;
    public GameObject playerTwoCharacter;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerJoin(CharacterSelector cs)
    {
        if (!hasPlayerOne)
            hasPlayerOne = true;
        else
        {
            cs.BecomPlayerTwo();
        }
    }

    public void SelectCharacter(CharacterSelector c, GameObject character)
    {
        if (c.isPlayerOne)
            playerOneCharacter = character;
        else
            playerTwoCharacter = character;
    }

    public void RemoveCharacter(CharacterSelector c)
    {
        if (c.isPlayerOne)
            playerOneCharacter = null;
        else
            playerTwoCharacter = null;
    }

    public void ChangeToCharacterSelect()
    {
        startMenu.SetActive(false);
        selectMenu.SetActive(true);
        pm.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        pm.playerPrefab = characterMenu;
    }
}
