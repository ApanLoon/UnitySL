using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.MonoBehaviours.UI.Floater.Components
{
    public class DragParent : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public float SnapDistance = 20f;
        protected RectTransform ParentTransform;
        protected Canvas Canvas;
        protected RectTransform CanvasTransform;

        private void Awake()
        {
            ParentTransform = transform.parent.GetComponent<RectTransform>();
            Canvas = GetComponentInParent<Canvas>();
            Canvas parentCanvas = Canvas.transform.parent?.GetComponentInParent<Canvas>(); // Note: At some point the floaters got their own Canvas, but for snapping we need the main Canvas
            if (parentCanvas != null)
            {
                Canvas = parentCanvas;
            }
            CanvasTransform = Canvas.GetComponent<RectTransform>();
        }
    
        public void OnDrag(PointerEventData eventData)
        {
            ParentTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 pos = ParentTransform.anchoredPosition;
            Vector2 size = ParentTransform.sizeDelta;
            Vector2 canvasSize = CanvasTransform.sizeDelta;

            // Check snapping against borders:

            // Right edge:
            if (Mathf.Abs((pos.x + size.x) - canvasSize.x) < SnapDistance)
            {
                pos.x = canvasSize.x - size.x;
            }
            // Left edge
            else if (Mathf.Abs(pos.x) < SnapDistance)
            {
                pos.x = 0f;
            }
            // Bottom edge:
            if (Mathf.Abs((pos.y - size.y) + canvasSize.y) < SnapDistance)
            {
                pos.y = size.y - canvasSize.y;
            }
            // Top edge
            else if (pos.y > -SnapDistance)
            {
                pos.y = 0f;
            }

            // Check snapping against other, enabled, floaters:
            Transform container = ParentTransform.parent;
            for (int i = 0; i < container.childCount; i++)
            {
                RectTransform floater = container.GetChild(i).GetComponent<RectTransform>();
                if (floater == null)
                {
                    Debug.LogError("DragParent: Floater container contains something without a RectTransform!");
                    continue;
                }

                if (floater == ParentTransform)
                {
                    // Don't try to snap to myself!
                    continue;
                }

                Vector2 floaterPos = floater.anchoredPosition;
                Vector2 floaterSize = floater.sizeDelta;

                // My right edge close to floater left edge:
                if (Mathf.Abs(pos.x + size.x - floaterPos.x) < SnapDistance)
                {
                    pos.x = floaterPos.x - size.x;
                }

                // My left edge close to floater right edge:
                if (Mathf.Abs(pos.x - (floaterPos.x + floaterSize.x)) < SnapDistance)
                {
                    pos.x = floaterPos.x + floaterSize.x;
                }

                // My bottom edge close to floater top edge:
                if (Mathf.Abs(pos.y - size.y - floaterPos.y) < SnapDistance)
                {
                    pos.y = floaterPos.y + size.y;
                }

                // My top edge close to floater bottom edge:
                if (Mathf.Abs(pos.y - (floaterPos.y - floaterSize.y)) < SnapDistance)
                {
                    pos.y = floaterPos.y - floaterSize.y;
                }

            }

            // Clamp to screen:
            pos.x = Mathf.Clamp(pos.x, SnapDistance - size.x, canvasSize.x - SnapDistance);
            pos.y = Mathf.Clamp(pos.y, SnapDistance - canvasSize.y, size.y - SnapDistance);

            // Set position, snapped or not:
            ParentTransform.anchoredPosition = pos;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Move floater to the top:
            ParentTransform.SetAsLastSibling();
        }
    }
}
