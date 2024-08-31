using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary> Transitions a BlobBall from being free roaming to being owned by a grid. </summary>
public class EnterGrid : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Grid where entered balls will be added")]
    private BlobGrid DestinationGrid;

    void OnTriggerEnter2D(Collider2D other)
    {
        BlobBall blob = other.GetComponent<BlobBall>();
        if (blob != null && blob.transform.localScale != Vector3.one)
        {
            BlobBallEntered(blob);
            return;
        }
    }

    /// <summary> Setup ball to enter and stay in the grid </summary>
    private void BlobBallEntered(BlobBall blobBall)
    {
        // Pop ball to grid size
        blobBall.transform.localScale = Vector3.one;

        // center the ball to enter closest grid column
        StartCoroutine(LerpToGrid(blobBall));
    }

    private void SetGridPhysics(Rigidbody2D rb)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    /// <summary> Moves <paramref name="blobBall"/> to closest column in <seealso cref="DestinationGrid"/> </summary>
    private IEnumerator LerpToGrid(BlobBall blobBall)
    {
        // prepare for lerp
        blobBall.FloorSensor.gameObject.SetActive(false); // ensure ball isn't added to the tilemap in the middle of lerping
        var rb = blobBall.GetComponent<Rigidbody2D>();
        SetGridPhysics(rb);

        // do lerp
        Vector2 start = blobBall.transform.position;
        Vector2 end = DestinationGrid.SnapToClosestCell(blobBall.transform.position);
        float time = 0;
        float secondsToComplete = 0.2f;
        while (time < secondsToComplete)
        {
            blobBall.transform.position = Vector2.Lerp(start, end, time / secondsToComplete);
            blobBall.transform.rotation = Quaternion.Lerp(blobBall.transform.rotation, Quaternion.identity, time / secondsToComplete);
            time += Time.deltaTime;
            yield return null;
        }
        blobBall.transform.position = end;
        blobBall.transform.rotation = Quaternion.identity;

        // finalize ball entry.
        rb.bodyType = RigidbodyType2D.Dynamic;
        blobBall.FloorSensor.gameObject.SetActive(true);

        // Notify grid to allow ball to interact with tilemap.
        DestinationGrid.AddToGrid(blobBall);
    }
}
