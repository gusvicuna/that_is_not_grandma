using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public class ExchangeLog
    {
        private readonly List<RoomId> _leakedRooms = new();
        private readonly HashSet<(string NpcId, string ClueId)> _sharedClues = new();

        public IReadOnlyList<RoomId> LeakedRooms => _leakedRooms;

        public ShareResult Share(NpcProfile npc, string clueId, RoomId clueRoom)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }
            if (string.IsNullOrWhiteSpace(clueId))
            {
                throw new ArgumentException("Clue id cannot be null, empty or whitespace.", nameof(clueId));
            }
            if (HasShared(npc.Id, clueId))
            {
                return new ShareResult(ShareOutcome.AlreadyShared, null, false, clueRoom);
            }

            _sharedClues.Add((npc.Id, clueId));

            bool leakedNewRoom = npc.LeaksToNotGrandma && !_leakedRooms.Contains(clueRoom);
            if (leakedNewRoom)
            {
                _leakedRooms.Add(clueRoom);
            }

            npc.Exchanges.TryGetReturn(clueId, out string returnedClueId);
            return new ShareResult(ShareOutcome.Accepted, returnedClueId, leakedNewRoom, clueRoom);
        }

        public bool HasShared(string npcId, string clueId)
        {
            return _sharedClues.Contains((npcId, clueId));
        }
    }
}
