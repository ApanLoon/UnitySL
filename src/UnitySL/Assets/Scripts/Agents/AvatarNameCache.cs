
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SLViewerLib.Communication.XmlRpc.DataTypes;

namespace Assets.Scripts.Agents
{
    public class AvatarNameCache
    {
        public static readonly AvatarNameCache Instance = new AvatarNameCache();
        
        protected Dictionary<Guid, AvatarName> NameCache = new Dictionary<Guid, AvatarName>();
        protected Dictionary<Guid, Request> PendingRequests = new Dictionary<Guid, Request>();


        protected class Request
        {
            public Guid AgentId { get; set; }
            public List<Action<Guid, AvatarName>> Callbacks { get; set; }
            public bool IsRequested { get; set; } = false;
        }

        protected Queue<Request> RequestsToProcess = new Queue<Request>();

        /// <summary>
        /// Requests a name for thew given agentId.
        ///
        /// If the name is cached, the callback will be called immediately. (on the main thread)
        ///
        /// If the name is not cached, a look up will be requested and the callback will be called
        /// when the response arrives from the server. (On the main thread)
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Get(Guid agentId, Action<Guid, AvatarName> callback)
        {
            bool foundName = false;
            bool shallRequest = true;
            if (NameCache.ContainsKey(agentId))
            {
                AvatarName avatarName = NameCache[agentId];
                ThreadManager.ExecuteOnMainThread(() => callback.Invoke(agentId, avatarName));
                foundName = true;
                
                if (avatarName.ExpiresOn < ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds())
                {
                    shallRequest = false;
                }
            }

            if (shallRequest)
            {
                lock (PendingRequests)
                {
                    if (PendingRequests.ContainsKey(agentId) == false)
                    {
                        PendingRequests[agentId] = new Request
                        {
                            AgentId = agentId,
                            Callbacks = new List<Action<Guid, AvatarName>>()
                        };
                    }
                    PendingRequests[agentId].Callbacks.Add(callback);
                }
            }

            return foundName;
        }

        /// <summary>
        /// Returns the name if it is already cached or null if it isn't. Does NOT trigger a name look up.
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public AvatarName GetImmediate(Guid agentId)
        {
            return NameCache.ContainsKey(agentId) ? NameCache[agentId] : null;
        }

        public AvatarNameCache()
        {
            Start();
        }

        #region Thread
        public void Start()
        {
            if (_threadLoopTask != null && _threadLoopTask.Status == TaskStatus.Running)
            {
                Logger.LogDebug("AvatarNameCache.Start: Already started.");
                return;
            }
            Logger.LogDebug($"AvatarNameCache.Start: Started");

            _cts = new CancellationTokenSource();
            _threadLoopTask = Task.Run(() => ThreadLoop(_cts.Token), _cts.Token);
        }

        public void Stop()
        {
            Logger.LogDebug($"AvatarNameCache.Stop");
            _cts.Cancel();

            _cts.Dispose();
        }

        public void Dispose()
        {
            Logger.LogDebug($"AvatarNameCache.Dispose");
            Stop();
        }

        private CancellationTokenSource _cts;
        private Task _threadLoopTask;

        protected async Task ThreadLoop(CancellationToken ct)
        {
            Logger.LogInfo($"AvatarNameCache.ThreadLoop: Running");

            while (ct.IsCancellationRequested == false)
            {
                if (Session.Instance.IsLoggedIn) // TODO: We probably need a better way to tell if we are logged in or not
                {
                    Capability cap = Agent.CurrentPlayer.Region.GetCapability("GetDisplayNames");
                    if (cap != null)
                    {
                        await RequestNamesViaCapability(cap);
                    }
                    else
                    {
                        // TODO: Use legacy name fetching
                        Logger.LogWarning("AvatarNameCache: No GetDisplayName capability in region.");
                    }
                }

                await Task.Delay(10, ct); // tune for your situation, can usually be omitted
            }
            // Cancelling appears to kill the task immediately without giving it a chance to get here
            Logger.LogInfo($"AvatarNameCache.ThreadLoop: Stopping...");
        }
        #endregion Thread

        // URL format is like:
        // http://pdp60.lindenlab.com:8000/agents/?ids=3941037e-78ab-45f0-b421-bd6e77c1804d&ids=0012809d-7d2d-4c24-9609-af1230a37715&ids=0019aaba-24af-4f0a-aa72-6457953cf7f0
        //
        // Apache can handle URLs of 4096 chars, but let's be conservative
        protected static readonly UInt32 NAME_URL_SEND_THRESHOLD = 3500;

        protected async Task RequestNamesViaCapability(Capability cap)
        {
            if (cap.CapabilityType != CapabilityType.Http)
            {
                Logger.LogWarning($"AvatarNameCache: GetDisplayName capability not of type HTTP. ({cap.CapabilityType})");
                return;
            }

            string url = "";
            lock (PendingRequests)
            {
                bool first = true;
                foreach (Request request in PendingRequests.Where(x => x.Value.IsRequested == false).Select(x => x.Value))
                {
                    if (url.Length >= NAME_URL_SEND_THRESHOLD)
                    {
                        break;
                    }
                    url += $"{(first ? "?" : "&")}ids={request.AgentId.ToString()}";
                    request.IsRequested = true;
                    first = false;
                }
            }

            if (url == "")
            {
                return;
            }

            url = $"{cap.Url}/{url}";
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                request.Method = "GET";
                var response = (HttpWebResponse)await request.GetResponseAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"WARN AvatarNameCache.RequestNamesViaCapability: Get failed with return code {response.StatusCode}.");
                }

                int length = (int)response.ContentLength;
                var contentStream = response.GetResponseStream();

                if (contentStream == null)
                {
                    throw new Exception($"AvatarNameCache.RequestNamesViaCapability: Failed to get response stream.");
                }

                var buffer = new byte[length];
                try
                {
                    int count = 0;
                    int start = 0;
                    while ((count = await contentStream.ReadAsync(buffer, start, length - start)) != 0)
                    {
                        start += count;
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

                XmlDocument document = new XmlDocument();
                document.Load(new StringReader(responseText));
                ProcessResponse(document);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"AvatarNameCache.RequestNamesViaCapability: Exception {e}");
            }
        }

        protected void ProcessResponse(XmlDocument document)
        {
            try
            {
                XmlNode node = document.LastChild; // This skips any XmlDeclaration
                if (node.Name != "llsd")
                {
                    throw new Exception($"Expected <llsd> got <{node.Name}>");
                }

                node = node.FirstChild;
                if (node.Name != "map")
                {
                    throw new Exception($"Expected <map> got <{node.Name}>");
                }

                node = node.FirstChild;
                if (node.Name != "key")
                {
                    throw new Exception($"Expected <key> got <{node.Name}>");
                }

                while (node != null)
                {
                    string key = node.InnerText;
                    node = node.NextSibling; // Move to value
                    if (node == null)
                    {
                        throw new Exception("Expected a value.");
                    }
                    switch (key)
                    {
                        case "agents":
                            ParseResponseAgents(node);
                            break;

                        case "bad_ids":
                            ParseResponseBadIds(node);
                            break;

                        case "bad-usernames":
                            ParseResponseBadUsernames(node);
                            break;

                        default:
                            Logger.LogWarning($"AvatarNameCache.ProcessResponse: Unexpected key \"{node.InnerText}\"");
                            break;
                    }
                    node = node.NextSibling; // Move to next key
                }
                Logger.LogDebug($"AvatarNameCache.ProcessResponse: Parse complete");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"AvatarNameCache.ProcessResponse: {e.Message}");
            }
        }

        protected void ParseResponseAgents(XmlNode node)
        {
            if (node.Name != "array")
            {
                throw new Exception($"Expected <array> got <{node.Name}>");
            }

            foreach (object childObject in node.ChildNodes)
            {
                XmlNode child = (XmlNode) childObject;
                if (child.Name != "map")
                {
                    throw new Exception($"Expected <map> got <{child.Name}>");
                }

                Guid agentId;
                AvatarName avatarName = new AvatarName();

                child = child.FirstChild;
                while (child != null)
                {
                    string key = child.InnerText;
                    child = child.NextSibling; // Move to value
                    if (child == null)
                    {
                        throw new Exception("Expected a value.");
                    }

                    switch (key)
                    {
                        case "username":
                            // We don't care about this since UserNames are always constructed.
                            //
                            //if (child.Name != "string")
                            //{
                            //    Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected username type string, got \"{child.Name}\"");
                            //}
                            //else
                            //{
                            //    avatarName.UserName = XmlRpcValue.EscapeString(child.InnerText.Trim()); // TODO: Not nice to depend on XmlRpc here
                            //}
                            break;

                        case "display_name":
                            if (child.Name != "string")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected display_name type string, got \"{child.Name}\"");
                            }
                            else
                            {
                                avatarName.DisplayName = XmlRpcValue.EscapeString(child.InnerText.Trim()); // TODO: Not nice to depend on XmlRpc here
                            }
                            break;

                        case "display_name_next_update":
                            if (child.Name != "date")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected display_name_next_update type date, got \"{child.Name}\"");
                            }
                            else
                            {
                                DateTime time = DateTime.Parse(XmlRpcValue.EscapeString(child.InnerText.Trim())); // TODO: Not nice to depend on XmlRpc here
                                // TODO: Get the seconds from UNIX epoch
                            }
                            break;

                        case "legacy_first_name":
                            if (child.Name != "string")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected legacy_first_name type string, got \"{child.Name}\"");
                            }
                            else
                            {
                                avatarName.FirstName = XmlRpcValue.EscapeString(child.InnerText.Trim()); // TODO: Not nice to depend on XmlRpc here
                            }
                            break;

                        case "legacy_last_name":
                            if (child.Name != "string")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected legacy_last_name type string, got \"{child.Name}\"");
                            }
                            else
                            {
                                avatarName.LastName = XmlRpcValue.EscapeString(child.InnerText.Trim()); // TODO: Not nice to depend on XmlRpc here
                            }
                            break;

                        case "id":
                            if (child.Name != "uuid")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected id type uuid, got \"{child.Name}\"");
                            }
                            else
                            {
                                agentId = Guid.Parse(XmlRpcValue.EscapeString(child.InnerText.Trim())); // TODO: Not nice to depend on XmlRpc here
                            }
                            break;

                        case "is_display_name_default":
                            if (child.Name != "boolean")
                            {
                                Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Expected is_display_name_default type boolean, got \"{child.Name}\"");
                            }
                            else
                            {
                                bool x = bool.Parse(XmlRpcValue.EscapeString(child.InnerText.Trim())); // TODO: Not nice to depend on XmlRpc here
                            }
                            break;

                        default:
                            Logger.LogWarning($"AvatarNameCache.ParseResponseAgents: Unexpected key \"{child.InnerText}\"");
                            break;
                    }

                    child = child.NextSibling; // Move to next key
                }
                Logger.LogDebug($"Parsed: First=\"{avatarName.FirstName}\", Last=\"{avatarName.LastName}\", DisplayName=\"{avatarName.DisplayName}\"");

                NameCache[agentId] = avatarName;

                if (PendingRequests.ContainsKey(agentId))
                {
                    Request request = PendingRequests[agentId];
                    lock (PendingRequests)
                    {
                        PendingRequests.Remove(agentId);
                    }

                    foreach (Action<Guid, AvatarName> callback in request.Callbacks)
                    {
                        ThreadManager.ExecuteOnMainThread(() => callback.Invoke(agentId, avatarName));
                    }
                }
            }
        }

        protected void ParseResponseBadIds(XmlNode node)
        {
            if (node.Name != "array")
            {
                throw new Exception($"Expected <array> got <{node.Name}>");
            }

            // TODO: Parse bad ids and remove them from the request list
        }

        protected void ParseResponseBadUsernames(XmlNode node)
        {
            if (node.Name != "array")
            {
                throw new Exception($"Expected <array> got <{node.Name}>");
            }

            // TODO: If we add the possibility to look up usernames, parse these here.
        }
    }
}
