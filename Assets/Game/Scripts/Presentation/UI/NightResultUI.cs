using TMPro;
using UnityEngine;
using Game.Domain;

public class NightResultUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text resultText;

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowResult(bool survived, LossReason lossReason)
    {
        if (panel == null || resultText == null)
        {
            Debug.LogError("NightResultUI is not assigned correctly.");
            return;
        }

        // If the player survives, don't show anything.
        // The game can continue normally.
        if (survived)
        {
            return;
        }

        // Only show the panel when the player loses.
        panel.SetActive(true);

        switch (lossReason)
        {
            case LossReason.DayClockExpired:
                resultText.text = "You ran out of time.";
                break;

            case LossReason.HidInLeakedRoom:
                resultText.text = "This room was leaked!";
                break;

            case LossReason.PoliceTrustLost:
                resultText.text = "You lost the police's trust.";
                break;

            default:
                resultText.text = "You didn't survive the night.";
                break;
        }
    }
}