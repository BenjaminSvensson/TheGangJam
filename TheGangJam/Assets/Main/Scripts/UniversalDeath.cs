using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UniversalDeath : MonoBehaviour
{
    [Header("References")]
    public ChickenController player;          // Your player controller
    public Transform playerVisual;            // The mesh/visual root
    public CameraController cameraController; // Your camera follow script

    [Header("Respawn Settings")]
    public Transform respawnPoint;       // Assign a default spawn or checkpoint
    public float respawnDelay = 2f;      // Seconds before respawn

    [Header("Powerups")]
    public List<PowerUp> powerupPrefabs; // Assign all powerup objects in scene
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();

    private void Start()
    {
        // Save original spawn positions for all powerups
        foreach (var p in powerupPrefabs)
        {
            originalPositions.Add(p.transform.position);
            originalRotations.Add(p.transform.rotation);
        }
    }

    public void KillPlayer()
    {
        // 1. Stop camera following
        if (cameraController != null)
            cameraController.enabled = false;

        // 2. Disable abilities
        if (player != null)
        {
            player.canWalk = true;   // keep walk
            player.canJump = false;
            player.canDoubleJump = false;
            player.canDash = false;
            player.canSprint = false;

            player.enabled = false; // freeze controller logic
        }

        // 3. Make player flop over
        if (playerVisual != null)
            playerVisual.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // 4. Reset powerups
        for (int i = 0; i < powerupPrefabs.Count; i++)
        {
            PowerUp p = powerupPrefabs[i];
            p.transform.position = originalPositions[i];
            p.transform.rotation = originalRotations[i];
            p.gameObject.SetActive(true);
        }

        // 5. Start respawn coroutine
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Move player root to respawn point
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;

            // Reset velocity
            player.ResetVelocity();

            if (cc != null) cc.enabled = true;
        }

        // Reset visuals
        if (playerVisual != null)
        {
            playerVisual.localRotation = Quaternion.identity;
            playerVisual.localPosition = Vector3.zero;
        }

        // Re‑enable player script
        player.enabled = true;

        // Re‑enable camera follow
        if (cameraController != null)
            cameraController.enabled = true;
    }
}
