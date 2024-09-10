using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuRebindPanel : MonoBehaviour
{
    /// <summary> Reference to all players in the game </summary>
    private static PlayerInput[] players;

    [Tooltip("Text that will be updated with the name of the current player who's controls are being modified.")]
    [SerializeField]
    private TextMeshProUGUI displayErrors;

    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private CanvasGroup activeWithPlayers;

    [SerializeField]
    private BreadcrumbManager breadcrumbManager;

    private string? initialText = null;

    private void OnEnable()
    {
        players = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);

        if (players == null || players.Length == 0)
        {
            activeWithPlayers.interactable = false;
            if (initialText == null)
            {
                initialText = displayErrors.text;
            }
            displayErrors.text = "No players have joined";
            return;
        }
        else
        {
            if (initialText != null)
            {
                displayErrors.text = initialText;
            }
            activeWithPlayers.interactable = true;
        }
    }

    public void OnDisable()
    {
        if (breadcrumbManager != null)
        {
            breadcrumbManager.CloseCurrent();
        }
    }

    public void ResetAllBindings()
    {
        foreach (var map in inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
}
