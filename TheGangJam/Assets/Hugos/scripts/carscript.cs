using UnityEngine;
using System.Collections;

public class CarLooper : MonoBehaviour
{
    [SerializeField] private Transform tunnelStart;
    [SerializeField] private Transform tunnelEnd;
    [SerializeField] private Transform carModel; // assign the visual mesh here
    [SerializeField] private float speed = 10f;
    private bool isWaiting = false;
    private Transform target;

    void Start()
    {
        target = tunnelEnd;
    }

    void Update()
    {
        if (target == null || isWaiting) return;

        // Rotate only the car model to face the target
        if (carModel != null)
            carModel.LookAt(target);

        // Move the root object forward
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            StartCoroutine(TeleportAfterDelay());
    }

    IEnumerator TeleportAfterDelay()
    {
        float rnd = Random.Range(0.2f, 1.5f);
        isWaiting = true;
        yield return new WaitForSeconds(rnd);
        transform.position = tunnelStart.position;
        isWaiting = false;
    }
}