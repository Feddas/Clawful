using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class FollowDistanceJoint : MonoBehaviour
{
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D connectedBody;
    private Rigidbody2D rb;
    public float currentDistance;
    public float targetDistance;
    public float wiggleRoom;
    // The strength of the force applied to move the object closer to the connected body
    public float forceStrength = 10f;

    void Start()
    {
        distanceJoint = GetComponent<DistanceJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        connectedBody = distanceJoint.connectedBody;
    }

    void FixedUpdate()
    {
        if (connectedBody == null)
        {
            Debug.LogWarning("DistanceJoint2D does not have a connected body.");
            return;
        }

        targetDistance = distanceJoint.distance;

        Vector2 currentPosition = rb.position;
        Vector2 connectedPosition = connectedBody.position;
        currentDistance = Vector2.Distance(currentPosition, connectedPosition);

        if (currentDistance > (targetDistance + wiggleRoom) || currentDistance < wiggleRoom)
        {
            ApplyForceTowardsConnectedBody(currentPosition, connectedPosition, currentDistance);
        }
    }

    private void ApplyForceTowardsConnectedBody(Vector2 currentPosition, Vector2 connectedPosition, float currentDistance)
    {
        // Calculate the direction vector from the current position to the connected body
        Vector2 direction = (connectedPosition - currentPosition).normalized;

        // Calculate the required force to move closer
        //Vector2 force = direction * forceStrength * (currentDistance - targetDistance);
        Vector2 force = direction * forceStrength;

        // Apply the force to the object
        rb.AddForce(force, ForceMode2D.Force);
    }
}
