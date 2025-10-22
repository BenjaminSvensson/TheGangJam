using TMPro;
using UnityEngine;

public class ChangeTMPText : MonoBehaviour
{
    public TMP_Text myTMPText; // Drag your TextMeshProUGUI component here
    private int timer = 60;    // starting seconds
    private float elapsed = 0f; // for 1-second increments

    void Update()
    {
        elapsed += Time.deltaTime;

        // Increment/decrement timer every 1 second
        if (elapsed >= 1f)
        {
            timer--;            // count down
            elapsed = 0f;       // reset elapsed

            if (timer < 0)
                timer = 0;      // stop at 0, optional

            // Calculate surrounding numbers
            int timerN2 = Mathf.Max(timer - 2, 0);
            int timerN1 = Mathf.Max(timer - 1, 0);
            int timerP1 = timer + 1;
            int timerP2 = timer + 2;

            // Build string
            myTMPText.text = timerN2 + " || " + timerN1 + " || " + timer + " || " + timerP1 + " || " + timerP2;
        }
    }
}
