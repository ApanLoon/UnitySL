using System;

namespace Assets.Scripts.Agents
{

    public class Relationship
    {
        [Flags]
        public enum Rights
        {
            None = 0,
            OnlineStatus = 1,
            MapLocation = 2,
            ModifyObjects = 4,

            VisibleMask = ModifyObjects | MapLocation
        }

        public static Relationship DefaultRelationship = new Relationship (Rights.OnlineStatus, Rights.OnlineStatus, false);
        public bool IsOnline 
        { 
            get => _isOnline;
            set
            {
                _isOnline = value;
                SerialNumber++;
            }
        }
        private bool _isOnline;

        public Rights GrantToAgent
        {
            get => _toAgent;
            protected set
            {
                _toAgent = value;
                SerialNumber++;
            }
        }
        private Rights _toAgent;

        public Rights GrantFromAgent
        {
            get => _fromAgent;
            protected set
            {
                _fromAgent = value;
                SerialNumber++;
            }
        }
        private Rights _fromAgent;

        /// <summary>
        /// Get the change count for this agent
        ///
        /// Every change to rights will increment the serial number
        /// allowing listeners to determine when a relationship value is actually new
        /// </summary>
        public int SerialNumber { get; protected set; }

        public bool IsRightGrantedTo (Rights rights)
        {
            return (GrantToAgent & rights) == rights;
        }

        public bool IsRightGrantedFrom (Rights rights)
        {
            return (GrantFromAgent & rights) == rights;
        }

        public void GrantRights (Rights toAgent, Rights fromAgent)
        {
            _toAgent |= toAgent;
            _fromAgent |= fromAgent;
            SerialNumber--; // The lines above both increment by one, compensate to make the number increase by one instead of two.
        }

        public void RevokeRights (Rights toAgent, Rights fromAgent)
        {
            _toAgent &= ~toAgent;
            _fromAgent &= ~fromAgent;
            SerialNumber--; // The lines above both increment by one, compensate to make the number increase by one instead of two.
        }

        public Relationship()
        {
            GrantToAgent = Rights.None;
            GrantFromAgent = Rights.None;
            IsOnline = false;
            SerialNumber = 0;
        }

        public Relationship (Rights toAgent, Rights fromAgent, bool isOnline)
        {
            GrantToAgent = toAgent;
            GrantFromAgent = fromAgent;
            IsOnline = isOnline;
            SerialNumber = 0;
        }
    }
}
