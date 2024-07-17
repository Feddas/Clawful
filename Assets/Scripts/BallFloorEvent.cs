using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Raises events when the little collider on the bottom of the ball touches things. </summary>
public class BallFloorEvent : MonoBehaviour
{
    public event Action<BlobBall> OnHitFloor;
    public event Action<BlobBall> OnFloorMissing;

    [Tooltip("Name of the bottom collider of both the left and right buckets")]
    public string BucketFloorName = "BucketFloor";
    public string GridName { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isTarget = other.name == BucketFloorName || other.name == GridName;
        if (isTarget)
        {
            if (OnHitFloor != null)
            {
                OnHitFloor.Invoke(this.transform.parent.GetComponent<BlobBall>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == GridName)
        {
            // Debug.Log(this.transform.parent.name + " no longer triggered by " + other.name);
            if (OnFloorMissing != null)
            {
                OnFloorMissing.Invoke(this.transform.parent.GetComponent<BlobBall>());
            }
        }
    }
}
