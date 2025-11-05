using UnityEngine;
using System.Collections;

public class NestScript : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public CameraController playerCameraController;
    public ChickenController player;
    public GameObject playerVisual;
    public GameObject egg;
    public Transform nestPoint;

    [Header("Timings")]
    public float sitAnimDelay = 1f;
    public float eggAppearDelay = 0.5f;
    public float zoomDuration = 2f;
    public float scaleUpDuration = 2f;
    public float sitCooldown = 3f;
    [Header("Scaling")]
    public float tinyScale = 0.1f;
    public Vector3 finalPlayerScale = Vector3.one;

    [Header("Camera Settings")]
    public float zoomOffsetY = 0.5f;
    public float zoomDistance = 3f;

    private bool hasPlayedOriginal = false;
    private bool isOnSitCooldown = false;

    [HideInInspector] public bool isRespawn = false;

    public void PlayNestSequence(bool respawn = false)
    {
        isRespawn = respawn;
        StartCoroutine(NestSequence());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ChickenController>() == player)
        {
            if (!isOnSitCooldown)
                StartCoroutine(NestSequence());
        }
    }

    private IEnumerator NestSequence()
    {
        CountdownTimer timer = Object.FindFirstObjectByType<CountdownTimer>();
        if (timer != null) timer.PauseTimer();

        player.enabled = false;
        if (playerCameraController != null) playerCameraController.enabled = false;

        Vector3 camStartPos = cameraTransform.position;
        Quaternion camStartRot = cameraTransform.rotation;

        if (!hasPlayedOriginal)
        {
            hasPlayedOriginal = true;
            yield return StartCoroutine(HatchSequence(camStartPos, camStartRot));
        }
        else if (isRespawn)
        {
            isRespawn = false;
            yield return StartCoroutine(HatchSequence(camStartPos, camStartRot));
        }
        else
        {
            yield return StartCoroutine(SitSequence(camStartPos, camStartRot));
            isOnSitCooldown = true;
            StartCoroutine(SitCooldownRoutine());
        }

        player.enabled = true;
        if (playerCameraController != null) playerCameraController.enabled = true;

        if (timer != null)
        {
            timer.ResetToMaxTime();
            timer.ResumeTimer();
        }
    }

    private IEnumerator HatchSequence(Vector3 camStartPos, Quaternion camStartRot)
    {
        playerVisual.SetActive(false);
        egg.SetActive(true);

        yield return StartCoroutine(ZoomIntoEgg(camStartPos, camStartRot));
        yield return StartCoroutine(ScalePlayerUp());

        yield return new WaitForSeconds(0.5f);
        egg.SetActive(false);

        cameraTransform.position = camStartPos;
        cameraTransform.rotation = camStartRot;
    }

    private IEnumerator SitSequence(Vector3 camStartPos, Quaternion camStartRot)
    {
        if (player.animator != null)
            player.animator.SetTrigger("Sit");

        yield return new WaitForSeconds(sitAnimDelay);

        if (nestPoint != null)
        {
            player.transform.position = nestPoint.position;
            player.transform.rotation = Quaternion.identity;
        }

        egg.SetActive(true);
        yield return new WaitForSeconds(eggAppearDelay);

        yield return StartCoroutine(ZoomIntoEgg(camStartPos, camStartRot));
        yield return StartCoroutine(ZoomBackOut(camStartPos, camStartRot));

        egg.SetActive(false);

        cameraTransform.position = camStartPos;
        cameraTransform.rotation = camStartRot;

        if (player.animator != null)
            player.animator.Play("Idle");
    }

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

        playerVisual.transform.localScale = finalPlayerScale;
    }

    private IEnumerator SitCooldownRoutine()
    {
        yield return new WaitForSeconds(sitCooldown);
        isOnSitCooldown = false;
    }
}
