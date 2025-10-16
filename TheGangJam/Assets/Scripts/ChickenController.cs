using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ChickenController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float dashSpeed = 12f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Abilities (toggled by PowerUps)")]
    public bool canWalk = true;
    public bool canJump = true;
    public bool canDoubleJump = false;
    public bool canDash = false;

    [Header("Jump Feel Settings")]
    public float coyoteTime = 0.15f;      // grace period after leaving ground
    public float jumpBufferTime = 0.15f;  // grace period before landing

    [Header("Ground Check Settings")]
    public Transform groundCheck;     // empty GameObject at feet
    public float groundRadius = 0.3f; // sphere radius
    public LayerMask groundMask;      // assign "Ground" layer

    [Header("Dash Settings")]
    public float dashDuration = 0.3f;   // how long the dash lasts
    public float dashCooldown = 1.0f;   // cooldown before dash can be used again

    [Header("Rotation Settings")]
    public float rotationSmoothTime = 0.1f; // smaller = snappier, larger = smoother
    private float rotationVelocity;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool hasDoubleJumped;

    private bool isDashing;
    private bool dashOnCooldown;

    // Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        // Input bindings
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        inputActions.Player.Dash.performed += ctx => TryDash();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleJumpLogic();
        HandleAnimations();
    }

    private void HandleMovement()
    {
        if (!canWalk) return;

        // Camera-relative movement
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 move = forward * moveInput.y + right * moveInput.x;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;

            // Smooth rotation
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            float speed = isDashing ? dashSpeed : walkSpeed;
            controller.Move(move.normalized * speed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        // Custom ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // keeps grounded
            hasDoubleJumped = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJumpLogic()
    {
        // Update timers
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        // Check jump conditions
        if (canJump && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0; // consume buffer
        }
        else if (canDoubleJump && jumpBufferCounter > 0 && !hasDoubleJumped && !isGrounded)
        {
            Jump();
            hasDoubleJumped = true;
            jumpBufferCounter = 0;
        }
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void TryDash()
    {
        if (!canDash || isDashing || dashOnCooldown) return;

        isDashing = true;
        dashOnCooldown = true;

        // End dash after duration
        Invoke(nameof(StopDash), dashDuration);

        // Reset cooldown after delay
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void StopDash()
    {
        isDashing = false;
    }

    private void ResetDashCooldown()
    {
        dashOnCooldown = false;
    }

    private void HandleAnimations()
    {
        if (!animator) return;

        float speedPercent = new Vector2(moveInput.x, moveInput.y).magnitude;
        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsDashing", isDashing);
    }

    // 👇 Collision with checkpoints to reset timer
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        NestScript checkpoint = hit.collider.GetComponent<NestScript>();
        if (checkpoint != null)
        {
            CountdownTimer timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
            {
                timer.ResetToMaxTime();
            }
        }
    }
}