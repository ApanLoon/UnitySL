using System;
using System.Threading.Tasks;
using Assets.Scripts.Agents;
using SLViewerLib.Communication.XmlRpc;
using SLViewerLib.Communication.XmlRpc.DataTypes;

public class Login
{
    public static readonly int ADDRESS_SIZE = 64; // TODO: This should not be here

    protected string SerialNumber { get; set; } = "123456789-abcdefgh";
    protected int LastExecEvent { get; set; }
    protected int LastExecDuration { get; set; }
    protected string Platform { get; set; } = "win";
    protected string PlatformVersion { get; set; } = "10.0.0";
    protected string PlatformVersionName { get; set; } = "Microsoft Windows 10 64-bit";
    protected string MachineId { get; set; } = "31E8BE2430618E626CA9218425264404";
    protected string ViewerVersion { get; set; }
    protected string ViewerChannel { get; set; } = "Second Life Unity";
    protected string HostId { get; set; } = "APA";

    public void Initialise(string machineId,
                           string serialNumber,
                           string viewerVersion,
                           string viewerChannel,
                           int lastExecEvent,
                           int lastExecDuration,
                           string platform,
                           string platformVersion,
                           string platformVersionName,
                           string hostId)
    {
        MachineId = machineId;
        SerialNumber = serialNumber;
        ViewerVersion = viewerVersion;
        ViewerChannel = viewerChannel;
        LastExecEvent = lastExecEvent;
        LastExecDuration = lastExecDuration;
        Platform = platform;
        PlatformVersion = platformVersion;
        PlatformVersionName = platformVersionName;
        HostId = hostId;
    }

    public async Task<LoginResponse> Connect(string uri, Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        if (slurl == null)
        {
            slurl = new Slurl(Slurl.SIM_LOCATION_LAST);
        }
        Logger.LogDebug($"INFO Login.Connect: Connecting {credential.First} {credential.Last} using {uri}.");

        XmlRpcParameterArray parameters = CreateLoginParameters(credential, slurl, getInventoryLibrary, godMode);
        
        XmlRpcResponse response = await XmlRpcClient.Call(uri, "login_to_simulator", parameters);
        
        LoginResponse loginResponse = new LoginResponse();
        if (response.FaultCode != 0)
        {
            loginResponse.LoginSucceeded = false;
            loginResponse.LoginFailReason = response.FaultCode.ToString();
            loginResponse.Message = response.FaultString;
            loginResponse.MessageId = "XmlRpcError";
            return loginResponse;
        }

        if (response.Parameters.Count != 1 || (response.Parameters[0] is XmlRpcStruct == false))
        {
            loginResponse.LoginSucceeded = false;
            loginResponse.LoginFailReason = "500";
            loginResponse.Message = "Login response contained incorrect parameters.";
            loginResponse.MessageId = "XmlRpcError";
            return loginResponse;
        }

        XmlRpcStruct responseData = (XmlRpcStruct)response.Parameters[0];

        if (    responseData.Has("login") == false
            || (responseData["login"] is XmlRpcString == false)
            || ((XmlRpcString)responseData["login"]).Value != "true")
        {
            loginResponse.LoginSucceeded = false;
            loginResponse.LoginFailReason = responseData["reason"]?.AsString;
            loginResponse.Message = responseData["message"]?.AsString;
            loginResponse.MessageId = responseData["message_id"]?.AsString;
            return loginResponse;
        }

        Logger.LogInfo("Login.Connect: Connection was successful.");

        if (ProcessLoginSuccessResponse(responseData, loginResponse))
        {
            loginResponse.LoginSucceeded = true;
            return loginResponse;
        }
        else
        {
            // Yet another error
        }

        return loginResponse;
    }

    protected bool ProcessLoginSuccessResponse(XmlRpcStruct responseData, LoginResponse loginResponse)
    {
        // TODO: Parse benefits
        // TODO: Parse "udp_blacklist"

        loginResponse.SessionId = Guid.Empty;
        if (responseData.Has("session_id"))
        {
            loginResponse.SessionId = Guid.Parse(responseData["session_id"].AsString);
        }
        if (loginResponse.SessionId == Guid.Empty)
        {
            return false;
        }

        #region Agent
        loginResponse.AgentId = Guid.Empty;
        if (responseData.Has("agent_id"))
        {
            loginResponse.AgentId = Guid.Parse(responseData["agent_id"].AsString);
        }
        if (loginResponse.AgentId == Guid.Empty)
        {
            return false;
        }

        // TODO: Send agentId and agentSessionId to the LLUrlEntryParcel


        //Guid agentSecureSessionId = Guid.Empty;
        //if (responseData.Has("secure_session_id"))
        //{
        //    agentSecureSessionId = Guid.Parse(responseData["secure_session_id"].AsString);
        //}

        string agentUserName = "";
        if (responseData.Has("first_name"))
        {
            agentUserName = responseData["first_name"].AsString.Replace('"', ' ').Trim(); // NOTE: login.cgi sends " to force names that look like numbers into strings
            loginResponse.FirstName = agentUserName;
        }
        if (responseData.Has("last_name"))
        {
            string lastName = responseData["last_name"].AsString.Replace('"', ' ').Trim(); // NOTE: login.cgi sends " to force names that look like numbers into strings
            loginResponse.LastName = lastName;
            if (lastName != "Resident")
            {
                agentUserName += $" {lastName}";
            }
        }
        string displayName = "";
        if (responseData.Has("display_name"))
        {
            displayName = responseData["display_name"].AsString.Replace('"', ' ').Trim(); // NOTE: login.cgi sends " to force names that look like numbers into strings
        }
        else if (agentUserName != "")
        {
            displayName = agentUserName;
        }
        else
        {
            // TODO: Construct display name from request credentials
        }
        loginResponse.DisplayName = displayName;

        //RegionMaturityLevel regionMaturityLevel = RegionMaturityLevel.A; // TODO: Get from settings
        //if (responseData.Has("agent_access_max"))
        //{
        //    Enum.TryParse<RegionMaturityLevel>(responseData["agent_access_max"].AsString, out regionMaturityLevel);
        //}

        //RegionMaturityLevel preferredMaturityLevel = RegionMaturityLevel.A; // TODO: Get from settings
        //if (responseData.Has("agent_region_access"))
        //{
        //    Enum.TryParse<RegionMaturityLevel>(responseData["agent_region_access"].AsString, out preferredMaturityLevel);
        //}

        //string agentStartLocation = "";
        //if (responseData.Has("start_location"))
        //{
        //    agentStartLocation = responseData["start_location"].AsString;
        //}

        //Vector3 agentStartLookAt = Vector3.forward;
        //if (responseData.Has("look_at"))
        //{
        //    // TODO: Decode "[r0.75787899999999996936,r0.65239599999999997593,r0]"
        //}

        #endregion Agent

        #region Region
        if (responseData.Has("region_x") && responseData.Has("region_y"))
        {
            UInt32 x = UInt32.Parse(responseData["region_x"].AsString);
            UInt32 y = UInt32.Parse(responseData["region_y"].AsString);
            loginResponse.RegionHandle = new RegionHandle (x, y);
        }

        loginResponse.CircuitCode = 0;
        loginResponse.SimIp = "";
        loginResponse.SimPort = 0;

        if (responseData.Has("circuit_code"))
        {
            loginResponse.CircuitCode = UInt32.Parse(responseData["circuit_code"].AsString);
        }
        if (responseData.Has("sim_ip"))
        {
            loginResponse.SimIp = responseData["sim_ip"].AsString;
        }
        if (responseData.Has("sim_port"))
        {
            loginResponse.SimPort = int.Parse(responseData["sim_port"].AsString);
        }
        if (loginResponse.CircuitCode == 0 || string.IsNullOrEmpty(loginResponse.SimIp) || loginResponse.SimPort == 0)
        {
            return false;
        }

        if (responseData.Has("seed_capability"))
        {
            loginResponse.SeedCapability = responseData["seed_capability"]?.AsString;
        }

        #endregion Region

        #region BuddyList

        if (responseData.Has("buddy-list") && responseData["buddy-list"] is XmlRpcArray)
        {
            loginResponse.BuddyList.Clear();
            foreach (XmlRpcValue value in (XmlRpcArray)responseData["buddy-list"])
            {
                if (value is XmlRpcStruct == false
                    || ((XmlRpcStruct)value).Has("buddy_id")           == false || ((XmlRpcStruct)value)["buddy_id"]           is XmlRpcString  == false
                    || ((XmlRpcStruct)value).Has("buddy_rights_given") == false || ((XmlRpcStruct)value)["buddy_rights_given"] is XmlRpcInteger == false
                    || ((XmlRpcStruct)value).Has("buddy_rights_has")   == false || ((XmlRpcStruct)value)["buddy_rights_has"]   is XmlRpcInteger == false
                    )
                {
                    continue;
                }

                XmlRpcStruct data = (XmlRpcStruct) value;
                Guid buddyId = Guid.Parse(data["buddy_id"].AsString);
                Relationship.Rights toAgent   = (Relationship.Rights)((XmlRpcInteger)data["buddy_rights_given"]).Value;
                Relationship.Rights fromAgent = (Relationship.Rights)((XmlRpcInteger)data["buddy_rights_has"]).Value;
                loginResponse.BuddyList[buddyId] = new Relationship(toAgent, fromAgent, false);
            }
        }
        #endregion BuddyList

        // TODO: Parse more things, see llstartup.cpp line 3439 and onwards

        // TODO: Non-existent Inventory.RootFolderId is a fail

        return true;
    }

    protected XmlRpcParameterArray CreateLoginParameters(Credential credential, Slurl slurl, bool getInventoryLibrary, bool godMode)
    {
        XmlRpcArray options = new XmlRpcArray();
        options.Append(new XmlRpcString("inventory-root"));
        options.Append(new XmlRpcString("inventory-skeleton"));
        //options.Append(new XmlRpcString("inventory-meat"));
        //options.Append(new XmlRpcString("inventory-skel-targets"));
        if (getInventoryLibrary == true)
        {
            options.Append(new XmlRpcString("inventory-lib-root"));
            options.Append(new XmlRpcString("inventory-lib-owner"));
            options.Append(new XmlRpcString("inventory-skel-lib"));
            //options.Append(new XmlRpcString("inventory-meat-lib"));
        }
        options.Append(new XmlRpcString("initial-outfit"));
        options.Append(new XmlRpcString("gestures"));
        options.Append(new XmlRpcString("display_names"));
        options.Append(new XmlRpcString("event_categories"));
        options.Append(new XmlRpcString("event_notifications"));
        options.Append(new XmlRpcString("classified_categories"));
        options.Append(new XmlRpcString("adult_compliant"));
        options.Append(new XmlRpcString("buddy-list"));
        options.Append(new XmlRpcString("newuser-config"));
        options.Append(new XmlRpcString("ui-config"));

        //send this info to login.cgi for stats gathering 
        //since viewerstats isn't reliable enough
        options.Append(new XmlRpcString("advanced-mode"));

        options.Append(new XmlRpcString("max-agent-groups"));
        options.Append(new XmlRpcString("map-server-url"));
        options.Append(new XmlRpcString("voice-config"));
        options.Append(new XmlRpcString("tutorial_setting"));
        options.Append(new XmlRpcString("login-flags"));
        options.Append(new XmlRpcString("global-textures"));

        if (godMode == true)
        {
            options.Append(new XmlRpcString("UseDebugMenus"));
            options.Append(new XmlRpcString("god-connect"));
        }

        XmlRpcStruct data = new XmlRpcStruct();
        data["start"] = new XmlRpcString(slurl.GetLoginString());
        data["agree_to_tos"] = new XmlRpcBoolean(false);
        data["read_critical"] = new XmlRpcBoolean(false);
        data["last_exec_event"] = new XmlRpcInteger(LastExecEvent);
        data["last_exec_duration"] = new XmlRpcInteger(LastExecDuration);
        data["mac"] = new XmlRpcString(MachineId);
        data["version"] = new XmlRpcString(ViewerVersion);
        data["channel"] = new XmlRpcString(ViewerChannel);
        data["platform"] = new XmlRpcString(Platform);
        data["address_size"] = new XmlRpcInteger(ADDRESS_SIZE);
        data["platform_version"] = new XmlRpcString(PlatformVersion);
        data["platform_string"] = new XmlRpcString(PlatformVersionName);
        data["id0"] = new XmlRpcString(SerialNumber);
        data["host_id"] = new XmlRpcString(HostId);
        data["extended_errors"] = new XmlRpcBoolean(true);

        data["passwd"] = new XmlRpcString(credential.Secret);
        data["first"] = new XmlRpcString(credential.First);
        data["last"] = new XmlRpcString(credential.Last);

        data["options"] = options;

        XmlRpcParameterArray parameters = new XmlRpcParameterArray();
        parameters.Append(data);
        return parameters;
    }
}
