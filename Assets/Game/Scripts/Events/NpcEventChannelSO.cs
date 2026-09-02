using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_Npc", menuName = "Game/Events/NPC")]
    public class NpcEventChannelSO : EventChannelSO<NpcSO> { }
}
