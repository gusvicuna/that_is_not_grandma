using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class DialogueOptionData
    {
        [SerializeField][TextArea] private string _text;
        [SerializeField] private int _targetIndex;

        public string Text => _text;
        public int TargetIndex => _targetIndex;
    }
}
