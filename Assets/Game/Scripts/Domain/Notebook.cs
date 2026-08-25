using System.Collections.Generic;

namespace Game.Domain
{
    public class Notebook
    {
        private readonly HashSet<string> _collectedClues;
        private readonly List<string> _insertionOrder;

        public Notebook()
        {
            _collectedClues = new HashSet<string>();
            _insertionOrder = new List<string>();
        }

        public int Count => _collectedClues.Count;

        public IReadOnlyList<string> CollectedIds => _insertionOrder.AsReadOnly();

        public bool Collect(string clueId)
        {
            if (clueId == null)
            {
                throw new System.ArgumentException("Clue ID cannot be null.", nameof(clueId));
            }
            if (string.IsNullOrEmpty(clueId.Trim()))
            {
                throw new System.ArgumentException("Clue ID cannot be empty.", nameof(clueId));
            }
            if (_collectedClues.Add(clueId))
            {
                _insertionOrder.Add(clueId);
                return true;
            }
            return false;
        }

        public bool Contains(string clueId)
        {
            return _collectedClues.Contains(clueId);
        }
    }
}
