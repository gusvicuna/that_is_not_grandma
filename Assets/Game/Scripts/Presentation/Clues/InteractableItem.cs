using UnityEngine;
using Game.Events;
using Game.Data;

namespace Game.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ClueSO _clue;
        [SerializeField] private ItemSO _item;
        [SerializeField] private ClueCollectedEventChannelSO _clueCollectedEventChannel;
        [SerializeField] private ItemInspectedEventChannelSO _itemInspectedEventChannel;

        public void Interact()
        {
            if (_clue != null)
            {
                _clueCollectedEventChannel.Raise(_clue);
                _clue = null;
            }
            if (_item != null && _item.IsInspectable)
            {
                _itemInspectedEventChannel.Raise(_item);
            }
        }
    }
}
