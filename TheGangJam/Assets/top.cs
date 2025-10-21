using System.Security.Cryptography;
using UnityEngine;

public class top : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool cracked = false;
    bool once = false;
    public int upforce = 3;
    private Vector3 orposition;
    private Quaternion orrotation;
    Rigidbody rb;
    void Start()
    {
       orposition = transform.position;
        orrotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(cracked)
        {
            if (!once)
            {
                rb.AddRelativeForce(transform.forward * upforce, ForceMode.Impulse);
                once = true;
            }
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            once = false;
            transform.position = orposition;
            transform.rotation = orrotation;
        }
    }
}
