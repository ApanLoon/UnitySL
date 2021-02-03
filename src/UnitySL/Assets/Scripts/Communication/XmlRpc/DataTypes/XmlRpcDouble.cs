using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcDouble : XmlRpcValue
    {
        public double Value { get; set; }

        public XmlRpcDouble(double value)
        {
            Value = value;
        }

        public XmlRpcDouble(XmlNode node)
        {
            Value = double.Parse(node.InnerText.Trim());
        }

        public override string ToXml()
        {
            return $"<double>{Value}</double>";
        }

        public override string AsString => $"{Value}";
        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcDouble {{ {Value} }}";
            return s;
        }
    }
}
