using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuRebindPanel : MonoBehaviour
{
    /// <summary> Reference to all players in the game </summary>
    private static PlayerInput[] players;

    [Tooltip("Text that will be updated with errors about players connected.")]
    [SerializeField]
    private TextMeshProUGUI displayErrors;

    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private CanvasGroup activeWithPlayers;

    [SerializeField]
    private BreadcrumbManager breadcrumbManager;

    /// <summary> caches the value of <see cref="displayErrors"/> text's serialized Unity Inspector value. Its text is overwritten with errors. Then is restored, with this variable, when there is no longer an error. </summary>
    private string textBeforeError = null;

    private void OnEnable()
    {
        players = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);

        // can't find players
        if (players == null || players.Length == 0)
        {
            activeWithPlayers.interactable = false;
            if (textBeforeError == null) // then cache the valuve of displayErrors.text before it is over written.
            {
                textBeforeError = displayErrors.text;
            }
            displayErrors.text = "No players have joined";
            return;
        }

        else // allow players to rebind keys
        {
            if (textBeforeError != null) // then restore the value of displayErrors.text from the cache
            {
                displayErrors.text = textBeforeError;
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
