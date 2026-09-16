using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_TensionLevel", menuName = "Game/Events/Tension Level")]
    public class TensionLevelEventChannelSO : EventChannelSO<TensionLevel> { }
}
