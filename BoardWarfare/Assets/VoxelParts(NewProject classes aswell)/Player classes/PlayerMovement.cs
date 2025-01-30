using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Основные параметры")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 3f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float crouchHeight = 0.5f;
    private float defaultHeight;

    [Header("Настройки подката")]
    public float slideDuration = 1f;
    public float slideSpeed = 25f;
    private bool isSliding = false;
    private float slideTimer;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool isCrouching = false;
    private bool isSprinting = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleSprint();
        HandleCrouchAndSlide();
        ApplyGravity();
        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (isSliding) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = isSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : walkSpeed);

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
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

    void HandleJump()
    {
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            moveDirection.y = jumpForce;
        }
    }

    void HandleCrouchAndSlide()
    {
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isSprinting) // Подкат
            {
                StartSlide();
            }
            else // Приседание
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

    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }
}
