using UnityEngine;
using Game.Domain;
using Game.Data;
using Game.Events;
using System.Collections.Generic;

namespace Game.Presentation
{
    public class NotebookController : MonoBehaviour
    {
        [SerializeField] private ClueEventChannelSO _clueCollectedEventChannel;
        private Notebook _notebook;
        private readonly List<ClueSO> _collectedClueSOs = new();

        private void Awake()
        {
            _notebook = new Notebook();
        }

        private void OnEnable()
        {
            _clueCollectedEventChannel.Raised += AddClue;
        }

        private void OnDisable()
        {
            _clueCollectedEventChannel.Raised -= AddClue;
        }

        public void AddClue(ClueSO clue)
        {
            if (_notebook.Collect(clue.Id))
                _collectedClueSOs.Add(clue);
        }

        public IReadOnlyList<ClueSO> GetCollectedClues()
        {
            return _collectedClueSOs.AsReadOnly();
        }
    }
}
