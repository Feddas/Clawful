using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

/// <summary> Copy of https://github.com/Feddas/SoArchitecture/blob/master/Setters/TextStringFormat.cs </summary>
namespace SoArchitecture
{
    /// <summary> Calls ToString() on all VariablesToFormat. Then puts them in the format string. </summary>
    public class TextStringFormatTmpro : MonoBehaviour
    {
        [Tooltip("Where to put the formatted text")]
        public TextMeshProUGUI OutputText;

        [Tooltip("First variable here is placed into {0} below, second into {1}, etc..")]
        public Object[] VariablesToFormat;

        [TextArea]
        [Tooltip("Use string.Format style, with the {0}'s")]
        public string Format;

        [SerializeField]
        [TextArea]
        [Tooltip("For debug only. Resulting text")]
        private string Formatted;

        private void Start()
        {
            UpdateText();
        }

        private void OnValidate()
        {
            UpdateText();
        }

        public void UpdateText()
        {
            // format the text
            if (VariablesToFormat != null && VariablesToFormat.Length > 0)
            {
                // string.Format calls ToString() on every supplied parameter
                Formatted = string.Format(Format, VariablesToFormat);
            }
            else // text contains an error
            {
                Formatted = string.Format("error: {0}s TextStringFromat has no values in VariablesToFormat", this.name);
            }

            // display the text
            if (OutputText != null)
            {
                OutputText.text = Formatted;
            }
        }
    }
}
