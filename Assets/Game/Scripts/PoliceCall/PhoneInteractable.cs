using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class PhoneInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private PoliceCallController _policeCallController;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            _policeCallController.OnAvailabilityChanged += Sync;
            Sync();
        }

        private void OnDisable()
        {
            _policeCallController.OnAvailabilityChanged -= Sync;
        }

        public void Interact()
        {
            _policeCallController.RequestCall();
        }

        private void Sync()
        {
            bool available = _policeCallController.IsPhoneAvailable;

            _spriteRenderer.enabled = available;
            _collider.enabled = available;
        }
    }
}
