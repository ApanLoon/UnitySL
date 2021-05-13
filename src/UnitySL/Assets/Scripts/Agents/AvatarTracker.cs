
using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent;

namespace Assets.Scripts.Agents
{
    public class AvatarTracker
    {
        public static AvatarTracker Instance { get; } = new AvatarTracker();
        public static readonly float CoarseFrequency  = 2.2f;
        public static readonly float FindFrequency    = 29.7f; // This results in a database query, so cut these back
        public static readonly float OfflineSeconds   = FindFrequency + 8f;

        public Dictionary<Guid, Relationship> BuddyInfo = new Dictionary<Guid, Relationship>();

        protected TrackingData CurrentTrackingData;

        /// <summary>
        /// Used to make sure that no call to NotifyObservers will do anything if a previous call is still in progress.
        ///
        ///  TODO: This feels like a clunky way to do this.
        /// 
        /// </summary>
        private bool _isNotifyObservers = false;

        protected List<FriendObserver> Observers = new List<FriendObserver>();
        protected Dictionary<Guid, HashSet<FriendObserver>> ParticularFriendObserverMap { get; set; } = new Dictionary<Guid, HashSet<FriendObserver>>();

        protected FriendObserver.ChangeType ModifyMask { get; set; }
        protected HashSet<Guid> ChangedBuddyIds { get; set; } = new HashSet<Guid>();


        public void AddBuddyList(Dictionary<Guid, Relationship> list)
        {
            foreach (KeyValuePair<Guid, Relationship> kv in list)
            {
                Guid agentId = kv.Key;
                if (BuddyInfo.ContainsKey(agentId))
                {
                    Relationship existingRelationship = BuddyInfo[agentId];
                    Relationship newRelationship = kv.Value;
                    Logger.LogWarning("AvatarTracker.AddBuddyList", $"!! Add buddy for existing buddy: {agentId}"
                                      + $" [{(existingRelationship.IsOnline ? "Online" : "Offline")} -> {(newRelationship.IsOnline ? "Online" : "Offline")}"
                                      + $", {existingRelationship.GrantToAgent} -> {newRelationship.GrantToAgent}"
                                      + $", {existingRelationship.GrantFromAgent} -> {newRelationship.GrantFromAgent}"
                                      + "]");
                }
                else
                {
                    BuddyInfo[agentId] = kv.Value;

                    AvatarNameCache.Instance.Get(agentId, OnAvatarNameReceived);

                    AddChangedMask (FriendObserver.ChangeType.Add, agentId);

                    Logger.LogDebug("AvatarTracker.AddBuddyList", $"Added buddy {agentId}, {(BuddyInfo[agentId].IsOnline ? "Online" : "Offline")}, TO: {BuddyInfo[agentId].GrantToAgent}, FROM: {BuddyInfo[agentId].GrantFromAgent}");
                }
            }
            NotifyObservers(); // TODO: Adding the change mask won't trigger observer notification as we don't have a main loop thingy.
        }

        public void ClearBuddyList()
        {
            foreach (KeyValuePair<Guid, Relationship> kv in BuddyInfo)
            {
                AddChangedMask(FriendObserver.ChangeType.Remove, kv.Key);
            }
            BuddyInfo.Clear();
            NotifyObservers(); // TODO: Adding the change mask won't trigger observer notification as we don't have a main loop thingy.
            // TODO: Do I need to do anything else here?
        }

        protected void OnAvatarNameReceived (Guid agentId, AvatarName avatarName)
        {
            //Logger.LogDebug("AvatarTracker.OnAvatarNameReceived", $"FirstName=\"{avatarName.FirstName}\", LastName=\"{avatarName.LastName}\", DisplayName=\"{avatarName.DisplayName}\"");
        }


        protected void AddChangedMask(FriendObserver.ChangeType changeType, Guid agentId)
        {
            ModifyMask |= changeType;
            if (agentId == Guid.Empty)
            {
                return;
            }

            ChangedBuddyIds.Add(agentId);
        }

        public void TerminateBuddy(Guid agentId)
        {
            //TODO: Send the "TerminateFriendship" Message through the circuit.
        }

        public Relationship GetBuddyInfo(Guid agentId)
        {
            return BuddyInfo.ContainsKey(agentId) ? BuddyInfo[agentId] : null;
        }

        public bool IsBuddy(Guid agentId)
        {
            return BuddyInfo.ContainsKey(agentId);
        }

        public void SetBuddyOnline(Guid agentId, bool isOnline)
        {
            if (BuddyInfo.ContainsKey(agentId) == false)
            {
                Logger.LogWarning("AvatarTracker.SetBuddyOnline", $"!! No buddy info found for {agentId}, setting to {(isOnline ? "Online" : "Offline")}");
                return;
            }

            Relationship info = BuddyInfo[agentId];
            info.IsOnline = isOnline;
            AddChangedMask(FriendObserver.ChangeType.Online, agentId);
            Logger.LogDebug("AvatarTracker.SetBuddyOnline", $"Set buddy {agentId} {(isOnline ? "Online" : "Offline")}");
        }

        public bool IsBuddyOnline(Guid agentId)
        {
            return BuddyInfo.ContainsKey(agentId) && BuddyInfo[agentId].IsOnline;
        }

        public void DeleteTrackingData()
        {
            CurrentTrackingData = null;
        }

        public void FindAgent()
        {
            if (CurrentTrackingData == null || CurrentTrackingData.AvatarId == Guid.Empty)
            {
                return;
            }

            // TODO: Send "FindAgent" message through circuit
        }

        public void AddObserver(FriendObserver observer)
        {
            if (observer == null)
            {
                return;
            }
            Observers.Add(observer);
        }

        public void RemoveObserver(FriendObserver observer)
        {
            if (observer == null)
            {
                return;
            }

            Observers.Remove(observer);
        }

        protected void NotifyObservers() // TODO: It is unclear when this is supposed to be called as we don't have an "idle loop"
        {
            if (_isNotifyObservers)
            {
                // Don't allow multiple calls.
                // new masks and ids will be processed later from idle.
                return;
            }
            _isNotifyObservers = true;

            foreach (FriendObserver observer in Observers)
            {
                observer.Changed(ModifyMask);
            }

            foreach (Guid buddyId in ChangedBuddyIds)
            {
                NotifyParticularFriendObservers(buddyId);
            }

            ModifyMask = FriendObserver.ChangeType.None;
            ChangedBuddyIds.Clear();
            _isNotifyObservers = false;
        }

        public void AddParticularFriendObserver(Guid buddy_id, FriendObserver observer)
        {
            if (buddy_id != Guid.Empty && observer != null)
            {
                if (ParticularFriendObserverMap.ContainsKey(buddy_id) == false)
                {
                    ParticularFriendObserverMap[buddy_id] = new HashSet<FriendObserver>();
                }
                ParticularFriendObserverMap[buddy_id].Add(observer);
            }
        }

        public void RemoveParticularFriendObserver(Guid buddy_id, FriendObserver observer)
        {
            if (buddy_id == Guid.Empty || observer == null)
            {
                return;
            }

            if (ParticularFriendObserverMap.ContainsKey(buddy_id) == false)
            {
                return;
            }

            ParticularFriendObserverMap[buddy_id].Remove(observer);

            // purge empty sets from the map
            if (ParticularFriendObserverMap[buddy_id].Count == 0)
            {
                ParticularFriendObserverMap.Remove(buddy_id);
            }
        }

        protected void NotifyParticularFriendObservers(Guid buddy_id)
        {
            if (ParticularFriendObserverMap.ContainsKey(buddy_id) == false)
            {
                return;
            }

            // Notify observers interested in buddy_id.
            foreach (FriendObserver observer in ParticularFriendObserverMap[buddy_id])
            {
                observer.Changed(ModifyMask);
            }
        }

        /// <summary>
        /// Applies a functor to every buddy in the buddy list. The functor may operate on or store the values the Add method is given.
        ///
        /// Do not actually modify the buddy list in the functor or bad things will happen.
        /// 
        /// </summary>
        /// <param name="functor"></param>
        public void ApplyFunctor (RelationshipFunctor functor)
        {
            foreach (KeyValuePair<Guid, Relationship> kv in BuddyInfo)
            {
                functor.Add(kv.Key, kv.Value);
            }
        }

        public void RegisterCallbacks()
        {
            //EventManager.Instance.OnFindAgentMessage += OnFindAgentMessage; //TODO: Create event for FindAgent
            EventManager.Instance.OnOnlineNotificationMessage += OnOnlineNotificationMessage;
            EventManager.Instance.OnOfflineNotificationMessage += OnOfflineNotificationMessage;
            //EventManager.Instance.OnTerminateFriendshipMessage += OnTerminateFriendshipMessage; //TODO: Create event for TerminateFriendship
            //EventManager.Instance.OnChangeUserRightsMessage += OnChangeUserRightsMessage; //TODO: Create event for ChangeUserRights
        }

        protected void OnOnlineNotificationMessage (OnlineNotificationMessage message)
        {
            //Logger.LogDebug("LLAvatarTracker::processOnlineNotification()", "");
            ProcessNotify (message.Agents, true);
        }

        protected void OnOfflineNotificationMessage(OfflineNotificationMessage message)
        {
            //Logger.LogDebug("LLAvatarTracker::processOfflineNotification()", "");
            ProcessNotify (message.Agents, false);
        }

        private void ProcessNotify(List<Guid> agents, bool isOnline)
        {
            int count = agents.Count;
            bool chatNotify = Settings.Instance.chat.notifyOnlineStatus;

            //Logger.LogDebug("AvatarTracker.ProcessNotify", $"Received {count} online notifications **** ");
            if (count <= 0)
            {
                return;
            }

            Guid trackingId;
            if (CurrentTrackingData != null)
            {
                trackingId = CurrentTrackingData.AvatarId;
            }
            for (int i = 0; i < count; i++)
            {
                Guid agentId = agents[i];
                Relationship info = GetBuddyInfo(agentId);
                if (info != null)
                {
                    SetBuddyOnline (agentId, isOnline);
                }
                else
                {
                    Logger.LogWarning("AvatarTracker.ProcessNotify", $"Received online notification for unknown buddy: {agentId} is {(isOnline ? "ONLINE" : "OFFLINE")}");
                }

                if (trackingId == agentId)
                {
                    // we were tracking someone who went offline
                    DeleteTrackingData();
                }

                //TODO: Update online status in calling card:
                //// *TODO: get actual inventory id
                //gInventory.addChangedMask(LLInventoryObserver::CALLING_CARD, LLUUID::null);
            }
            if (chatNotify)
            {
                // Look up the name of this agent for the notification
                // Hmm.. Not quite this: AvatarNameCache.Instance.Get(agentId, (id, avatarName) => OnAvatarNameCacheNotify(id, avatarName, isOnline))
                // TODO: LLAvatarNameCache::get(agent_id, boost::bind(&on_avatar_name_cache_notify, _1, _2, online, payload));
            }

            ModifyMask |= FriendObserver.ChangeType.Online;
            NotifyObservers();
            // TODO: Notify inventory observers: gInventory.notifyObservers();

        }

        #region Tracking
        public class TrackingData
        {
            public bool HasData { get; set; }
            public bool HasCoarseData { get; set; }

            public Guid AvatarId { get; set; }
            public string Name { get; set; }
            public Vector3Double CoarseLocation { get; set; }
        }
        #endregion Tracking

        #region FriendObserver
        public class FriendObserver
        {
            [Flags]
            public enum ChangeType : UInt32
            {
                None = 0,
                Add = 1,
                Remove = 2,
                Online = 4,
                Powers = 8,

                All = 0xffffffff
            }

            public virtual void Changed(ChangeType changeType)
            {
            }
        }
        #endregion FriendObserver

        #region RelationshipFunctor
        /// <summary>
        /// This is used as a base class for doing operations on all buddies.
        /// </summary>
        public abstract class RelationshipFunctor
        {
            public abstract bool Add(Guid buddyId, Relationship relationship);
        }

        /// <summary>
        /// Collect set of LLUUIDs we're a proxy for
        /// </summary>
        public class CollectProxyBuddies : RelationshipFunctor
        {
            public HashSet<Guid> Buddies { get; protected set; } = new HashSet<Guid>();
            public override bool Add(Guid buddyId, Relationship relationship)
            {
                if (relationship.IsRightGrantedFrom(Relationship.Rights.ModifyObjects))
                {
                    Buddies.Add(buddyId);
                }
                return true;
            }
        };

        /// <summary>
        /// Collect dictionary sorted map of name -> agent_id for every online buddy
        /// </summary>
        public class CollectMappableBuddies : RelationshipFunctor
        {
            public Dictionary<Guid, string> Buddies { get; protected set; } = new Dictionary<Guid, string>();
            public override bool Add(Guid buddyId, Relationship relationship)
            {
                if (relationship.IsOnline && relationship.IsRightGrantedFrom(Relationship.Rights.MapLocation))
                {
                    AvatarName avatarName = AvatarNameCache.Instance.GetImmediate(buddyId);
                    Buddies[buddyId] = avatarName != null ? avatarName.DisplayName : "";
                }

                return true;
            }
        };

        // Collect dictionary sorted map of name -> agent_id for every online buddy
        public class CollectOnlineBuddies : RelationshipFunctor
        {
            public Dictionary<Guid, string> Buddies { get; protected set; } = new Dictionary<Guid, string>();
            public override bool Add(Guid buddyId, Relationship relationship)
            {
                if (relationship.IsOnline)
                {
                    AvatarName avatarName = AvatarNameCache.Instance.GetImmediate(buddyId);
                    Buddies[buddyId] = avatarName != null ? avatarName.GetUserName() : "";
                }

                return true;
            }

        };

        // collect dictionary sorted map of name -> agent_id for every buddy, one map is offline and the other map is online.
        public class CollectAllBuddies : RelationshipFunctor
        {
            public Dictionary<Guid, string> BuddiesOnline { get; protected set; } = new Dictionary<Guid, string>();
            public Dictionary<Guid, string> BuddiesOffline { get; protected set; } = new Dictionary<Guid, string>();
            public override bool Add(Guid buddyId, Relationship relationship)
            {
                AvatarName avatarName = AvatarNameCache.Instance.GetImmediate(buddyId);
                if (relationship.IsOnline)
                {
                    BuddiesOnline[buddyId] = avatarName != null ? avatarName.GetCompleteName() : "";
                }
                else
                {
                    BuddiesOffline[buddyId] = avatarName != null ? avatarName.GetCompleteName() : "";
                }

                return true;
            }
        };

        #endregion RelationshipFunctor

    }
}
