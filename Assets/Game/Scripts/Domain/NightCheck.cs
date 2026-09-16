using System.Collections.Generic;

namespace Game.Domain
{
    /// <summary>
    /// The whole night, in one function: did the player hide, and did they hide somewhere they had
    /// already told the Uncle about?
    /// </summary>
    public static class NightCheck
    {
        /// <summary>
        /// True when the player lives to the next morning. <paramref name="reason"/> is only
        /// meaningful when this returns false — on a surviving night it carries a default value
        /// that looks valid and means nothing. Read it inside the failure branch, nowhere else.
        /// </summary>
        public static bool Survives(RoomId? hidingRoom, IReadOnlyCollection<RoomId> leakedRooms, out LossReason reason)
        {
            if (hidingRoom == null)
            {
                reason = LossReason.DayClockExpired;
                return false;
            }
            if (leakedRooms != null)
            {
                foreach (RoomId leaked in leakedRooms)
                {
                    if (leaked == hidingRoom.Value)
                    {
                        reason = LossReason.HidInLeakedRoom;
                        return false;
                    }
                }
            }
            reason = default;
            return true;
        }
    }
}
