using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using Game.Data;
using Game.Domain;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// The single listener of the SFX channel. Pools its AudioSources so a one-shot never allocates,
    /// and picks variations through NoRepeatPicker so a repeated cue doesn't sound like a machine gun.
    /// </summary>
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueEventChannelSO _sfxRequestedChannel;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private AudioMixerGroup _ambienceGroup;
        [SerializeField] private int _poolSize = 8;

        private ObjectPool<AudioSource> _sourcePool;
        private readonly Dictionary<AudioCueSO, NoRepeatPicker> _pickersByCue = new();
        private readonly System.Random _random = new();

        private void Awake()
        {
            _sourcePool = new ObjectPool<AudioSource>(
                CreateSource,
                source => source.gameObject.SetActive(true),
                source => { source.Stop(); source.gameObject.SetActive(false); },
                source => Destroy(source.gameObject),
                collectionCheck: false,
                defaultCapacity: _poolSize,
                maxSize: _poolSize);

            Prewarm();
        }

        private void OnEnable()
        {
            _sfxRequestedChannel.Raised += Play;
        }

        private void OnDisable()
        {
            _sfxRequestedChannel.Raised -= Play;
        }

        public void Play(AudioCueSO cue)
        {
            // An empty cue is silence, not an error: placeholder slots stay unfilled for days.
            if (cue == null || !cue.HasClips)
            {
                return;
            }

            AudioClip clip = cue.GetClip(PickerFor(cue).Next());
            if (clip == null)
            {
                return;
            }

            AudioSource source = _sourcePool.Get();
            source.outputAudioMixerGroup = GroupFor(cue.Bus);
            source.pitch = cue.RandomPitch();
            source.volume = 1f;
            source.PlayOneShot(clip, cue.RandomVolume());

            StartCoroutine(ReleaseWhenFinished(source, clip.length / Mathf.Max(0.01f, source.pitch)));
        }

        private AudioMixerGroup GroupFor(AudioBus bus) => bus switch
        {
            AudioBus.Ambience => _ambienceGroup,
            _ => _sfxGroup
        };

        private NoRepeatPicker PickerFor(AudioCueSO cue)
        {
            if (!_pickersByCue.TryGetValue(cue, out NoRepeatPicker picker))
            {
                picker = new NoRepeatPicker(cue.ClipCount, _random);
                _pickersByCue.Add(cue, picker);
            }
            return picker;
        }

        private IEnumerator ReleaseWhenFinished(AudioSource source, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _sourcePool.Release(source);
        }

        /// <summary>Allocates every source up front so gameplay never pays for one.</summary>
        private void Prewarm()
        {
            var sources = new AudioSource[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                sources[i] = _sourcePool.Get();
            }
            for (int i = 0; i < _poolSize; i++)
            {
                _sourcePool.Release(sources[i]);
            }
        }

        private AudioSource CreateSource()
        {
            var child = new GameObject("SfxSource");
            child.transform.SetParent(transform, false);

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = _sfxGroup;
            return source;
        }
    }
}
