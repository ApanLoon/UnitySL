using System;
using System.Xml;
using SLViewerLib.Communication.XmlRpc.DataTypes;

namespace SLViewerLib.Communication.XmlRpc
{
    public class XmlRpcResponse
    {
        public int FaultCode { get; set; }
        public string FaultString { get; set; }
        public XmlRpcParameterArray Parameters { get; set; }

        public XmlRpcResponse (XmlNode node)
        {
            if (node.Name != "methodResponse")
            {
                throw new Exception($"WARN XmlRpcResponse.Constructor: Unexpected node name, expected \"methodResponse\" found \"{node.Name}\".");
            }

            if (node.ChildNodes.Count != 1)
            {
                throw new Exception("WARN XmlRpcResponse.Constructor: Unexpected number of nodes in methodResponse.");
            }

            XmlNode child = node.FirstChild;

            switch (child.Name)
            {
                case "params":
                    Parameters = new XmlRpcParameterArray(child);
                    break;

                case "fault":
                    ParseFault(child);
                    break;

                default:
                    throw new Exception($"WARN XmlRpcResponse.Constructor: Unexpected child node: \"{child.Name}\".");
            }
        }

        protected void ParseFault(XmlNode faultNode)
        {
            Parameters = new XmlRpcParameterArray();

            if (faultNode.Name != "fault")
            {
                throw new Exception($"WARN XmlRpcResponse.ParseFault: Unexpected node name, expected \"fault\" found \"{faultNode.Name}\".");
            }

            if (faultNode.ChildNodes.Count != 1)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: Unexpected number of nodes in fault.");
            }

            XmlNode valueNode = faultNode.FirstChild;
            if (valueNode.Name != "value")
            {
                throw new Exception($"WARN XmlRpcResponse.ParseFault: Unexpected node name, expected \"value\" found \"{valueNode.Name}\".");
            }
            if (valueNode.ChildNodes.Count != 1)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: Unexpected number of nodes in value.");
            }

            XmlRpcValue faultValue = XmlRpcValue.FromXmlNode(valueNode.FirstChild);
            if (faultValue is XmlRpcStruct == false)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: Unexpected value type.");
            }

            XmlRpcStruct fault = (XmlRpcStruct) faultValue;
            if (fault.Has("faultCode") == false)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: Fault response has no faultCode.");
            }
            if (fault["faultCode"] is XmlRpcInteger == false)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: faultCode is not an integer.");
            }
            FaultCode = ((XmlRpcInteger)fault["faultCode"]).Value;

            if (fault.Has("faultString") == false)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: Fault response has no faultString.");
            }
            if (fault["faultString"] is XmlRpcString == false)
            {
                throw new Exception("WARN XmlRpcResponse.ParseFault: faultString is not a string.");
            }
            FaultString = ((XmlRpcString)fault["faultString"]).Value;
        }

        public string ToString(string indent = "")
        {
            string s = $"{indent}XmlRpcResponse\n{indent}{{\n";
            if (FaultCode != 0)
            {
                s += $"{indent}    FaultCode   = {FaultCode}\n" 
                   + $"{indent}    FaultString = {FaultString}\n";
            }
            else
            {
                s += Parameters.ToString(indent + "    ");
            }
            s += $"{indent}}}\n";
            return s;
        }
    }
}
