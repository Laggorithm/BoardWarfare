using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Скорость движения
    public float rotationSpeed = 10f; // Скорость поворота

    private void Update()
    {
        // Получение ввода от клавиш WASD или стрелок
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Движение вперед-назад
        Vector3 moveDirection = transform.forward * verticalInput;

        // Перемещение игрока
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Поворот игрока
        transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
    }
}