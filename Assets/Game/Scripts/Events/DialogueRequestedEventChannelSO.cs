using Game.Data;
using UnityEngine;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "DialogueRequestedEventChannel", menuName = "Game/Events/Dialogue Requested")]
    public class DialogueRequestedEventChannelSO : ScriptableObject
    {
        public event System.Action<DialogueSO> Raised;
        public void Raise(DialogueSO dialogue)
        {
            Raised?.Invoke(dialogue);
        }
    }
}
