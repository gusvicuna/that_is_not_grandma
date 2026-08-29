using TMPro;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public enum TimeOfDay
    {
        Day,
        Night
    }

    [SerializeField] private DayClock dayClock;
    [SerializeField] private TMP_Text timeOfDayText;
    [SerializeField] private float nightDuration = 30f;

    public TimeOfDay CurrentTime { get; private set; }

    private float _nightTimeRemaining;

    private void Start()
    {
        StartDay();
    }

    private void Update()
    {
        if (CurrentTime == TimeOfDay.Day)
        {
            if (dayClock.HasExpired)
            {
                StartNight();
            }

            return;
        }

        if (CurrentTime == TimeOfDay.Night)
        {
            _nightTimeRemaining -= Time.deltaTime;

            if (_nightTimeRemaining <= 0f)
            {
                StartDay();
            }
        }
    }

    private void StartDay()
    {
        CurrentTime = TimeOfDay.Day;

        dayClock.StartClock();

        UpdateTimeUI();

        Debug.Log("Day has started!");
    }

    private void StartNight()
    {
        CurrentTime = TimeOfDay.Night;

        dayClock.StopClock();

        _nightTimeRemaining = nightDuration;

        UpdateTimeUI();

        Debug.Log("Night has started!");
    }

    private void UpdateTimeUI()
    {
        if (timeOfDayText == null)
        {
            return;
        }

        timeOfDayText.text =
            CurrentTime == TimeOfDay.Day ? "DAY" : "NIGHT";
    }
}