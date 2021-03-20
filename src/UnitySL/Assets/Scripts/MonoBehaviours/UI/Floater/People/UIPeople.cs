using System;
using System.Collections.Generic;
using Assets.Scripts.Agents;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MonoBehaviours.UI.Floater.People
{
    public class UIPeople : MonoBehaviour
    {
        [SerializeField] protected FriendListItemTemplate friendsListItems;
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected RectTransform menuPanelContainer;

        protected Dictionary<Guid, FriendListItem> ListItemByAgentId = new Dictionary<Guid, FriendListItem>();

        protected class PeopleFriendsObserver : AvatarTracker.FriendObserver
        {
            protected UIPeople People;

            public PeopleFriendsObserver(UIPeople people)
            {
                People = people;
            }

            public override void Changed(ChangeType change)
            {
                Logger.LogDebug($"People.PeopleFriendsObserver.Changed: {change}");

                switch (change)
                {
                    case ChangeType.Add:
                    case ChangeType.Remove:
                    case ChangeType.Online:
                        People.GetFriends();
                        break;
                }
            }
        }

        private void Start()
        {
            friendsListItems.Initialize();
            AvatarTracker.Instance.AddObserver(new PeopleFriendsObserver(this));
            GetFriends(); // TODO: There is no real point in calling this here as the friends list has not been populated yet.
        }

        protected void GetFriends()
        {
            AvatarTracker.CollectAllBuddies functor = new AvatarTracker.CollectAllBuddies();
            AvatarTracker.Instance.ApplyFunctor(functor);

            friendsListItems.Clear();
            ListItemByAgentId.Clear();

            foreach (KeyValuePair<Guid, string> kv in functor.BuddiesOnline)
            {
                CreateItem(kv.Key, kv.Value);
            }
            foreach (KeyValuePair<Guid, string> kv in functor.BuddiesOffline)
            {
                CreateItem(kv.Key, kv.Value);
            }
            Canvas.ForceUpdateCanvases(); // Rebuilding ALL canvases is bad but so is UGUI.
            menuPanelContainer.sizeDelta = new Vector2(scrollRect.viewport.sizeDelta.x, 0);
        }

        protected void CreateItem(Guid agentId, string name)
        {
            FriendListItem item = friendsListItems.InstantiateTemplate();
            ListItemByAgentId[agentId] = item;
            if (string.IsNullOrEmpty(name)) // TODO: Ideally the names would be cached when the functor is applied, but when adding the buddy list to the AvatarTracker and that causes an observer update, they aren't.
            {
                name = agentId.ToString();

                AvatarNameCache.Instance.Get(agentId, (id, avatarName) =>
                {
                    if (ListItemByAgentId.ContainsKey(id))
                    {
                        ListItemByAgentId[id].SetName(avatarName.DisplayName);
                    }
                });
            }
            item.Set(name, AvatarTracker.Instance.GetBuddyInfo(agentId));
        }
    }

    [Serializable] public class FriendListItemTemplate : Template<FriendListItem> { };
}
