using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Presentation
{
    public class ClueDropSlot : MonoBehaviour, IDropHandler
    {
        [SerializeField] private ClueSharePanelView _panel;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }
            DraggableClueItem droppedItem = eventData.pointerDrag.GetComponent<DraggableClueItem>();
            if (droppedItem != null && droppedItem.Clue != null)
            {
                _panel.OnClueDropped(droppedItem);
            }
        }
    }
}
