using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class MusicLayerMixerTests
    {
        private const float _tolerance = 0.001f;

        [Test]
        public void Ctor_NonPositiveRate_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MusicLayerMixer(0f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MusicLayerMixer(1f, -1f));
        }

        [Test]
        public void GetWeight_NewMixer_IsZeroForEveryLayer()
        {
            var mixer = new MusicLayerMixer(1f, 1f);

            foreach (MusicLayerId layer in Enum.GetValues(typeof(MusicLayerId)))
            {
                Assert.That(mixer.GetWeight(layer), Is.EqualTo(0f).Within(_tolerance),
                    $"layer {layer} does not start silent");
            }
        }

        [Test]
        public void Tick_FadesTowardTargetAtFadeInRate()
        {
            var mixer = new MusicLayerMixer(2f, 2f);
            mixer.SetTarget(MusicLayerId.Approach, 1f);

            mixer.Tick(0.25f);

            Assert.That(mixer.GetWeight(MusicLayerId.Approach), Is.EqualTo(0.5f).Within(_tolerance));
        }

        [Test]
        public void Tick_NeverOvershootsTarget()
        {
            var mixer = new MusicLayerMixer(2f, 2f);
            mixer.SetTarget(MusicLayerId.Approach, 1f);

            mixer.Tick(10f);

            Assert.That(mixer.GetWeight(MusicLayerId.Approach), Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void Tick_FadesOutAtFadeOutRate()
        {
            // Fades are asymmetric on purpose: tension arrives fast and leaves slowly.
            var mixer = new MusicLayerMixer(10f, 1f);
            mixer.SetTarget(MusicLayerId.Approach, 1f);
            mixer.SnapToTargets();

            mixer.SetTarget(MusicLayerId.Approach, 0f);
            mixer.Tick(0.5f);

            Assert.That(mixer.GetWeight(MusicLayerId.Approach), Is.EqualTo(0.5f).Within(_tolerance));
        }

        [Test]
        public void Tick_DoesNotMoveLayersAlreadyAtTarget()
        {
            var mixer = new MusicLayerMixer(1f, 1f);
            mixer.SetTarget(MusicLayerId.Bed, 1f);
            mixer.SnapToTargets();

            mixer.Tick(5f);

            Assert.That(mixer.GetWeight(MusicLayerId.Bed), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(mixer.GetWeight(MusicLayerId.Lie), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void Tick_NegativeDeltaTime_Throws()
        {
            var mixer = new MusicLayerMixer(1f, 1f);

            Assert.Throws<ArgumentOutOfRangeException>(() => mixer.Tick(-0.1f));
        }

        [Test]
        public void SetTarget_ClampsToUnitRange()
        {
            var mixer = new MusicLayerMixer(1f, 1f);

            mixer.SetTarget(MusicLayerId.Lie, 5f);
            mixer.SnapToTargets();
            Assert.That(mixer.GetWeight(MusicLayerId.Lie), Is.EqualTo(1f).Within(_tolerance));

            mixer.SetTarget(MusicLayerId.Lie, -5f);
            mixer.SnapToTargets();
            Assert.That(mixer.GetWeight(MusicLayerId.Lie), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void SnapToTargets_AppliesTargetsImmediately()
        {
            var mixer = new MusicLayerMixer(0.1f, 0.1f);
            mixer.SetTarget(MusicLayerId.Bed, 1f);
            mixer.SetTarget(MusicLayerId.Approach, 0.5f);

            mixer.SnapToTargets();

            Assert.That(mixer.GetWeight(MusicLayerId.Bed), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(mixer.GetWeight(MusicLayerId.Approach), Is.EqualTo(0.5f).Within(_tolerance));
        }

        [Test]
        public void Tick_ManySmallSteps_ReachesTargetExactly()
        {
            // A frame-by-frame fade must land on the target, not near it: a layer stuck at
            // 0.998 is a music bed that never fully returns to silence.
            var mixer = new MusicLayerMixer(1f, 1f);
            mixer.SetTarget(MusicLayerId.Approach, 1f);

            for (int i = 0; i < 100; i++)
            {
                mixer.Tick(0.016f);
            }

            Assert.That(mixer.GetWeight(MusicLayerId.Approach), Is.EqualTo(1f).Within(_tolerance));
        }
    }
}
