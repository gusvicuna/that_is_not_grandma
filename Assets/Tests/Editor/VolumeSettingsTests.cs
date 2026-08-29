using System;
using NUnit.Framework;
using Game.Domain;

namespace Game.Tests.Editor
{
    public class VolumeSettingsTests
    {
        private const float _tolerance = 0.0001f;

        private VolumeSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new VolumeSettings();
        }

        [Test]
        public void Ctor_Default_IsFullVolumeAndUnmuted()
        {
            Assert.That(_settings.Get(AudioBus.Master), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_settings.Get(AudioBus.Music), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_settings.Get(AudioBus.Sfx), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_settings.Get(AudioBus.Ambience), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(_settings.IsMuted, Is.False);
        }

        [Test]
        public void Ctor_OutOfRangeValues_AreClamped()
        {
            var settings = new VolumeSettings(2f, -1f, 0.5f, 3f);

            Assert.That(settings.Get(AudioBus.Master), Is.EqualTo(1f).Within(_tolerance));
            Assert.That(settings.Get(AudioBus.Music), Is.EqualTo(0f).Within(_tolerance));
            Assert.That(settings.Get(AudioBus.Sfx), Is.EqualTo(0.5f).Within(_tolerance));
            Assert.That(settings.Get(AudioBus.Ambience), Is.EqualTo(1f).Within(_tolerance));
        }

        [Test]
        public void Set_ClampsToUnitRange()
        {
            _settings.Set(AudioBus.Music, 4f);
            Assert.That(_settings.Get(AudioBus.Music), Is.EqualTo(1f).Within(_tolerance));

            _settings.Set(AudioBus.Music, -4f);
            Assert.That(_settings.Get(AudioBus.Music), Is.EqualTo(0f).Within(_tolerance));
        }

        [Test]
        public void Set_UndefinedBus_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _settings.Set((AudioBus)99, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => _settings.Get((AudioBus)99));
        }

        [Test]
        public void EffectiveLinear_BusIsScaledByMaster()
        {
            _settings.Set(AudioBus.Master, 0.5f);
            _settings.Set(AudioBus.Music, 0.8f);

            Assert.That(_settings.EffectiveLinear(AudioBus.Music), Is.EqualTo(0.4f).Within(_tolerance));
        }

        [Test]
        public void EffectiveLinear_Master_IsNotScaledByItself()
        {
            _settings.Set(AudioBus.Master, 0.5f);

            Assert.That(_settings.EffectiveLinear(AudioBus.Master), Is.EqualTo(0.5f).Within(_tolerance));
        }

        [Test]
        public void EffectiveLinear_WhenMuted_IsZeroForEveryBus()
        {
            _settings.SetMuted(true);

            foreach (AudioBus bus in Enum.GetValues(typeof(AudioBus)))
            {
                Assert.That(_settings.EffectiveLinear(bus), Is.EqualTo(0f).Within(_tolerance),
                    $"bus {bus} is not silent while muted");
            }
        }

        [Test]
        public void SetMuted_DoesNotChangeStoredVolumes()
        {
            _settings.Set(AudioBus.Sfx, 0.6f);
            _settings.SetMuted(true);
            _settings.SetMuted(false);

            Assert.That(_settings.Get(AudioBus.Sfx), Is.EqualTo(0.6f).Within(_tolerance));
            Assert.That(_settings.EffectiveLinear(AudioBus.Sfx), Is.EqualTo(0.6f).Within(_tolerance));
        }

        [Test]
        public void Changed_RaisedOnlyWhenValueActuallyChanges()
        {
            int raised = 0;
            _settings.Changed += () => raised++;

            _settings.Set(AudioBus.Music, 0.5f);
            Assert.That(raised, Is.EqualTo(1));

            _settings.Set(AudioBus.Music, 0.5f);
            Assert.That(raised, Is.EqualTo(1), "setting the same value must not raise Changed");

            _settings.SetMuted(false);
            Assert.That(raised, Is.EqualTo(1), "muting to the current state must not raise Changed");

            _settings.SetMuted(true);
            Assert.That(raised, Is.EqualTo(2));
        }
    }
}
