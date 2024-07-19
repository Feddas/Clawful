using UnityEngine;
using UnityEngine.Events;

/// <summary> Transitions a BlobBall from being free roaming to being owned by a grid. </summary>
public class EnterGrid : MonoBehaviour
{
    [Tooltip("When a qualifying ball has entered, notify BlobGrid.AddToGrid for additional modifications.")]
    public UnityEvent<BlobBall> OnBlobEntered;

    void OnTriggerEnter2D(Collider2D other)
    {
        BlobBall blob = other.GetComponent<BlobBall>();
        if (blob != null && blob.transform.localScale != Vector3.one)
        {
            BlobBallEntered(blob);
            return;
        }
    }

    private void BlobBallEntered(BlobBall blobBall)
    {
        // setup ball to enter and stay in the grid
        blobBall.transform.localScale = Vector3.one;
        SetGridPhysics(blobBall.GetComponent<Rigidbody2D>());

        // raise that ball is ready to be centered, to enter a grid column
        if (OnBlobEntered != null)
        {
            OnBlobEntered.Invoke(blobBall);
        }
    }

    private void SetGridPhysics(Rigidbody2D rb)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }
}
