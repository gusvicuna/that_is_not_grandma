using UnityEngine;
using Game.Data;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// One cue, one channel, one job: the UI's way into the SFX stack without every widget knowing
    /// what a cue is. <see cref="Play"/> takes no arguments so it shows up in the Button.onClick
    /// dropdown — sonifying a button is inspector work, not a code change.
    /// </summary>
    public class UiSfxEmitter : MonoBehaviour
    {
        [SerializeField] private AudioCueSO _cue;
        [SerializeField] private AudioCueEventChannelSO _sfxRequestedChannel;

        /// <summary>
        /// An unwired emitter is silence, not an exception: a panel dropped into a test scene
        /// without the audio stack must still work.
        /// </summary>
        public void Play()
        {
            if (_cue == null || _sfxRequestedChannel == null)
            {
                return;
            }
            _sfxRequestedChannel.Raise(_cue);
        }
    }
}
