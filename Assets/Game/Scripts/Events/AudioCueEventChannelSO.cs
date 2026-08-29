using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_AudioCue", menuName = "Game/Events/Audio Cue")]
    public class AudioCueEventChannelSO : EventChannelSO<AudioCueSO> { }
}
