using UnityEngine;

public class TrappedLillypad : MonoBehaviour
{
    [Header("Settings")]
    public float sinkDistance = 2f;       
    public float sinkSpeed = 2f;          
    public float resetDelay = 3f;         
    public float riseSpeed = 2f;          

    [Header("Audio")]
    public AudioClip plungeSound;         
    private AudioSource audioSource;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isSinking = false;
    private bool isResetting = false;

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.down * sinkDistance;

        // Ensure we have an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ChickenController>() != null && !isSinking && !isResetting)
        {
            StartCoroutine(SinkRoutine());
        }
    }

    private System.Collections.IEnumerator SinkRoutine()
    {
        isSinking = true;

        // 🔊 Play plunge sound once
        if (plungeSound != null && audioSource != null)
            audioSource.PlayOneShot(plungeSound);

        // Move down
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, sinkSpeed * Time.deltaTime);
            yield return null;
        }

        // Wait before rising back
        yield return new WaitForSeconds(resetDelay);

        isSinking = false;
        isResetting = true;

        // Move back up
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, riseSpeed * Time.deltaTime);
            yield return null;
        }

        isResetting = false;
    }
}
