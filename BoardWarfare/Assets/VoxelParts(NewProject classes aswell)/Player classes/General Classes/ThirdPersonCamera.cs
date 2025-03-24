using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;  // Игрок
    public float distance = 3.5f;
    public float height = 1.5f;
    public float rotationSpeed = 5f;
    public float mouseSensitivityX = 100f;
    public float mouseSensitivityY = 100f;
    public float minYAngle = -20f;
    public float maxYAngle = 50f;

    private float currentX = 0f;
    private float currentY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        currentX += mouseX;
        currentY -= mouseY;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
    }

    void LateUpdate()
    {
        // Вычисление позиции и вращение камеры вокруг игрока
        Vector3 direction = new Vector3(0, height, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position + rotation * direction;
        transform.LookAt(target.position + Vector3.up * height);
    }
}
