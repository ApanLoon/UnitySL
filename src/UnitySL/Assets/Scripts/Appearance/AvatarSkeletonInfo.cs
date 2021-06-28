
using System;
using System.Collections.Generic;
using System.Xml;

namespace Assets.Scripts.Appearance
{
    public class AvatarSkeletonInfo
    {
        public int NumBones { get; protected set; }
        public int NumCollisionVolumes { get; protected set; }

        public List<AvatarBoneInfo> BoneInfoList = new List<AvatarBoneInfo>();

        public static AvatarSkeletonInfo ParseXml(XmlNode root)
        {
            if (root.NodeType != XmlNodeType.Element)
            {
                throw new Exception("Invalid root node.");
            }

            XmlElement rootElement = (XmlElement)root;

            AvatarSkeletonInfo avatarSkeletonInfo = new AvatarSkeletonInfo();

            avatarSkeletonInfo.NumBones = ParseInt(rootElement.GetAttribute("num_bones"), "Couldn't find number of bones.", "Couldn't parse number of bones");
            int.TryParse(rootElement.GetAttribute("num_collision_volumes"), out var i);
            avatarSkeletonInfo.NumCollisionVolumes = i;

            foreach (XmlNode childNode in rootElement.ChildNodes)
            {
                AvatarBoneInfo boneInfo = AvatarBoneInfo.ParseXml(childNode);
                avatarSkeletonInfo.BoneInfoList.Add(boneInfo);
            }

            return avatarSkeletonInfo;
        }

        protected static int ParseInt(string s, string emptyMessage, string parseErrorMessage)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception(emptyMessage);
            }

            try
            {
                return int.Parse(s);
            }
            catch (Exception e)
            {
                throw new Exception(parseErrorMessage, e);
            }
        }
    }
}
