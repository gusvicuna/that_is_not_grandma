using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    /// <summary>
    /// The music stems. AUTHORING CONTRACT: every layer is the same length and tempo, mixed to be
    /// heard together. They start in one call and never stop — only their volume moves.
    /// </summary>
    [CreateAssetMenu(fileName = "MUS_Main", menuName = "Game/Audio/Music Layer Set")]
    public class MusicLayerSetSO : ScriptableObject
    {
        [SerializeField] private MusicLayerEntryData[] _layers;

        public bool TryGetClip(MusicLayerId layer, out AudioClip clip)
        {
            foreach (MusicLayerEntryData entry in _layers)
            {
                if (entry.Layer == layer && entry.Clip != null)
                {
                    clip = entry.Clip;
                    return true;
                }
            }

            clip = null;
            return false;
        }
    }
}
