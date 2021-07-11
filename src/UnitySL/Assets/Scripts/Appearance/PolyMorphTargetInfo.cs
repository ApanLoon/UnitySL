using System;
using System.Xml;
using Assets.Scripts.Extensions.SystemExtensions;

namespace Assets.Scripts.Appearance
{
    public class PolyMorphTargetInfo
    {
        public void ParseXml(XmlNode rootNode)
        {
            if (rootNode.NodeType != XmlNodeType.Element || rootNode.Name != "param" || rootNode.GetFirstChild()?.Name != "param_morph")
            {
                throw new Exception("Invalid root node.");
            }

            XmlElement rootElement = (XmlElement)rootNode;

            // TODO: Incomplete, check line 261 in llpolymorph.cpp
        }
    }
}