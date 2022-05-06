using System;
using System.Xml;
using Assets.Scripts.Extensions.UnityExtensions;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class AvatarAttachmentInfo
    {
        public string Name { get; set; }
        public string Joint { get; set; }
        public Vector3 Position { get; set; }
        public bool HasPosition { get; set; }
        public Vector3 RotationEuler { get; set; }
        public bool HasRotationEuler { get; set; }
        public Int32 Group { get; set; }
        public Int32 AttachmentId { get; set; }
        public Int32 PieMenuSlice { get; set; } // TODO: Should we even have a pie menu?
        public bool IsVisibleInFirstPerson { get; set; }
        public bool IsHudAttachment { get; set; }

        public void ParseXml(XmlNode rootNode)
        {
            if (rootNode.NodeType != XmlNodeType.Element)
            {
                throw new Exception("Invalid root node.");
            }
            XmlElement rootElement = (XmlElement)rootNode;

            Name = rootElement.GetAttribute("name").Trim();
            if (string.IsNullOrEmpty(Name))
            {
                throw new Exception("No name supplied for attachment point.");
            }

            Joint = rootElement.GetAttribute("joint").Trim();
            if (string.IsNullOrEmpty(Joint))
            {
                throw new Exception($"No bone (joint) supplied for attachment point {Name}.");
            }

            string s = rootElement.GetAttribute("position").Trim();
            HasPosition = false;
            if (string.IsNullOrEmpty(s) == false)
            {
                Position.Parse(s);
                HasPosition = true;
            }

            s = rootElement.GetAttribute("rotation").Trim();
            HasRotationEuler = false;
            if (string.IsNullOrEmpty(s) == false)
            {
                RotationEuler.Parse(s);
                HasRotationEuler = true;
            }

            s = rootElement.GetAttribute("group").Trim();
            Group = -1111; // Bad value
            if (string.IsNullOrEmpty(s) == false)
            {
                Group = Int32.Parse(s);
            }

            s = rootElement.GetAttribute("id").Trim();
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception($"No id supplied for attachment point {Name}.");
            }
            AttachmentId = Int32.Parse(s);

            s = rootElement.GetAttribute("pie_slice").Trim();
            PieMenuSlice = 0;
            if (string.IsNullOrEmpty(s) == false)
            {
                PieMenuSlice = Int32.Parse(s);
            }

            s = rootElement.GetAttribute("visible_in_first_person").Trim();
            IsVisibleInFirstPerson = false;
            if (string.IsNullOrEmpty(s) == false)
            {
                IsVisibleInFirstPerson = bool.Parse(s);
            }

            s = rootElement.GetAttribute("hud").Trim();
            IsHudAttachment = false;
            if (string.IsNullOrEmpty(s) == false)
            {
                IsHudAttachment = bool.Parse(s);
            }
        }
    }
}