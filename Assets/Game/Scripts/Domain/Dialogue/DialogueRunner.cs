

namespace Game.Domain
{
    public class DialogueRunner
    {
        private DialogueGraph _graph;
        private int _currentIndex;
        private bool _isFinished;

        public DialogueRunner(DialogueGraph graph)
        {
            _graph = graph ?? throw new System.ArgumentNullException(nameof(graph));
            _currentIndex = 0;
            _isFinished = false;
        }


        public bool IsFinished => _isFinished;
        public int CurrentIndex
        {
            get
            {
                if (_isFinished)
                {
                    throw new System.InvalidOperationException("DialogueRunner is finished.");
                }
                return _currentIndex;
            }
        }

        public bool CurrentHasOptions
        {
            get
            {
                if (_isFinished)
                {
                    throw new System.InvalidOperationException("DialogueRunner is finished.");
                }
                return _graph[_currentIndex].OptionTargets.Count > 0;
            }
        }

        public void Advance()
        {
            if (_isFinished)
            {
                throw new System.InvalidOperationException("DialogueRunner is finished.");
            }

            DialogueNode currentNode = _graph[_currentIndex];

            if (CurrentHasOptions)
            {
                throw new System.InvalidOperationException("Current node has options. Use SelectOption to choose an option.");
            }
            else if (currentNode.NextIndex == DialogueGraph.EndIndex)
            {
                _isFinished = true;
            }
            _currentIndex = currentNode.NextIndex;
        }

        public void Choose(int optionIndex)
        {
            if (_isFinished)
            {
                throw new System.InvalidOperationException("DialogueRunner is finished.");
            }

            DialogueNode currentNode = _graph[_currentIndex];
            if (!CurrentHasOptions)
            {
                throw new System.InvalidOperationException("Current node has no options.");
            }
            else if (optionIndex < 0 || optionIndex >= currentNode.OptionTargets.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(optionIndex), "Option index is out of range.");
            }
            else if (currentNode.OptionTargets[optionIndex] == DialogueGraph.EndIndex)
            {
                _isFinished = true;
            }
            _currentIndex = currentNode.OptionTargets[optionIndex];
        }
    }
}
