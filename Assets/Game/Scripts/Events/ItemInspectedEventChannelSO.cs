using System;
using UnityEngine;
using Game.Data;

namespace Game.Events
{
    [CreateAssetMenu(fileName = "ItemInspectedEventChannel", menuName = "Game/Events/Item Inspected")]
    public class ItemInspectedEventChannelSO : ScriptableObject
    {
        public event Action<ItemSO> Raised;

        public void Raise(ItemSO item)
        {
            Raised?.Invoke(item);
        }
    }
}
