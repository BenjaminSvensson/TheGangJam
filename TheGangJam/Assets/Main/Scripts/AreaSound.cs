using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class AreaTrigger : MonoBehaviour
{
    [Header("Area Settings")]
    public string areaName = "New Area";
    public AudioClip entrySound;
    public AudioClip ambianceClip;

    [Header("UI")]
    public TMP_Text areaNameText;
    public float fadeDuration = 1f;
    public float holdDuration = 2f;

    [Header("Audio Sources")]
    public AudioSource sfxSource;       // For entry sound
    public AudioSource ambianceSourceA; // For crossfade
    public AudioSource ambianceSourceB;

    private static HashSet<string> visitedAreas = new HashSet<string>();
    private static AudioSource currentAmbianceSource;
    private static AudioSource nextAmbianceSource;

    private Coroutine showNameRoutine;
    private Coroutine crossfadeRoutine;

    private void Awake()
    {
        if (ambianceSourceA != null && ambianceSourceB != null)
        {
            currentAmbianceSource = ambianceSourceA;
            nextAmbianceSource = ambianceSourceB;
        }

        // Ensure text starts invisible but active
        if (areaNameText != null)
        {
            Color c = areaNameText.color;
            c.a = 0f;
            areaNameText.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // First-time entry logic
        if (!visitedAreas.Contains(areaName))
        {
            visitedAreas.Add(areaName);

            // Play entry sound once
            if (entrySound != null && sfxSource != null)
                sfxSource.PlayOneShot(entrySound);

            // Show area name
            if (areaNameText != null)
            {
                if (showNameRoutine != null) StopCoroutine(showNameRoutine);
                showNameRoutine = StartCoroutine(ShowAreaName());
            }
        }

        // Switch ambiance every time
        if (ambianceClip != null && currentAmbianceSource != null && nextAmbianceSource != null)
        {
            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            crossfadeRoutine = StartCoroutine(CrossfadeAmbiance(ambianceClip));
        }
    }

    private IEnumerator ShowAreaName()
    {
        areaNameText.text = areaName;
        Color c = areaNameText.color;

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            areaNameText.color = c;
            yield return null;
        }
        c.a = 1;
        areaNameText.color = c;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            areaNameText.color = c;
            yield return null;
        }
        c.a = 0;
        areaNameText.color = c;
    }

    private IEnumerator CrossfadeAmbiance(AudioClip newClip)
    {
        if (currentAmbianceSource.clip == newClip) yield break; // already playing

        nextAmbianceSource.clip = newClip;
        nextAmbianceSource.volume = 0f;
        nextAmbianceSource.loop = true;
        nextAmbianceSource.Play();

        float duration = 2f; // crossfade time
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float lerp = t / duration;
            currentAmbianceSource.volume = Mathf.Lerp(1f, 0f, lerp);
            nextAmbianceSource.volume = Mathf.Lerp(0f, 1f, lerp);
            yield return null;
        }

        currentAmbianceSource.Stop();
        currentAmbianceSource.volume = 0f;
        nextAmbianceSource.volume = 1f;

        // Swap references
        var temp = currentAmbianceSource;
        currentAmbianceSource = nextAmbianceSource;
        nextAmbianceSource = temp;
    }
}
