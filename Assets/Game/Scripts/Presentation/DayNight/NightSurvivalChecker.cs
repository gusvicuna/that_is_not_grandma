using System.Collections.Generic;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Remembers where the player is hiding and which rooms have leaked, and resolves the night
    /// when it falls. The rule itself is NightCheck — this only feeds it and reports.
    /// </summary>
    public class NightSurvivalChecker : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO _nightStarted;
        [SerializeField] private RoomIdEventChannelSO _playerHid;
        [SerializeField] private RoomIdEventChannelSO _roomChanged;
        [SerializeField] private RoomIdEventChannelSO _roomLeaked;
        [SerializeField] private BoolEventChannelSO _nightResolved;
        [SerializeField] private GameLostEventChannelSO _gameLost;

        private readonly HashSet<RoomId> _leakedRooms = new();
        private RoomId? _hidingRoom;

        public IReadOnlyCollection<RoomId> LeakedRooms => _leakedRooms;

        private void OnEnable()
        {
            if (_nightStarted != null) _nightStarted.Raised += OnNightStarted;
            if (_playerHid != null) _playerHid.Raised += OnPlayerHid;
            if (_roomChanged != null) _roomChanged.Raised += OnRoomChanged;
            if (_roomLeaked != null) _roomLeaked.Raised += OnRoomLeaked;
        }

        private void OnDisable()
        {
            if (_nightStarted != null) _nightStarted.Raised -= OnNightStarted;
            if (_playerHid != null) _playerHid.Raised -= OnPlayerHid;
            if (_roomChanged != null) _roomChanged.Raised -= OnRoomChanged;
            if (_roomLeaked != null) _roomLeaked.Raised -= OnRoomLeaked;
        }

        private void OnPlayerHid(RoomId room)
        {
            _hidingRoom = room;
        }

        /// <summary>Leaving the room means leaving the hiding spot.</summary>
        private void OnRoomChanged(RoomId room)
        {
            _hidingRoom = null;
        }

        /// <summary>Leaked rooms accumulate for the whole run and never reset.</summary>
        private void OnRoomLeaked(RoomId room)
        {
            _leakedRooms.Add(room);
        }

        private void OnNightStarted()
        {
            bool survived = NightCheck.Survives(_hidingRoom, _leakedRooms, out LossReason reason);
            _hidingRoom = null;
            _nightResolved.Raise(survived);
            if (!survived)
            {
                _gameLost.Raise(reason);
            }
        }
    }
}
