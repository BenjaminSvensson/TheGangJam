using UnityEngine;

public class KillaScript : MonoBehaviour
{
    [Header("References")]
    public UniversalDeath deathManager; 
    public string playerTag = "Player";        

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (deathManager != null)
            {
                deathManager.KillPlayer();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            if (deathManager != null)
            {
                deathManager.KillPlayer();
            }
            else
            {
                Debug.LogWarning("No UniversalDeathManager assigned to ColliderKill!");
            }
        }
    }
}
