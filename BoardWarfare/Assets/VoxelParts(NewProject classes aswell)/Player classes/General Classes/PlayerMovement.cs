using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Основные параметры")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 4f;   // снижена скорость спринта
    public float crouchSpeed = 2f;
    public float gravity = 20f;
    public float crouchHeight = 0.5f;
    private float defaultHeight;
    public float currentSpeed = 0f;

    [Header("Настройки дэша")]
    public float dashDistance = 4f;  // уменьшено расстояние дэша
    public float dashDuration = 0.3f; // уменьшена длительность дэша
    public float dashCooldown = 1.5f;

    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 dashDirection;
    private Animator animator;

    public Transform cameraTransform; // Ссылка на камеру для направления движения

    private bool canDash = true;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isDashing = false;
    private bool isStunned = false;  // Флаг для стана

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        defaultHeight = controller.height;
        currentSpeed = walkSpeed;

    }

    void Update()
    {
        if (isStunned) return;  // Блокируем движение, если персонаж оглушен

        if (Input.GetKey(KeyCode.LeftShift)) 
        {
            currentSpeed = sprintSpeed;  // Спринт
            Debug.Log("Sprint");  // Выводим в консоль, когда активирован спринт
        }
        // Проверка на приседание (если нет спринта)
        else if (isCrouching)  // Если персонаж приседает
        {
            currentSpeed = crouchSpeed;  // Присед
            Debug.Log("Crouch");  // Выводим в консоль, когда активирован режим приседания
        }
        // Если не спринт и не присед, то обычная ходьба
        else
        {
            currentSpeed = walkSpeed;
            Debug.Log("Walk");  // Выводим в консоль, когда обычная ходьба
        }

        HandleMovement();  // Обрабатываем движение и спринт
        HandleCrouch();    // Обрабатываем приседание
        HandleDash();      // Обрабатываем дэш
        ApplyGravity();    // Применяем гравитацию
        UpdateAnimator();  // Обновляем анимации
    }

    // Метод для обработки движения

    public void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Если нет ввода (клавиши не нажаты), то сбрасываем движение
        if (moveX == 0f && moveZ == 0f)
        {
            moveDirection = Vector3.zero;  // Нет движения
            animator.SetFloat("Speed", 0f);  // Скорость в аниматоре тоже обнуляем
            return;  // Прерываем выполнение, чтобы не обновлять направление движения
        }

        // Получаем направление на основе камеры
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Игнорируем вертикальную составляющую камеры
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Рассчитываем направление движения
        Vector3 move = forward * moveZ + right * moveX;

        // Устанавливаем направление движения
        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;

        // Поворачиваем игрока в сторону движения на 180 градусов
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            targetRotation *= Quaternion.Euler(0f, 180f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Передаем в аниматор реальную скорость
        animator.SetFloat("Speed", new Vector3(moveDirection.x, 0f, moveDirection.z).magnitude);
    }


    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("VelocityZ", moveDirection.z);
    }

    // Метод для начала стана
    public void ApplyStun()
    {
        isStunned = true;
    }

    // Метод для снятия стана
    public void RemoveStun()
    {
        isStunned = false;
    }


    // Убрана логика подката – при нажатии LeftControl всегда запускается обычное приседание
    void HandleCrouch()
    {
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }
    }

    void StartCrouch()
    {
        isCrouching = true;
        controller.height = crouchHeight;
    }

    void StopCrouch()
    {
        isCrouching = false;
        controller.height = defaultHeight;
    }

    void HandleDash()
    {
        if (isDashing || !canDash) return;  // Если уже в дэше или на кулдауне, не выполняем рывок

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;  // Начинаем дэш
        canDash = false;   // Запрещаем повторный дэш, пока идет кулдаун

        float dashStartTime = Time.time;  // Запоминаем время начала дэша

        Vector3 dashVelocity = transform.forward * dashDistance / dashDuration;  // Расчет скорости рывка

        while (Time.time < dashStartTime + dashDuration)
        {
            controller.Move(dashVelocity * Time.deltaTime);  // Двигаем игрока вперед во время дэша
            yield return null;  // Ждем следующий кадр
        }

        isDashing = false;  // Дэш закончен
        yield return new WaitForSeconds(dashCooldown);  // Ждем окончания кулдауна
        canDash = true;  // Разрешаем следующий дэш
    }


    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }
}
