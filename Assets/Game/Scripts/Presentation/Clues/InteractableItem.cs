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
        [SerializeField] private ClueEventChannelSO _clueCollectedEventChannel;
        [SerializeField] private ItemEventChannelSO _itemInspectedEventChannel;

        /// <summary>
        /// The item goes first on purpose: the inspect popup opens on the item's description and
        /// appends the clue's line as its second page, so the clue has to arrive with the popup
        /// already on screen.
        /// </summary>
        public void Interact()
        {
            if (_item != null && _item.IsInspectable)
            {
                _itemInspectedEventChannel.Raise(_item);
            }
            if (_clue != null)
            {
                _clueCollectedEventChannel.Raise(_clue);
                _clue = null;
            }
        }
    }
}
