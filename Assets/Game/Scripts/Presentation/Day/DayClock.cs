using UnityEngine;

public class DayClock : MonoBehaviour
{
    [SerializeField] private float duration = 120f;

    public bool HasExpired { get; private set; }

    private float _timeRemaining;
    private bool _isRunning;

    public void StartClock()
    {
        _timeRemaining = duration;
        HasExpired = false;
        _isRunning = true;
    }

    public void StopClock()
    {
        _isRunning = false;
    }

    private void Update()
    {
        if (!_isRunning || HasExpired)
        {
            return;
        }

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            HasExpired = true;
            _isRunning = false;
        }
    }
}