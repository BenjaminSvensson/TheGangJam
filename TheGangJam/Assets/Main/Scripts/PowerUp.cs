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
    public TMP_Text pickupText;              // Assign a UI text (not a child of this object)

    [TextArea]
    public string[] pickupMessages;          // 👈 multiple messages now
    public float fadeDuration = 0.5f;
    public float holdDuration = 1.5f;

    private Renderer[] renderers;
    private Collider myCollider;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        myCollider = GetComponent<Collider>();

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

        // Timer logic
        if (timer != null)
        {
            timer.StartTimer();
            timer.AddBonusTime(bonusTime);
        }

        // Play pickup sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main != null ? Camera.main.transform.position : transform.position);

        // Show multiple pickup texts
        if (pickupText != null && pickupMessages != null && pickupMessages.Length > 0)
            TemporaryTextFader.FadeSequence(pickupText, pickupMessages, fadeDuration, holdDuration);

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

        enabled = false;
    }

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

    // Helper class handles text fading
    private class TemporaryTextFader : MonoBehaviour
    {
        public static void FadeSequence(TMP_Text text, string[] messages, float fadeDuration, float holdDuration)
        {
            if (text == null || messages == null || messages.Length == 0)
                return;

            var go = new GameObject("TempTextFader");
            DontDestroyOnLoad(go);
            var helper = go.AddComponent<TemporaryTextFader>();
            helper.StartCoroutine(helper.DoFadeSequence(text, messages, fadeDuration, holdDuration));
        }

        private IEnumerator DoFadeSequence(TMP_Text text, string[] messages, float fadeDuration, float holdDuration)
        {
            text.gameObject.SetActive(true);

            foreach (var message in messages)
            {
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
            }

            Destroy(gameObject);
        }
    }
}
