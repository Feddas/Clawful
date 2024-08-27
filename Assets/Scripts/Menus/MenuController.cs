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
using UnityEngine.SceneManagement;

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
    public int idOne;
    public GameObject playerTwoCharacter;
    public int idTwo;

    public float readyTimer = 5;
    public bool isReady;
    private bool doneLoad;

    public string currentLevel;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isReady && !doneLoad)
        {
            if (readyTimer > 0)
                readyTimer -= Time.fixedDeltaTime;
            else if (readyTimer <= 0)
            {
                doneLoad = true;
                SceneManager.LoadScene(currentLevel);
            }
        }
    }

    //private void Awake()
    //{
    //    if (isReady)
    //    {
    //        print("find PM");
    //        pm = FindAnyObjectByType<PlayerInputManager>();
    //        pm.playerPrefab = playerOneCharacter;
    //        pm.JoinPlayer(0, idOne);
    //        pm.playerPrefab = playerTwoCharacter;
    //        pm.JoinPlayer(1, idTwo);
    //    }
    //}

    private void OnLevelWasLoaded()
    {
        if (isReady)
        {
            print("find PM");
            pm = FindAnyObjectByType<PlayerInputManager>();
            pm.playerPrefab = playerOneCharacter;
            pm.JoinPlayer(0,idOne);
            pm.playerPrefab = playerTwoCharacter;
            pm.JoinPlayer(1, idTwo);
            Destroy(this.gameObject);
        }
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

    public void SelectCharacter(CharacterSelector c, GameObject character, int newID)
    {
        if (c.isPlayerOne)
        {
            playerOneCharacter = character;
            idOne = newID;
        }
        else
        {
            playerTwoCharacter = character;
            idTwo = newID;
        }

        if (playerTwoCharacter != null && playerOneCharacter != null)
            isReady = true;
        else
        {
            isReady = false;
            readyTimer = 5;
        }
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
