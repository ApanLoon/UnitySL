using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcInteger : XmlRpcValue
    {
        public int Value { get; set; }

        public XmlRpcInteger(int value)
        {
            Value = value;
        }

        public XmlRpcInteger(XmlNode node)
        {
            Value = int.Parse(node.InnerText.Trim());
        }

        public override string ToXml()
        {
            return $"<int>{Value}</int>";
        }

        public override string AsString => $"{Value}";

        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcInteger {{ {Value} }}";
            return s;
        }
    }
}
