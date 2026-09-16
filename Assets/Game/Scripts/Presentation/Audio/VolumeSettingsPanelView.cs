using UnityEngine;
using UnityEngine.UI;
using Game.Domain;

namespace Game.Presentation
{
    /// <summary>
    /// Three sliders and a mute toggle, in its own prefab so it drops into the pause menu unchanged
    /// once that screen exists. Ambience has no slider — it rides the master.
    /// </summary>
    public class VolumeSettingsPanelView : MonoBehaviour
    {
        [SerializeField] private VolumeController _volumeController;
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _muteToggle;

        private void OnEnable()
        {
            ShowSavedValues();

            _masterSlider.onValueChanged.AddListener(SetMasterVolume);
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            _sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            _muteToggle.onValueChanged.AddListener(SetMuted);
        }

        private void OnDisable()
        {
            _masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            _sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
            _muteToggle.onValueChanged.RemoveListener(SetMuted);
        }

        /// <summary>Without the WithoutNotify variants this would write the values straight back.</summary>
        private void ShowSavedValues()
        {
            _masterSlider.SetValueWithoutNotify(_volumeController.GetVolume(AudioBus.Master));
            _musicSlider.SetValueWithoutNotify(_volumeController.GetVolume(AudioBus.Music));
            _sfxSlider.SetValueWithoutNotify(_volumeController.GetVolume(AudioBus.Sfx));
            _muteToggle.SetIsOnWithoutNotify(_volumeController.IsMuted);
        }

        private void SetMasterVolume(float value)
        {
            _volumeController.SetVolume(AudioBus.Master, value);
        }

        private void SetMusicVolume(float value)
        {
            _volumeController.SetVolume(AudioBus.Music, value);
        }

        private void SetSfxVolume(float value)
        {
            _volumeController.SetVolume(AudioBus.Sfx, value);
        }

        private void SetMuted(bool muted)
        {
            _volumeController.SetMuted(muted);
        }
    }
}
