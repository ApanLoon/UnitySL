using System;

namespace Assets.Scripts.Agents
{
    public class AvatarName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        /// <summary>
        /// True if the DisplayName is constructed from FirstName and LastName
        /// </summary>
        public bool IsDisplayNameLegacy { get; set; }

        /// <summary>
        /// UNIX time in seconds when this name expires.
        /// </summary>
        public double ExpiresOn { get; set; }

        public string GetCompleteName (bool useParenthesis = true, bool forceCompleteName = false)
        {
            bool useDisplayNames = Settings.instance.general.useDisplayNames;
            if (!useDisplayNames && !forceCompleteName)
            {
                return GetUserName();
            }

            if (string.IsNullOrEmpty(GetUserName()) || IsDisplayNameLegacy)
            {
                return DisplayName;
            }

            string name = DisplayName;
            bool useUserNames = Settings.instance.general.useUserNames;
            if (useUserNames ||forceCompleteName)
            {
                name += useParenthesis ? $" ({GetUserName()})" : $" [{GetUserName()}]";
            }

            return name;
        }

        public string GetUserName(bool lowerCase = false)
        {
            if (string.IsNullOrEmpty(LastName) || LastName == "Resident")
            {
                // If we cannot create a user name from the legacy strings, use the display name. If the LastName is "Resident" omit it.
                return string.IsNullOrEmpty(FirstName) ? DisplayName : FirstName;
            }
            else
            {
                return lowerCase ? $"{FirstName}.{LastName}".ToLower() : $"{FirstName} {LastName}";
            }
        }
    }
}
