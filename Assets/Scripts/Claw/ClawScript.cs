using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClawScript : MonoBehaviour
{
    public bool isPlayerOne;
    public bool canFunction = false;
    public float speed = 2f;
    public float yPosition;

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

    public Vector2 moveInput;
    private Rigidbody2D rb;

    private InputAction moveAction;
    private InputAction clawAction;

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
    public List<ClawArmScript> claws;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        djRB = dj2D.GetComponent<Rigidbody2D>();
        yPosition = transform.position.y;
        minDistance = dj2D.distance;
        maxDistance = dj2D.distance + distanceOffset;

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
    }

    private void OnEnable()
    {
        var playerInput = GetComponentInParent<PlayerInput>();

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
        clawAction.performed -= OnActivateClaw;
    }

    void FixedUpdate()
    {
        if (canFunction)
        {
            Move();
            AdjustDistance();
        }
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
        if (canFunction)
        {
            ActivateClaw();
        }
    }

    private void DetectDoubleTap(Vector2 input)
    {
        if (canFunction)
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
        Vector2 targetVelocity = new Vector2 (moveInput.x * speed, 0);
        rb.AddForce(targetVelocity * rb.mass, ForceMode2D.Force);
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
                transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);
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

    public void ActivateClaw()
    {
        foreach (ClawArmScript claw in claws)
        {
            claw.ClawControl();
        }
    }
}
