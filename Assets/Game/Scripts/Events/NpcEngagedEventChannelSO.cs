using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_NpcEngaged", menuName = "Game/Events/Npc Engaged")]
    public class NpcEngagedEventChannelSO : ScriptableObject
    {
        public event Action<NpcSO> Raised;

        public void Raise(NpcSO npc)
        {
            Raised?.Invoke(npc);
        }
    }
}
