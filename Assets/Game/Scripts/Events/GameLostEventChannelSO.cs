using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    /// <summary>
    /// Every way the run can end badly arrives here: the police losing patience (plan 06) and both
    /// ways to lose a night (plan 04). The LossReason is the shared vocabulary between them.
    /// </summary>
    [CreateAssetMenu(fileName = "CH_GameLost", menuName = "Game/Events/Game Lost")]
    public class GameLostEventChannelSO : EventChannelSO<LossReason> { }
}
