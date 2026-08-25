using System.Collections.Generic;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Presentation
{
    public class NotebookView : MonoBehaviour
    {
        [SerializeField] private NotebookController _notebookController;
        [SerializeField] private GameObject _notebookUI;
        [SerializeField] private Transform _clueTextContainer;
        [SerializeField] private TextMeshProUGUI _clueTextFieldPrefab;
        [SerializeField] private InputActionReference _toggleNotebookAction;

        private void OnEnable()
        {
            _toggleNotebookAction.action.performed += ToggleNotebook;
            _toggleNotebookAction.action.Enable();
        }

        private void OnDisable()
        {
            _toggleNotebookAction.action.performed -= ToggleNotebook;
            _toggleNotebookAction.action.Disable();
        }

        private void ShowNotebook()
        {
            IReadOnlyCollection<ClueSO> collectedClues = _notebookController.GetCollectedClues();
            foreach (ClueSO clue in collectedClues)
            {
                TextMeshProUGUI clueTextField = Instantiate(_clueTextFieldPrefab, _clueTextContainer);
                clueTextField.text = "- " + clue.Text;
            }
        }

        private void HideNotebook()
        {
            // Hide the notebook UI
            _notebookUI.SetActive(false);
            // Destroy all clue text fields
            foreach (Transform child in _clueTextContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void ToggleNotebook(InputAction.CallbackContext context)
        {
            if (_notebookUI.activeSelf)
            {
                HideNotebook();
            }
            else
            {
                ShowNotebook();
                _notebookUI.SetActive(true);
            }
        }
    }
}
