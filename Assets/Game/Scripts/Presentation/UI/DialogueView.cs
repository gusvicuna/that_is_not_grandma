using System;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class DialogueView : MonoBehaviour
    {
        [Serializable]
        private class SpeakerStyle
        {
            public Color textColor = Color.white;
            public FontStyles fontStyle = FontStyles.Normal;
        }

        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private GameObject _speakerPortrait;
        [SerializeField] private GameObject[] _dialogueOptionButtons;
        [SerializeField] private DialogueController _dialogueController;
        [SerializeField] private SpeakerStyle _npcStyle;
        [SerializeField] private SpeakerStyle _playerStyle;
        [SerializeField] private SpeakerStyle _monologueStyle;

        private TextMeshProUGUI[] _optionLabels;

        private void Awake()
        {
            _optionLabels = new TextMeshProUGUI[_dialogueOptionButtons.Length];
            for (int i = 0; i < _dialogueOptionButtons.Length; i++)
            {
                _optionLabels[i] = _dialogueOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                int captured = i;
                _dialogueOptionButtons[i].GetComponent<Button>().onClick.AddListener(() => _dialogueController.ChooseOption(captured));
            }
        }

        private void OnEnable()
        {
            _dialogueController.OnNodeChanged += UpdateDialogueUI;
        }

        private void OnDisable()
        {
            _dialogueController.OnNodeChanged -= UpdateDialogueUI;
        }

        private void UpdateDialogueUI()
        {
            if (!_dialogueController.IsDialogueActive)
            {
                _dialoguePanel.SetActive(false);
                return;
            }

            DialogueNodeData node = _dialogueController.CurrentNodeData;
            bool isMonologue = node.SpeakerType == SpeakerType.InnerMonologue;

            _dialoguePanel.SetActive(true);
            _speakerNameText.gameObject.SetActive(!isMonologue);
            if (_speakerPortrait != null)
            {
                _speakerPortrait.SetActive(!isMonologue);
            }
            if (!isMonologue)
            {
                _speakerNameText.text = node.SpeakerName;
            }

            SpeakerStyle style = StyleFor(node.SpeakerType);
            _dialogueText.color = style.textColor;
            _dialogueText.fontStyle = style.fontStyle;
            _dialogueText.text = node.Text;

            for (int i = 0; i < _dialogueOptionButtons.Length; i++)
            {
                bool active = i < node.Options.Length;
                _dialogueOptionButtons[i].SetActive(active);
                if (active)
                {
                    _optionLabels[i].text = node.Options[i].Text;
                }
            }
        }

        private SpeakerStyle StyleFor(SpeakerType type)
        {
            switch (type)
            {
                case SpeakerType.Player:
                    return _playerStyle;
                case SpeakerType.InnerMonologue:
                    return _monologueStyle;
                default:
                    return _npcStyle;
            }
        }

        public void AdvanceDialogue()
        {
            _dialogueController.AdvanceDialogue();
        }
    }
}
