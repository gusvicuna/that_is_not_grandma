
using System;
using UnityEngine;

namespace Game.Events
{
    [CreateAssetMenu(
        fileName = "RoomChangedEventChannel",
        menuName = "Game/Events/Room Changed"
    )]
    public class RoomChangedEventChannelSO : ScriptableObject
    {
        public event Action<int> Raised;

        public void Raise(int roomIndex)
        {
            Raised?.Invoke(roomIndex);
        }
    }
}

