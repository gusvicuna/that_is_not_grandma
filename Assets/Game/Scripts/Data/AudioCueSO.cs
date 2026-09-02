using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    /// <summary>
    /// One sound event: its clip variations and the ranges they are humanized within.
    /// An empty cue is silence, never an exception.
    /// </summary>
    [CreateAssetMenu(fileName = "SFX_Cue", menuName = "Game/Audio/Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private AudioBus _bus = AudioBus.Sfx;

        [Header("Randomization")]
        [SerializeField][Range(0f, 1f)] private float _volumeMin = 1f;
        [SerializeField][Range(0f, 1f)] private float _volumeMax = 1f;
        // The range floor is 0.5 because Web only supports positive pitch.
        [SerializeField][Range(0.5f, 2f)] private float _pitchMin = 1f;
        [SerializeField][Range(0.5f, 2f)] private float _pitchMax = 1f;

        public AudioBus Bus => _bus;
        public bool HasClips => _clips != null && _clips.Length > 0;
        public int ClipCount => _clips == null ? 0 : _clips.Length;

        public AudioClip GetClip(int index)
        {
            return _clips[index];
        }

        public float RandomVolume()
        {
            return Random.Range(_volumeMin, _volumeMax);
        }

        public float RandomPitch()
        {
            return Random.Range(_pitchMin, _pitchMax);
        }

        private void OnValidate()
        {
            _volumeMax = Mathf.Max(_volumeMax, _volumeMin);
            _pitchMax = Mathf.Max(_pitchMax, _pitchMin);
        }
    }
}
