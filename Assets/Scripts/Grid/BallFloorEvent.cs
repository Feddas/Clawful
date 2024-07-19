using System;
using UnityEngine;

/// <summary> Raises events when the little collider on the bottom of the ball touches things. </summary>
public class BallFloorEvent : MonoBehaviour
{
    /// <summary> Hit bottom of bucket or tile on the tilemap </summary>
    public event Action<BlobBall> OnHitFloor;

    /// <summary> Previously raised OnHitFloor, notify that floor was deleted. </summary>
    public event Action<BlobBall> OnFloorMissing;

    [Tooltip("Name of the bottom collider of both the left and right buckets")]
    public string BucketFloorName = "BucketFloor";

    /// <summary> What grid this ball belongs to </summary>
    public string GridName { get; set; }

    /// <summary> Max Height a ball can have to interact with this grid. </summary>
    public float MaxHeight { get; set; } = int.MinValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isTarget = other.name == BucketFloorName || other.name == GridName;
        if (isTarget && this.transform.position.y < MaxHeight)
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
            if (OnFloorMissing != null)
            {
                OnFloorMissing.Invoke(this.transform.parent.GetComponent<BlobBall>());
            }
        }
    }
}
