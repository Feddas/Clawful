using SoArchitecture;
using System.Linq;
using TMPro;
using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    [Tooltip("Where to put the victory text")]
    public TextMeshProUGUI VictoryText;

    [Tooltip("Score values of all players to be considered for Victory")]
    public IntVariable[] AvailableScores;

    void OnEnable()
    {
        var highscore = AvailableScores.Max(s => s.Value);
        var winners = AvailableScores
            .Where(s => s.Value == highscore)
            .Select(s => s.name.Replace("Score", ""));

        if (winners.Count() > 1)
        {
            VictoryText.text = "Players " + string.Join(" & ", winners) + " tied!";
        }
        else
        {
            VictoryText.text = "Player " + winners.First() + " won!";
        }
    }
}
