using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class DialogueNodeData
    {
        [SerializeField] private SpeakerType _speakerType;
        [SerializeField] private string _speakerName;
        [SerializeField][TextArea] private string _text;
        [SerializeField] private DialogueOptionData[] _options;
        [SerializeField] private int _nextIndex;

        public SpeakerType SpeakerType => _speakerType;
        public string SpeakerName => _speakerName;
        public string Text => _text;
        public DialogueOptionData[] Options => _options;
        public int NextIndex => _nextIndex;
    }
}
