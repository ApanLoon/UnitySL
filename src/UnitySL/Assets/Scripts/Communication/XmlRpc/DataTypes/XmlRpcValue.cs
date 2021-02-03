using System;
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public abstract class XmlRpcValue
    {
        public static string EscapeString(string input) // TODO: This should probably not be here
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return input.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        public static XmlRpcValue FromXmlNode(XmlNode node)
        {
            XmlRpcValue value;
            switch (node.Name)
            {
                case "array":
                    value = new XmlRpcArray(node);
                    break;

                case "base64":
                    value = new XmlRpcBase64(node);
                    break;

                case "boolean":
                    value = new XmlRpcBoolean(node);
                    break;

                case "dateTime.iso8601":
                    value = new XmlRpcDateTime(node);
                    break;

                case "double":
                    value = new XmlRpcDouble(node);
                    break;

                case "int":
                case "i4":
                    value = new XmlRpcInteger(node);
                    break;

                case "nil":
                    value = new XmlRpcNil();
                    break;

                case "string":
                    value = new XmlRpcString(node);
                    break;

                case "struct":
                    value = new XmlRpcStruct(node);
                    break;

                default:
                    throw new Exception($"Unexpected node: {node.Name}");
            }

            return value;
        }

        public virtual string ToXml()
        {
            throw new Exception("Top level ToXml called.");
        }

        public virtual string AsString => "Top level AsString.";

        public virtual string ToString(string indent)
        {
            throw new Exception("Top level ToString(int) called.");
        }
    }
}
