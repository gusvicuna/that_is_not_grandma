using System;
using UnityEngine;
using Game.Domain;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_RoomLeaked", menuName = "Game/Events/Room Leaked")]
    public class RoomLeakedEventChannelSO : ScriptableObject
    {
        public event Action<RoomId> Raised;

        public void Raise(RoomId roomId)
        {
            Raised?.Invoke(roomId);
        }
    }
}
