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
        [SerializeField] private HideConfirmView _hideConfirmView;
        [SerializeField] private ItemInspectPopup _itemInspectPopup;

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
            // The hide prompt is a modal: without this the raycast underneath keeps firing and the
            // player can collect a clue through the panel.
            if (_hideConfirmView != null && _hideConfirmView.IsOpen)
            {
                return;
            }
            // The inspect popup is closed by the next click anywhere, and that click is consumed:
            // dismissing it must not also interact with whatever is underneath.
            if (_itemInspectPopup != null && _itemInspectPopup.IsOpen)
            {
                _itemInspectPopup.Hide();
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
