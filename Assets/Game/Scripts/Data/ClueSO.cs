using UnityEngine;
using Game.Domain;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Clue", menuName = "Game/Data/Clue")]
    public class ClueSO : ScriptableObject
    {
        [Tooltip("Internal name. Never shown to the player.")]
        [SerializeField] private string _id;

        [Tooltip("The long line — the monologue shown when the clue is found.")]
        [SerializeField][TextArea] private string _text;

        [Tooltip("The short line, for the notebook list and the clue chips in the share panel. " +
                 "Left empty it falls back to the long text, so an unwritten clue still reads.")]
        [SerializeField] private string _shortText;

        [SerializeField] private RoomId _roomId;
        [SerializeField] private bool _isEvidence;

        public string Id => _id;
        public string Text => _text;

        /// <summary>
        /// What the player reads in a list rather than in a popup. The fallback is deliberate:
        /// every clue asset predates this field, and a blank row in the notebook would be a worse
        /// bug than a row that is too long.
        /// </summary>
        public string ShortText => string.IsNullOrWhiteSpace(_shortText) ? _text : _shortText;

        public RoomId RoomId => _roomId;
        public bool IsEvidence => _isEvidence;
    }
}
