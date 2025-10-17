using UnityEngine;
using System.Collections;

public class NestScript : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;             
    public CameraController playerCameraController; 
    public Transform cameraFocusPoint;             
    public ChickenController player;               
    public GameObject playerVisual;                
    public GameObject egg;                        

    [Header("Cutscene Settings")]
    public float cameraPanDuration = 3f;          
    public float cameraOrbitDistance = 5f;        
    public float cameraOrbitHeight = 2f;          

    [Header("Cooldown Settings")]
    public float cooldownTime = 5f;

    private bool onCooldown = false;
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown) return;

        ChickenController chicken = other.GetComponent<ChickenController>();
        if (chicken != null && chicken == player)
        {
            playerInside = true;
            StartCoroutine(NestSequence());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ChickenController>() == player)
        {
            playerInside = false;
            if (!onCooldown)
                StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator NestSequence()
    {
        CountdownTimer timer = Object.FindFirstObjectByType<CountdownTimer>();

        // Pause timer
        if (timer != null) timer.PauseTimer();

        // Hide player visuals & disable movement
        if (playerVisual != null) playerVisual.SetActive(false);
        player.enabled = false;

        // Disable player camera controller
        if (playerCameraController != null) playerCameraController.enabled = false;

        // Show egg
        if (egg != null) egg.SetActive(true);

        // Save camera state
        Vector3 camStartPos = cameraTransform.position;
        Quaternion camStartRot = cameraTransform.rotation;

        // Camera pan
        float elapsed = 0f;
        while (elapsed < cameraPanDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraPanDuration);

            // Orbit around nest
            float angle = t * 360f;
            Vector3 orbitOffset = Quaternion.Euler(0f, angle, 0f) * Vector3.back * cameraOrbitDistance;
            Vector3 finalPos = cameraFocusPoint.position + orbitOffset + Vector3.up * cameraOrbitHeight;

            cameraTransform.position = finalPos;

            // 👇 Pan downward over time
            Vector3 lookTarget = cameraFocusPoint.position;
            float downwardOffset = Mathf.Lerp(0f, 0.2f, t); // adjust -2f for how far down you want
            lookTarget += Vector3.down * downwardOffset;

            cameraTransform.LookAt(lookTarget);

            if (timer != null) timer.ResetToMaxTime();
            yield return null;
        }


        // Hide egg again
        if (egg != null) egg.SetActive(false);

        // Snap player to nest transform
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;

        // Restore player
        if (playerVisual != null) playerVisual.SetActive(true);
        player.enabled = true;

        // Restore camera
        cameraTransform.position = camStartPos;
        cameraTransform.rotation = camStartRot;

        // Re‑enable player camera controller
        if (playerCameraController != null) playerCameraController.enabled = true;

        // Resume timer
        if (timer != null) timer.ResumeTimer();
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        float remaining = cooldownTime;
        while (remaining > 0f)
        {
            if (playerInside)
            {
                yield return null;
                continue;
            }
            remaining -= Time.deltaTime;
            yield return null;
        }
        onCooldown = false;
    }
}
