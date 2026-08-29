using Game.Data;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Shows an item's description. It is a modal: <see cref="ClickRouter"/> closes it with the next
    /// click anywhere instead of a dedicated button.
    /// </summary>
    public class ItemInspectPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private TMPro.TextMeshProUGUI _inspectText;
        [SerializeField] private ItemEventChannelSO _itemInspectedEventChannel;

        public bool IsOpen => _popup != null && _popup.activeSelf;

        private void OnEnable()
        {
            _itemInspectedEventChannel.Raised += Show;
        }

        private void OnDisable()
        {
            _itemInspectedEventChannel.Raised -= Show;
        }

        public void Show(ItemSO item)
        {
            _inspectText.text = item.Description;
            _popup.SetActive(true);
        }

        public void Hide()
        {
            _popup.SetActive(false);
        }
    }
}
