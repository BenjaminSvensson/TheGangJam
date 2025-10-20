using UnityEngine;

public class rototo : MonoBehaviour
{ 
    public float rotationspeed = 1f;
    void Update()
    {
        transform.Rotate(0f, 0f, rotationspeed * Time.deltaTime);
    }
}
