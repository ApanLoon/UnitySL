
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcBoolean : XmlRpcValue
    {
        public bool Value { get; set; }

        public XmlRpcBoolean(bool value)
        {
            Value = value;
        }

        public XmlRpcBoolean(XmlNode node)
        {
            Value = node.InnerText.Trim() != "0";
        }

        public override string ToXml()
        {
            return $"<boolean>{(Value ? "1" : "0")}</boolean>";
        }

        public override string AsString => Value ? "true" : "false";

        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcBoolean {{ {Value} }}";
            return s;
        }
    }
}
