using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSmoothTime = 0.1f; // How snappy the turning is

    [Header("Dependencies")]
    // We need the camera to know which way is "Forward"
    [SerializeField] private Transform cameraTransform;

    // Internal State
    private Rigidbody rb;
    private InputSystem_Actions playerActions;
    private Vector2 moveInput;
    private float currentVelocity; // Helper for smooth rotation

    private void Awake()
    {
        playerActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();

        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable() => playerActions.Player.Enable();
    private void OnDisable() => playerActions.Player.Disable();

    private void Update()
    {
        // 1. Read Input
        moveInput = playerActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        RotatePlayerToCamera();
        MovePlayer();
    }

    private void RotatePlayerToCamera()
    {
        // If we have a camera, we want the player to face the same direction
        // but ONLY on the Y axis (we don't want the player leaning back if looking up)

        float targetAngle = cameraTransform.eulerAngles.y;

        // Smoothly dampen the rotation so it doesn't snap instantly (looks better)
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);

        rb.MoveRotation(Quaternion.Euler(0f, angle, 0f));
    }

    private void MovePlayer()
    {
        // Now that the player is facing the camera direction (thanks to RotatePlayerToCamera),
        // we can simply use the player's own Forward and Right vectors.

        // transform.forward points where the player (and camera) is looking.
        // transform.right points to the player's right.

        Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);

        // Normalize to prevent diagonal movement being faster (Pythagoras theorem)
        if (moveDirection.magnitude > 1)
            moveDirection.Normalize();

        // Apply movement
        Vector3 finalMovement = moveDirection * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + finalMovement);
    }
}