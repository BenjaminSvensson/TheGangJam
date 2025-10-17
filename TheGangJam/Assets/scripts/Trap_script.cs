using UnityEngine;
using System.Collections;

public class Trap_script : MonoBehaviour
{
    private Vector3 startPosition;
    public bool triggerd = false;
    public bool trigger_trap = false;
    public float moveDistance = 0.3f;  // increased so it's visible
    public float moveSpeed = 3f;       // units per second

    void Start()
    {
        startPosition = transform.position;
        Debug.Log("Start position: " + startPosition);
    }

    void Update()
    {
        if (!triggerd && trigger_trap)
        {
            triggerd = true;
            StartCoroutine(SlideUpAndDown());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggerd)
        {
            triggerd = true;
            StartCoroutine(SlideUpAndDown());
        }
    }

    private IEnumerator SlideUpAndDown()
    {
        Vector3 targetPosition = startPosition + new Vector3(0f, moveDistance, 0f);

        // Slide up
        while (Vector3.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap exactly to target
        transform.position = targetPosition;

        // Wait at the top
        yield return new WaitForSeconds(2f);

        // Slide back down
        while (Vector3.Distance(transform.position, startPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap exactly to start
        transform.position = startPosition;

        // Reset trigger so it can activate again
        triggerd = false;
    }
}