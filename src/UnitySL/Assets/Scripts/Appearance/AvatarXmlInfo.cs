
using System;
using System.Xml;
using Assets.Scripts.Extensions.SystemExtensions;

namespace Assets.Scripts.Appearance
{
    public class AvatarXmlInfo
    {
        public bool ParseXmlSkeletonNode(XmlElement rootElement)
        {
            XmlNode skeletonNode = rootElement.ChildNodes.GetChildByName("skeleton");
            if (skeletonNode == null || skeletonNode.NodeType != XmlNodeType.Element)
            {
                throw new Exception("Missing or invalid skeleton node");
            }
            XmlElement skeletonElement = (XmlElement) skeletonNode;

            foreach (XmlNode childNode in skeletonElement.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "param": // Skeleton distortions:
                        // TODO: Not implemented, see line 1805 in llavatarappearance.cpp
                        break;

                    case "attachment_point":
                        // TODO: Not implemented
                        break;
                }
            }

            return true;
        }
        public bool ParseXmlMeshNodes(XmlElement rootElement)
        {
            throw new NotImplementedException();
        }
        public bool ParseXmlColorNodes(XmlElement rootElement)
        {
            throw new NotImplementedException();
        }
        public bool ParseXmlLayerNodes(XmlElement rootElement)
        {
            throw new NotImplementedException();
        }
        public bool ParseXmlDriverNodes(XmlElement rootElement)
        {
            throw new NotImplementedException();
        }
        public bool ParseXmlMorphNodes(XmlElement rootElement)
        {
            throw new NotImplementedException();
        }

        public static AvatarXmlInfo ParseXml(XmlNode root)
        {
            if (root.NodeType != XmlNodeType.Element)
            {
                throw new Exception("Invalid root node.");
            }

            XmlElement rootElement = (XmlElement)root;

            AvatarXmlInfo avatarXmlInfo = new AvatarXmlInfo();

            avatarXmlInfo.ParseXmlSkeletonNode(rootElement);
            //avatarXmlInfo.ParseXmlMeshNodes(rootElement);
            //avatarXmlInfo.ParseXmlColorNodes(rootElement);
            //avatarXmlInfo.ParseXmlLayerNodes(rootElement);
            //avatarXmlInfo.ParseXmlDriverNodes(rootElement);
            //avatarXmlInfo.ParseXmlMorphNodes(rootElement);

            return avatarXmlInfo;

        }
    }
}
