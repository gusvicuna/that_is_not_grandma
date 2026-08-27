using System.Collections.Generic;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class ClueSharePanelView : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private ExchangeController _exchangeController;
        [SerializeField] private NotebookController _notebookController;

        [Header("UI Elements")]
        [SerializeField] private GameObject _clueSharePanel;
        [SerializeField] private Image _speakerPortrait;
        [SerializeField] private TextMeshProUGUI _speakerName;
        [SerializeField] private Transform _cluesContainer;
        [SerializeField] private DraggableClueItem _cluePrefab;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _closeButton;

        [Header("Placeholders")]
        [SerializeField] private string _noReturnPlaceholder = "[NOTHING_IN_RETURN]";
        [SerializeField] private string _noCluesPlaceholder = "[NO_CLUES_TO_SHARE]";

        private readonly List<DraggableClueItem> _spawnedClueItems = new();

        private void Awake()
        {
            _closeButton.onClick.AddListener(RequestClose);
            _clueSharePanel.SetActive(false);
        }

        private void OnEnable()
        {
            _exchangeController.OnExchangeStateChanged += SyncToExchangeState;
        }

        private void OnDisable()
        {
            _exchangeController.OnExchangeStateChanged -= SyncToExchangeState;
        }

        private void SyncToExchangeState()
        {
            if (_exchangeController.IsExchangeActive)
            {
                Open();
            }
            else
            {
                Hide();
            }
        }

        private void Open()
        {
            NpcSO npc = _exchangeController.CurrentNpc;
            _speakerPortrait.sprite = npc.Portrait;
            _speakerPortrait.enabled = npc.Portrait != null;
            _speakerName.text = npc.DisplayName;
            _speakerName.color = npc.Color;
            _resultText.text = string.Empty;

            // Activate first: clue items instantiated under an inactive panel never run Awake
            _clueSharePanel.SetActive(true);
            PopulateClues();
        }

        private void Hide()
        {
            ClearClues();
            _clueSharePanel.SetActive(false);
        }

        private void PopulateClues()
        {
            ClearClues();
            IReadOnlyList<ClueSO> collectedClues = _notebookController.GetCollectedClues();
            if (collectedClues.Count == 0)
            {
                _resultText.text = _noCluesPlaceholder;
                return;
            }
            foreach (ClueSO clue in collectedClues)
            {
                DraggableClueItem clueItem = Instantiate(_cluePrefab, _cluesContainer);
                clueItem.Init(clue, !_exchangeController.HasSharedWithCurrentNpc(clue));
                _spawnedClueItems.Add(clueItem);
            }
        }

        private void ClearClues()
        {
            foreach (DraggableClueItem clueItem in _spawnedClueItems)
            {
                Destroy(clueItem.gameObject);
            }
            _spawnedClueItems.Clear();
        }

        public void OnClueDropped(DraggableClueItem droppedItem)
        {
            ClueSO returnedClue = _exchangeController.Share(droppedItem.Clue);
            ClearClues();
            _resultText.text = returnedClue != null ? returnedClue.Text : _noReturnPlaceholder;
        }

        private void RequestClose()
        {
            _exchangeController.CloseExchange();
        }
    }
}
