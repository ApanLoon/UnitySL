using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.MonoBehaviours.UI.ToolTips
{
    public class ToolTip : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Time in seconds between raycasts.")]
        protected float PollInterval = 0.5f;

        [SerializeField]
        [Tooltip("Maximum distance for raycasts.")]
        protected float MaxDistance = 256f;

        [SerializeField]
        [Tooltip("Horizontal offset of the left and right side of the screen respectively. (ViewPort scale)")]
        protected Vector2 OffsetX = new Vector2(20f, -10f);

        [SerializeField]
        [Tooltip("Vertical offset of the bottom and top side of the screen respectively. (ViewPort scale)")]
        protected Vector2 OffsetY = new Vector2(5f, -20f);

        protected RectTransform RectTransform;
        protected TMP_Text Text;

        protected bool IsActive = false;
        protected float TimeToNextPoll = 0f;

        private void Awake()
        {
            RectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            Text = GetComponentInChildren<TMP_Text>(true);
        }

        private void Update()
        {
            Vector3 mousePos = Input.mousePosition;

            TimeToNextPoll -= Time.deltaTime;
            if (TimeToNextPoll <= 0f)
            {
                TimeToNextPoll = PollInterval;

                ToolTipTarget target = Raycast(mousePos);
                IsActive = target != null;
                RectTransform.gameObject.SetActive(IsActive);
                if (target != null)
                {
                    Text.text = target.Text;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
                    //SetPosition(mousePos); // TODO: Do we want the tool tip to stay in position or...
                }
            }

            if (IsActive == false)
            {
                return;
            }

            SetPosition(mousePos); // TODO: do we want the tool tip to follow the mouse?
        }

        protected void SetPosition(Vector3 mousePos)
        {
            Vector3 vpPos = Camera.main.ScreenToViewportPoint(mousePos);

            bool isOnLeftSide = vpPos.x < 0.5f;
            bool isOnBottomSide = vpPos.y < 0.5f;

            Vector2 anchor = new Vector2(isOnLeftSide ? 0f : 1f, isOnBottomSide ? 0f : 1f);
            RectTransform.anchorMin = anchor;
            RectTransform.anchorMax = anchor;
            RectTransform.pivot = anchor;

            Vector3 offset = new Vector3(isOnLeftSide ? OffsetX.x : OffsetX.y, isOnBottomSide ? OffsetY.x : OffsetY.y, 0f);

            Vector3 spPos = Camera.main.ViewportToScreenPoint(vpPos);
            RectTransform.position = spPos + offset;
        }

        protected ToolTipTarget Raycast(Vector3 mousePos)
        {
            ToolTipTarget target;

            // Check UI:
            var pointerEventData = new PointerEventData(EventSystem.current) {position = mousePos};
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            foreach (RaycastResult result in raycastResults)
            {
                target = result.gameObject.GetComponent<ToolTipTarget>();
                if (target != null)
                {
                    return target;
                }
            }

            // Check world space:
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, MaxDistance))
            {
                return null;
            }

            target = hit.transform.GetComponentInChildren<ToolTipTarget>();
            return target != null ? target : hit.transform.GetComponentInParent<ToolTipTarget>();
        }
    }
}
