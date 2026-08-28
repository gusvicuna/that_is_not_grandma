using System;

namespace Game.Domain
{
    public class VolumeSettings
    {
        public float Master { get; private set; }
        public float Music { get; private set; }
        public float Sfx { get; private set; }
        public float Ambience { get; private set; }

        public event Action Changed;

        // Mute is a flag, not a value: the sliders keep whatever the player set, and only
        // EffectiveLinear goes silent. Zeroing the values instead loses them on unmute.
        private bool _isMuted;

        public VolumeSettings(float master = 1f, float music = 1f, float sfx = 1f, float ambience = 1f)
        {
            //Every value clamped between 0 and 1 without using mathf
            Master = master < 0f ? 0f : (master > 1f ? 1f : master);
            Music = music < 0f ? 0f : (music > 1f ? 1f : music);
            Sfx = sfx < 0f ? 0f : (sfx > 1f ? 1f : sfx);
            Ambience = ambience < 0f ? 0f : (ambience > 1f ? 1f : ambience);
        }

        public float Get(AudioBus bus)
        {
            return bus switch
            {
                AudioBus.Master => Master,
                AudioBus.Music => Music,
                AudioBus.Sfx => Sfx,
                AudioBus.Ambience => Ambience,
                _ => throw new ArgumentOutOfRangeException(nameof(bus), bus, null)
            };
        }

        public void Set(AudioBus bus, float value)
        {
            value = value < 0f ? 0f : (value > 1f ? 1f : value);
            if (!(value == Get(bus)))
            {
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
        }

        public bool IsMuted => _isMuted;

        public void SetMuted(bool muted)
        {
            if (muted == _isMuted)
            {
                return;
            }
            _isMuted = muted;
            Changed?.Invoke();
        }

        public float EffectiveLinear(AudioBus bus)
        {
            if (IsMuted)
            {
                return 0f;
            }
            if (bus == AudioBus.Master)
            {
                return Master;
            }
            else
            {
                return Get(bus) * Master;
            }
        }
    }
}
