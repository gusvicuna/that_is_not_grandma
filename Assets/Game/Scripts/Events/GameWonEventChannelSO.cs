using System;
using UnityEngine;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "GameWonEventChannel", menuName = "Game/Events/Game Won")]
    public class GameWonEventChannelSO : ScriptableObject
    {
        public event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }
    }
}