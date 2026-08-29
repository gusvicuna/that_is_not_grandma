using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class TimeOfDayTests
    {
        private const int StartHour = 8;   // 8 AM
        private const int EndHour = 20;    // 8 PM
        private const int Step = 5;

        private static TimeOfDay At(float progress)
        {
            return TimeOfDay.FromDayProgress(progress, StartHour, EndHour, Step);
        }

        [Test]
        public void FromDayProgress_AtDawn_IsTheStartHour()
        {
            Assert.That(At(0f).ToString(), Is.EqualTo("8:00 AM"));
        }

        [Test]
        public void FromDayProgress_AtNightfall_IsTheEndHour()
        {
            Assert.That(At(1f).ToString(), Is.EqualTo("8:00 PM"));
        }

        [Test]
        public void FromDayProgress_Midway_IsTwoInTheAfternoon()
        {
            Assert.That(At(0.5f).ToString(), Is.EqualTo("2:00 PM"));
        }

        [Test]
        public void FromDayProgress_RoundsMinutesDownToTheStep()
        {
            // 7 minutes into a 12-hour day.
            TimeOfDay reading = At(7f / (12f * 60f));

            Assert.That(reading.Minute, Is.EqualTo(5));
            Assert.That(reading.Hour24, Is.EqualTo(8));
        }

        [Test]
        public void FromDayProgress_ProgressOutsideRange_IsClamped()
        {
            Assert.That(At(-2f).ToString(), Is.EqualTo("8:00 AM"));
            Assert.That(At(5f).ToString(), Is.EqualTo("8:00 PM"));
        }

        // The evening tension fires at 18:00, which is 600 of the day's 720 minutes. These two
        // tests straddle that boundary by half a minute rather than sitting exactly on it: at the
        // boundary itself the answer depends on how the runtime rounds the float progress, which
        // is a property of the platform, not of the rule.

        [Test]
        public void FromDayProgress_JustPastSix_IsSixInTheEvening()
        {
            TimeOfDay reading = At(600.5f / 720f);

            Assert.That(reading.Hour24, Is.EqualTo(18));
            Assert.That(reading.ToString(), Is.EqualTo("6:00 PM"));
        }

        [Test]
        public void FromDayProgress_JustBeforeSix_IsStillFiveFiftyFive()
        {
            TimeOfDay reading = At(599.5f / 720f);

            Assert.That(reading.ToString(), Is.EqualTo("5:55 PM"));
        }

        [Test]
        public void FromDayProgress_FourMinutesPastSix_StillReadsSix()
        {
            // The 5-minute step holds the reading until 6:05.
            TimeOfDay reading = At(604.5f / 720f);

            Assert.That(reading.ToString(), Is.EqualTo("6:00 PM"));
        }

        [Test]
        public void TotalMinutes_CountsFromMidnight()
        {
            Assert.That(new TimeOfDay(18, 30).TotalMinutes, Is.EqualTo(18 * 60 + 30));
        }

        [Test]
        public void ToString_Noon_ReadsTwelvePm()
        {
            Assert.That(new TimeOfDay(12, 0).ToString(), Is.EqualTo("12:00 PM"));
        }

        [Test]
        public void ToString_Midnight_ReadsTwelveAm()
        {
            Assert.That(new TimeOfDay(0, 5).ToString(), Is.EqualTo("12:05 AM"));
        }

        [Test]
        public void Ctor_HourOutsideTheDay_Throws()
        {
            Assert.That(() => new TimeOfDay(24, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new TimeOfDay(-1, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Ctor_MinuteOutsideTheHour_Throws()
        {
            Assert.That(() => new TimeOfDay(8, 60), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void FromDayProgress_DayThatEndsBeforeItStarts_Throws()
        {
            Assert.That(
                () => TimeOfDay.FromDayProgress(0.5f, 20, 8, Step),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void FromDayProgress_StepOutsideAnHour_Throws()
        {
            Assert.That(
                () => TimeOfDay.FromDayProgress(0.5f, StartHour, EndHour, 0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
