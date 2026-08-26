using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Item", menuName = "Game/Data/Item")]
    public class ItemSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField][TextArea] private string _description;
        [SerializeField] private bool _isInspectable;

        public string Id => _id;
        public string Description => _description;
        public bool IsInspectable => _isInspectable;
    }
}
