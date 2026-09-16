using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "CH_Item", menuName = "Game/Events/Item")]
    public class ItemEventChannelSO : EventChannelSO<ItemSO> { }
}
