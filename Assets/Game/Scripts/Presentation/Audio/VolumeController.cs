using System;
using UnityEngine;
using UnityEngine.Audio;
using Game.Domain;

namespace Game.Presentation
{
    /// <summary>
    /// Owns the run's VolumeSettings, pushes them into the AudioMixer as decibels and persists them.
    /// Volume is the only mixer property the Web platform supports, which is all this touches.
    /// </summary>
    public class VolumeController : MonoBehaviour
    {
        private const string _masterKey = "audio.master";
        private const string _musicKey = "audio.music";
        private const string _sfxKey = "audio.sfx";
        private const string _ambienceKey = "audio.ambience";
        private const string _mutedKey = "audio.muted";

        private static readonly AudioBus[] _buses =
        {
            AudioBus.Master, AudioBus.Music, AudioBus.Sfx, AudioBus.Ambience
        };

        [SerializeField] private AudioMixer _mixer;

        [Header("Exposed mixer parameters")]
        [SerializeField] private string _masterParameter = "MasterVolume";
        [SerializeField] private string _musicParameter = "MusicVolume";
        [SerializeField] private string _sfxParameter = "SfxVolume";
        [SerializeField] private string _ambienceParameter = "AmbienceVolume";

        private VolumeSettings _settings;

        /// <summary>Raised after a change reaches the mixer, so views can refresh.</summary>
        public event Action Changed;

        public bool IsMuted => _settings.IsMuted;

        private void Awake()
        {
            _settings = LoadSettings();
            _settings.Changed += OnSettingsChanged;

            WarnAboutMissingParameters();
            ApplyToMixer();
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.Changed -= OnSettingsChanged;
            }
        }

        public float GetVolume(AudioBus bus)
        {
            return _settings.Get(bus);
        }

        public void SetVolume(AudioBus bus, float value)
        {
            _settings.Set(bus, value);
        }

        public void SetMuted(bool muted)
        {
            _settings.SetMuted(muted);
        }

        private VolumeSettings LoadSettings()
        {
            var settings = new VolumeSettings(
                PlayerPrefs.GetFloat(_masterKey, 1f),
                PlayerPrefs.GetFloat(_musicKey, 1f),
                PlayerPrefs.GetFloat(_sfxKey, 1f),
                PlayerPrefs.GetFloat(_ambienceKey, 1f));

            settings.SetMuted(PlayerPrefs.GetInt(_mutedKey, 0) == 1);
            return settings;
        }

        private void OnSettingsChanged()
        {
            ApplyToMixer();
            Save();
            Changed?.Invoke();
        }

        private void ApplyToMixer()
        {
            foreach (AudioBus bus in _buses)
            {
                _mixer.SetFloat(ParameterFor(bus), VolumeCurve.ToDecibels(_settings.EffectiveLinear(bus)));
            }
        }

        private string ParameterFor(AudioBus bus) => bus switch
        {
            AudioBus.Master => _masterParameter,
            AudioBus.Music => _musicParameter,
            AudioBus.Sfx => _sfxParameter,
            AudioBus.Ambience => _ambienceParameter,
            _ => throw new ArgumentOutOfRangeException(nameof(bus), bus, null)
        };

        /// <summary>
        /// Checked once, not per change: a name that resolves at startup resolves forever, and
        /// warning on every slider frame would bury the message. A typo here silences a whole bus
        /// and reads like a code bug.
        /// </summary>
        private void WarnAboutMissingParameters()
        {
            foreach (AudioBus bus in _buses)
            {
                string parameter = ParameterFor(bus);
                if (!_mixer.GetFloat(parameter, out float _))
                {
                    Debug.LogWarning($"Mixer parameter '{parameter}' not found — is it exposed?", this);
                }
            }
        }

        private void Save()
        {
            PlayerPrefs.SetFloat(_masterKey, _settings.Get(AudioBus.Master));
            PlayerPrefs.SetFloat(_musicKey, _settings.Get(AudioBus.Music));
            PlayerPrefs.SetFloat(_sfxKey, _settings.Get(AudioBus.Sfx));
            PlayerPrefs.SetFloat(_ambienceKey, _settings.Get(AudioBus.Ambience));
            PlayerPrefs.SetInt(_mutedKey, _settings.IsMuted ? 1 : 0);
            // On Web this is what actually reaches IndexedDB; a tab closed without it loses the setting.
            PlayerPrefs.Save();
        }
    }
}
