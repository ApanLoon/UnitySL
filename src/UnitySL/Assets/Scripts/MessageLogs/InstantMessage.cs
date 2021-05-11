using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;
using Assets.Scripts.Extensions.UnityExtensions;
using UnityEngine;

namespace Assets.Scripts.MessageLogs
{
    public class InstantMessage : LogMessage
    {
        public Guid AgentId { get; set; }
        public Guid SessionId { get; set; }

        public bool IsFromGroup { get; set; }
        public Guid ToAgentId { get; set; }
        public UInt32 ParentEstateId { get; set; }
        public Guid RegionId { get; set; }
        public Vector3 Position { get; set; }
        public OnlineMode OnlineMode { get; set; }
        public DialogType DialogType { get; set; }
        public Guid Id { get; set; }
        public string MessageText { get; set; }
        public byte[] BinaryBucket { get; set; }

        public InstantMessage(ImprovedInstantMessageMessage message)
        {
            AgentId = message.AgentId;
            SessionId = message.SessionId;
            IsFromGroup = message.IsFromGroup;
            ToAgentId = message.ToAgentId;
            ParentEstateId = message.ParentEstateId;
            RegionId = message.RegionId;
            Position = message.Position;
            OnlineMode = message.OnlineMode;
            DialogType = message.DialogType;
            Id = message.Id;
            MessageText = message.MessageText;
            SenderName = message.FromAgentName;
            BinaryBucket = message.BinaryBucket;

            Timestamp = DateTimeOffset.Now;
        }

        //TODO: Colours should be fetched from a UIPalette for consistency
        protected Color NameColour = new Color(0.7333f, 0.898f, 0.4824f, 1.0f);

        public override string ToRtfString()
        {
            string s = $"[{Timestamp:t}] {NameColour.ToRtfString()}{SenderName}</color>";
            switch (DialogType)
            {
                case DialogType.NothingSpecial:
                    s = $"{s}: {MessageText}";
                    break;

                case DialogType.LureUser:
                    s = $"{DialogType} (Teleport offer): {MessageText}";
                    break;

                case DialogType.TeleportRequest:
                    s = $"{DialogType}: {MessageText}";
                    break;

                case DialogType.TypingStart:
                    s = $"{s} started typing.";
                    break;

                case DialogType.TypingStop:
                    s = $"{s} stopped typing.";
                    break;

                case DialogType.MessageBox:
                case DialogType.GroupInvitation:
                case DialogType.InventoryOffered:
                case DialogType.InventoryAccepted:
                case DialogType.InventoryDeclined:
                case DialogType.GroupVote:
                case DialogType.GroupMessage_DEPRECATED:
                case DialogType.TaskInventoryOffered:
                case DialogType.TaskInventoryAccepted:
                case DialogType.TaskInventoryDeclined:
                case DialogType.NewUserDefault:
                case DialogType.SessionInvite:
                case DialogType.SessionP2PInvite:
                case DialogType.SessionGroupStart:
                case DialogType.SessionConferenceStart:
                case DialogType.SessionSend:
                case DialogType.SessionLeave:
                case DialogType.FromTask:
                case DialogType.DoNotDisturbAutoResponse:
                case DialogType.ConsoleAndChatHistory:
                case DialogType.LureAccepted:
                case DialogType.LureDeclined:
                case DialogType.GodLikeLureUser:
                case DialogType.GroupElection_DEPRECATED:
                case DialogType.GotoUrl:
                case DialogType.FromTaskAsAlert:
                case DialogType.GroupNotice:
                case DialogType.GroupNoticeInventoryAccepted:
                case DialogType.GroupNoticeInventoryDeclined:
                case DialogType.GroupInvitationAccept:
                case DialogType.GroupInvitationDecline:
                case DialogType.GroupNoticeRequested:
                case DialogType.FriendshipOffered:
                case DialogType.FriendshipAccepted:
                case DialogType.FriendshipDeclined_DEPRECATED:
                    s = $"{s}: {DialogType}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return s;
        }
    }
}
