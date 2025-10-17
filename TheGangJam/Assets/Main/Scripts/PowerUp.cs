using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    public enum PowerupType { Jump, DoubleJump, Dash, Sprint }
    public PowerupType type;

    private void OnTriggerEnter(Collider other)
    {
        ChickenController player = other.GetComponent<ChickenController>();
        if (player != null)
        {
            switch (type)
            {
                case PowerupType.Jump: player.canJump = true; break;
                case PowerupType.DoubleJump: player.canDoubleJump = true; break;
                case PowerupType.Dash: player.canDash = true; break;
                case PowerupType.Sprint: player.canSprint = true; break;
            }
            gameObject.SetActive(false);
        }
    }
}
