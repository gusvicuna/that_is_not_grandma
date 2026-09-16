using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    /// <summary>
    /// Which ambience loop belongs to each room, plus the night one. An unmapped room is a legal
    /// state and means silence.
    /// </summary>
    [CreateAssetMenu(fileName = "AMB_House", menuName = "Game/Audio/Ambience Bank")]
    public class AmbienceBankSO : ScriptableObject
    {
        [SerializeField] private AmbienceEntryData[] _entries;

        [Header("Night")]
        [Tooltip("The night is not a room, so it is not in the map above.")]
        [SerializeField] private AudioClip _nightAmbience;
        [SerializeField][Range(0f, 1f)] private float _nightVolume = 1f;

        public AudioClip NightAmbience => _nightAmbience;
        public float NightVolume => _nightVolume;

        public bool TryGet(RoomId room, out AudioClip clip, out float volume)
        {
            foreach (AmbienceEntryData entry in _entries)
            {
                if (entry.Room == room && entry.Clip != null)
                {
                    clip = entry.Clip;
                    volume = entry.Volume;
                    return true;
                }
            }

            clip = null;
            volume = 0f;
            return false;
        }
    }
}
