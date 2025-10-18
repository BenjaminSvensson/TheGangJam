using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ChickenController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;
    public Transform visualRoot;
    public AudioSource audioSource;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Abilities")]
    public bool canWalk = true;
    public bool canJump = true;
    public bool canDoubleJump = false;
    public bool canDash = false;
    public bool canSprint = true;
    public bool canSlowFall = true;

    [Header("Momentum Settings")]
    public float acceleration = 10f;
    public float deceleration = 15f;
    private Vector3 currentMoveVelocity;

    [Header("Jump Feel Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1.0f;

    [Header("Rotation Settings")]
    public float rotationSmoothTime = 0.1f;
    private float rotationVelocity;

    [Header("Slow Fall Settings")]
    public float slowFallGravityScale = 0.3f;
    public float slowFallTiltAngle = 80f;
    public float tiltSmoothSpeed = 8f;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool hasDoubleJumped;

    private bool isDashing;
    private bool dashOnCooldown;
    private Vector3 dashVelocity;

    private bool isSprinting;
    private bool isSlowFalling;

    // Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        inputActions.Player.Dash.performed += ctx => TryDash();

        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (isDashing)
        {
            // Dash overrides normal movement
            controller.Move(dashVelocity * Time.deltaTime);
            return;
        }

        HandleMovement();
        HandleGravity();
        HandleJumpLogic();
        HandleAnimations();
        UpdateSlowFallVisual();
    }

    private void HandleMovement()
    {
        if (!canWalk) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 inputDir = forward * moveInput.y + right * moveInput.x;

        float baseSpeed = walkSpeed;
        if (canSprint && isSprinting) baseSpeed *= sprintMultiplier;

        Vector3 targetVelocity = inputDir.normalized * baseSpeed;

        if (inputDir.magnitude > 0.1f)
            currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetVelocity, acceleration * Time.deltaTime);
        else
            currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, Vector3.zero, deceleration * Time.deltaTime);

        if (currentMoveVelocity.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(currentMoveVelocity.x, currentMoveVelocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        controller.Move(currentMoveVelocity * Time.deltaTime);
    }

    private void HandleGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            hasDoubleJumped = false;
            isSlowFalling = false;
            ResetTilt();
        }

        float appliedGravity = gravity;

        if (canSlowFall && !isGrounded)
        {
            bool jumpHeld = inputActions.Player.Jump.ReadValue<float>() > 0.1f;

            if (jumpHeld && hasDoubleJumped)
            {
                appliedGravity = gravity * slowFallGravityScale;
                isSlowFalling = true;
            }
            else
            {
                isSlowFalling = false;
            }
        }

        velocity.y += appliedGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJumpLogic()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        if (canJump && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump(false);
            jumpBufferCounter = 0;
        }
        else if (canDoubleJump && jumpBufferCounter > 0 && !hasDoubleJumped && !isGrounded)
        {
            Jump(true);
            hasDoubleJumped = true;
            jumpBufferCounter = 0;
        }
    }

    private void Jump(bool isDouble)
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void TryDash()
    {
        if (!canDash || isDashing || dashOnCooldown) return;

        isDashing = true;
        dashOnCooldown = true;

        // Apply dash impulse once
        dashVelocity = transform.forward * dashSpeed;

        Invoke(nameof(StopDash), dashDuration);
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void StopDash()
    {
        isDashing = false;
        dashVelocity = Vector3.zero;
    }

    private void ResetDashCooldown() => dashOnCooldown = false;

    private void HandleAnimations()
    {
        if (!animator) return;

        float speedPercent = new Vector2(moveInput.x, moveInput.y).magnitude;
        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsDashing", isDashing);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsSlowFalling", isSlowFalling);
    }

    private void UpdateSlowFallVisual()
    {
        if (visualRoot == null) return;

        Quaternion targetRotation = Quaternion.identity;

        if (isSlowFalling)
            targetRotation = Quaternion.Euler(slowFallTiltAngle, 0f, 0f);

        visualRoot.localRotation = Quaternion.Slerp(
            visualRoot.localRotation,
            targetRotation,
            Time.deltaTime * tiltSmoothSpeed
        );
    }

    private void ResetTilt()
    {
        if (visualRoot != null)
            visualRoot.localRotation = Quaternion.identity;
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        currentMoveVelocity = Vector3.zero;
        isSlowFalling = false;
        ResetTilt();
    }
}
