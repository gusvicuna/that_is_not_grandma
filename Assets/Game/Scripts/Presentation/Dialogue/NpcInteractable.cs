using Game.Data;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Data")]
        [SerializeField] private DialogueSO _dialogue;
        [SerializeField] private NpcSO _npc;
        [Header("Event Channels")]
        [SerializeField] private DialogueEventChannelSO _dialogueRequestedEventChannel;
        [SerializeField] private NpcEventChannelSO _npcEngagedEventChannel;

        public NpcSO Npc => _npc;

        /// <summary>Rebinds what this NPC says next. Called by the story director's scene binder.</summary>
        public void SetDialogue(DialogueSO dialogue)
        {
            _dialogue = dialogue;
        }

        public void Interact()
        {
            _npcEngagedEventChannel.Raise(_npc);
            _dialogueRequestedEventChannel.Raise(_dialogue);
        }
    }
}
