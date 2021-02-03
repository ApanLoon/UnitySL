using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using SLViewerLib.Communication.XmlRpc.DataTypes;

namespace SLViewerLib.Communication.XmlRpc
{
    public class XmlRpcParameterArray : IEnumerable<XmlRpcValue>
    {
        protected List<XmlRpcValue> Data = new List<XmlRpcValue>();

        public XmlRpcParameterArray()
        {
        }

        public XmlRpcParameterArray(XmlRpcStruct map) // TODO: It seems as if this is dependent on the order of items in the map, I don't know if a Dictionary can guarantee that.
        {
            foreach (KeyValuePair<string, XmlRpcValue> kv in map)
            {
                Data.Add(kv.Value);
            }
        }

        public XmlRpcParameterArray(XmlNode paramsNode)
        {
            if (paramsNode.Name != "params")
            {
                throw new Exception($"WARN XmlRpcParameterArray.constructor: Unexpected root node, expected \"params\" found \"{paramsNode.Name}\".");
            }

            foreach (XmlNode paramNode in paramsNode.ChildNodes)
            {
                if (paramNode.Name != "param")
                {
                    throw new Exception($"WARN XmlRpcParameterArray.constructor: Unexpected node, expected \"param\" found \"{paramNode.Name}\".");
                }

                if (paramNode.ChildNodes.Count != 1)
                {
                    throw new Exception("WARN XmlRpcParameterArray.constructor: Unexpected number of nodes in param.");
                }
                XmlNode valueNode = paramNode.FirstChild;

                if (valueNode.Name != "value")
                {
                    Logger.LogWarning($"XmlRpcParameterArray.constructor: Unexpected node, expected \"value\" found \"{valueNode.Name}\".");
                    continue;
                }
                if (valueNode.ChildNodes.Count != 1)
                {
                    throw new Exception("WARN XmlRpcParameterArray.constructor: Unexpected number of nodes in value.");
                }

                XmlRpcValue item = XmlRpcValue.FromXmlNode(valueNode.FirstChild);
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

        public  string ToXml()
        {
            string s = "<params>";
            foreach (XmlRpcValue value in Data)
            {
                s += $"<param><value>{value.ToXml()}</value></param>";
            }
            s += "</params>";
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

        public string ToString(string indent = "")
        {
            string s = $"{indent}XmlRpcParametersArray\n{indent}{{\n";
            foreach (XmlRpcValue value in Data)
            {
                s += $"{value.ToString(indent + "    ")}\n";
            }
            s += "{indent}}\n";
            return s;
        }
    }
}
