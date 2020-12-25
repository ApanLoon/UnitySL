
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcBase64 : XmlRpcValue
    {
        public byte[] Value;

        public XmlRpcBase64(byte[] value)
        {
            Value = value;
        }

        public XmlRpcBase64(string data)
        {
            Value = System.Convert.FromBase64String(data);
        }

        public XmlRpcBase64(XmlNode node)
        {
            Value = System.Convert.FromBase64String(node.InnerText.Trim());
        }

        public byte this[int index]
        {
            get => Value[index];
            set => Value[index] = value;
        }

        public override string ToXml()
        {
            return $"<base64>{AsString}</base64>";
        }

        public override string AsString => System.Convert.ToBase64String(Value);

        public override string ToString()
        {
            return AsString;
        }

        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcBase64 {{ {AsString} }}";
            return s;
        }
    }
}
