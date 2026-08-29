using Game.Domain;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Marks a scene object the story can show, hide or move. One per NPC/item *instance*: the same
    /// id appears once per room the actor can be in, and moving it is hiding one copy and showing
    /// another.
    /// </summary>
    public class StoryActor : MonoBehaviour
    {
        [Tooltip("Shared by every copy of this character or item, e.g. npc_uncle, item_phone.")]
        [SerializeField] private string _id;
        public string Id => _id;

        [Tooltip("The room this particular copy sits in.")]
        [SerializeField] private RoomId _room;
        public RoomId Room => _room;

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
