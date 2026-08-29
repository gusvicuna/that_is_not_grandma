using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// A hiding place, one per room. Clicking it only asks — the day ends on "yes", so nobody
    /// loses an afternoon to a misclick.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] private RoomId _room;
        [SerializeField] private RoomIdEventChannelSO _playerHid;
        [SerializeField] private DayNightCycle _dayNightCycle;
        [SerializeField] private HideConfirmView _hideConfirmView;

        [Tooltip("Hiding ends the day on the spot instead of waiting the clock out inside a wardrobe.")]
        [SerializeField] private bool _hidingEndsDayImmediately = true;

        public void Interact()
        {
            if (_hideConfirmView == null)
            {
                Hide();
                return;
            }
            _hideConfirmView.Ask(this);
        }

        /// <summary>Called by the confirmation panel. Cancelling calls nothing and costs nothing.</summary>
        public void Hide()
        {
            _playerHid.Raise(_room);
            if (_hidingEndsDayImmediately)
            {
                _dayNightCycle.EndDayNow();
            }
        }
    }
}
