using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Agents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [SerializeField] protected GameObject MarkerPrefab;
    [SerializeField] protected RectTransform MarkerContainer;

    protected class MarkerInfo
    {
        public GameObject GameObject;
        public RectTransform Transform;
        public Image Image;
        public TMP_Text Text;
    }

    protected Dictionary<Guid, MarkerInfo> MarkerInfoByAgentId = new Dictionary<Guid, MarkerInfo>();

    private void Start()
    {
        EventManager.Instance.OnCoarseLocationUpdateMessage += OnCoarseLocationUpdateMessage; // TODO: This should probably use some higher level mechanism to get updates. Possibly AvatarTracker
        EventManager.Instance.OnLogout += () =>
        {
            // Remove markers:
            for (int i = MarkerInfoByAgentId.Count - 1; i >= 0; i--)
            {
                Guid key = MarkerInfoByAgentId.Keys.ElementAt(i);
                MarkerInfo mi = MarkerInfoByAgentId[key];
                Destroy(MarkerInfoByAgentId[key].GameObject);
                MarkerInfoByAgentId.Remove(key);
            }
        };
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
                mi.Text = mi.GameObject.GetComponentInChildren<TMP_Text>(true);

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

            mi.Text.text = location.AgentId.ToString();

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

    protected void OnNameReceived(Guid agentId, AvatarName avatarName)
    {
        if (MarkerInfoByAgentId.ContainsKey(agentId))
        {
            MarkerInfoByAgentId[agentId].Text.text = avatarName.DisplayName;
        }
    }

    protected Vector2 ScalePosition(Vector3Byte position)
    {
        return new Vector2(MarkerContainer.rect.width * position.x / 255f, MarkerContainer.rect.height * position.z / 255f);
    }
}
