using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Functionality for all buttons related to the in-game pause panel </summary>
public class MenuPause : MonoBehaviour
{
    [Tooltip("When true, the pause panel is show in the Unity Scene view.")]
    [SerializeField]
    private bool ShowInEditor;

    [Tooltip("First button to have focus when the menu is opened. Also tells the UI nav system where to start from.")]
    [SerializeField]
    private Button FirstSelected;

    [Header("Sub Parts")]
    [SerializeField]
    private GameObject PauseStartButton;

    [SerializeField]
    private GameObject PausePanel;

    private void OnValidate()
    {
        PauseActive(ShowInEditor);
    }

    void Start()
    {
        PauseStop(); // hide pause panel
    }

    /// <summary> Called by buttons UnityEvents to show that their functionality isn't in yet. </summary>
    public void UsePlaceholder(TextMeshProUGUI text)
    {
        text.text = "placeholder";
        Debug.LogWarning(text.transform.parent.name + "s' UnityEvent is using placeholder functions.");
    }

    /// <summary> controller start button pressed or StartPause button's UnityEvent </summary>
    public void PauseStart()
    {
        PauseActive(true);
    }

    /// <summary> controller start button pressed again or PausePanels "x" button's UnityEvent </summary>
    public void PauseStop()
    {
        PauseActive(false);
    }

    private void PauseActive(bool isActive)
    {
        Time.timeScale = isActive ? 0f : 1f;
        PauseStartButton.SetActive(false == isActive);
        PausePanel.SetActive(isActive);

        if (isActive)
        {
            FirstSelected.Select();
        }
    }

    /// <summary> Called by exit button's UnityEvent </summary>
    public void OnExit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#elif UNITY_WEBGL
        // Application.OpenURL(webplayerQuitURL);
#else
        Application.Quit();
#endif
    }
}
