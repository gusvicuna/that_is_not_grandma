using UnityEngine;
using Game.Domain;

public class HidingSpot : MonoBehaviour
{
    [SerializeField] private Rooms room;
    [SerializeField] private NightSurvivalChecker survivalChecker;
    [SerializeField] private NightResultUI resultUI;
    [SerializeField] private DayNightCycle dayNightCycle;

    public Rooms Room => room;

    private void OnMouseDown()
    {
        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle is not assigned.");
            return;
        }

        if (dayNightCycle.CurrentTime != DayNightCycle.TimeOfDay.Night)
        {
            Debug.Log("It is still day. You cannot hide yet.");
            return;
        }

        bool survived = survivalChecker.DoesThePlayerSurvive(
            room,
            out LossReason lossReason
        );

        resultUI.ShowResult(survived, lossReason);
    }
}
