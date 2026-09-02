using Game.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Presentation
{
    public class NpcVisual : MonoBehaviour
    {
        [SerializeField] private NpcSO _npc;
        [FormerlySerializedAs("_worldSprite")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (_npc.WorldSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _npc.WorldSprite;
            }
        }
    }
}
