using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_PoliceCallOutcome", menuName = "Game/Events/Police Call Outcome")]
    public class PoliceCallOutcomeEventChannelSO : EventChannelSO<PoliceCallOutcome> { }
}
