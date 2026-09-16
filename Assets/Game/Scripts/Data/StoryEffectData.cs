using System;
using Game.Domain;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// One effect of a beat. Only the fields its kind needs are read; the rest stay empty in the
    /// inspector.
    /// </summary>
    [Serializable]
    public class StoryEffectData
    {
        [SerializeField] private StoryEffectKind _kind;
        public StoryEffectKind Kind => _kind;

        [Tooltip("ShowActor / HideActor / MoveActor: the id on the StoryActor components to affect.")]
        [SerializeField] private string _actorId;
        public string ActorId => _actorId;

        [Tooltip("MoveActor: the room the actor should end up in.")]
        [SerializeField] private RoomId _room;
        public RoomId Room => _room;

        [Tooltip("SetNpcDialogue: whose dialogue changes.")]
        [SerializeField] private NpcSO _npc;
        public NpcSO Npc => _npc;

        [Tooltip("SetNpcDialogue / PlayDialogue: the dialogue to bind or to play.")]
        [SerializeField] private DialogueSO _dialogue;
        public DialogueSO Dialogue => _dialogue;

        [Tooltip("SetTension: the level to raise on CH_TensionChanged.")]
        [SerializeField] private TensionLevel _tension;
        public TensionLevel Tension => _tension;

        [Tooltip("SetFlag: the story flag to set, e.g. met_cousin.")]
        [SerializeField] private string _flag;
        public string Flag => _flag;
    }
}
