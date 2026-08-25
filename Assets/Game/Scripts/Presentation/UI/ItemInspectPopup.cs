using Game.Data;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    public class ItemInspectPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private TMPro.TextMeshProUGUI _inspectText;
        [SerializeField] private ItemInspectedEventChannelSO _itemInspectedEventChannel;

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
