using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Presentation
{
    public class ClickRouter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionReference _clickAction;
        [SerializeField] private InputActionReference _pointAction;

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
