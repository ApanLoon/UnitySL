using System;
using System.Collections.Generic;
using Assets.Scripts.Agents;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.UI.Floater.People
{
    public class People : MonoBehaviour
    {
        [SerializeField] protected Transform ListContent;
        [SerializeField] protected GameObject ListItemPrefab;

        protected Dictionary<Guid, FriendListItem> ListItemByAgentId = new Dictionary<Guid, FriendListItem>();

        protected class PeopleFriendsObserver : AvatarTracker.FriendObserver
        {
            protected People People;

            public PeopleFriendsObserver(People people)
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
            AvatarTracker.Instance.AddObserver(new PeopleFriendsObserver(this));
            GetFriends(); // TODO: There is no real point in calling this here as the friends list has not been populated yet.
        }

        protected void GetFriends()
        {
            AvatarTracker.CollectAllBuddies functor = new AvatarTracker.CollectAllBuddies();
            AvatarTracker.Instance.ApplyFunctor(functor);

            for (int i = 0; i < ListContent.childCount; i++)
            {
                Destroy(ListContent.GetChild(i).gameObject);
            }
            ListItemByAgentId.Clear();

            foreach (KeyValuePair<Guid, string> kv in functor.BuddiesOnline)
            {
                CreateItem(kv.Key, kv.Value);
            }
            foreach (KeyValuePair<Guid, string> kv in functor.BuddiesOffline)
            {
                CreateItem(kv.Key, kv.Value);
            }
        }

        protected void CreateItem (Guid agentId, string name)
        {
            GameObject go = Instantiate(ListItemPrefab, ListContent);
            FriendListItem friendListItem = go.GetComponent<FriendListItem>();
            ListItemByAgentId[agentId] = friendListItem;
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
            friendListItem.Set(name, AvatarTracker.Instance.BuddyInfo[agentId]);
        }
    }
}
