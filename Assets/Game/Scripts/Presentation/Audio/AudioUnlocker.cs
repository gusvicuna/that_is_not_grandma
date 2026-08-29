using UnityEngine;
using UnityEngine.InputSystem;
using Game.Events;

namespace Game.Presentation
{
    /// <summary>
    /// Browsers refuse audio until the user interacts with the page, so the whole stack starts paused
    /// and waits for the first click. Nothing plays before this raises its channel.
    /// </summary>
    public class AudioUnlocker : MonoBehaviour
    {
        [SerializeField] private InputActionReference _clickAction;
        [SerializeField] private VoidEventChannelSO _audioUnlockedChannel;

        public bool IsUnlocked { get; private set; }

        private void Awake()
        {
            AudioListener.pause = true;
        }

        private void OnEnable()
        {
            if (IsUnlocked)
            {
                return;
            }
            _clickAction.action.performed += OnFirstGesture;
            _clickAction.action.Enable();
        }

        private void OnDisable()
        {
            // Unsubscribe but never Disable: ClickRouter shares this action and would go deaf.
            _clickAction.action.performed -= OnFirstGesture;
        }

        /// <summary>Also callable from a "click to start" button, if one ever exists.</summary>
        public void Unlock()
        {
            if (IsUnlocked)
            {
                return;
            }

            IsUnlocked = true;
            AudioListener.pause = false;
            _clickAction.action.performed -= OnFirstGesture;
            _audioUnlockedChannel.Raise();
        }

        private void OnFirstGesture(InputAction.CallbackContext context)
        {
            Unlock();
        }
    }
}
