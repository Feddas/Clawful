using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawArmScript : MonoBehaviour
{
    public Quaternion openRot;
    public Quaternion closedRot;
    private float time = 0;
    public float closeDuration;
    public float openDuration;
    public bool canFunction = true;
    private bool clawOpen = true;

    // Start is called before the first frame update
    void Start()
    {
        openRot = transform.localRotation;
        closedRot = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ClawControl()
    {
        if (canFunction)
        {
            if (clawOpen)
            {
                time = 0;
                canFunction = false;
                StartCoroutine(CloseClaw());
            }
            else
            {
                time = 0;
                canFunction = false;
                StartCoroutine(OpenClaw());
            }
        }
    }

    IEnumerator CloseClaw()
    {
        while (time < openDuration)
        {
            float t = time / openDuration;

            // Interpolate rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, closedRot, t);
            time += Time.deltaTime;

            yield return null;
        }
        transform.localRotation = closedRot;
        clawOpen = false;
        canFunction = true;
    }

    IEnumerator OpenClaw()
    {
        while (time < openDuration)
        {
            float t = time / openDuration;

            // Interpolate rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, openRot, t);
            time += Time.deltaTime;

            yield return null;
        }
        transform.localRotation = openRot;
        clawOpen = true;
        canFunction = true;
    }
}
