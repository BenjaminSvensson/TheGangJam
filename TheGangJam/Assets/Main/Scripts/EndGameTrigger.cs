using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    public float floatSpeed = 2f;      // how fast the player rises
    public float spinSpeed = 180f;     // degrees per second

    private bool endSequenceActive = false;
    private Transform playerTransform;
    private Camera mainCam;
    private Vector3 camLockPos;
    private Quaternion camLockRot;

    private void OnTriggerEnter(Collider other)
    {
        ChickenController chicken = other.GetComponent<ChickenController>();
        if (chicken != null)
        {
            playerTransform = chicken.transform;

            // Stop timers
            var speedrunTimer = FindObjectOfType<SimpleTimerTMP>();
            if (speedrunTimer != null) speedrunTimer.StopTimer();

            var countdown = FindObjectOfType<CountdownTimer>();
            if (countdown != null) countdown.StopTimer();

            // Lock camera position/rotation
            mainCam = Camera.main;
            if (mainCam != null)
            {
                camLockPos = mainCam.transform.position;
                camLockRot = mainCam.transform.rotation;

                // Disable player camera script so it stops moving
                var camController = mainCam.GetComponent<CameraController>();
                if (camController != null) camController.enabled = false;
            }

            // Disable player control
            chicken.enabled = false;

            // Start end sequence
            endSequenceActive = true;

            // Center the speedrun timer UI
            var timer = FindObjectOfType<SimpleTimerTMP>();
            if (timer != null && timer.timerText != null)
            {
                var rect = timer.timerText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
        }
    }

    private void LateUpdate()
    {
        if (endSequenceActive && playerTransform != null)
        {
            // Float upwards
            playerTransform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Spin around Y axis
            playerTransform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            // Keep camera locked
            if (mainCam != null)
            {
                mainCam.transform.position = camLockPos;
                mainCam.transform.rotation = camLockRot;
            }
        }
    }
}
