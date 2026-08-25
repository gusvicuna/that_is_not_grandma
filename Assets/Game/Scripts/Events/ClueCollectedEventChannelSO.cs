using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "ClueCollectedEventChannel", menuName = "Game/Events/Clue Collected")]
    public class ClueCollectedEventChannelSO : ScriptableObject
    {
        public event Action<ClueSO> Raised;

        public void Raise(ClueSO clue)
        {
            Raised?.Invoke(clue);
        }
    }
}
