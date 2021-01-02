using System;
using System.Threading.Tasks;
using SLViewerLib.Communication.XmlRpc;
using SLViewerLib.Communication.XmlRpc.DataTypes;
using UnityEngine;

public class Login
{
    public static Login Instance => _instance;

    private static Login _instance = new Login();

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

    protected UInt32 CircuitCode { get; set; }
    protected Guid SessionId { get; set; }
    protected Guid AgentId { get; set; }

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

    public async Task Connect (
        Credential credential,
        Slurl slurl = null,
        bool getInventoryLibrary = true,
        bool godMode = false)
    {
        string uri = GridManager.Instance.CurrentGrid.LoginUri;
        await Connect(uri, credential, slurl, getInventoryLibrary, godMode);
    }

    public async Task Connect(string uri, Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        if (slurl == null)
        {
            slurl = new Slurl(Slurl.SIM_LOCATION_LAST);
        }
        Logger.LogDebug($"INFO Login.Connect: Connecting {credential.First} {credential.Last} using {uri}.");

        XmlRpcParameterArray parameters = CreateLoginParameters(credential, slurl, getInventoryLibrary, godMode);
        
        XmlRpcResponse response = await XmlRpcClient.Call(uri, "login_to_simulator", parameters);
        if (response.FaultCode != 0)
        {
            Logger.LogWarning($"Login.Connect: Login failed. ({response.FaultCode} {response.FaultString})");
            return;
        }

        if (response.Parameters.Count != 1 || (response.Parameters[0] is XmlRpcStruct == false))
        {
            Logger.LogWarning("Login.Connect: Login response contained incorrect parameters.");
            return;
        }

        XmlRpcStruct responseData = (XmlRpcStruct)response.Parameters[0];

        if (    responseData.Has("login") == false
            || (responseData["login"] is XmlRpcString == false)
            || ((XmlRpcString)responseData["login"]).Value != "true")
        {

            // message_args = " "
            // reason       = "key", "update", "tos"
            // message      = "Sorry! We couldn't log you in. Please check to make sure you entered the right * Username (like bobsmith12 or steller.sunshine) * Password Also, please make sure your Caps Lock key is off."
            // message_id   = "LoginFailedAuthenticationFailed"
            Logger.LogWarning($"Login.Connect: {responseData["message_id"].AsString} {responseData["message"].AsString}");
            return;
        }

        Logger.LogInfo("Login.Connect: Connection was successful.");

        if (ProcessLoginSuccessResponse(responseData))
        {
            // Go to state STATE_WORLD_INIT

            await Region.CurrentRegion.Circuit.SendUseCircuitCode(CircuitCode, SessionId, AgentId);
            Logger.LogInfo("UseCircuitCode was acked.");
            await Region.CurrentRegion.Circuit.SendCompleteAgentMovement(AgentId, SessionId, CircuitCode);
            Logger.LogInfo("CompleteAgentMovement was acked.");

        }
        else
        {
            // Yet another error
        }
    }

    protected bool ProcessLoginSuccessResponse(XmlRpcStruct responseData)
    {
        // TODO: Parse benefits
        // TODO: Parse "udp_blacklist"

        SessionId = Guid.Empty;
        if (responseData.Has("session_id"))
        {
            SessionId = Guid.Parse(responseData["session_id"].AsString);
        }
        if (SessionId == Guid.Empty)
        {
            return false;
        }

        #region Agent
        AgentId = Guid.Empty;
        if (responseData.Has("agent_id"))
        {
            AgentId = Guid.Parse(responseData["agent_id"].AsString);
        }
        if (AgentId == Guid.Empty)
        {
            return false;
        }

        Agent agent = new Agent(AgentId);
        Agent.SetCurrentPlayer(agent);

        // TODO: Send agentId and agentSessionId to the LLUrlEntryParcel


        Guid agentSecureSessionId = Guid.Empty;
        if (responseData.Has("secure_session_id"))
        {
            agentSecureSessionId = Guid.Parse(responseData["secure_session_id"].AsString);
        }

        string agentUserName = "";
        if (responseData.Has("first_name"))
        {
            agentUserName = responseData["first_name"].AsString.Replace('"', ' ').Trim(); // NOTE: login.cgi sends " to force names that look like numbers into strings
            agent.FirstName = agentUserName;
        }
        if (responseData.Has("last_name"))
        {
            string lastName = responseData["last_name"].AsString.Replace('"', ' ').Trim(); // NOTE: login.cgi sends " to force names that look like numbers into strings
            agent.LastName = lastName;
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
        agent.DisplayName = displayName;

        RegionMaturityLevel regionMaturityLevel = RegionMaturityLevel.A; // TODO: Get from settings
        if (responseData.Has("agent_access_max"))
        {
            Enum.TryParse<RegionMaturityLevel>(responseData["agent_access_max"].AsString, out regionMaturityLevel);
        }

        RegionMaturityLevel preferredMaturityLevel = RegionMaturityLevel.A; // TODO: Get from settings
        if (responseData.Has("agent_region_access"))
        {
            Enum.TryParse<RegionMaturityLevel>(responseData["agent_region_access"].AsString, out preferredMaturityLevel);
        }

        string agentStartLocation = "";
        if (responseData.Has("start_location"))
        {
            agentStartLocation = responseData["start_location"].AsString;
        }

        Vector3 agentStartLookAt = Vector3.forward;
        if (responseData.Has("look_at"))
        {
            // TODO: Decode "[r0.75787899999999996936,r0.65239599999999997593,r0]"
        }

        EventManager.Instance.RaiseOnAgentDataChanged(agent);
        #endregion Agent

        #region Region
        Region region = new Region();
        Region.SetCurrentRegion(region);

        if (responseData.Has("region_x") && responseData.Has("region_y"))
        {
            UInt32 x = UInt32.Parse(responseData["region_x"].AsString);
            UInt32 y = UInt32.Parse(responseData["region_y"].AsString);
            region.Handle = new RegionHandle (x, y);
        }

        CircuitCode = 0;
        string simIp = "";
        int simPort = 0;
        if (responseData.Has("circuit_code"))
        {
            CircuitCode = UInt32.Parse(responseData["circuit_code"].AsString);
        }
        if (responseData.Has("sim_ip"))
        {
            simIp = responseData["sim_ip"].AsString;
        }
        if (responseData.Has("sim_port"))
        {
            simPort = int.Parse(responseData["sim_port"].AsString);
        }
        if (CircuitCode == 0 || string.IsNullOrEmpty(simIp) || simPort == 0)
        {
            return false;
        }
        region.Circuit = SlMessageSystem.Instance.EnableCircuit(simIp, simPort);

        #endregion Region



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
