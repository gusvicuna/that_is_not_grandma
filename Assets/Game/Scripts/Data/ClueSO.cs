using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Clue", menuName = "Game/Data/Clue")]
    public class ClueSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField][TextArea] private string _text;
        [SerializeField] private RoomId _roomId;
        [SerializeField] private bool _isEvidence;

        public string Id => _id;
        public string Text => _text;
        public RoomId RoomId => _roomId;
        public bool IsEvidence => _isEvidence;
    }
}
