using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float minSpeed = 10f;   // minimum degrees per second
    public float maxSpeed = 50f;   // maximum degrees per second
    public float changeInterval = 2f; // how often to pick a new random direction

    private Vector3 rotationAxis;
    private float rotationSpeed;
    private float timer;

    private void Start()
    {
        PickNewRotation();
    }

    private void Update()
    {
        // Rotate continuously
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);

        // Timer to change direction/speed
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            PickNewRotation();
            timer = 0f;
        }
    }

    private void PickNewRotation()
    {
        // Random direction
        rotationAxis = Random.onUnitSphere; // random 3D direction
        // Random speed
        rotationSpeed = Random.Range(minSpeed, maxSpeed);
    }
}
