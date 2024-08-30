using TMPro;
using UnityEngine;

/// <summary> Show the Clawful version in a TextMeshProUGUI </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ShowVersion : MonoBehaviour
{
    /// <summary> automatically find text that will hold the version string </summary>
    private TextMeshProUGUI versionLabel
    {
        get
        {
            return _versionLabel ??= this.GetComponent<TextMeshProUGUI>();
        }
    }
    private TextMeshProUGUI _versionLabel;

    void Start()
    {
        OnValidate();
    }

    void OnValidate()
    {
        versionLabel.text = Application.version;
    }
}
