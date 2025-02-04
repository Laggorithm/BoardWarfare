using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Основные параметры")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 3f;
    public float gravity = 20f;
    public float crouchHeight = 0.5f;
    private float defaultHeight;

    [Header("Настройки подката")]
    public float slideDuration = 1f;
    public float slideSpeed = 25f;
    private bool isSliding = false;
    private float slideTimer;

    [Header("Настройки дэша")]
    public float dashDistance = 5f;
    public float dashDuration = 0.5f;
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
        HandleCrouchAndSlide();
        HandleDash();
        ApplyGravity();
        controller.Move(moveDirection * Time.deltaTime);
    }

    // Метод для обработки движения
    public void HandleMovement()
    {
        if (isSliding || isDashing) return;  // Если мы в процессе подката или дэша, движение заблокировано

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

    void HandleCrouchAndSlide()
    {
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isSprinting)
            {
                StartSlide();
            }
            else
            {
                StartCrouch();
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                StopCrouch();
                isSliding = false;
            }
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

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        moveDirection = transform.forward * slideSpeed;
        moveDirection.y = 0;
        StartCrouch();
    }

    void HandleDash()
    {
        if (isDashing || !canDash) return;

        // Check for movement key + dash (spacebar)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.W)) dashDirection = transform.forward;
            else if (Input.GetKey(KeyCode.S)) dashDirection = -transform.forward;
            else if (Input.GetKey(KeyCode.A)) dashDirection = -transform.right;
            else if (Input.GetKey(KeyCode.D)) dashDirection = transform.right;
            else dashDirection = -transform.forward; // Default: dash backward if no movement key

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
