using UnityEngine;
using Game.Domain;

public class NightHiding : MonoBehaviour
{
    [SerializeField] private NightSurvivalChecker survivalChecker;

    public void HideInRoom(Rooms room)
    {
        bool survived = survivalChecker.DoesThePlayerSurvive(
            room,
            out LossReason lossReason
        );

        if (survived)
        {
            Debug.Log("Player survived the night.");
        }
        else
        {
            Debug.Log("Player lost: " + lossReason);
        }
    }
}
