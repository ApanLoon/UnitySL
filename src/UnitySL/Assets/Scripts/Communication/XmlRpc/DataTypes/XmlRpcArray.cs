using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcArray : XmlRpcValue, IEnumerable<XmlRpcValue>
    {
        protected List<XmlRpcValue> Data = new List<XmlRpcValue>();

        public XmlRpcArray()
        {
        }

        public XmlRpcArray(XmlNode node)
        {
            if (node.ChildNodes.Count != 1)
            {
                throw new Exception("WARN XmlRpcArray.constructor: Unexpected number of nodes in array.");
            }

            XmlNode data = node.FirstChild;
            if (data.Name != "data")
            {
                throw new Exception($"WARN XmlRpcArray.constructor: Unexpected node in xml tree: {data.Name}");
            }

            foreach (XmlNode childNode in data.ChildNodes)
            {
                if (childNode.Name != "value")
                {
                    Logger.LogWarning("XmlRpcArray.constructor", $"Unexpected node in xml tree: {childNode.Name}");
                    continue;
                }

                if (childNode.ChildNodes.Count != 1)
                {
                    Logger.LogWarning("XmlRpcArray.constructor", $"Unexpected number of nodes in value.");
                    continue;
                }

                XmlRpcValue item = XmlRpcValue.FromXmlNode(childNode.FirstChild);
                Append(item);
            }
        }

        public int Count => Data.Count;

        public void Append(XmlRpcValue value)
        {
            Data.Add(value);
        }

        public void Remove(int index)
        {
            Data.RemoveAt(index);
        }

        public override string ToXml()
        {
            string s = "<array><data>";
            foreach (XmlRpcValue value in Data)
            {
                s += $"<value>{value.ToXml()}</value>";
            }
            s += "</data></array>";
            return s;
        }

        public override string AsString
        {
            get
            {
                string s = "";
                foreach (XmlRpcValue value in Data)
                {
                    if (s != "")
                    {
                        s += ", ";
                    }
                    s += $"{value.AsString}";
                }
                s += "}";
                return s;
            }
        }

        public override string ToString(string indent)
        {
            string s = $"XmlRpcArray\n{indent}{{\n";
            foreach (XmlRpcValue value in Data)
            {
                s += $"{value.ToString(indent + "    ")}\n";
            }
            s += $"{indent}}}";
            return s;
        }

        public XmlRpcValue this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public IEnumerator<XmlRpcValue> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
