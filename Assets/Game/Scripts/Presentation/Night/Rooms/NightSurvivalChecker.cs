using System.Collections.Generic;
using UnityEngine;
using Game.Domain;

public class NightSurvivalChecker : MonoBehaviour
{
    [SerializeField] private List<Rooms> leakedRooms = new List<Rooms>();

    public bool DoesThePlayerSurvive(
        Rooms hidingRoom,
        out LossReason lossReason)
    {
        if (hidingRoom == null)
        {
            lossReason = LossReason.HidInLeakedRoom;
            Debug.Log("Player did not survive the night.");
            return false;
        }

        if (leakedRooms.Contains(hidingRoom))
        {
            lossReason = LossReason.HidInLeakedRoom;
            Debug.Log("Player did not survive the night: Hid in a leaked room.");
            return false;
        }

        // Player survived.
        lossReason = default;

        Debug.Log("Player survived the night!");

        return true;
    }
}