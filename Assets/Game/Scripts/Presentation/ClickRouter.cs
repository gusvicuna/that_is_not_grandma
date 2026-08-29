using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Presentation
{
    public class ClickRouter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionReference _clickAction;
        [SerializeField] private InputActionReference _pointAction;
        [SerializeField] private DialogueController _dialogueController;
        [SerializeField] private ExchangeController _exchangeController;

        private void OnEnable()
        {
            _clickAction.action.performed += OnClick;
            _clickAction.action.Enable();
            _pointAction.action.Enable();
        }

        private void OnDisable()
        {
            _clickAction.action.performed -= OnClick;
            _clickAction.action.Disable();
            _pointAction.action.Disable();
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (_dialogueController.IsDialogueActive)
            {
                // Choice nodes are advanced by their option buttons, not by clicking the world
                if (!_dialogueController.CurrentNodeHasOptions)
                {
                    _dialogueController.AdvanceDialogue();
                }
                return;
            }
            if (_exchangeController.IsExchangeActive)
            {
                return;
            }
            InteractAtPointer();
        }

        private void InteractAtPointer()
        {
            Ray ray = _camera.ScreenPointToRay(_pointAction.action.ReadValue<Vector2>());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact();
            }
        }
    }
}
