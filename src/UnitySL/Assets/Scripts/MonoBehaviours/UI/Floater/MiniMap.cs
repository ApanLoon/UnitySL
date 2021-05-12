﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Agents;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Map;
using Assets.Scripts.MonoBehaviours.UI.ToolTips;
using Assets.Scripts.Regions;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MonoBehaviours.UI.Floater
{
    public class MiniMap : MonoBehaviour
    {
        [SerializeField] protected RectTransform MapParent;
        [SerializeField] protected RawImage ParcelGrid;
        [SerializeField] protected GameObject MarkerPrefab;
        [SerializeField] protected RectTransform MarkerContainer;

        protected class MarkerInfo
        {
            public GameObject GameObject;
            public RectTransform Transform;
            public Image Image;
            public ToolTipTarget ToolTipTarget;
        }

        protected Dictionary<Guid, MarkerInfo> MarkerInfoByAgentId = new Dictionary<Guid, MarkerInfo>();

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
                MarkerInfo mi;
                if (MarkerInfoByAgentId.ContainsKey(location.AgentId))
                {
                    mi = MarkerInfoByAgentId[location.AgentId];
                }
                else
                {
                    mi = new MarkerInfo();
                    mi.GameObject = Instantiate(MarkerPrefab, MarkerContainer);
                    mi.Transform = mi.GameObject.GetComponent<RectTransform>();
                    mi.Image = mi.GameObject.GetComponent<Image>();
                    mi.ToolTipTarget = mi.GameObject.GetComponent<ToolTipTarget>();

                    MarkerInfoByAgentId[location.AgentId] = mi;
                }
            
                mi.Transform.anchoredPosition = ScalePosition(location.Position);

                Color c = Color.green;
                if (location.IsYou)
                {
                    c = Color.white;
                    youTransform = mi.Transform;
                }
                if (location.IsPrey)
                {
                    c = Color.red;
                    preyTransform = mi.Transform;
                }
                mi.Image.color = c;

                mi.ToolTipTarget.Text = location.AgentId.ToString();

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
            for (int i = MarkerInfoByAgentId.Count - 1; i >= 0 ; i--)
            {
                Guid key = MarkerInfoByAgentId.Keys.ElementAt(i);
                MarkerInfo mi = MarkerInfoByAgentId[key];
                if (message.Locations.FirstOrDefault(x => x.AgentId == key) == null)
                {
                    Destroy(MarkerInfoByAgentId[key].GameObject);
                    MarkerInfoByAgentId.Remove(key);
                }
            }
        }

        private void OnLogout()
        {
            // Remove markers:
            for (int i = MarkerInfoByAgentId.Count - 1; i >= 0; i--)
            {
                Guid key = MarkerInfoByAgentId.Keys.ElementAt(i);
                MarkerInfo mi = MarkerInfoByAgentId[key];
                Destroy(MarkerInfoByAgentId[key].GameObject);
                MarkerInfoByAgentId.Remove(key);
            }
        }

        protected void OnNameReceived(Guid agentId, AvatarName avatarName)
        {
            if (MarkerInfoByAgentId.ContainsKey(agentId))
            {
                MarkerInfoByAgentId[agentId].ToolTipTarget.Text = avatarName.DisplayName;
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
    }
}
