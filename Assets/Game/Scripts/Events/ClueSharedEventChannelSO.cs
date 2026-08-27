using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_ClueShared", menuName = "Game/Events/Clue Shared")]
    public class ClueSharedEventChannelSO : ScriptableObject
    {
        public event Action<NpcSO, ClueSO> Raised;

        public void Raise(NpcSO npc, ClueSO clue)
        {
            Raised?.Invoke(npc, clue);
        }
    }
}
