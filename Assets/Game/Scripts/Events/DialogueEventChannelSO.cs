using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_Dialogue", menuName = "Game/Events/Dialogue")]
    public class DialogueEventChannelSO : EventChannelSO<DialogueSO> { }
}
