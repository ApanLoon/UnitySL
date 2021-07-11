using System;
using System.Collections.Generic;
using System.Xml;
using Assets.Scripts.Extensions.UnityExtensions;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class PolySkeletalDistortionInfo
    {
        public List<PolySkeletalBoneInfo> BoneInfoList = new List<PolySkeletalBoneInfo>();

        public void ParseXml(XmlNode rootNode)
        {
            if (rootNode.NodeType != XmlNodeType.Element || rootNode.Name != "param_skeleton")
            {
                throw new Exception("Invalid root node.");
            }

            XmlElement rootElement = (XmlElement)rootNode;

            foreach (XmlNode boneNode in rootElement.ChildNodes)
            {
                if (boneNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (boneNode.Name)
                {
                    case "bone":
                        XmlElement boneElement = (XmlElement)boneNode;
                        string name = boneElement.GetAttribute("name");
                        if (string.IsNullOrEmpty(name))
                        {
                            Logger.LogWarning("PolySkeletalDistortionInfo.ParseXml", "No bone name specified for skeletal param.");
                            continue;
                        }

                        string s = boneElement.GetAttribute("scale");
                        if (string.IsNullOrEmpty(s))
                        {
                            Logger.LogWarning("PolySkeletalDistortionInfo.ParseXml", $"No scale specified for bone {name}.");
                            continue;
                        }
                        Vector3 scale = new Vector3();
                        scale.Parse(s);

                        // optional offset deformation (translation)
                        Vector3 pos = new Vector3();
                        bool hasPos = false;
                        s = boneElement.GetAttribute("offset");
                        if (string.IsNullOrEmpty(s) == false)
                        {
                            pos.Parse(s);
                            hasPos = true;
                        }

                        BoneInfoList.Add(new PolySkeletalBoneInfo(name, scale, pos, hasPos));
                        break;

                    default:
                        Logger.LogWarning("PolySkeletalDistortionInfo.ParseXml", $"Unrecognised element {boneNode.Name} in skeletal distortion");
                        break;
                }
            }

        }
    }
}