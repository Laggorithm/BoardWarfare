using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Основные параметры")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 4f;
    public float crouchSpeed = 2f;
    public float currentSpeed = 0f;
    public float gravity = 20f;

    [Header("Настройки дэша")]
    public float dashDistance = 4f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.5f;

    [Header("Ссылки")]
    [SerializeField] private Transform cameraTransform; // SerializeField позволяет видеть private поле в инспекторе

    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 dashDirection;
    private Animator animator;

    private bool canDash = true;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isDashing = false;
    private bool isStunned = false;

    private float verticalVelocity;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    void Awake()
    {
        // Инициализация в Awake, который вызывается раньше Start
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Если камера не назначена, попробуем найти её
        if (cameraTransform == null)
        {
            FindMainCamera();
        }
    }

    void Start()
    {
        // Дополнительная проверка в Start
        if (cameraTransform == null)
        {
            Debug.LogError("Камера не назначена! Движение не будет работать.");
            enabled = false; // Отключаем скрипт, если камера не найдена
            return;
        }

        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (isStunned || cameraTransform == null) return;

        HandleMovement();
        HandleDash();
        HandleSpeedChanges();

        ApplyGravity();

        if (!isDashing)
        {
            ApplyMovement();
        }
    }

    private void FindMainCamera()
    {
        // Поиск активной камеры
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        foreach (Camera cam in cameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                cameraTransform = cam.transform;
                Debug.Log("Найдена камера: " + cam.gameObject.name);
                return;
            }
        }

        Debug.LogWarning("Активная камера не найдена в сцене!");
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (cameraTransform == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-moveDirection); // Поворот на 180 градусов
        }

        moveDirection *= currentSpeed;

        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        moveDirection.y = verticalVelocity;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleDash()
    {
        if (!canDash || cameraTransform == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dashDirection = moveDirection != Vector3.zero
                ? moveDirection.normalized
                : -Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;
        float startTime = Time.time;

        if (animator != null)
        {
            animator.SetBool("IsDashing", true);
        }

        while (Time.time < startTime + dashDuration)
        {
            if (controller != null)
            {
                controller.Move(dashDirection * (dashDistance / dashDuration) * Time.deltaTime);
            }
            animator.SetFloat("VelocityX", dashDirection.x);
            animator.SetFloat("VelocityZ", dashDirection.z);
            yield return null;
        }

        isDashing = false;
        dashCooldownTimer = dashCooldown;

        if (animator != null)
        {
            animator.SetBool("IsDashing", false);
        }

        while (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            yield return null;
        }

        canDash = true;
    }


    private void HandleSpeedChanges()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed;
            isCrouching = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            isSprinting = false;
            isCrouching = false;
        }
    }

    public void ApplyStun()
    {
        isStunned = true;
        moveDirection = Vector3.zero;
    }

    public void RemoveStun()
    {
        isStunned = false;
    }

    // Метод для ручного назначения камеры (можно вызывать из других скриптов)
    public void SetCamera(Transform newCamera)
    {
        cameraTransform = newCamera;
        if (cameraTransform == null)
        {
            Debug.LogWarning("Передана null камера!");
        }
    }
}