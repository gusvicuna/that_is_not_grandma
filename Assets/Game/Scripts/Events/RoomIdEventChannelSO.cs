using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_RoomId", menuName = "Game/Events/Room Id")]
    public class RoomIdEventChannelSO : EventChannelSO<RoomId> { }
}
