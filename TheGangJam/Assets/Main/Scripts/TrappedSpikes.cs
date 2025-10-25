using UnityEngine;

public class TrappedSpikes : MonoBehaviour
{
    [Header("Spike Settings")]
    public Transform spike;          
    public float riseHeight = 2f;   
    public float riseSpeed = 5f;    
    public float resetDelay = 2f;   
    public float resetSpeed = 3f;    

    [Header("Audio")]
    public AudioClip spikeSound;     
    private AudioSource audioSource;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isTriggered = false;

    private void Start()
    {
        if (spike == null) spike = transform; //fallback
        startPos = spike.position;
        targetPos = startPos + Vector3.up * riseHeight;

        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ChickenController>() != null && !isTriggered)
        {
            StartCoroutine(RiseRoutine());
        }
    }

    private System.Collections.IEnumerator RiseRoutine()
    {
        isTriggered = true;

       
        if (spikeSound != null && audioSource != null)
            audioSource.PlayOneShot(spikeSound);

       
        while (Vector3.Distance(spike.position, targetPos) > 0.01f)
        {
            spike.position = Vector3.MoveTowards(spike.position, targetPos, riseSpeed * Time.deltaTime);
            yield return null;
        }

        
        yield return new WaitForSeconds(resetDelay);

        
        while (Vector3.Distance(spike.position, startPos) > 0.01f)
        {
            spike.position = Vector3.MoveTowards(spike.position, startPos, resetSpeed * Time.deltaTime);
            yield return null;
        }

        isTriggered = false;
    }
}
