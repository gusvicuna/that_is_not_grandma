using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class CrossfaderTests
    {
        private const float _tolerance = 0.001f;
        private const float _duration = 2f;

        private const string _kitchen = "amb_kitchen";
        private const string _bedroom = "amb_bedroom";
        private const string _bathroom = "amb_bathroom";

        private Crossfader _crossfader;

        [SetUp]
        public void SetUp()
        {
            _crossfader = new Crossfader(_duration);
        }

        [Test]
        public void Ctor_NonPositiveDuration_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Crossfader(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Crossfader(-1f));
        }

        [Test]
        public void NewCrossfader_IsSilentAndIdle()
        {
            Assert.That(_crossfader.IncomingTrackId, Is.Null);
            Assert.That(_crossfader.OutgoingTrackId, Is.Null);
            Assert.That(_crossfader.IsFading, Is.False);
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void To_FirstTrack_FadesItIn()
        {
            _crossfader.To(_kitchen);

            Assert.That(_crossfader.IncomingTrackId, Is.EqualTo(_kitchen));
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(0f).Within(_tolerance));
            Assert.That(_crossfader.IsFading, Is.True);

            _crossfader.Tick(_duration * 0.5f);
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(0.5f).Within(_tolerance));

            _crossfader.Tick(_duration * 0.5f);
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_crossfader.IsFading, Is.False);
        }

        [Test]
        public void Weights_AlwaysSumToOne()
        {
            _crossfader.To(_kitchen);
            AssertWeightsSumToOne();

            _crossfader.Tick(_duration * 0.3f);
            AssertWeightsSumToOne();

            _crossfader.To(_bedroom);
            AssertWeightsSumToOne();

            _crossfader.Tick(_duration * 0.7f);
            AssertWeightsSumToOne();

            _crossfader.Tick(_duration * 5f);
            AssertWeightsSumToOne();
        }

        [Test]
        public void To_SameTrackMidFade_DoesNotRestartFade()
        {
            // Re-entering the room you are already fading into must not rewind the fade,
            // or a player pacing between two rooms never hears either ambience.
            _crossfader.To(_kitchen);
            _crossfader.Tick(_duration * 0.5f);

            _crossfader.To(_kitchen);

            Assert.That(_crossfader.IncomingTrackId, Is.EqualTo(_kitchen));
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(0.5f).Within(_tolerance));
        }

        [Test]
        public void To_SameTrackAfterFadeCompleted_DoesNothing()
        {
            _crossfader.To(_kitchen);
            _crossfader.Tick(_duration);

            _crossfader.To(_kitchen);

            Assert.That(_crossfader.IsFading, Is.False);
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_crossfader.OutgoingTrackId, Is.Null);
        }

        [Test]
        public void To_NewTrackMidFade_ResumesFromCurrentWeight()
        {
            // The invariant: the track that was fading in keeps the exact level it already
            // had, now as the outgoing one. No jump, no pop, no stacked ambiences.
            _crossfader.To(_kitchen);
            _crossfader.Tick(_duration * 0.5f);

            _crossfader.To(_bedroom);

            Assert.That(_crossfader.IncomingTrackId, Is.EqualTo(_bedroom));
            Assert.That(_crossfader.OutgoingTrackId, Is.EqualTo(_kitchen));
            Assert.That(_crossfader.OutgoingWeight, Is.EqualTo(0.5f).Within(_tolerance),
                "the previously incoming track changed level across the swap");
        }

        [Test]
        public void Tick_CompletesFadeAndClearsOutgoingTrack()
        {
            _crossfader.To(_kitchen);
            _crossfader.Tick(_duration);
            _crossfader.To(_bathroom);

            _crossfader.Tick(_duration);

            Assert.That(_crossfader.IncomingTrackId, Is.EqualTo(_bathroom));
            Assert.That(_crossfader.OutgoingTrackId, Is.Null);
            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_crossfader.IsFading, Is.False);
        }

        [Test]
        public void Tick_PastDuration_DoesNotOvershoot()
        {
            _crossfader.To(_kitchen);

            _crossfader.Tick(_duration * 10f);

            Assert.That(_crossfader.IncomingWeight, Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void To_Null_FadesToSilence()
        {
            // An unmapped room is a legal state: fade out, don't keep the last room playing.
            _crossfader.To(_kitchen);
            _crossfader.Tick(_duration);

            _crossfader.To(null);

            Assert.That(_crossfader.IncomingTrackId, Is.Null);
            Assert.That(_crossfader.OutgoingTrackId, Is.EqualTo(_kitchen));

            _crossfader.Tick(_duration);

            Assert.That(_crossfader.OutgoingWeight, Is.EqualTo(0f).Within(_tolerance));
            Assert.That(_crossfader.IsFading, Is.False);
        }

        [Test]
        public void Tick_NegativeDeltaTime_Throws()
        {
            _crossfader.To(_kitchen);

            Assert.Throws<ArgumentOutOfRangeException>(() => _crossfader.Tick(-0.1f));
        }

        private void AssertWeightsSumToOne()
        {
            Assert.That(_crossfader.IncomingWeight + _crossfader.OutgoingWeight,
                Is.EqualTo(1f).Within(_tolerance),
                "incoming and outgoing weights must always sum to 1");
        }
    }
}
