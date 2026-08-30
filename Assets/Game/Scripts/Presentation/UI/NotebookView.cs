using System.Collections.Generic;
using Game.Data;
using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class NotebookView : MonoBehaviour
    {
        [SerializeField] private NotebookController _notebookController;
        [SerializeField] private GameObject _notebookUI;
        [SerializeField] private Transform _clueTextContainer;
        [SerializeField] private TextMeshProUGUI _clueTextFieldPrefab;

        [Header("Ways to open it")]
        [Tooltip("Keyboard shortcut. Optional — the HUD button alone is enough.")]
        [SerializeField] private InputActionReference _toggleNotebookAction;

        [Tooltip("HUD button that opens and closes the notebook. Optional — the shortcut alone is enough.")]
        [SerializeField] private Button _toggleNotebookButton;

        [Header("Room labels")]
        [Tooltip("How each line is laid out. {0} is the clue, {1} is the room it was found in.")]
        [SerializeField] private string _lineFormat = "- {0} ({1})";

        [Tooltip("Room names as the player reads them. Defaults are the four rooms named in the GDD.")]
        [SerializeField] private string _kitchenLabel = "Kitchen";
        [SerializeField] private string _livingRoomLabel = "Living room";
        [SerializeField] private string _bedroomLabel = "Bedroom";
        [SerializeField] private string _bathroomLabel = "Bathroom";

        private void OnEnable()
        {
            if (_toggleNotebookAction != null)
            {
                _toggleNotebookAction.action.performed += OnToggleActionPerformed;
                _toggleNotebookAction.action.Enable();
            }
            if (_toggleNotebookButton != null)
            {
                _toggleNotebookButton.onClick.AddListener(ToggleNotebook);
            }
        }

        private void OnDisable()
        {
            if (_toggleNotebookAction != null)
            {
                _toggleNotebookAction.action.performed -= OnToggleActionPerformed;
                _toggleNotebookAction.action.Disable();
            }
            if (_toggleNotebookButton != null)
            {
                _toggleNotebookButton.onClick.RemoveListener(ToggleNotebook);
            }
        }

        private void ShowNotebook()
        {
            IReadOnlyCollection<ClueSO> collectedClues = _notebookController.GetCollectedClues();
            foreach (ClueSO clue in collectedClues)
            {
                TextMeshProUGUI clueTextField = Instantiate(_clueTextFieldPrefab, _clueTextContainer);
                clueTextField.text = string.Format(_lineFormat, clue.ShortText, LabelFor(clue.RoomId));
            }
        }

        /// <summary>
        /// The room a clue came from, spelled the way the player reads it. It belongs on the
        /// notebook because the run is lost by hiding in a room whose clues leaked, and until now
        /// nothing on screen said which room a clue belonged to.
        /// </summary>
        private string LabelFor(RoomId roomId)
        {
            switch (roomId)
            {
                case RoomId.Kitchen:
                    return _kitchenLabel;

                case RoomId.LivingRoom:
                    return _livingRoomLabel;

                case RoomId.Bedroom:
                    return _bedroomLabel;

                case RoomId.Bathroom:
                    return _bathroomLabel;

                default:
                    return roomId.ToString();
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

        private void OnToggleActionPerformed(InputAction.CallbackContext context)
        {
            ToggleNotebook();
        }

        /// <summary>
        /// Public so a HUD button can call it — either through this component's own
        /// <see cref="_toggleNotebookButton"/> field or straight from a Button's OnClick list.
        /// </summary>
        public void ToggleNotebook()
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
