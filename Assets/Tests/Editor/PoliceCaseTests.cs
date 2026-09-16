using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class PoliceCaseTests
    {
        private const bool Evidence = true;
        private const bool WrongClue = false;

        private const int FirstCallDay = 2;

        private PoliceCase _case;

        [SetUp]
        public void SetUp()
        {
            _case = new PoliceCase();
        }

        [Test]
        public void Ctor_Defaults_TrustIsTwoAndDayIsZero()
        {
            Assert.That(_case.TrustRemaining, Is.EqualTo(2));
            Assert.That(_case.CurrentDay, Is.EqualTo(0));
            Assert.That(_case.IsResolved, Is.False);
            Assert.That(_case.IsPhoneAvailable, Is.False);
            Assert.That(_case.CanCall, Is.False);
        }

        [Test]
        public void Ctor_TrustBelowOne_Throws()
        {
            Assert.That(() => new PoliceCase(0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Ctor_FirstAvailableDayBelowOne_Throws()
        {
            Assert.That(() => new PoliceCase(2, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void IsPhoneAvailable_BeforeFirstAvailableDay_IsFalse()
        {
            _case.StartDay(1);

            Assert.That(_case.IsPhoneAvailable, Is.False);
            Assert.That(_case.CanCall, Is.False);
        }

        [Test]
        public void IsPhoneAvailable_OnFirstAvailableDay_IsTrue()
        {
            _case.StartDay(FirstCallDay);

            Assert.That(_case.IsPhoneAvailable, Is.True);
            Assert.That(_case.CanCall, Is.True);
        }

        [Test]
        public void StartDay_InvalidDay_Throws()
        {
            Assert.That(() => _case.StartDay(0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Call_BeforeFirstAvailableDay_UnavailableAndTrustUnchanged()
        {
            _case.StartDay(1);

            PoliceCallOutcome outcome = _case.Call(WrongClue);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Unavailable));
            Assert.That(_case.TrustRemaining, Is.EqualTo(2));
            Assert.That(_case.IsResolved, Is.False);
        }

        [Test]
        public void Call_BeforeAnyDayStarted_Unavailable()
        {
            PoliceCallOutcome outcome = _case.Call(Evidence);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Unavailable));
            Assert.That(_case.IsResolved, Is.False);
        }

        [Test]
        public void Call_WithEvidence_WonAndCaseResolved()
        {
            _case.StartDay(FirstCallDay);

            PoliceCallOutcome outcome = _case.Call(Evidence);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Won));
            Assert.That(_case.IsResolved, Is.True);
            Assert.That(_case.TrustRemaining, Is.EqualTo(2));
            Assert.That(_case.IsPhoneAvailable, Is.False);
        }

        [Test]
        public void Call_WithWrongClue_WrongEvidenceAndTrustDecreases()
        {
            _case.StartDay(FirstCallDay);

            PoliceCallOutcome outcome = _case.Call(WrongClue);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.WrongEvidence));
            Assert.That(_case.TrustRemaining, Is.EqualTo(1));
            Assert.That(_case.IsResolved, Is.False);
        }

        [Test]
        public void Call_LastTrustWithWrongClue_TrustLostAndCaseResolved()
        {
            _case.StartDay(FirstCallDay);
            _case.Call(WrongClue);
            _case.StartDay(FirstCallDay + 1);

            PoliceCallOutcome outcome = _case.Call(WrongClue);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.TrustLost));
            Assert.That(_case.TrustRemaining, Is.EqualTo(0));
            Assert.That(_case.IsResolved, Is.True);
            Assert.That(_case.IsPhoneAvailable, Is.False);
        }

        [Test]
        public void Call_TwiceInTheSameDay_SecondIsUnavailable()
        {
            _case.StartDay(FirstCallDay);
            _case.Call(WrongClue);

            PoliceCallOutcome outcome = _case.Call(WrongClue);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Unavailable));
            Assert.That(_case.TrustRemaining, Is.EqualTo(1), "an unavailable call must not cost trust");
            Assert.That(_case.CanCall, Is.False);
            Assert.That(_case.IsPhoneAvailable, Is.True, "the phone stays in the room, it just refuses the call");
        }

        [Test]
        public void Call_AfterStartingANewDay_IsAllowedAgain()
        {
            _case.StartDay(FirstCallDay);
            _case.Call(WrongClue);

            _case.StartDay(FirstCallDay + 1);

            Assert.That(_case.CanCall, Is.True);
            Assert.That(_case.Call(Evidence), Is.EqualTo(PoliceCallOutcome.Won));
        }

        [Test]
        public void Call_AfterWinning_Unavailable()
        {
            _case.StartDay(FirstCallDay);
            _case.Call(Evidence);
            _case.StartDay(FirstCallDay + 1);

            PoliceCallOutcome outcome = _case.Call(Evidence);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Unavailable));
            Assert.That(_case.CanCall, Is.False);
        }

        [Test]
        public void Call_AfterTrustLost_Unavailable()
        {
            _case.StartDay(FirstCallDay);
            _case.Call(WrongClue);
            _case.StartDay(FirstCallDay + 1);
            _case.Call(WrongClue);
            _case.StartDay(FirstCallDay + 2);

            PoliceCallOutcome outcome = _case.Call(Evidence);

            Assert.That(outcome, Is.EqualTo(PoliceCallOutcome.Unavailable));
            Assert.That(_case.TrustRemaining, Is.EqualTo(0));
        }

        [Test]
        public void Call_WithCustomStartingTrust_SurvivesThatManyWrongCalls()
        {
            var lenientCase = new PoliceCase(3, FirstCallDay);

            lenientCase.StartDay(FirstCallDay);
            Assert.That(lenientCase.Call(WrongClue), Is.EqualTo(PoliceCallOutcome.WrongEvidence));

            lenientCase.StartDay(FirstCallDay + 1);
            Assert.That(lenientCase.Call(WrongClue), Is.EqualTo(PoliceCallOutcome.WrongEvidence));

            lenientCase.StartDay(FirstCallDay + 2);
            Assert.That(lenientCase.Call(WrongClue), Is.EqualTo(PoliceCallOutcome.TrustLost));
            Assert.That(lenientCase.IsResolved, Is.True);
        }

        [Test]
        public void Call_WithCustomFirstAvailableDay_FollowsThatDay()
        {
            var earlyCase = new PoliceCase(2, 1);

            earlyCase.StartDay(1);

            Assert.That(earlyCase.IsPhoneAvailable, Is.True);
            Assert.That(earlyCase.Call(Evidence), Is.EqualTo(PoliceCallOutcome.Won));
        }
    }
}
