using Game.Data;
using UnityEngine;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "DialogueFinishedEventChannel", menuName = "Game/Events/Dialogue Finished")]
    public class DialogueFinishedEventChannelSO : ScriptableObject
    {
        public event System.Action<DialogueSO> Raised;
        public void Raise(DialogueSO dialogue)
        {
            Raised?.Invoke(dialogue);
        }
    }
}
