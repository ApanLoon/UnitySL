namespace SLViewerLib.Communication.XmlRpc.DataTypes
{
    public class XmlRpcNil : XmlRpcValue
    {
        public override string ToXml()
        {
            return "<nil/>";
        }
        public override string AsString => "nil";

        public override string ToString(string indent)
        {
            string s = $"{indent}XmlRpcNil";
            return s;
        }
    }
}
