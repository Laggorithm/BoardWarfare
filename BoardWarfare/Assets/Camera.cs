using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public Transform turret;  // Reference to the turret's transform
    public Vector3 offset;    // Fixed offset from the player's position
    public float heightOffset = 5.0f; // Height adjustment to ensure a good view

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player transform is not assigned in the Camera script.");
        }
        if (turret == null)
        {
            Debug.LogError("Turret transform is not assigned in the Camera script.");
        }
    }

    private void LateUpdate()
    {
        if (player != null && turret != null)
        {
            // Calculate the desired position based on the player's position and the fixed offset
            Vector3 playerPosition = player.position;
            Vector3 turretDirection = turret.forward; // The direction the turret is facing
            Vector3 desiredPosition = playerPosition + offset;

            // Adjust camera position to follow turret's direction
            desiredPosition = playerPosition + offset - turretDirection * heightOffset;

            // Update the camera's position to the desired position
            transform.position = desiredPosition;

            // Ensure the camera always looks at the player's position
            transform.LookAt(player.position + Vector3.up * (heightOffset / 2)); // Adjusted to keep the player in view
        }
    }
}
