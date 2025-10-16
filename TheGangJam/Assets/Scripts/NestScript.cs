using UnityEngine;

public class NestScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ChickenController>() != null)
        {
            CountdownTimer timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
            {
                timer.ResetToMaxTime();
            }
        }
    }
}
