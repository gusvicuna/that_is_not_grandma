using System.Collections.Generic;
using Game.Data;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Shows an item's description, and then — when that item also handed over a clue — the clue's
    /// line as a second page: first what the protagonist sees, then what she makes of it.
    ///
    /// It is a modal: <see cref="ClickRouter"/> drives it with the next click anywhere instead of a
    /// dedicated button, advancing to the next page until there are none left.
    ///
    /// The second page is deliberately only queued while the popup is already open. The clue
    /// channel also fires when an NPC hands a clue back in an exchange, and a conclusion popping up
    /// over the exchange panel would be worse than one that never appears.
    /// </summary>
    public class ItemInspectPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private TMPro.TextMeshProUGUI _inspectText;
        [SerializeField] private ItemEventChannelSO _itemInspectedEventChannel;

        [Tooltip("Same channel the notebook listens to. The clue's long text becomes the popup's second page.")]
        [SerializeField] private ClueEventChannelSO _clueCollectedEventChannel;

        private readonly Queue<string> _pendingPages = new();

        public bool IsOpen => _popup != null && _popup.activeSelf;

        private void OnEnable()
        {
            _itemInspectedEventChannel.Raised += Show;
            if (_clueCollectedEventChannel != null)
            {
                _clueCollectedEventChannel.Raised += QueueConclusion;
            }
        }

        private void OnDisable()
        {
            _itemInspectedEventChannel.Raised -= Show;
            if (_clueCollectedEventChannel != null)
            {
                _clueCollectedEventChannel.Raised -= QueueConclusion;
            }
        }

        public void Show(ItemSO item)
        {
            _pendingPages.Clear();
            _inspectText.text = item.Description;
            _popup.SetActive(true);
        }

        /// <summary>
        /// Adds the clue's line after the item's. Ignored unless the popup is already showing the
        /// item that carried the clue — see the note on the class.
        /// </summary>
        public void QueueConclusion(ClueSO clue)
        {
            if (!IsOpen || clue == null || string.IsNullOrWhiteSpace(clue.Text))
            {
                return;
            }
            _pendingPages.Enqueue(clue.Text);
        }

        /// <summary>
        /// One click forward: the next page, or the end of the popup.
        /// </summary>
        public void Advance()
        {
            if (_pendingPages.Count > 0)
            {
                _inspectText.text = _pendingPages.Dequeue();
                return;
            }
            Hide();
        }

        public void Hide()
        {
            _pendingPages.Clear();
            _popup.SetActive(false);
        }
    }
}
