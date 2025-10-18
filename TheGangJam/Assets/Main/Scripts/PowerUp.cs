using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PowerUp : MonoBehaviour
{
    public enum PowerupType { Jump, DoubleJump, Dash, Sprint, SlowFall }

    [Header("Powerup Settings")]
    public PowerupType type;

    [Header("Timer Bonus")]
    public float bonusTime = 20f;

    [Header("Feedback")]
    public AudioClip pickupSound;
    public TMP_Text pickupText;             // Assign a UI text (not a child of this object)
    [TextArea]
    public string pickupMessage = "Powerup Collected!";
    public float fadeDuration = 0.5f;
    public float holdDuration = 1.5f;

    private Renderer[] renderers;
    private Collider myCollider;

    private void Awake()
    {
        // Cache components for hiding the pickup
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        myCollider = GetComponent<Collider>();

        // Ensure pickupText starts invisible
        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(true);
            pickupText.alpha = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<ChickenController>();
        var timer = FindFirstObjectByType<CountdownTimer>();

        if (player == null)
            return;

        // Grant ability
        switch (type)
        {
            case PowerupType.Jump: player.canJump = true; break;
            case PowerupType.DoubleJump: player.canDoubleJump = true; break;
            case PowerupType.Dash: player.canDash = true; break;
            case PowerupType.Sprint: player.canSprint = true; break;
            case PowerupType.SlowFall: player.canSlowFall = true; break;
        }

        // Add time bonus
        if (timer != null)
        {
            timer.maxTime += bonusTime;
            timer.currentTime = Mathf.Min(timer.currentTime + bonusTime, timer.maxTime);
        }

        // Play pickup sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main != null ? Camera.main.transform.position : transform.position);

        // Show pickup text (runs independently)
        if (pickupText != null)
            TemporaryTextFader.Fade(pickupText, pickupMessage, fadeDuration, holdDuration);

        // Hide visuals and disable collider (don’t destroy!)
        HidePowerUp();
    }

    private void HidePowerUp()
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = false;
        }

        if (myCollider != null)
            myCollider.enabled = false;

        enabled = false; // disable this script to prevent re-triggering
    }

    // Allow death system or reset logic to restore the pickup
    public void ResetPowerUp()
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = true;
        }

        if (myCollider != null)
            myCollider.enabled = true;

        enabled = true;
    }

    // Helper class handles text fading on a persistent object
    private class TemporaryTextFader : MonoBehaviour
    {
        private TMP_Text text;

        public static void Fade(TMP_Text text, string message, float fadeDuration, float holdDuration)
        {
            if (text == null)
                return;

            var go = new GameObject("TempTextFader");
            DontDestroyOnLoad(go);
            var helper = go.AddComponent<TemporaryTextFader>();
            helper.StartCoroutine(helper.DoFade(text, message, fadeDuration, holdDuration));
        }

        private IEnumerator DoFade(TMP_Text text, string message, float fadeDuration, float holdDuration)
        {
            text.gameObject.SetActive(true);
            text.text = message;

            // Fade in
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                text.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            text.alpha = 1f;

            yield return new WaitForSeconds(holdDuration);

            // Fade out
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                text.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            text.alpha = 0f;

            Destroy(gameObject);
        }
    }
}
