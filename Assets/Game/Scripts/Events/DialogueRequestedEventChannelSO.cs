using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(
        fileName = "DialogueRequestedEventChannel",
        menuName = "Game/Events/Dialogue Requested"
    )]
    public class DialogueRequestedEventChannelSO : ScriptableObject
    {
        public event Action<DialogueSO> Raised;

        public void Raise(DialogueSO dialogue)
        {
            Raised?.Invoke(dialogue);
        }
    }
}
