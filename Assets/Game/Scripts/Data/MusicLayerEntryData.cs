using System;
using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    [Serializable]
    public class MusicLayerEntryData
    {
        [SerializeField] private MusicLayerId _layer;
        [SerializeField] private AudioClip _clip;

        public MusicLayerId Layer => _layer;
        public AudioClip Clip => _clip;
    }
}
