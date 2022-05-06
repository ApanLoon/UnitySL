
using System;
using System.Collections.Generic;
using System.Xml;
using Assets.Scripts.Extensions.SystemExtensions;

namespace Assets.Scripts.Appearance
{
    public class AvatarXmlInfo
    {
        public List<PolySkeletalDistortionInfo> SkeletalDistortionInfoList = new List<PolySkeletalDistortionInfo>();
        public List<AvatarAttachmentInfo> AttachmentInfoList = new List<AvatarAttachmentInfo>();
        public List<AvatarMeshInfo> MeshInfoList = new List<AvatarMeshInfo>();

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
                if (childNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (childNode.Name)
                {
                    case "param": // Skeleton distortions:
                        XmlNode paramNode = childNode.GetFirstChild();
                        switch (paramNode.Name)
                        {
                            case "param_skeleton":
                                PolySkeletalDistortionInfo skeletalDistortionInfo = new PolySkeletalDistortionInfo();
                                skeletalDistortionInfo.ParseXml(paramNode);
                                SkeletalDistortionInfoList.Add(skeletalDistortionInfo);
                                break;

                            case "param_morph":
                                throw new Exception("Can't specify morph parameter in skeleton definition.");
                            default:
                                throw new Exception($"Unknown parameter type in skeleton definition. ({paramNode.Name})");
                        }
                        break;

                    case "attachment_point":
                        AvatarAttachmentInfo attachmentInfo = new AvatarAttachmentInfo();
                        attachmentInfo.ParseXml(childNode);
                        AttachmentInfoList.Add(attachmentInfo);
                        break;
                }
            }

            return true;
        }

        public bool ParseXmlMeshNodes(XmlElement rootElement)
        {
            foreach (XmlNode childNode in rootElement.ChildNodes.Where(x => x.Name == "mesh"))
            {
                AvatarMeshInfo avatarMeshInfo = new AvatarMeshInfo();
                avatarMeshInfo.ParseXml(childNode);
                MeshInfoList.Add(avatarMeshInfo);
            }

            return true;
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
            avatarXmlInfo.ParseXmlMeshNodes(rootElement);
            //avatarXmlInfo.ParseXmlColorNodes(rootElement);
            //avatarXmlInfo.ParseXmlLayerNodes(rootElement);
            //avatarXmlInfo.ParseXmlDriverNodes(rootElement);
            //avatarXmlInfo.ParseXmlMorphNodes(rootElement);

            return avatarXmlInfo;

        }
    }
}
