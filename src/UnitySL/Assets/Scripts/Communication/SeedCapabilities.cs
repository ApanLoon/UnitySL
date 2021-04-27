using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SLViewerLib.Communication.XmlRpc.DataTypes;

public class SeedCapabilities
{
    public static string[] CapabilityNames = new[]
    {
		"AbuseCategories",
	    "AcceptFriendship",
	    "AcceptGroupInvite", // ReadOfflineMsgs recieved messages only!!!
	    "AgentPreferences",
	    "AgentState",
	    "AttachmentResources",
	    "AvatarPickerSearch",
	    "AvatarRenderInfo",
	    "CharacterProperties",
	    "ChatSessionRequest",
	    "CopyInventoryFromNotecard",
	    "CreateInventoryCategory",
	    "DeclineFriendship",
	    "DeclineGroupInvite", // ReadOfflineMsgs recieved messages only!!!
	    "DispatchRegionInfo",
	    "DirectDelivery",
	    "EnvironmentSettings",
	    "EstateAccess",
	    "EstateChangeInfo",
	    "EventQueueGet",
        "ExtEnvironment",

	    "FetchLib2",
	    "FetchLibDescendents2",
	    "FetchInventory2",
	    "FetchInventoryDescendents2",
	    "IncrementCOFVersion",

        "InventoryAPIv3",
        "LibraryAPIv3",

	    "GetDisplayNames",
	    "GetExperiences",
	    "AgentExperiences",
	    "FindExperienceByName",
	    "GetExperienceInfo",
	    "GetAdminExperiences",
	    "GetCreatorExperiences",
	    "ExperiencePreferences",
	    "GroupExperiences",
	    "UpdateExperience",
	    "IsExperienceAdmin",
	    "IsExperienceContributor",
	    "RegionExperiences",
        "ExperienceQuery",
	    "GetMetadata",
	    "GetObjectCost",
	    "GetObjectPhysicsData",
	    "GroupAPIv1",
	    "GroupMemberData",
	    "GroupProposalBallot",
	    "HomeLocation",
	    "LandResources",
	    "LSLSyntax",
	    "MapLayer",
	    "MapLayerGod",
	    "MeshUploadFlag",	
	    "NavMeshGenerationStatus",
	    "NewFileAgentInventory",
	    "ObjectAnimation",
	    "ObjectMedia",
	    "ObjectMediaNavigate",
	    "ObjectNavMeshProperties",
	    "ParcelPropertiesUpdate",
	    "ParcelVoiceInfoRequest",
	    "ProductInfoRequest",
	    "ProvisionVoiceAccountRequest",
	    "ReadOfflineMsgs", // Requires to respond reliably: AcceptFriendship, AcceptGroupInvite, DeclineFriendship, DeclineGroupInvite
	    "RemoteParcelRequest",
	    "RenderMaterials",
	    "RequestTextureDownload",
	    "ResourceCostSelected",
	    "RetrieveNavMeshSrc",
	    "SearchStatRequest",
	    "SearchStatTracking",
	    "SendPostcard",
	    "SendUserReport",
	    "SendUserReportWithScreenshot",
	    "ServerReleaseNotes",
	    "SetDisplayName",
	    "SimConsoleAsync",
	    "SimulatorFeatures",
	    "StartGroupProposal",
	    "TerrainNavMeshProperties",
	    "TextureStats",
	    "UntrustedSimulatorMessage",
	    "UpdateAgentInformation",
	    "UpdateAgentLanguage",
	    "UpdateAvatarAppearance",
	    "UpdateGestureAgentInventory",
	    "UpdateGestureTaskInventory",
	    "UpdateNotecardAgentInventory",
	    "UpdateNotecardTaskInventory",
	    "UpdateScriptAgent",
	    "UpdateScriptTask",
        "UpdateSettingsAgentInventory",
        "UpdateSettingsTaskInventory",
	    "UploadBakedTexture",
        "UserInfo",
	    "ViewerAsset", 
	    "ViewerBenefits",
	    "ViewerMetrics",
	    "ViewerStartAuction",
	    "ViewerStats"
    };

	/// <summary>
	/// Requests all the capabilities from the seed capability.
	///
	/// TODO: I might have to bite the bullet and add LLSD classes - each grant could be of different types.
	/// 
	/// NOTE: The mono version of HttpClient can't accept untrusted certificates and simulators use self-signed certificates. Therefore we use the deprecated HttpWebRequest.
	/// 
	/// </summary>
	/// <param name="uri"></param>
	/// <returns></returns>
	public static async Task<List<Capability>> RequestCapabilities(string uri)
    {
        string postData = BuildPostData();
		try
		{
            byte[] dataBuffer = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            request.Method = "POST";
            request.ContentLength = dataBuffer.Length;
            request.ContentType = "application/xml";
            using (var outStream = await request.GetRequestStreamAsync())
            {
				outStream.Write(dataBuffer, 0, dataBuffer.Length);
            }

            var response = (HttpWebResponse)await request.GetResponseAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"SeedCaspabilites: Failed to request capability grants. ({response.StatusCode})");
            }

            int length = (int)response.ContentLength;
            var contentStream = response.GetResponseStream();

			if (contentStream == null)
            {
                throw new Exception($"SeedCaspabilites: Failed to get response stream.");
            }
            //Logger.LogDebug("SeedCapabilities.RequestCapabilities", "Got response stream.");

            var buffer = new byte[length];
            try
            {
                int count = 0;
                int start = 0;
                while ((count = await contentStream.ReadAsync(buffer, start, length - start)) != 0)
                {
                    start += count;
                    //Logger.LogDebug("SeedCapabilities.RequestCapabilities", $"Read {count} bytes. ({start}/{length})");
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
            List<Capability> grants = BuildCapabilityMap(document);

            return grants;
        }
		catch (Exception e)
		{
			Logger.LogWarning("SeedCapabilities.RequestCapabilities", $"Exception {e}");
		}
		return null;

	}

    private static List<Capability> BuildCapabilityMap(XmlDocument document)
    {
        List<Capability> list = new List<Capability>();

        foreach (string name in CapabilityNames)
        {
			Capability cap = new Capability(name);
            list.Add(cap);

            XmlNode node = document.GetElementsByTagName("key").Cast<XmlNode>().FirstOrDefault((x) => x.InnerText == name);
            if (node == null)
            {
                cap.IsGranted = false;
                continue;
            }

            cap.IsGranted = true;
            XmlNode data = node.NextSibling;
            switch (data.Name)
            {
                case "string":
                    cap.CapabilityType = CapabilityType.Http;
                    cap.Url = data.InnerText;
                    break;

                case "map":
                {
                    cap.CapabilityType = CapabilityType.MessageSystem;
                    XmlNode throttle = data.ChildNodes.Cast<XmlNode>().FirstOrDefault((x) => x.Name == "throttle");
                    if (throttle != null)
                    {
                        cap.Throttle = int.Parse(throttle.FirstChild.InnerText); // TODO: Check that the child node exists and is named "integer"
                    }
                    XmlNode useSsl = data.ChildNodes.Cast<XmlNode>().FirstOrDefault((x) => x.Name == "use-ssl");
                    if (useSsl != null)
                    {
                        cap.UseSsl = bool.Parse(useSsl.FirstChild.InnerText); // TODO: Check that the child node exists and is named "boolean"
                    }
                    XmlNode viaCache = data.ChildNodes.Cast<XmlNode>().FirstOrDefault((x) => x.Name == "via-cache");
                    if (viaCache != null)
                    {
                        cap.ViaCache = bool.Parse(viaCache.FirstChild.InnerText); // TODO: Check that the child node exists and is named "boolean"
                    }
                    break;
                }
            }
		}

        return list;
    }

	protected static string BuildPostData()
    {
        string data = "<llsd><array>";
        foreach (string capabilityName in CapabilityNames)
        {
            data += $"<string>{XmlRpcValue.EscapeString(capabilityName)}</string>"; // TODO: Not nice to depend on XmlRpc here
        }
        data += "</array></llsd>";
        return data;
    }
}
