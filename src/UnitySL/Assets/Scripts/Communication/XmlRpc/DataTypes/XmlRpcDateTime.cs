using System;
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcDateTime : XmlRpcValue
    {
        public DateTime Value { get; set; }

        public XmlRpcDateTime(DateTime value)
        {
            Value = value;
        }

        public XmlRpcDateTime(XmlNode node)
        {
            Value = DateTime.Parse(node.InnerText.Trim());
        }

        public override string ToXml()
        {
            return $"<dateTime.iso8601>{Value:o}</dateTime.iso8601>";
        }

        public override string AsString => $"{Value:o}";

        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcDateTime {{ {Value:o} }}";
            return s;
        }
    }
}
