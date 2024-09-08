using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ReadySetGo : MonoBehaviour
{
    [SerializeField]
    private List<string> words = new List<string>() { "Ready", "Set", "GO!" };

    [SerializeField]
    private float SecondsPerWord = 3;

    [SerializeField]
    private AnimationCurve Easing = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, -4, 0));

    /// <summary> automatically find text that will hold string values </summary>
    private TextMeshProUGUI label
    {
        get
        {
            return _label ??= this.GetComponent<TextMeshProUGUI>();
        }
    }
    private TextMeshProUGUI _label;

    void OnEnable()
    {
        StartCoroutine(FadeWordsInSequence());
    }

    /// <param name="isControllable"> en/disable all claw rig game objects in the scene </param>
    private void ClawsControllable(bool isControllable)
    {
        var claws = FindObjectsByType<ClawScript>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(c => c.transform.parent.gameObject)
            .Where(c => c.name.Contains("(Clone)"));

        foreach (var claw in claws)
        {
            claw.SetActive(isControllable);
        }
    }

    /// <summary> Shows all <seealso cref="words"/> one after another. </summary>
    /// <param name="callback"> Action preformed after all words have been shown </param>
    private IEnumerator FadeWordsInSequence()
    {
        yield return null; // give a frame for claws to spawn in
        ClawsControllable(false);

        foreach (string word in words)
        {
            label.text = word;
            yield return EaseIn();
        }

        // finished all words
        label.text = "";
        ClawsControllable(true);
    }

    private IEnumerator EaseIn()
    {
        float time = 0;
        float secondsToComplete = SecondsPerWord / 2;
        while (time < secondsToComplete)
        {
            float percent = time / secondsToComplete;
            this.transform.localScale = Vector3.one * Easing.Evaluate(percent);
            // label.alpha = percent; // transparency doesn't look to good and isn't performant. Leaving this dead code in there as a reminder not to bother with it.
            time += Time.deltaTime;
            yield return null;
        }
        this.transform.localScale = Vector3.one * Easing.Evaluate(1f);
    }
}