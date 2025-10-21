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
    public Transform nestPoint; // Empty GameObject for snapping player

    [Header("Cutscene Toggles")]
    public bool playSitAnimation = true;
    public bool showEgg = true;
    public bool zoomIntoEgg = true;
    public bool hidePlayerDuringZoom = true;
    public bool scalePlayerFromTiny = true;

    [Header("Timings")]
    public float sitAnimDelay = 1f;
    public float eggAppearDelay = 0.5f;
    public float zoomDuration = 2f;
    public float scaleUpDuration = 2f;

    [Header("Scaling")]
    public float tinyScale = 0.1f;                 // starting scale
    public Vector3 finalPlayerScale = Vector3.one; // target scale

    [Header("Camera Settings")]
    public float zoomOffsetY = 0.5f;
    public float zoomDistance = 3f;

    [Header("Cooldown Settings")]
    public float cooldownTime = 5f;

    private bool onCooldown = false;
    private bool playerInside = false;
    private bool hasPlayedOriginal = false; // Track first time

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
        if (timer != null) timer.PauseTimer();

        // Disable player control
        player.enabled = false;
        if (playerCameraController != null) playerCameraController.enabled = false;

        Vector3 camStartPos = cameraTransform.position;
        Quaternion camStartRot = cameraTransform.rotation;

        // --- FIRST TIME: Egg only intro ---
        if (!hasPlayedOriginal)
        {
            hasPlayedOriginal = true;

            if (playerVisual != null) playerVisual.SetActive(false);
            if (egg != null) egg.SetActive(true);

            if (zoomIntoEgg)
                yield return StartCoroutine(ZoomIntoEgg(camStartPos, camStartRot));

            // Egg → Chicken transition
            if (egg != null) egg.SetActive(false);
            if (scalePlayerFromTiny && playerVisual != null)
                yield return StartCoroutine(ScalePlayerUp());

            // Restore camera
            cameraTransform.position = camStartPos;
            cameraTransform.rotation = camStartRot;
        }
        else
        {
            // --- SUBSEQUENT TIMES ---
            if (playSitAnimation && player.animator != null)
                player.animator.SetTrigger("Sit");

            yield return new WaitForSeconds(sitAnimDelay);

            if (nestPoint != null)
            {
                player.transform.position = nestPoint.position;
                player.transform.rotation = Quaternion.identity; // face (0,0,0)
            }

            if (showEgg && egg != null)
            {
                egg.SetActive(true);
                yield return new WaitForSeconds(eggAppearDelay);
            }

            if (zoomIntoEgg)
                yield return StartCoroutine(ZoomIntoEgg(camStartPos, camStartRot));

            if (hidePlayerDuringZoom && playerVisual != null)
                playerVisual.SetActive(false);

            // Zoom back out
            yield return StartCoroutine(ZoomBackOut(camStartPos, camStartRot));

            if (scalePlayerFromTiny && playerVisual != null)
                yield return StartCoroutine(ScalePlayerUp());

            if (egg != null) egg.SetActive(false);

            cameraTransform.position = camStartPos;
            cameraTransform.rotation = camStartRot;
        }

        // Restore player control
        player.enabled = true;
        if (playerCameraController != null) playerCameraController.enabled = true;

        if (timer != null)
        {
            timer.ResetToMaxTime();
            timer.ResumeTimer();
        }
    }

    // --- Modular Steps ---

    private IEnumerator ZoomIntoEgg(Vector3 camStartPos, Quaternion camStartRot)
    {
        float elapsed = 0f;
        Vector3 zoomTarget = egg.transform.position + Vector3.up * zoomOffsetY
                             - cameraTransform.forward * zoomDistance;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            cameraTransform.position = Vector3.Lerp(camStartPos, zoomTarget, t);
            cameraTransform.LookAt(egg.transform.position);

            yield return null;
        }
    }

    private IEnumerator ZoomBackOut(Vector3 camStartPos, Quaternion camStartRot)
    {
        float elapsed = 0f;
        Vector3 zoomTarget = egg.transform.position + Vector3.up * zoomOffsetY
                             - cameraTransform.forward * zoomDistance;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            cameraTransform.position = Vector3.Lerp(zoomTarget, camStartPos, t);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, camStartRot, t);

            yield return null;
        }
    }

    private IEnumerator ScalePlayerUp()
    {
        playerVisual.SetActive(true);
        playerVisual.transform.localScale = Vector3.one * tinyScale;

        float elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / scaleUpDuration);

            playerVisual.transform.localScale = Vector3.Lerp(Vector3.one * tinyScale, finalPlayerScale, t);
            yield return null;
        }

        // Snap to exact final scale
        playerVisual.transform.localScale = finalPlayerScale;
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
