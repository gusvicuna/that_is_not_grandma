using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    /// <summary>
    /// "Hide here?" — yes ends the day, no does nothing at all.
    /// This component lives on an object that stays active; only <see cref="_panel"/> is toggled,
    /// so its Awake always runs and the buttons are always wired.
    /// </summary>
    public class HideConfirmView : MonoBehaviour
    {
        [Tooltip("The panel object to show and hide. Must start disabled.")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private HidingSpot _pendingSpot;

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(Cancel);
            _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(Confirm);
            _cancelButton.onClick.RemoveListener(Cancel);
        }

        public void Ask(HidingSpot spot)
        {
            _pendingSpot = spot;
            _panel.SetActive(true);
        }

        public void Confirm()
        {
            HidingSpot spot = _pendingSpot;
            Close();
            if (spot != null)
            {
                spot.Hide();
            }
        }

        public void Cancel()
        {
            Close();
        }

        private void Close()
        {
            _pendingSpot = null;
            _panel.SetActive(false);
        }
    }
}
