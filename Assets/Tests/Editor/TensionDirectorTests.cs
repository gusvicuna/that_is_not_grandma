using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class TensionDirectorTests
    {
        private const float _tolerance = 0.001f;
        private const float _lieMotifSeconds = 2f;

        private TensionDirector _director;

        [SetUp]
        public void SetUp()
        {
            _director = new TensionDirector(_lieMotifSeconds);
        }

        [Test]
        public void Ctor_NonPositiveLieDuration_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TensionDirector(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TensionDirector(-1f));
        }

        [Test]
        public void Level_NewDirector_IsCalm()
        {
            Assert.That(_director.Level, Is.EqualTo(TensionLevel.Calm));
            Assert.That(_director.IsLieMotifActive, Is.False);
        }

        [Test]
        public void GetTarget_Bed_IsAlwaysFull()
        {
            foreach (TensionLevel level in Enum.GetValues(typeof(TensionLevel)))
            {
                _director.SetTension(level);

                Assert.That(_director.GetTarget(MusicLayerId.Bed), Is.EqualTo(1f).Within(_tolerance),
                    $"the bed layer dropped at tension level {level}");
            }
        }

        [Test]
        public void GetTarget_Approach_ScalesWithTensionLevel()
        {
            _director.SetTension(TensionLevel.Calm);
            Assert.That(_director.GetTarget(MusicLayerId.Approach), Is.EqualTo(0f).Within(_tolerance));

            _director.SetTension(TensionLevel.Uneasy);
            Assert.That(_director.GetTarget(MusicLayerId.Approach), Is.EqualTo(0.5f).Within(_tolerance));

            _director.SetTension(TensionLevel.Alert);
            Assert.That(_director.GetTarget(MusicLayerId.Approach), Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void SetTension_UndefinedLevel_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _director.SetTension((TensionLevel)99));
        }

        [Test]
        public void GetTarget_UndefinedLayer_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _director.GetTarget((MusicLayerId)99));
        }

        [Test]
        public void PulseLieMotif_ActivatesLieLayer()
        {
            _director.PulseLieMotif();

            Assert.That(_director.IsLieMotifActive, Is.True);
            Assert.That(_director.GetTarget(MusicLayerId.Lie), Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void Tick_LieMotifExpiresAfterDuration()
        {
            _director.PulseLieMotif();

            _director.Tick(_lieMotifSeconds);

            Assert.That(_director.IsLieMotifActive, Is.False);
            Assert.That(_director.GetTarget(MusicLayerId.Lie), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void Tick_LieMotifStaysActiveBeforeDuration()
        {
            _director.PulseLieMotif();

            _director.Tick(_lieMotifSeconds - 0.1f);

            Assert.That(_director.IsLieMotifActive, Is.True);
        }

        [Test]
        public void PulseLieMotif_WhileActive_RefreshesFullDuration()
        {
            // Two marked conversations back to back must not cut the motif short.
            _director.PulseLieMotif();
            _director.Tick(1.5f);

            _director.PulseLieMotif();
            _director.Tick(1.5f);

            Assert.That(_director.IsLieMotifActive, Is.True);
        }

        [Test]
        public void Tick_AfterExpiry_KeepsLieLayerSilent()
        {
            _director.PulseLieMotif();
            _director.Tick(_lieMotifSeconds * 5f);

            _director.Tick(1f);

            Assert.That(_director.IsLieMotifActive, Is.False);
            Assert.That(_director.GetTarget(MusicLayerId.Lie), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void Tick_NegativeDeltaTime_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _director.Tick(-0.1f));
        }
    }
}
