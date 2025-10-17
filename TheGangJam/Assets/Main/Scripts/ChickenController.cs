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
    public float dashSpeed = 12f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Abilities")]
    public bool canWalk = true;
    public bool canJump = true;
    public bool canDoubleJump = false;
    public bool canDash = false;
    public bool canSprint = true;

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
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.0f;

    [Header("Rotation Settings")]
    public float rotationSmoothTime = 0.1f;
    private float rotationVelocity;

    [Header("Juice Settings")]
    public float walkSquashAmount = 0.05f;
    public float walkSquashSpeed = 6f;
    public float jumpSquashAmount = 0.3f;
    public float jumpSquashDuration = 0.15f;
    public float dashStretchAmount = 0.2f;
    public float dashStretchDuration = 0.2f;

    [Header("Slope Alignment")]
    [SerializeField] private Vector3 modelRotationOffset = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float feetOffset = 0f;

    [Header("Audio Clips")]
    public AudioClip[] jumpClips;
    public AudioClip[] doubleJumpClips;
    public AudioClip[] landClips;
    public AudioClip[] walkClips;
    public AudioClip[] dashClips;
    [Range(0f, 0.3f)] public float pitchVariation = 0.1f;
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool hasDoubleJumped;

    private bool isDashing;
    private bool dashOnCooldown;
    private bool isSprinting;

    // Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float stepTimer;

    // Visuals
    private Vector3 defaultScale;
    private bool wasGrounded;
    private float squashTimer;
    private bool isSquashing;

    // Dash stretch
    private bool dashStretching;
    private float dashStretchTimer;

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
        AlignVisualToSurface();

        wasGrounded = isGrounded;
    }

    private void HandleMovement()
    {
        if (!canWalk) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 inputDir = forward * moveInput.y + right * moveInput.x;

        float baseSpeed = isDashing ? dashSpeed : walkSpeed;
        if (canSprint && isSprinting && !isDashing) baseSpeed *= sprintMultiplier;

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

        // Walking SFX
        if (isGrounded && moveInput.magnitude > 0.1f && !isDashing)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayRandomClip(walkClips);
                stepTimer = stepInterval;
            }
        }
        else stepTimer = 0f;
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
        PlayRandomClip(isDouble ? doubleJumpClips : jumpClips);
        TriggerSquashStretch();
    }

    private void TryDash()
    {
        if (!canDash || isDashing || dashOnCooldown) return;

        isDashing = true;
        dashOnCooldown = true;

        PlayRandomClip(dashClips);

        dashStretching = true;
        dashStretchTimer = 0f;

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
        animator.SetBool("IsSprinting", isSprinting);
    }

    private void HandleVisuals()
    {
        if (visualRoot == null) return;

        float moveAmount = new Vector2(moveInput.x, moveInput.y).magnitude;

        // Walk squash/stretch (Y axis)
        if (moveAmount > 0.1f && isGrounded && !isSquashing && !dashStretching)
        {
            float cycle = Mathf.Sin(Time.time * walkSquashSpeed);
            Vector3 walkScale = defaultScale + new Vector3(0, cycle, 0) * walkSquashAmount;
            visualRoot.localScale = walkScale;
            float offsetY = (defaultScale.y - walkScale.y) * 0.5f;
            visualRoot.localPosition = new Vector3(0, offsetY, 0);
        }
        else if (!isSquashing && !dashStretching)
        {
            // Smoothly reset when idle
            visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, defaultScale, Time.deltaTime * 10f);
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
        }

        // Landing squash
        if (!wasGrounded && isGrounded)
        {
            TriggerSquashStretch();
            PlayRandomClip(landClips);
        }

        // Jump/Land squash/stretch (Y axis)
        if (isSquashing)
        {
            squashTimer += Time.deltaTime;
            float t = squashTimer / jumpSquashDuration;

            if (t < 0.5f)
            {
                // squash phase (shorter Y)
                visualRoot.localScale = Vector3.Lerp(defaultScale,
                    new Vector3(defaultScale.x, defaultScale.y - jumpSquashAmount, defaultScale.z),
                    t * 2f);
            }
            else if (t < 1f)
            {
                // stretch phase (taller Y)
                visualRoot.localScale = Vector3.Lerp(
                    new Vector3(defaultScale.x, defaultScale.y - jumpSquashAmount, defaultScale.z),
                    new Vector3(defaultScale.x, defaultScale.y + jumpSquashAmount, defaultScale.z),
                    (t - 0.5f) * 2f);
            }
            else
            {
                // return to normal
                visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, defaultScale, Time.deltaTime * 10f);
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);

                if (Mathf.Abs(visualRoot.localScale.y - defaultScale.y) < 0.01f)
                {
                    visualRoot.localScale = defaultScale;
                    visualRoot.localPosition = Vector3.zero;
                    isSquashing = false;
                }
            }
        }

        // Dash stretch effect (Z axis)
        if (dashStretching && visualRoot != null)
        {
            dashStretchTimer += Time.deltaTime;
            float t = dashStretchTimer / dashStretchDuration;

            if (t < 0.5f)
            {
                visualRoot.localScale = Vector3.Lerp(defaultScale,
                    new Vector3(defaultScale.x, defaultScale.y, defaultScale.z + dashStretchAmount),
                    t * 2f);
            }
            else if (t < 1f)
            {
                visualRoot.localScale = Vector3.Lerp(
                    new Vector3(defaultScale.x, defaultScale.y, defaultScale.z + dashStretchAmount),
                    defaultScale,
                    (t - 0.5f) * 2f);
            }
            else
            {
                visualRoot.localScale = defaultScale;
                dashStretching = false;
            }
        }
    }

    private void TriggerSquashStretch()
    {
        if (visualRoot == null) return;
        squashTimer = 0f;
        isSquashing = true;
    }

    private void AlignVisualToSurface()
    {
        if (visualRoot == null) return;

        if (isGrounded && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, groundMask))
        {
            Vector3 surfaceNormal = hit.normal;

            Vector3 forward = transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;

            Quaternion targetRot = Quaternion.LookRotation(forward, surfaceNormal);
            targetRot *= Quaternion.Euler(modelRotationOffset);

            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, Time.deltaTime * 10f);

            float groundOffset = hit.point.y - transform.position.y;
            visualRoot.localPosition = new Vector3(0, groundOffset + feetOffset, 0);
        }
        else
        {
            visualRoot.rotation = Quaternion.Slerp(
                visualRoot.rotation,
                transform.rotation * Quaternion.Euler(modelRotationOffset),
                Time.deltaTime * 10f
            );
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
        }
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, clips.Length);
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clips[index]);
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        currentMoveVelocity = Vector3.zero;
    }


}
