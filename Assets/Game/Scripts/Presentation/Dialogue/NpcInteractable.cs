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
        [SerializeField] private DialogueRequestedEventChannelSO _dialogueRequestedEventChannel;
        [SerializeField] private NpcEngagedEventChannelSO _npcEngagedEventChannel;

        public void Interact()
        {
            _npcEngagedEventChannel.Raise(_npc);
            _dialogueRequestedEventChannel.Raise(_dialogue);
        }
    }
}
