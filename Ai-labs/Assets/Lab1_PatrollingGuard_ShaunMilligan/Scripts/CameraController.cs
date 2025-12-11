using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target; // Drag your Player object here
    [Tooltip("The offset from the player (Right, Up, Back)")]
    [SerializeField] private Vector3 offset = new Vector3(1.0f, 1.5f, -4.0f); // Standard TPS offset

    [Header("Input Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float lookUpMin = -40f; // How high you can look up
    [SerializeField] private float lookDownMax = 60f; // How low you can look down

    // Internal State
    private InputSystem_Actions playerActions;
    private float pitch = 0.0f; // Up/Down angle
    private float yaw = 0.0f;   // Left/Right angle

    private void Awake()
    {
        playerActions = new InputSystem_Actions();
    }

    private void OnEnable() => playerActions.Player.Enable();
    private void OnDisable() => playerActions.Player.Disable();

    private void Start()
    {
        // 1. Capture and Hide the Mouse Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize angles to current camera rotation to prevent snapping
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
    }

    // LateUpdate runs after the Player has moved in Update/FixedUpdate.
    // This prevents the camera from "jittering" or lagging behind the player.
    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        FollowTarget();
    }

    private void HandleRotation()
    {
        // Read mouse delta (change in position)
        Vector2 lookInput = playerActions.Player.Look.ReadValue<Vector2>();

        // Adjust Yaw (Horizontal) and Pitch (Vertical)
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity; // Subtracting moves view Up when mouse goes Up

        // Clamp the Pitch (prevent neck breaking or flipping)
        pitch = Mathf.Clamp(pitch, lookUpMin, lookDownMax);
    }

    private void FollowTarget()
    {
        // 1. Create the rotation based on input
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 2. Rotate the Offset to match the camera's new angle
        // This ensures that if we look right, the camera swings around the player
        Vector3 rotatedOffset = targetRotation * offset;

        // 3. Calculate final position
        Vector3 finalPosition = target.position + rotatedOffset;

        // 4. Apply transforms
        transform.position = finalPosition;
        transform.rotation = targetRotation;
    }
}