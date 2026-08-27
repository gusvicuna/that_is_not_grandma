using Game.Data;
using Game.Events;
using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueSO _dialogue;
        [SerializeField] private DialogueRequestedEventChannelSO _dialogueRequestedEventChannel;

        public void Interact()
        {
            _dialogueRequestedEventChannel.Raise(_dialogue);
        }
    }
}
