using System;
using UnityEngine;

namespace Game.Events
{
    [CreateAssetMenu(
        fileName = "DayStartedEventChannel",
        menuName = "Game/Events/Day Started"
    )]
    public class DayStartedEventChannelSO : ScriptableObject
    {
        public event Action<int> Raised;

        public void Raise(int day)
        {
            Raised?.Invoke(day);
        }
    }
}
