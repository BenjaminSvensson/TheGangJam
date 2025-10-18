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
    public TMP_Text pickupText; // assign in Inspector
    public string pickupMessage = "Powerup Collected!";
    public float fadeDuration = 0.5f;
    public float holdDuration = 1.5f;

    private AudioSource audioSource;
    private static Coroutine textRoutine;

    private void Awake()
    {
        // Use a dedicated AudioSource in the scene for SFX
        audioSource = FindObjectOfType<AudioSource>();

        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(true); // ensure active
            Color c = pickupText.color;
            c.a = 0f;
            pickupText.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ChickenController player = other.GetComponent<ChickenController>();
        CountdownTimer timer = FindObjectOfType<CountdownTimer>();

        if (player != null)
        {
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
                timer.currentTime += bonusTime;
                if (timer.currentTime > timer.maxTime)
                    timer.currentTime = timer.maxTime;
            }

            // Play sound
            if (pickupSound != null && audioSource != null)
                audioSource.PlayOneShot(pickupSound);

            // Show text
            if (pickupText != null)
            {
                if (textRoutine != null) StopCoroutine(textRoutine);
                textRoutine = StartCoroutine(ShowPickupText());
            }

            // Disable pickup
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowPickupText()
    {
        pickupText.text = pickupMessage;
        Color c = pickupText.color;

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            pickupText.color = c;
            yield return null;
        }
        c.a = 1;
        pickupText.color = c;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            pickupText.color = c;
            yield return null;
        }
        c.a = 0;
        pickupText.color = c;
    }
}
