using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    /// <summary>
    /// Defined by plan 06 (police call) and written here because plan 04's night check needs it
    /// first. Same file name, same payload — do not write a second one when plan 06 merges.
    /// </summary>
    [CreateAssetMenu(fileName = "CH_GameLost", menuName = "Game/Events/Game Lost")]
    public class GameLostEventChannelSO : EventChannelSO<LossReason> { }
}
