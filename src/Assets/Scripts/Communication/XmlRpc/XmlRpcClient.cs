using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SLViewerLib.Communication.XmlRpc
{
    public class XmlRpcClient
    {
        protected static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<XmlRpcResponse> Call(string uri, string methodName, XmlRpcParameterArray parameters = null)
        {
            XmlRpcRequest request = new XmlRpcRequest(methodName, parameters);
            return await Call(uri, request);
        }

        public static async Task<XmlRpcResponse> Call(string uri, XmlRpcRequest request)
        {
            try
            {
                Uri requestUri = new Uri(uri);

                HttpContent content = new StringContent(request.ToXml(), Encoding.UTF8, "text/xml");
                content.Headers.ContentType.CharSet = null; // LL server gets upset if we specify this (Closes the connection or responds with "Bad request")
                //Logger.Log($"XmlRpcClient.Call: content: \n{request.ToXml()}");

                HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    RequestUri = requestUri,
                    Method = HttpMethod.Post,
                    Headers =
                    {
                        {"Host", requestUri.Host + (requestUri.IsDefaultPort ? "" : $":{requestUri.Port}")},
                        {"User-Agent", "SecondLifeUnity/0.1" },
                        {"Accept", "*/*" }
                    },
                    Content = content
                };

                //HttpResponseMessage responseMessage = await HttpClient.SendAsync(requestMessage);
                //string responseText = await responseMessage.Content.ReadAsStringAsync();

                HttpResponseMessage responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                //Logger.Log("XmlRpcClient.Call: Request sent.");

                if (responseMessage.IsSuccessStatusCode == false)
                {
                    throw new Exception($"WARN XmlRpcClient.Call: Post failed with return code {responseMessage.StatusCode} {responseMessage.ReasonPhrase}.");
                }

                var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                //Logger.Log("XmlRpcClient.Call: Got response stream.");

                int length = (int)(responseMessage.Content.Headers.ContentLength ?? 2048);
                var buffer = new byte[length];
                try
                {
                    int count = 0;
                    int start = 0;
                    while ((count = contentStream.Read(buffer, start, length - start)) != 0)
                    {
                        start += count;
                        //Logger.LogDebug($"XmlRpcClient.Call: Read {count} bytes. ({start}/{length})");
                    }
                }
                catch (IOException ex)
                {
                    if (!ex.Message.StartsWith("The response ended prematurely"))
                    {
                        throw;
                    }
                }
                string responseText = Encoding.UTF8.GetString(buffer).Replace("\0", "");

                //File.WriteAllText("loginResponse.xml", responseText);

                XmlDocument document = new XmlDocument();
                document.Load(new StringReader(responseText));
                return new XmlRpcResponse(document.LastChild); // This skips any XmlDeclaration
            }
            catch (Exception e)
            {
                Logger.LogWarning($"XmlRpcClient.Call: Exception {e}");
            }
            return null;
        }
    }
}
