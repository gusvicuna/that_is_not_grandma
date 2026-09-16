using System;
using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    [Serializable]
    public class AmbienceEntryData
    {
        [SerializeField] private RoomId _room;
        [SerializeField] private AudioClip _clip;
        [SerializeField][Range(0f, 1f)] private float _volume = 1f;

        public RoomId Room => _room;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }
}
