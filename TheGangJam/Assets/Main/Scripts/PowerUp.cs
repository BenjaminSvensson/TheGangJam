using UnityEngine;

public enum AbilityType { Walk, Jump, DoubleJump, Dash }

public class PowerUp : MonoBehaviour
{
    public GameObject Base;
    [Header("Ability To Unlock")]
    public AbilityType ability;

    [Header("Settings")]
    public bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        ChickenController player = other.GetComponent<ChickenController>();
        if (player != null)
        {
            switch (ability)
            {
                case AbilityType.Walk: player.canWalk = true; break;
                case AbilityType.Jump: player.canJump = true; break;
                case AbilityType.DoubleJump: player.canDoubleJump = true; break;
                case AbilityType.Dash: player.canDash = true; break;
            }

            if (destroyOnPickup)
                Destroy(Base);
        }
    }
}
