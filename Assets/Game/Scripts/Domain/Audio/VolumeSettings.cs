using System;

namespace Game.Domain
{
    /// <summary>
    /// Per-bus linear volume plus a global mute. Mute is a flag, not a value: the sliders keep what
    /// the player set, and only EffectiveLinear goes silent.
    /// </summary>
    public class VolumeSettings
    {
        public float Master { get; private set; }
        public float Music { get; private set; }
        public float Sfx { get; private set; }
        public float Ambience { get; private set; }

        public event Action Changed;

        private bool _isMuted;

        public VolumeSettings(float master = 1f, float music = 1f, float sfx = 1f, float ambience = 1f)
        {
            Master = Clamp01(master);
            Music = Clamp01(music);
            Sfx = Clamp01(sfx);
            Ambience = Clamp01(ambience);
        }

        public bool IsMuted => _isMuted;

        public float Get(AudioBus bus) => bus switch
        {
            AudioBus.Master => Master,
            AudioBus.Music => Music,
            AudioBus.Sfx => Sfx,
            AudioBus.Ambience => Ambience,
            _ => throw new ArgumentOutOfRangeException(nameof(bus), bus, null)
        };

        public void Set(AudioBus bus, float value)
        {
            value = Clamp01(value);
            if (value == Get(bus))
            {
                return;
            }

            switch (bus)
            {
                case AudioBus.Master: Master = value; break;
                case AudioBus.Music: Music = value; break;
                case AudioBus.Sfx: Sfx = value; break;
                case AudioBus.Ambience: Ambience = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(bus), bus, null);
            }

            Changed?.Invoke();
        }

        public void SetMuted(bool muted)
        {
            if (muted == _isMuted)
            {
                return;
            }

            _isMuted = muted;
            Changed?.Invoke();
        }

        /// <summary>What the mixer actually receives: every bus rides the master, and mute wins.</summary>
        public float EffectiveLinear(AudioBus bus)
        {
            if (_isMuted)
            {
                return 0f;
            }
            return bus == AudioBus.Master ? Master : Get(bus) * Master;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
