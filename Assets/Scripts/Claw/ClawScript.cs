using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ClawData
{
    public Transform claw;
    public Vector3 startP;
    public Quaternion closedRot;
    public Quaternion openedRot;
    //used with hinge joints.
    //public HingeJoint2D rightClaw;
    //public Vector3 rightP;
    //public float rightClose;
    //public float rightOpen;
}
public class ClawScript : MonoBehaviour
{
    public bool isPlayerOne;
    public float speed = 2f;
    public float acceleration = 10f;
    //public float jumpForce = 15f;

    public DistanceJoint2D dj2D;
    public Rigidbody2D djRB;
    public float distanceChangeRate = 0.1f;
    public float minDistance = 0.5f; // Minimum limit for the distance
    public float maxDistance = 5f; // Maximum limit for the distance
    public float distanceOffset = 8; //How far the max distance can go from its current position

    //middle of rope reference
    public GameObject middleRope;
    private DistanceJoint2D jointOne;
    private  DistanceJoint2D jointTwo;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    private InputAction moveAction;
    private InputAction clawAction;
    //private InputAction jumpAction;

    //double tap dash inputs
    private float lastTapTimeLeft;
    private float lastTapTimeRight;
    public float dashForce = 2f; // Force multiplier for dashing
    public float doubleTapTime = 0.2f; // Time threshold for detecting a double tap

    // Charging variables
    public bool isCharging;
    public float chargeForce;
    public float chargeDistance;
    public float maxChargeForce = 100f;
    public float chargeRate = 10f;
    public float chargeFRate = 100;
    public float chargeDRate = 0.5f;
    public float maxChargeDistance = 3f;

    [Header ("Claw Reference")]
    public List<ClawData> claws;
    private float time = 0;
    public float closeDuration;
    public float openDuration;
    public HingeJoint2D clawLeft;
    public HingeJoint2D clawRight;
    public float clawStrength; //public so maybe something can weaken it?

    private bool canFunction = true;
    private bool clawOpen = true;
    //public bool isGrounded = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        djRB = dj2D.GetComponent<Rigidbody2D>();
        minDistance = dj2D.distance;
        maxDistance = dj2D.distance + distanceOffset;

        foreach (ClawData c in claws)
        {
            c.openedRot = c.claw.rotation;
        }

        DistanceJoint2D[] joints = middleRope.GetComponents<DistanceJoint2D>();
        if (joints.Length >= 2)
        {
            jointOne = joints[0];
            jointTwo = joints[1];
        }
        else
        {
            Debug.LogError("Not Enough Joints on Rope");
        }

        JointMotor2D left = clawLeft.motor;
        left.motorSpeed = clawStrength;
        clawLeft.motor = left;
        JointMotor2D right = clawRight.motor;
        right.motorSpeed = -clawStrength;
        clawRight.motor = right;
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        clawAction = playerInput.actions["ActivateClaw"];
        //jumpAction = playerInput.actions["Jump"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        clawAction.performed += OnActivateClaw;
        //jumpAction.performed += OnJump;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        //clawAction.performed -= OnActivateClaw;
    }

    void FixedUpdate()
    {
        Move();
        AdjustDistance();
    }

    private void Update()
    {
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        DetectDoubleTap(input);
        moveInput = input;
    }

    private void OnActivateClaw(InputAction.CallbackContext context)
    {
        ActivateClaw();
    }

    private void DetectDoubleTap(Vector2 input)
    {
        if (input.x < 0) // Left movement key pressed
        {
            if (Time.time - lastTapTimeLeft < doubleTapTime)
            {
                DashLeft();
            }
            lastTapTimeLeft = Time.time;
        }
        else if (input.x > 0) // Right movement key pressed
        {
            if (Time.time - lastTapTimeRight < doubleTapTime)
            {
                DashRight();
            }
            lastTapTimeRight = Time.time;
        }
    }

    private void DashLeft()
    {
        rb.AddForce(Vector2.left * dashForce, ForceMode2D.Impulse);
    }
    private void DashRight()
    {
        rb.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);
    }

    private void Move()
    {
        Vector2 targetVelocity = moveInput * speed;
        Vector2 velocityChange = targetVelocity - rb.velocity;
        Vector2 accelerationVector = velocityChange.normalized * acceleration;

        // Limit the acceleration to not exceed the required change in velocity
        if (accelerationVector.magnitude > velocityChange.magnitude)
        {
            accelerationVector = velocityChange;
        }

        rb.AddForce(accelerationVector * rb.mass, ForceMode2D.Force);
    }

    private void AdjustDistance()
    {
        float combinedMass = rb.mass;
        if (dj2D.connectedBody != null)
        {
            combinedMass += dj2D.connectedBody.mass;
        }

        // Adjust the rate based on combined mass
        float adjustedRate = distanceChangeRate / combinedMass * Time.fixedDeltaTime;

        // Charge logic for player one
        if (isPlayerOne && moveInput.y > 0 && dj2D.distance <= minDistance)
        {
            isCharging = true;
            chargeForce = Mathf.Min(chargeForce + chargeFRate * Time.fixedDeltaTime, maxChargeForce);
            chargeDistance = Mathf.Min(chargeDistance + chargeDRate * Time.fixedDeltaTime, maxChargeDistance);
        }
        else if (isPlayerOne)
        {
            if (isCharging)
            {
                //StartCoroutine(LaunchClaw());
                isCharging = false;
                dj2D.distance = Mathf.Clamp(chargeDistance, minDistance, maxDistance);
                jointOne.distance = dj2D.distance / 2;
                jointTwo.distance = dj2D.distance / 2;
                djRB.AddForce(-djRB.transform.up * chargeForce, ForceMode2D.Impulse);
                chargeForce = 0;
                chargeDistance = minDistance;
            }

            if (moveInput.y > 0)
            {
                dj2D.distance = Mathf.Clamp(dj2D.distance - adjustedRate, minDistance, maxDistance);
                jointOne.distance = dj2D.distance / 2;
                jointTwo.distance = dj2D.distance / 2;
            }
            else if (moveInput.y < 0)
            {
                dj2D.distance = Mathf.Clamp(dj2D.distance + adjustedRate, minDistance, maxDistance);
                jointOne.distance = dj2D.distance / 2;
                jointTwo.distance = dj2D.distance / 2;
            }
        }

        // Charge logic for player two
        if (!isPlayerOne && moveInput.y > 0 && dj2D.distance <= minDistance)
        {
            isCharging = true;
            chargeForce = Mathf.Min(chargeForce + chargeFRate * Time.fixedDeltaTime, maxChargeForce);
            chargeDistance = Mathf.Min(chargeDistance + chargeDRate * Time.fixedDeltaTime, maxChargeDistance);
        }
        else if (!isPlayerOne)
        {
            if (isCharging)
            {
                //StartCoroutine(LaunchClaw());
            }

            if (moveInput.y > 0)
            {
                dj2D.distance = Mathf.Clamp(dj2D.distance - adjustedRate, minDistance, maxDistance);
                jointOne.distance = dj2D.distance / 2;
                jointTwo.distance = dj2D.distance / 2;
            }
            else if (moveInput.y < 0)
            {
                dj2D.distance = Mathf.Clamp(dj2D.distance + adjustedRate, minDistance, maxDistance);
                jointOne.distance = dj2D.distance / 2;
                jointTwo.distance = dj2D.distance / 2;
            }
        }
    }

    public void LaunchClaw()
    {
        jointOne.distance = dj2D.distance / 2;
        jointTwo.distance = dj2D.distance / 2;
    }

    //IEnumerator LaunchClaw()
    //{
    //    isCharging = false;
    //    dj2D.distance = Mathf.Clamp(chargeDistance, minDistance, maxDistance);
    //    yield return new WaitForSeconds(0.5f);
    //    // Calculate direction from DistanceJoint2D target to the claw
    //    Vector2 direction = (dj2D.connectedAnchor - rb.position).normalized;
    //    djRB.AddForce(direction * chargeForce, ForceMode2D.Impulse);
    //    chargeForce = 0;
    //    chargeDistance = minDistance;
    //}

    public void ActivateClaw()
    {
        if (canFunction)
        {
            if (!clawOpen)
            {
                print("open");
                time = 0;
                //canFunction = false;
                JointMotor2D left = clawLeft.motor;
                left.motorSpeed = clawStrength;
                clawLeft.motor = left;
                JointMotor2D right = clawRight.motor;
                right.motorSpeed = -clawStrength;
                clawRight.motor = right;
                clawOpen = true;
                //StartCoroutine(OpenClaw());
                //OpenC();
            }
            else
            {
                print("close");
                time = 0;
                //canFunction = false;
                JointMotor2D left = clawLeft.motor;
                left.motorSpeed = -clawStrength;
                clawLeft.motor = left;
                JointMotor2D right = clawRight.motor;
                right.motorSpeed = clawStrength;
                clawRight.motor = right;
                clawOpen = false;
                //StartCoroutine(CloseClaw());
                //CloseC();
            }
        }
    }

    IEnumerator CloseClaw()
    {

        while (time < closeDuration)
        {
            time += Time.deltaTime;
            float t = time / closeDuration;

            for (int i = 0; i < claws.Count; i++)
            {
                ClawData clawData = claws[i];
                Quaternion initialRot = claws[i].claw.localRotation;
                Quaternion closedRot = Quaternion.identity;

                // Interpolate rotation
                clawData.claw.localRotation = Quaternion.Lerp(initialRot, closedRot, t);
            }

            yield return null;
        }

        // Ensure all claws are exactly at the closed rotation
        foreach (ClawData clawData in claws)
        {
            clawData.claw.localRotation = Quaternion.identity;
        }

        clawOpen = false;
        canFunction = true;
    }

    IEnumerator OpenClaw()
    {
        // Store initial rotations
        List<Quaternion> initialRotations = new List<Quaternion>();
        foreach (ClawData clawData in claws)
        {
            initialRotations.Add(clawData.claw.localRotation);
        }

        while (time < openDuration)
        {
            time += Time.deltaTime;
            float t = time / openDuration;

            for (int i = 0; i < claws.Count; i++)
            {
                ClawData clawData = claws[i];
                Quaternion initialRot = initialRotations[i];
                Quaternion openRot = clawData.openedRot;

                // Interpolate rotation
                clawData.claw.localRotation = Quaternion.Lerp(initialRot, openRot, t);
            }

            yield return null;
        }

        // Ensure all claws are exactly at the closed rotation
        foreach (ClawData clawData in claws)
        {
            clawData.claw.localRotation = clawData.openedRot;
        }

        clawOpen = true;
        canFunction = true;
    }
}
