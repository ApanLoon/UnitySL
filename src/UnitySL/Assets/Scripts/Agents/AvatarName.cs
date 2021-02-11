using System;

namespace Assets.Scripts.Agents
{
    public class AvatarName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        public string UserName { get; set; } //=> $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// UNIX time in seconds when this name expires.
        /// </summary>
        public double ExpiresOn { get; set; }
    }
}
