using UnityEngine;

public class FloatingLog : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f; // degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // axis to rotate around

    void Update()
    {
        // Rotate the log around the chosen axis
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}