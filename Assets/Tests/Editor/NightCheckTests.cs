using System.Collections.Generic;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class NightCheckTests
    {
        private static IReadOnlyCollection<RoomId> Leaked(params RoomId[] rooms)
        {
            return new HashSet<RoomId>(rooms);
        }

        // On a surviving night `reason` carries no meaning — the tests below discard it on
        // purpose, the same way NightSurvivalChecker must only read it when Survives returns false.

        [Test]
        public void Survives_HiddenInSafeRoom_True()
        {
            bool survived = NightCheck.Survives(RoomId.Bathroom, Leaked(RoomId.Kitchen), out _);

            Assert.That(survived, Is.True);
        }

        [Test]
        public void Survives_NoLeakedRooms_True()
        {
            bool survived = NightCheck.Survives(RoomId.Kitchen, Leaked(), out _);

            Assert.That(survived, Is.True);
        }

        [Test]
        public void Survives_NullLeakedRooms_TreatedAsEmpty()
        {
            bool survived = NightCheck.Survives(RoomId.Kitchen, null, out _);

            Assert.That(survived, Is.True);
        }

        [Test]
        public void Survives_NotHidden_FalseWithDayClockExpired()
        {
            bool survived = NightCheck.Survives(null, Leaked(RoomId.Kitchen), out LossReason reason);

            Assert.That(survived, Is.False);
            Assert.That(reason, Is.EqualTo(LossReason.DayClockExpired));
        }

        [Test]
        public void Survives_HiddenInLeakedRoom_FalseWithHidInLeakedRoom()
        {
            bool survived = NightCheck.Survives(RoomId.Bedroom, Leaked(RoomId.Kitchen, RoomId.Bedroom), out LossReason reason);

            Assert.That(survived, Is.False);
            Assert.That(reason, Is.EqualTo(LossReason.HidInLeakedRoom));
        }

        [Test]
        public void Survives_NotHiddenAndNoLeaks_FalseWithDayClockExpired()
        {
            bool survived = NightCheck.Survives(null, Leaked(), out LossReason reason);

            Assert.That(survived, Is.False);
            Assert.That(reason, Is.EqualTo(LossReason.DayClockExpired));
        }
    }
}
