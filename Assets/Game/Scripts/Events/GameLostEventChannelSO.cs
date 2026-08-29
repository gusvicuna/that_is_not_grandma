using System;
using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "GameLostEventChannel", menuName = "Game/Events/Game Lost")]
    public class GameLostEventChannelSO : ScriptableObject
    {
        public event Action<LossReason> Raised;

        public void Raise(LossReason reason)
        {
            Raised?.Invoke(reason);
        }
    }
}
