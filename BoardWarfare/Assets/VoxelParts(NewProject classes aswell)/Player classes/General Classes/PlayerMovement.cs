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

    //[Header("Настройки подката")]
    // Подкат удалён – его функционал заменён обычным приседанием

    [Header("Настройки дэша")]
    public float dashDistance = 4f;  // уменьшено расстояние дэша
    public float dashDuration = 0.3f; // уменьшена длительность дэша
    public float dashCooldown = 1.5f;
    private bool canDash = true;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isDashing = false;
    private Vector3 dashDirection;

    private bool isStunned = false;  // Флаг для стана

    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
    }

    void Update()
    {
        if (isStunned) return;  // Блокируем движение, если персонаж оглушен

        HandleMovement();
        HandleSprint();
        HandleCrouch();
        HandleDash();
        ApplyGravity();
        controller.Move(moveDirection * Time.deltaTime);
    }

    // Метод для обработки движения
    public void HandleMovement()
    {
        if (isDashing) return;  // Если мы в процессе дэша, движение заблокировано

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = isSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : walkSpeed);

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
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

    void HandleSprint()
    {
        if (controller.isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
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
        if (isDashing || !canDash) return;

        // Если нажата клавиша дэша (Space), определяем направление по нажатию клавиш движения
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.W)) dashDirection = transform.forward;
            else if (Input.GetKey(KeyCode.S)) dashDirection = -transform.forward;
            else if (Input.GetKey(KeyCode.A)) dashDirection = -transform.right;
            else if (Input.GetKey(KeyCode.D)) dashDirection = transform.right;
            else dashDirection = -transform.forward; // по умолчанию дэшаем назад, если нет нажатых клавиш движения

            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;
        controller.enabled = false;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dashDirection * dashDistance;
        float elapsedTime = 0;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        controller.enabled = true;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }
}
