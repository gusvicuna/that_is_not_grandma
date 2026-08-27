using System;
using Game.Data;
using Game.Domain;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    public class DialogueController : MonoBehaviour
    {
        public event Action OnNodeChanged;
        [SerializeField] private DialogueRequestedEventChannelSO _dialogueRequestedEventChannel;
        [SerializeField] private DialogueFinishedEventChannelSO _dialogueFinishedEventChannel;
        private DialogueRunner _dialogueRunner;
        private DialogueSO _currentDialogue;
        private DialogueNodeData _currentNodeData;
        public DialogueNodeData CurrentNodeData => _currentNodeData;
        public bool IsDialogueActive => _dialogueRunner != null;

        private void OnEnable()
        {
            _dialogueRequestedEventChannel.Raised += OnDialogueRequested;
        }

        private void OnDisable()
        {
            _dialogueRequestedEventChannel.Raised -= OnDialogueRequested;
        }

        private void OnDialogueRequested(DialogueSO dialogue)
        {
            if (IsDialogueActive)
            {
                return;
            }
            _currentDialogue = dialogue;
            _dialogueRunner = new DialogueRunner(dialogue.ToGraph());
            _currentNodeData = _currentDialogue.Nodes[_dialogueRunner.CurrentIndex];
            OnNodeChanged?.Invoke();
        }

        public void AdvanceDialogue()
        {
            if (_dialogueRunner == null)
            {
                Debug.LogWarning("No dialogue is currently running.");
                return;
            }

            _dialogueRunner.Advance();
            if (_dialogueRunner.IsFinished)
            {
                HandleDialogueFinished();
            }
            else
            {
                _currentNodeData = _currentDialogue.Nodes[_dialogueRunner.CurrentIndex];
                OnNodeChanged?.Invoke();
            }
        }

        private void HandleDialogueFinished()
        {
            _dialogueFinishedEventChannel.Raise(_currentDialogue);
            _dialogueRunner = null;
            _currentDialogue = null;
            _currentNodeData = null;
            OnNodeChanged?.Invoke();
        }

        public void ChooseOption(int optionIndex)
        {
            if (_dialogueRunner == null)
            {
                Debug.LogWarning("No dialogue is currently running.");
                return;
            }

            _dialogueRunner.Choose(optionIndex);
            if (_dialogueRunner.IsFinished)
            {
                HandleDialogueFinished();
            }
            else
            {
                _currentNodeData = _currentDialogue.Nodes[_dialogueRunner.CurrentIndex];
                OnNodeChanged?.Invoke();
            }
        }

    }
}
