using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class DayClockTests
    {
        private const float SecondsPerDay = 180f;
        private const float MinimumAfterSpend = 5f;
        private const float Tolerance = 0.0001f;

        private DayClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new DayClock(SecondsPerDay);
        }

        [Test]
        public void Ctor_NonPositiveSecondsPerDay_Throws()
        {
            Assert.That(() => new DayClock(0f), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new DayClock(-1f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Ctor_NewClock_StartsFull()
        {
            Assert.That(_clock.Remaining, Is.EqualTo(SecondsPerDay).Within(Tolerance));
            Assert.That(_clock.NormalizedRemaining, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(_clock.IsExpired, Is.False);
        }

        [Test]
        public void Tick_ReducesRemaining()
        {
            _clock.Tick(30f);

            Assert.That(_clock.Remaining, Is.EqualTo(150f).Within(Tolerance));
            Assert.That(_clock.IsExpired, Is.False);
        }

        [Test]
        public void Tick_NegativeDelta_Throws()
        {
            Assert.That(() => _clock.Tick(-1f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Spend_ReducesRemaining()
        {
            _clock.Spend(12f);

            Assert.That(_clock.Remaining, Is.EqualTo(168f).Within(Tolerance));
        }

        [Test]
        public void Spend_NegativeCost_Throws()
        {
            Assert.That(() => _clock.Spend(-1f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Tick_PastZero_ClampsToZeroAndExpires()
        {
            _clock.Tick(SecondsPerDay + 60f);

            Assert.That(_clock.Remaining, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(_clock.NormalizedRemaining, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(_clock.IsExpired, Is.True);
        }

        [Test]
        public void Spend_PastZero_ClampsToZeroAndExpires()
        {
            _clock.Tick(SecondsPerDay - 5f);
            _clock.Spend(12f);

            Assert.That(_clock.Remaining, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(_clock.IsExpired, Is.True);
        }

        [Test]
        public void NormalizedRemaining_HalfSpent_IsHalf()
        {
            _clock.Tick(SecondsPerDay / 2f);

            Assert.That(_clock.NormalizedRemaining, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Ctor_NegativeMinimumAfterSpend_Throws()
        {
            Assert.That(() => new DayClock(SecondsPerDay, -1f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Ctor_MinimumAfterSpendNotBelowTheDay_Throws()
        {
            Assert.That(
                () => new DayClock(SecondsPerDay, SecondsPerDay),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Spend_WouldDropBelowFloor_StopsAtTheFloor()
        {
            var clock = new DayClock(SecondsPerDay, MinimumAfterSpend);
            clock.Tick(SecondsPerDay - 8f);

            clock.Spend(12f);

            Assert.That(clock.Remaining, Is.EqualTo(MinimumAfterSpend).Within(Tolerance));
            Assert.That(clock.IsExpired, Is.False);
        }

        [Test]
        public void Spend_AlreadyBelowFloor_GivesNoTimeBack()
        {
            var clock = new DayClock(SecondsPerDay, MinimumAfterSpend);
            clock.Tick(SecondsPerDay - 2f);

            clock.Spend(12f);

            Assert.That(clock.Remaining, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Spend_WellAboveFloor_ChargesTheFullCost()
        {
            var clock = new DayClock(SecondsPerDay, MinimumAfterSpend);

            clock.Spend(12f);

            Assert.That(clock.Remaining, Is.EqualTo(SecondsPerDay - 12f).Within(Tolerance));
        }

        [Test]
        public void Tick_WithAFloorSet_StillReachesZero()
        {
            var clock = new DayClock(SecondsPerDay, MinimumAfterSpend);

            clock.Tick(SecondsPerDay);

            Assert.That(clock.Remaining, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(clock.IsExpired, Is.True);
        }

        [Test]
        public void ResetForNewDay_RestoresFullDay()
        {
            _clock.Tick(SecondsPerDay);

            _clock.ResetForNewDay();

            Assert.That(_clock.Remaining, Is.EqualTo(SecondsPerDay).Within(Tolerance));
            Assert.That(_clock.IsExpired, Is.False);
        }
    }
}
