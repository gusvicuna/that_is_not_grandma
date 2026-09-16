using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class VolumeCurveTests
    {
        private const float _tolerance = 0.01f;

        [Test]
        public void ToDecibels_FullVolume_IsZeroDecibels()
        {
            Assert.That(VolumeCurve.ToDecibels(1f), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void ToDecibels_Zero_IsMinDecibels()
        {
            Assert.That(VolumeCurve.ToDecibels(0f), Is.EqualTo(VolumeCurve.MinDecibels));
        }

        [Test]
        public void ToDecibels_Half_IsAboutMinusSixDecibels()
        {
            // Half the linear amplitude is ~-6.02 dB. A slider at 50% that reads -40 dB
            // is the classic "why is my volume control useless at the bottom" bug.
            Assert.That(VolumeCurve.ToDecibels(0.5f), Is.EqualTo(-6.02f).Within(_tolerance));
        }

        [Test]
        public void ToDecibels_OutOfRange_IsClamped()
        {
            Assert.That(VolumeCurve.ToDecibels(2f), Is.EqualTo(0f).Within(_tolerance));
            Assert.That(VolumeCurve.ToDecibels(-1f), Is.EqualTo(VolumeCurve.MinDecibels));
        }

        [Test]
        public void FromDecibels_RoundTrips_ToOriginalLinear()
        {
            float[] linearValues = { 0.25f, 0.5f, 0.75f, 1f };

            foreach (float linear in linearValues)
            {
                float roundTripped = VolumeCurve.FromDecibels(VolumeCurve.ToDecibels(linear));

                Assert.That(roundTripped, Is.EqualTo(linear).Within(0.001f),
                    $"round trip failed for linear value {linear}");
            }
        }

        [Test]
        public void FromDecibels_MinDecibels_IsSilence()
        {
            Assert.That(VolumeCurve.FromDecibels(VolumeCurve.MinDecibels), Is.EqualTo(0f).Within(0.001f));
        }
    }
}
