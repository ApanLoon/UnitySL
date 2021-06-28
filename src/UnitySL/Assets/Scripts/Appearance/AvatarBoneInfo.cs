
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class AvatarBoneInfo
    {
        public string Name { get; set; }
        public string Support { get; set; }
        public string Aliases { get; set; }
        public bool IsJoint { get; set; } = false;
        public Vector3 Pos { get; set; }
        public Vector3 End { get; set; }
        public Vector3 Rot { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Pivot { get; set; }
        public List<AvatarBoneInfo> Children = new List<AvatarBoneInfo>();

        public static AvatarBoneInfo ParseXml(XmlNode root)
        {
            if (root.NodeType != XmlNodeType.Element)
            {
                throw new Exception($"Invalid root node. ({root.NodeType})");
            }

            AvatarBoneInfo info = new AvatarBoneInfo();

            XmlElement rootElement = (XmlElement) root;
            switch (root.Name)
            {
                case "bone":
                    info.IsJoint = true;
                    info.Name = rootElement.GetAttribute("name");
                    if (string.IsNullOrEmpty(info.Name))
                    {
                        throw new Exception("Bone without name.");
                    }

                    info.Aliases = rootElement.GetAttribute("aliases"); //Aliases are not required.
                    break;

                case "collision_volume":
                    info.IsJoint = false;
                    info.Name = rootElement.GetAttribute("name");
                    if (string.IsNullOrEmpty(info.Name))
                    {
                        info.Name = "Collision Volume";
                    }
                    break;

                default:
                    throw new Exception($"Invalid root node. ({root.Name})");
            }

            info.Pos = ParseVector3(rootElement.GetAttribute("pos"), "Bone without position. ({info.Name})", "Couldn't parse position. ({info.Name})");
            info.Rot = ParseVector3(rootElement.GetAttribute("rot"), "Bone without rotation. ({info.Name})", "Couldn't parse rotation. ({info.Name})");
            info.Scale = ParseVector3(rootElement.GetAttribute("scale"), "Bone without scale. ({info.Name})", "Couldn't parse scale. ({info.Name})");
            
            info.End = ParseVector3(rootElement.GetAttribute("end"), "Bone without end. ({info.Name})", "Couldn't parse end. ({info.Name})", true);
            
            info.Support = rootElement.GetAttribute("support");
            if (string.IsNullOrEmpty(info.Support))
            {
                Logger.LogWarning("AvatarBoneInfo.ParseXml", $"Bone without support. ({info.Name})");
            }

            if (info.IsJoint)
            {
                info.Pivot = ParseVector3(rootElement.GetAttribute("pivot"), "Bone without pivot. ({info.Name})", "Couldn't parse pivot. ({info.Name})");
            }

            // Parse children:
            foreach (XmlNode childNode in rootElement.ChildNodes)
            {
                AvatarBoneInfo childInfo = AvatarBoneInfo.ParseXml(childNode);
                info.Children.Add(childInfo);
            }

            return info;
        }

        protected static Vector3 ParseVector3(string s, string emptyMessage, string parseErrorMessage, bool isOptional = false)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (!isOptional)
                {
                    throw new Exception(emptyMessage);
                }

                Logger.LogWarning("AvatarBoneInfo.ParseVector3", emptyMessage);
                return Vector3.zero;
            }

            try
            {
                Vector3 v = new Vector3();
                int i = 0;
                foreach (string c in s.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    v[i++] = float.Parse(c);
                }
                return v;
            }
            catch (Exception e)
            {
                throw new Exception(parseErrorMessage, e);
            }
        }
    }
}
