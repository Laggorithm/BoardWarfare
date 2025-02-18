using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -3f);
    public float maxVerticalAngle = 50f;

    private float xRotation = 0f;
    private bool isThirdPerson = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            isThirdPerson = !isThirdPerson;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);

        if (isThirdPerson)
        {
            Quaternion rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
            Vector3 desiredPosition = playerBody.position + rotation * thirdPersonOffset;
            transform.position = desiredPosition;
            transform.LookAt(playerBody.position + Vector3.up * 1.5f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
            transform.position = playerBody.position;
        }
    }
}
