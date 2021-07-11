
using System;
using System.Collections.Generic;
using System.Xml;
using Assets.Scripts.Extensions.SystemExtensions;

namespace Assets.Scripts.Appearance
{
    public class AvatarMeshInfo
    {
        public string MeshType { get; set; }
        public Int32 Lod { get; set; }
        public string MeshFileName { get; set; }
        public string ReferenceMeshName { get; set; }
        public float MinPixelArea { get; set; }
        public List<MorphInfoPair> PolyMorphTargetInfoList = new List<MorphInfoPair>();

        public void ParseXml(XmlNode rootNode)
        {
            if (rootNode.NodeType != XmlNodeType.Element || rootNode.Name != "mesh")
            {
                throw new Exception("Invalid root node.");
            }

            XmlElement rootElement = (XmlElement)rootNode;

            MeshType = rootElement.GetAttribute("type").Trim();
            if (string.IsNullOrEmpty(MeshType))
            {
                throw new Exception("No type supplied for mesh.");
            }

            string s = rootElement.GetAttribute("lod").Trim();
            Lod = 0;
            if (string.IsNullOrEmpty(s) == false)
            {
                Lod = Int32.Parse(s);
            }

            MeshFileName = rootElement.GetAttribute("file_name").Trim();
            if (string.IsNullOrEmpty(MeshType))
            {
                throw new Exception($"No file name supplied for mesh of type {MeshType}.");
            }

            ReferenceMeshName = rootElement.GetAttribute("reference");

            s = rootElement.GetAttribute("min_pixel_area").Trim();
            if (string.IsNullOrEmpty(s) == false)
            {
                MinPixelArea = float.Parse(s);
            }
            else // We weren't given a pixel area, let's see if we got a pixel width instead and compute the area:
            {
                float minPixelWidth = 0.1f; // If we weren't given a width, use this.
                s = rootElement.GetAttribute("min_pixel_width").Trim();
                if (string.IsNullOrEmpty(s) == false)
                {
                    minPixelWidth = float.Parse(s);
                }
                MinPixelArea = minPixelWidth * minPixelWidth;
            }

            // Parse visual params for this node only if we haven't already
            foreach (XmlNode paramNode in rootElement.ChildNodes)
            {
                if (paramNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement paramElement = (XmlElement)paramNode;

                XmlNode childNode = paramNode.GetFirstChild();
                switch (childNode.Name)
                {
                    case "param_morph":
                        PolyMorphTargetInfo polyMorphTargetInfo = new PolyMorphTargetInfo();
                        polyMorphTargetInfo.ParseXml(paramNode);
                        bool isShared = false;
                        s = paramElement.GetAttribute("shared").Trim();
                        if (string.IsNullOrEmpty(s) == false)
                        {
                            isShared = s != "0"; // NOTE: These booleans are 0/1 NOT true/false like others...
                        }
                        PolyMorphTargetInfoList.Add(new MorphInfoPair(polyMorphTargetInfo, isShared));
                        break;

                    case "param_skeleton":
                        throw new Exception("Can't specify skeleton parameter in a mesh definition.");
                    default:
                        throw new Exception($"Unknown parameter type in mesh definition. ({paramNode.Name})");
                }
            }
        }
    }

    public class MorphInfoPair
    {
        public PolyMorphTargetInfo MorphInfo { get; set; }
        public bool IsShared { get; set; }

        public MorphInfoPair(PolyMorphTargetInfo morphInfo, bool isShared)
        {
            MorphInfo = morphInfo;
            IsShared = isShared;
        }
    }
}
