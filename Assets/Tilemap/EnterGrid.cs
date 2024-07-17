using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary> Transitions a BlobBall from being free roaming to being owned by a grid. </summary>
public class EnterGrid : MonoBehaviour
{
    //public UnityEvent<DropBall> OnBallEntered;
    public UnityEvent<BlobBall> OnBlobEntered;

    void OnTriggerEnter2D(Collider2D other)
    {
        //DropBall db = other.GetComponent<DropBall>();
        //if (db != null && db.transform.localScale != Vector3.one)
        //{
        //    DropBallEntered(db);
        //    return;
        //}

        BlobBall blob = other.GetComponent<BlobBall>();
        if (blob != null && blob.transform.localScale != Vector3.one)
        {
            BlobBallEntered(blob);
            return;
        }
    }

    //void DropBallEntered(DropBall db)
    //{
    //    // setup ball to enter and stay in the grid
    //    db.transform.localScale = Vector3.one;
    //    SetGridPhysics(db.GetComponent<Rigidbody2D>());

    //    // raise that ball is ready to be centered, to enter a grid column
    //    if (OnBallEntered != null)
    //    {
    //        OnBallEntered.Invoke(db);
    //    }
    //}

    void BlobBallEntered(BlobBall blobBall)
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

    void SetGridPhysics(Rigidbody2D rb)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }
}
