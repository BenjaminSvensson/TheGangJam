using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LilypadBounceTrigger : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float sinkAmount = 0.2f;       
    public float sinkSpeed = 5f;          
    public float returnSpeed = 2f;      

    [Header("Sound")]
    public AudioClip bounceSound;        

    private Vector3 startPos;
    private bool isBouncing;
    private AudioSource audioSource;

    private void Start()
    {
        startPos = transform.position;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Trigger
        if (other.CompareTag("Player") && !isBouncing)
        {
            StartCoroutine(BounceRoutine());

            if (audioSource && bounceSound)
                audioSource.PlayOneShot(bounceSound);
        }
    }

    private IEnumerator BounceRoutine()
    {
        isBouncing = true;

        Vector3 targetPos = startPos + Vector3.down * sinkAmount;

        //Move down
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * sinkSpeed);
            yield return null;
        }

        //Move back up
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, startPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        transform.position = startPos;
        isBouncing = false;
    }
}
