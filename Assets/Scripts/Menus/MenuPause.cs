using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary> Functionality for all buttons related to the in-game pause panel </summary>
[RequireComponent(typeof(BreadcrumbManager))]
public class MenuPause : MonoBehaviour
{
    [Tooltip("InputAction or button used to show and hide the pause menu.")]
    [SerializeField]
    private InputAction TogglePause;

    [Tooltip("When true, the pause panel is show in the Unity Scene view.")]
    [SerializeField]
    private bool ShowInEditor;

    [Header("Sub Parts")]
    [SerializeField]
    private GameObject PauseStartButton;

    [SerializeField]
    private GameObject PausePanel;

    [Tooltip("Windows that should also be closed when the pause menu is closed.")]
    [SerializeField]
    private List<GameObject> Modals;

    public BreadcrumbManager breadcrumbs
    {
        get
        {
            return _breadcrumbs ??= this.GetComponent<BreadcrumbManager>();
        }
    }
    private BreadcrumbManager _breadcrumbs;

    private void OnValidate()
    {
        PauseActive(ShowInEditor);
    }

    void OnEnable()
    {
        breadcrumbs.OpenNewCrumb(PauseStartButton);
        PauseStop();

        TogglePause.Enable();
        TogglePause.performed += TogglePause_performed;
    }

    private void OnDisable()
    {
        TogglePause.performed -= TogglePause_performed;
        TogglePause.Disable();

        PauseActive(false); // ensure timescale doesn't remain 0

        breadcrumbs.CloseAll();
    }

    private void TogglePause_performed(InputAction.CallbackContext obj)
    {
        // current active state
        bool isActive = false == PauseStartButton.activeInHierarchy;

        // toggle to its opposite
        PauseActive(false == isActive);
        if (isActive)
        {
            breadcrumbs.CloseAll(but: 1);
        }
        else
        {
            breadcrumbs.OpenNewCrumb(PausePanel);
        }
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

    /// <summary> Changes timescale. Showing and hiding of the pausemenu is controlled by <seealso cref="breadcrumbs"/> </summary>
    private void PauseActive(bool isActive)
    {
        Time.timeScale = isActive ? 0f : 1f;
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
