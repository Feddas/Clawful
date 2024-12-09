using ShareDevice;
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

    [Tooltip("If false, players can't move their claws while the sequence is playing.")]
    [SerializeField]
    private bool IsClawControllableDuringSequence = false;

    [Tooltip("If true, the claw is respawned as it becomes controllable.")]
    [SerializeField]
    private bool IsRespawningClaw = false;

    [SerializeField]
    private UnityEngine.Events.UnityEvent OnSequenceCompleted;

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
            if (isControllable && IsRespawningClaw) // reset position of players prefab
            {
                claw.GetComponentInParent<PlayerInputRespawn>()?.DoRespawn();
            }
        }
    }

    /// <summary> Shows all <seealso cref="words"/> one after another. </summary>
    /// <param name="callback"> Action preformed after all words have been shown </param>
    private IEnumerator FadeWordsInSequence()
    {
        this.transform.localScale = Vector3.zero;
        yield return null; // give a frame for claws to spawn in
        ClawsControllable(IsClawControllableDuringSequence);

        foreach (string word in words)
        {
            label.text = word;
            yield return EaseIn();
        }

        // finished all words
        label.text = "";
        OnSequenceCompleted?.Invoke();
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
