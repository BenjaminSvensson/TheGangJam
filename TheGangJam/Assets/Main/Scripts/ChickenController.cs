using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ChickenController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;
    public Transform visualRoot; // child mesh/animator

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
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;

    [Header("Dash Settings")]
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.0f;

    [Header("Rotation Settings")]
    public float rotationSmoothTime = 0.1f;
    private float rotationVelocity;

    [Header("Juice Settings")]
    public float walkSquashAmount = 0.05f;   // subtle squash while walking
    public float walkSquashSpeed = 6f;       // speed of the cycle
    public float jumpSquashAmount = 0.3f;    // bigger squash for jump/land
    public float jumpSquashDuration = 0.15f;

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

    // Visuals
    private Vector3 defaultScale;
    private bool wasGrounded;
    private float squashTimer;
    private bool isSquashing;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        inputActions.Player.Dash.performed += ctx => TryDash();
    }

    private void Start()
    {
        if (visualRoot != null)
            defaultScale = visualRoot.localScale;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleJumpLogic();
        HandleAnimations();
        HandleVisuals();

        wasGrounded = isGrounded; // track landing
    }

    private void HandleMovement()
    {
        if (!canWalk) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 move = forward * moveInput.y + right * moveInput.x;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            float speed = isDashing ? dashSpeed : walkSpeed;
            controller.Move(move.normalized * speed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            hasDoubleJumped = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJumpLogic()
    {
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        if (canJump && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0;
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
        TriggerSquashStretch(); // squash on jump
    }

    private void TryDash()
    {
        if (!canDash || isDashing || dashOnCooldown) return;

        isDashing = true;
        dashOnCooldown = true;

        Invoke(nameof(StopDash), dashDuration);
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void StopDash() => isDashing = false;
    private void ResetDashCooldown() => dashOnCooldown = false;

    private void HandleAnimations()
    {
        if (!animator) return;

        float speedPercent = new Vector2(moveInput.x, moveInput.y).magnitude;
        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsDashing", isDashing);
    }

    private void HandleVisuals()
    {
        if (visualRoot == null) return;

        // --- Subtle walk squash/stretch ---
        float moveAmount = new Vector2(moveInput.x, moveInput.y).magnitude;
        if (moveAmount > 0.1f && isGrounded && !isSquashing)
        {
            float cycle = Mathf.Sin(Time.time * walkSquashSpeed);

            // Scale (Z emphasis, subtle)
            Vector3 walkScale = defaultScale + new Vector3(0, -cycle, cycle) * walkSquashAmount;
            visualRoot.localScale = walkScale;

            // Position offset to keep feet grounded
            float offsetY = (defaultScale.y - walkScale.y) * 0.5f; 
            visualRoot.localPosition = new Vector3(0, offsetY, 0);
        }
        else if (!isSquashing)
        {
            // Reset smoothly when idle
            visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, defaultScale, Time.deltaTime * 10f);
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
        }


        // --- Landing squash ---
        if (!wasGrounded && isGrounded)
        {
            TriggerSquashStretch();
        }

        // --- Jump/Land squash/stretch animation ---
        if (isSquashing)
        {
            squashTimer += Time.deltaTime;
            float t = squashTimer / jumpSquashDuration;

            if (t < 0.5f)
            {
                // squash phase (Z axis emphasis)
                visualRoot.localScale = Vector3.Lerp(defaultScale,
                    new Vector3(defaultScale.x, defaultScale.y, defaultScale.z - jumpSquashAmount),
                    t * 2f);
            }
            else if (t < 1f)
            {
                // stretch phase
                visualRoot.localScale = Vector3.Lerp(
                    new Vector3(defaultScale.x, defaultScale.y, defaultScale.z - jumpSquashAmount),
                    new Vector3(defaultScale.x, defaultScale.y, defaultScale.z + jumpSquashAmount),
                    (t - 0.5f) * 2f);
            }
            else
            {
                // return to normal
                visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, defaultScale, Time.deltaTime * 10f);
                if (Mathf.Abs(visualRoot.localScale.z - defaultScale.z) < 0.01f)
                {
                    visualRoot.localScale = defaultScale;
                    isSquashing = false;
                }
            }
        }
    }

    private void TriggerSquashStretch()
    {
        if (visualRoot == null) return;
        squashTimer = 0f;
        isSquashing = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        NestScript checkpoint = hit.collider.GetComponent<NestScript>();
        if (checkpoint != null)
        {
            CountdownTimer timer = Object.FindFirstObjectByType<CountdownTimer>();
            if (timer != null)
            {
                timer.ResetToMaxTime();
            }
        }
    }
}
