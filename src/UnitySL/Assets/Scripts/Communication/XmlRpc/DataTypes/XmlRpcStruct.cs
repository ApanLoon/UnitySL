using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcStruct : XmlRpcValue, IEnumerable<KeyValuePair<string, XmlRpcValue>>
    {
        protected Dictionary<string, XmlRpcValue> Data = new Dictionary<string, XmlRpcValue>();

        public XmlRpcStruct()
        {
        }

        public XmlRpcStruct(XmlNode node)
        {
            foreach (XmlNode member in node.ChildNodes)
            {
                if (member.Name != "member")
                {
                    Logger.LogWarning($"XmlRpcStruct.constructor: Unexpected node in xml tree: {member.Name}");
                    continue;
                }

                if (member.ChildNodes.Count != 2)
                {
                    Logger.LogWarning($"XmlRpcStruct.constructor: Unexpected number of nodes in member node.");
                    continue;
                }
                string key = member.FirstChild.InnerText.Trim();
                XmlNode valueNode = member.LastChild;
                if (valueNode.ChildNodes.Count != 1)
                {
                    Logger.LogWarning($"XmlRpcStruct.constructor: Unexpected number of nodes in value node for member \"{key}\".");
                    continue;
                }
                XmlRpcValue value = XmlRpcValue.FromXmlNode(valueNode.FirstChild);
                Data[key] = value;
            }
        }

        public int Count => Data.Count;

        public override string ToXml()
        {
            string s = "<struct>";
            foreach (KeyValuePair<string, XmlRpcValue> kv in Data)
            {
                s += $"<member><name>{kv.Key}</name><value>{kv.Value.ToXml()}</value></member>";
            }
            s += "</struct>";
            return s;
        }

        public override string AsString
        {
            get
            {
                string s = "";
                foreach (KeyValuePair<string, XmlRpcValue> kv in Data)
                {
                    if (s != "")
                    {
                        s += ", ";
                    }
                    s += $"{kv.Key} = {kv.Value.AsString}";
                }
                s += "}";
                return s;
            }
        }

        public override string ToString(string indent)
        {
            string s = $"XmlRpcStruct\n{indent}{{\n";
            foreach (KeyValuePair<string, XmlRpcValue> kv in Data)
            {
                s += $"{indent}    {kv.Key} = {kv.Value.ToString("")}\n";
            }
            s += $"{indent}}}";
            return s;
        }

        public bool Has(string key)
        {
            return Data.ContainsKey(key);
        }

        public XmlRpcValue this[string key]
        {
            get => Data[key];
            set => Data[key] = value;
        }

        public IEnumerator<KeyValuePair<string, XmlRpcValue>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
