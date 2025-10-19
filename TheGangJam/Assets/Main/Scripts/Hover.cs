using UnityEngine;

public class HoverMotion : MonoBehaviour
{
    [Header("Hover Settings")]
    public float amplitude = 0.5f;  
    public float frequency = 1f;     

    private Vector3 startPos;

    void Start()
    {
        //Get original position
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
