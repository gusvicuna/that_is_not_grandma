using System;
using UnityEngine;
using UnityEngine.Serialization;
using Game.Data;
using Game.Domain;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// One looping source per music layer. They all start in the same call on unlock and are never
    /// stopped again — only their volume moves. That is the whole sync strategy on Web, where
    /// PlayScheduled is unreliable.
    /// </summary>
    public class MusicLayerPlayer : MonoBehaviour
    {
        [Serializable]
        private class LayerSource
        {
            [SerializeField] private MusicLayerId _layer;
            [SerializeField] private AudioSource _source;

            public MusicLayerId Layer => _layer;
            public AudioSource Source => _source;
        }

        [SerializeField] private MusicLayerSetSO _layerSet;
        [FormerlySerializedAs("_sources")]
        [SerializeField] private LayerSource[] _layerSources;
        [SerializeField] private VoidEventChannelSO _audioUnlockedChannel;

        private void Awake()
        {
            foreach (LayerSource layerSource in _layerSources)
            {
                AudioSource source = layerSource.Source;
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f;
                source.volume = 0f;

                if (_layerSet.TryGetClip(layerSource.Layer, out AudioClip clip))
                {
                    source.clip = clip;
                }
            }
        }

        private void OnEnable()
        {
            _audioUnlockedChannel.Raised += StartEveryLayer;
        }

        private void OnDisable()
        {
            _audioUnlockedChannel.Raised -= StartEveryLayer;
        }

        public void ApplyWeights(MusicLayerMixer mixer)
        {
            foreach (LayerSource layerSource in _layerSources)
            {
                if (layerSource.Source.clip != null)
                {
                    layerSource.Source.volume = mixer.GetWeight(layerSource.Layer);
                }
            }
        }

        private void StartEveryLayer()
        {
            // Same frame for all of them: no PlayScheduled, no dspTime, no drift.
            foreach (LayerSource layerSource in _layerSources)
            {
                AudioSource source = layerSource.Source;
                if (source.clip != null && !source.isPlaying)
                {
                    source.Play();
                }
            }
        }
    }
}
