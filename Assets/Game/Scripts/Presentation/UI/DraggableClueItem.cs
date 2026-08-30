using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Presentation
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableClueItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float SharedClueAlpha = 0.4f;
        private const float GhostAlpha = 0.8f;

        [SerializeField] private TextMeshProUGUI _label;

        private CanvasGroup _canvasGroup;
        private Canvas _rootCanvas;
        private RectTransform _ghost;
        private ClueSO _clue;
        private bool _isDraggable;

        public ClueSO Clue => _clue;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        public void Init(ClueSO clue, bool isDraggable)
        {
            _clue = clue;
            _isDraggable = isDraggable;
            _label.text = clue.ShortText;
            _canvasGroup.alpha = isDraggable ? 1f : SharedClueAlpha;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDraggable)
            {
                // Clearing pointerDrag cancels the drag: no OnDrag, OnEndDrag or OnDrop will follow
                eventData.pointerDrag = null;
                return;
            }
            _ghost = CreateGhost();
            _ghost.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_ghost != null)
            {
                _ghost.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DestroyGhost();
        }

        // The panel destroys these items on drop, which can happen before OnEndDrag is
        // delivered; the ghost lives under the root canvas and would outlive its owner
        private void OnDisable()
        {
            DestroyGhost();
        }

        private void OnDestroy()
        {
            DestroyGhost();
        }

        private RectTransform CreateGhost()
        {
            GameObject ghost = Instantiate(gameObject, _rootCanvas.transform);
            Destroy(ghost.GetComponent<DraggableClueItem>());

            CanvasGroup ghostCanvasGroup = ghost.GetComponent<CanvasGroup>();
            ghostCanvasGroup.blocksRaycasts = false;
            ghostCanvasGroup.alpha = GhostAlpha;

            return (RectTransform)ghost.transform;
        }

        private void DestroyGhost()
        {
            if (_ghost != null)
            {
                Destroy(_ghost.gameObject);
                _ghost = null;
            }
        }
    }
}
