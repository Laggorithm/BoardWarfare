using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public bool IsRotationActive = false;
    public float moveSpeed = 2f;            // Speed of camera movement
    public float screenEdgeThreshold = 0.1f; // Threshold for detecting the cursor at the screen's edges
    public float pitchAngle = 35f;           // Fixed downward pitch angle
    public float rotationSpeed = 5f;         // Speed of smooth rotation
    public float rotationAngle = 30f;        // Angle to rotate when pressing E or Q
    public float rotationStopThreshold = 0.1f; // Threshold to determine when the camera is "stopped"
    public bool isMovementEnabled = false;    // Toggle for enabling/disabling movement

    private Vector3 movement;
    private float fixedYPosition;            // Store the initial Y position to keep it fixed
    private Quaternion targetRotation;       // Smooth target rotation

    private readonly float[] allowedAngles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f, -180 }; // Allowed rotation angles

    void Start()
    {
        // Set initial pitch angle and fix Y position
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = pitchAngle;
        transform.eulerAngles = currentRotation;
        fixedYPosition = transform.position.y;

        // Lock cursor within the game screen and hide it
        Cursor.lockState = CursorLockMode.Confined; // Lock cursor within screen
        Cursor.visible = true; // Set to false to hide the cursor if needed

        targetRotation = transform.rotation; // Initialize target rotation
    }

    void Update()
    {
        HandleToggleMovement();
        if (isMovementEnabled)
        {
            HandleMovement();
        }
        HandleRotation();
    }

    void HandleToggleMovement()
    {
        // Toggle movement on or off when 'C' is pressed
        if (Input.GetKeyDown(KeyCode.C))
        {
            isMovementEnabled = !isMovementEnabled;
        }
    }

    void HandleMovement()
    {
        // Get mouse position in screen space
        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Determine movement direction based on mouse position relative to screen edges
        float moveX = 0f;
        float moveZ = 0f;

        if (mousePos.x < screenWidth * screenEdgeThreshold)      // Mouse near left edge
        {
            moveX = -moveSpeed;
        }
        else if (mousePos.x > screenWidth * (1 - screenEdgeThreshold)) // Mouse near right edge
        {
            moveX = moveSpeed;
        }

        if (mousePos.y < screenHeight * screenEdgeThreshold)     // Mouse near bottom edge
        {
            moveZ = -moveSpeed;
        }
        else if (mousePos.y > screenHeight * (1 - screenEdgeThreshold)) // Mouse near top edge
        {
            moveZ = moveSpeed;
        }

        // Move the camera based on the calculated direction (no Y movement)
        movement = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);

        // Keep the camera's vertical position fixed
        Vector3 newPosition = transform.position;
        newPosition.y = fixedYPosition;
        transform.position = newPosition;
    }

    void HandleRotation()
    {
        // Check if E or Q is pressed for rotation
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateCamera(rotationAngle); // Rotate clockwise
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateCamera(-rotationAngle); // Rotate counterclockwise
        }

        // Smoothly rotate to the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Snap to the closest allowed angle if the rotation is close to stopping
        if (Quaternion.Angle(transform.rotation, targetRotation) < rotationStopThreshold)
        {
            targetRotation = Quaternion.Euler(pitchAngle, SnapToAllowedAngle(transform.eulerAngles.y), 0f);
        }
    }

    void RotateCamera(float angle)
    {
        // Adjust the target Y rotation smoothly
        Vector3 currentEulerAngles = transform.eulerAngles;
        currentEulerAngles.y += angle;

        // Set the target rotation to the next rotation angle
        targetRotation = Quaternion.Euler(pitchAngle, currentEulerAngles.y, 0f);
    }

    private float SnapToAllowedAngle(float angle)
    {
        // Normalize the angle to the range of 0 to 360 degrees
        angle = (angle + 360) % 360;

        float closestAngle = allowedAngles[0];
        float smallestDifference = Mathf.Abs(angle - closestAngle);

        foreach (float allowedAngle in allowedAngles)
        {
            // Normalize the allowed angle as well
            float normalizedAllowedAngle = (allowedAngle + 360) % 360;

            // Calculate the difference considering wrapping
            float difference = Mathf.Abs(angle - normalizedAllowedAngle);
            if (difference > 180)
            {
                difference = 360 - difference; // Adjust for wrapping
            }

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestAngle = allowedAngle;
            }
        }

        return closestAngle;
    }
}
