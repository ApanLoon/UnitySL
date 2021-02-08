
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Agents
{
    public class TrackingData
    {
        public bool HasData { get; set; }
        public bool HasCoarseData { get; set; }

        public Guid AvatarId { get; set; }
        public string Name { get; set; }
        public Vector3Double CoarseLocation { get; set; }
    }

    public class FriendObserver
    {
        [Flags]
        public enum ChangeType : UInt32
        {
            None   = 0,
            Add    = 1,
            Remove = 2,
            Online = 4,
            Powers = 8,

            All = 0xffffffff
        }

        public virtual void Changed(ChangeType changeType)
        {
        }
    }

    public class AvatarTracker
    {
        public static AvatarTracker Instance { get; } = new AvatarTracker();
        public static readonly float CoarseFrequency  = 2.2f;
        public static readonly float FindFrequency    = 29.7f; // This results in a database query, so cut these back
        public static readonly float OfflineSeconds   = FindFrequency + 8f;

        public Dictionary<Guid, Relationship> BuddyInfo = new Dictionary<Guid, Relationship>();

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
                    Logger.LogWarning($"!! Add buddy for existing buddy: {agentId}"
                                      + $" [{(existingRelationship.IsOnline ? "Online" : "Offline")} -> {(newRelationship.IsOnline ? "Online" : "Offline")}"
                                      + $", {existingRelationship.GrantToAgent} -> {newRelationship.GrantToAgent}"
                                      + $", {existingRelationship.GrantFromAgent} -> {newRelationship.GrantFromAgent}"
                                      + "]");
                }
                else
                {
                    BuddyInfo[agentId] = kv.Value;

                    //TODO: Do things with AvatarNameCache
                    //// pre-request name for notifications?
                    //LLAvatarName av_name;
                    //LLAvatarNameCache::get(agent_id, &av_name);

                    AddChangedMask (FriendObserver.ChangeType.Add, agentId);
                    
                    Logger.LogDebug($"Added buddy {agentId}, {(BuddyInfo[agentId].IsOnline ? "Online" : "Offline")}, TO: {BuddyInfo[agentId].GrantToAgent}, FROM: {BuddyInfo[agentId].GrantFromAgent}");
                }
            }
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
    }
}
