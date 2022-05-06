using System.Xml;

namespace Assets.Scripts.Extensions.SystemExtensions
{
    public static class XmlNodeExtensions
    {
        /// <summary>
        /// Returns the first child node that isn't a xml declaration or comment. Returns null if no child is found.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="skipXmlDeclaration">Set to false to not skip xml declarations.</param>
        /// <param name="skipComments">Set to false to not skip comments.</param>
        /// <returns></returns>
        public static XmlNode GetFirstChild(this XmlNode node, bool skipXmlDeclaration = true, bool skipComments = true)
        {
            if (node.HasChildNodes == false)
            {
                return null;
            }

            int i = 0;
            while (i < node.ChildNodes.Count
                   && (   skipXmlDeclaration && node.ChildNodes[i].NodeType == XmlNodeType.XmlDeclaration
                       || skipComments       && node.ChildNodes[i].NodeType == XmlNodeType.Comment))
            {
                i++;
            }

            return i < node.ChildNodes.Count ? node.ChildNodes[i] : null;
        }
    }
}
