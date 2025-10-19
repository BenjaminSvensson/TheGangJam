using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UniversalDeath : MonoBehaviour
{
    [Header("References")]
    public ChickenController player;          
    public Transform playerVisual;            
    public CameraController cameraController; 

    [Header("Respawn Settings")]
    public Transform respawnPoint;       
    public float respawnDelay = 2f;      

    [Header("Powerups")]
    public List<PowerUp> powerupPrefabs; //PowerupObjectForRespawnFunc Later
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();

    [Header("Audio")]
    public AudioSource audioSource;      
    public AudioClip deathSound;         
    public AudioClip respawnSound;       

    private void Start()
    {
        //Save powerup positions for respawing
        foreach (var p in powerupPrefabs)
        {
            originalPositions.Add(p.transform.position);
            originalRotations.Add(p.transform.rotation);
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void KillPlayer()
    {
        
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        //StopCamFollow
        if (cameraController != null)
            cameraController.enabled = false;

        if (player != null)
        {
            player.canWalk = true;   
            player.canJump = false;
            player.canDoubleJump = false;
            player.canDash = false;
            player.canSprint = false;

            player.enabled = false;
        }
        if (playerVisual != null)
            playerVisual.localRotation = Quaternion.Euler(90f, 0f, 0f);

        //Powerup respawn
        for (int i = 0; i < powerupPrefabs.Count; i++)
        {
            PowerUp p = powerupPrefabs[i];

           
            p.transform.position = originalPositions[i];
            p.transform.rotation = originalRotations[i];

           
            if (!p.gameObject.activeSelf)
                p.gameObject.SetActive(true);

            
            p.ResetPowerUp();
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            //Chicken to nestlocation for respawn dudes
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;

            player.ResetVelocity();

            if (cc != null) cc.enabled = true;
        }

        if (playerVisual != null)
        {
            playerVisual.localRotation = Quaternion.identity;
            playerVisual.localPosition = Vector3.zero;
        }
        player.enabled = true;

        if (cameraController != null)
            cameraController.enabled = true;

        if (audioSource != null && respawnSound != null)
            audioSource.PlayOneShot(respawnSound);
    }
}
