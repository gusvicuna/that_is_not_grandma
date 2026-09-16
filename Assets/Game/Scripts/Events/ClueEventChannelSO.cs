using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_Clue", menuName = "Game/Events/Clue")]
    public class ClueEventChannelSO : EventChannelSO<ClueSO> { }
}
