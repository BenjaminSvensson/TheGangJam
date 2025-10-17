using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float sensitivity = 200f;
    public float smoothTime = 0.05f;

    [Header("Collision Settings")]
    public float minDistance = 0.5f;
    public float collisionBuffer = 0.2f;
    public LayerMask collisionMask;

    private Vector2 lookInput;
    private PlayerInputActions inputActions;
    private float yaw, pitch;

    private float currentDistance;
    private float distanceVelocity;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentDistance = distance;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        // Yaw o Pitch
        yaw += lookInput.x * sensitivity * 0.01f;
        pitch -= lookInput.y * sensitivity * 0.01f;
        pitch = Mathf.Clamp(pitch, -30f, 70f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Cameraposcollision
        Vector3 desiredPos = target.position - rotation * Vector3.forward * distance;

        // Collision check
        float targetDist = distance;
        if (Physics.SphereCast(target.position, 0.2f, (desiredPos - target.position).normalized,
                               out RaycastHit hit, distance, collisionMask))
        {
            targetDist = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, distance);
        }

        // Smooth the distance value
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref distanceVelocity, smoothTime);

        // Final camera position
        Vector3 finalPos = target.position - rotation * Vector3.forward * currentDistance;

        transform.position = finalPos;
        transform.rotation = rotation;
    }
}
