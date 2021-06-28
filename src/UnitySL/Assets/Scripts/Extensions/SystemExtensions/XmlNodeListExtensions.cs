using System.Xml;

namespace Assets.Scripts.Extensions.SystemExtensions
{
    public static class XmlNodeListExtensions
    {
        /// <summary>
        /// Returns the node with the given name or null.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlNode GetChildByName(this XmlNodeList list, string name)
        {
            foreach (XmlNode node in list)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
