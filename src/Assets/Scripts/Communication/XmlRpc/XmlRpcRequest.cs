
using SLViewerLib.Communication.XmlRpc.DataTypes;

namespace SLViewerLib.Communication.XmlRpc
{
    public class XmlRpcRequest
    {
        /// <summary>
        /// Name of method being called
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public XmlRpcParameterArray Parameters { get; set; }

        public XmlRpcRequest(string methodName, XmlRpcParameterArray parameters = null)
        {
            MethodName = methodName;
            if (parameters == null)
            {
                parameters = new XmlRpcParameterArray();
            }
            Parameters = parameters;
        }

        public string ToXml()
        {
            string s = "<?xml version=\"1.0\"?><methodCall>";
            s += $"<methodName>{XmlRpcValue.EscapeString(MethodName)}</methodName>";
            s += Parameters.ToXml();
            s += "</methodCall>";
            return s;
        }
    }
}
