using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class DialogueNodeData
    {
        [SerializeField] private NpcSO _speaker;
        public NpcSO Speaker => _speaker;
        [SerializeField] private SpeakerType _speakerType;
        public SpeakerType SpeakerType => _speakerType;
        [SerializeField][TextArea] private string _text;
        public string Text => _text;
        [SerializeField] private DialogueOptionData[] _options;
        public DialogueOptionData[] Options => _options;
        [SerializeField] private int _nextIndex;
        public int NextIndex => _nextIndex;
    }
}
