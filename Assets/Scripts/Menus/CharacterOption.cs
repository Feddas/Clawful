using ShareDevice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> clickable button for each player to select their claw character option. </summary>
public class CharacterOption : MonoBehaviour
{
    [SerializeField]
    private GameObject clawPrefab;

    /// <summary> Loads <seealso cref="clawPrefab"/> for the player that clicked the button. Called by <seealso cref="EventTrigger"/> component's Submit UnityEvent on this same gameojbect </summary>
    public void OnSubmit(BaseEventData eventData)
    {
        // Debug.Log($"{Time.frameCount} {this.name} clicked on by {eventData.currentInputModule.name}");
        var player = eventData.currentInputModule.GetComponent<PlayerInputRespawn>();
        player.RespawnAs(clawPrefab);
    }
}
