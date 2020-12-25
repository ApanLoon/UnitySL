using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcString : XmlRpcValue
    {
        public string Value { get; set; }

        public XmlRpcString(string value)
        {
            Value = value;
        }

        public XmlRpcString(XmlNode node)
        {
            Value = node.InnerText.Trim();
        }

        public override string ToXml()
        {
            return $"<string>{EscapeString(Value)}</string>";
        }

        public override string AsString => Value;
        
        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcString {{ \"{Value}\" }}";
            return s;
        }
    }
}
