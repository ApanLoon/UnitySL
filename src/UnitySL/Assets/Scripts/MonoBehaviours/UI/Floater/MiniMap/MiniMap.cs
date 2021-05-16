using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Agents;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Map;
using Assets.Scripts.Regions;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MonoBehaviours.UI.Floater.MiniMap
{
    public class MiniMap : MonoBehaviour
    {
        [SerializeField] protected RectTransform MapParent;
        [SerializeField] protected RawImage ParcelGrid;
        [SerializeField] protected RectTransform MarkerContainer;
        [SerializeField] protected MapMarkerTemplate MapMarkers;

        protected Dictionary<Guid, MiniMapMarker> MarkerByAgentId = new Dictionary<Guid, MiniMapMarker>();

        protected bool IsMouseOver = false;
        protected float Zoom = 1f;
        protected const float ZoomMin = 0.5f;
        protected const float ZoomMax = 3f;
        protected float ZoomSensitivity = 8f;

        private void OnEnable()
        {
            if (Agent.CurrentPlayer != null)
            {
                OnParcelOverlayChanged(Agent.CurrentPlayer.Region);
            }

            EventManager.Instance.OnParcelOverlayChanged += OnParcelOverlayChanged; 
            EventManager.Instance.OnCoarseLocationUpdateMessage += OnCoarseLocationUpdateMessage; // TODO: This should probably use some higher level mechanism to get updates. Possibly AvatarTracker
            EventManager.Instance.OnLogout += OnLogout;
        }
        private void OnDisable()
        {
            EventManager.Instance.OnParcelOverlayChanged -= OnParcelOverlayChanged;
            EventManager.Instance.OnCoarseLocationUpdateMessage -= OnCoarseLocationUpdateMessage;
            EventManager.Instance.OnLogout -= OnLogout;
        }

        private void Update()
        {
            if (IsMouseOver == true)
            {
                Vector2 scrollDelta = Input.mouseScrollDelta;
                if (Mathf.Abs(scrollDelta.y) > float.Epsilon)
                {
                    Zoom += scrollDelta.y * Zoom * ZoomSensitivity * Time.deltaTime;
                    Zoom = Mathf.Clamp(Zoom, ZoomMin, ZoomMax);
                    MapParent.localScale = new Vector3(Zoom, Zoom, 1f);
                }
            }
        }

        protected void OnParcelOverlayChanged(Region region)
        {
            ParcelGrid.texture = region.ParcelOverlay.ParcelOverlayMinimapBorderTexture;
            ParcelGrid.gameObject.SetActive(true);
        }

        protected void OnCoarseLocationUpdateMessage(CoarseLocationUpdateMessage message)
        {
            Transform youTransform = null;
            Transform preyTransform = null;
            foreach (CoarseLocation location in message.Locations)
            {
                MiniMapMarker marker;
                if (MarkerByAgentId.ContainsKey(location.AgentId))
                {
                    marker = MarkerByAgentId[location.AgentId];
                }
                else
                {
                    marker = MapMarkers.InstantiateTemplate();
                    MarkerByAgentId[location.AgentId] = marker;
                }
            
                marker.Transform.anchoredPosition = ScalePosition(location.Position);

                Color c = Color.green;
                if (location.IsYou)
                {
                    c = Color.white;
                    youTransform = marker.Transform;
                }
                if (location.IsPrey)
                {
                    c = Color.red;
                    preyTransform = marker.Transform;
                }
                marker.Image.color = c;

                marker.ToolTipTarget.Text = location.AgentId.ToString();

                AvatarNameCache.Instance.Get(location.AgentId, OnNameReceived);

            }

            // Force visibility of prey and you:
            if (preyTransform != null)
            {
                preyTransform.SetAsLastSibling();
            }

            if (youTransform != null)
            {
                youTransform.SetAsLastSibling();
            }

            // Remove markers that have left:
            for (int i = MarkerByAgentId.Count - 1; i >= 0 ; i--)
            {
                Guid key = MarkerByAgentId.Keys.ElementAt(i);
                MiniMapMarker marker = MarkerByAgentId[key];
                if (message.Locations.FirstOrDefault(x => x.AgentId == key) == null)
                {
                    MapMarkers.ReturnItemToPool(marker);
                    MarkerByAgentId.Remove(key);
                }
            }
        }

        private void OnLogout()
        {
            MapMarkers.Clear();
        }

        protected void OnNameReceived(Guid agentId, AvatarName avatarName)
        {
            if (MarkerByAgentId.ContainsKey(agentId))
            {
                MarkerByAgentId[agentId].ToolTipTarget.Text = avatarName.DisplayName;
            }
        }

        protected Vector2 ScalePosition(Vector3Byte position)
        {
            return new Vector2(MarkerContainer.rect.width * position.x / 255f, MarkerContainer.rect.height * position.z / 255f);
        }

        public void OnMouseEnter()
        {
            IsMouseOver = true;
        }

        public void OnMouseExit()
        {
            IsMouseOver = false;
        }

        [Serializable] public class MapMarkerTemplate : Template<MiniMapMarker> { }
    }
}
