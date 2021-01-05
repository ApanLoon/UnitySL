using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Session
{
    public static Session Instance = new Session();

    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }
    public UInt32 CircuitCode { get; set; } // TODO: Should not be here

    public async Task Start (Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        string uri = GridManager.Instance.CurrentGrid.LoginUri;
        await Start(uri, credential, slurl, getInventoryLibrary, godMode);
    }

    public async Task Start (string uri, Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        #region Login
        Logger.LogDebug("LOGIN------------------------------");
        Login login = new Login();
        LoginResponse loginResponse = await login.Connect(uri, credential, slurl, getInventoryLibrary, godMode);

        if (loginResponse.LoginSucceeded == false)
        {
            switch (loginResponse.LoginFailReason)
            {
                case "key":
                    break;

                case "update":
                    break;

                case "tos":
                    break;
            }
            Logger.LogWarning($"Login.Connect: {loginResponse.MessageId} {loginResponse.Message}");
            return;
        }

        SessionId = loginResponse.SessionId;
        #endregion Login

        #region WorldInit
        Logger.LogDebug("WORLD_INIT-------------------------");

        AgentId = loginResponse.AgentId;
        Agent agent = new Agent(loginResponse.AgentId)
        {
            DisplayName = loginResponse.DisplayName,
            FirstName = loginResponse.FirstName,
            LastName = loginResponse.LastName
        };
        Agent.SetCurrentPlayer(agent);
        EventManager.Instance.RaiseOnAgentDataChanged(agent);

        Region region = new Region
        {
            Handle = loginResponse.RegionHandle,
            SeedCapability = loginResponse.SeedCapability
        };
        Region.SetCurrentRegion(region);

        Logger.LogInfo("Requesting capability grants...");
        Task<Dictionary<string, string>> seedCapabilitiesTask = SeedCapabilities.RequestCapabilities(region.SeedCapability);

        agent.CurrentRegion = region;

        #endregion WorldInit

        #region MultimediaInit
        Logger.LogDebug("MULTIMEDIA_INIT--------------------");
        #endregion MultimediaInit

        #region FontInit
        Logger.LogDebug("FONT_INIT--------------------------");
        #endregion FontInit

        #region SeedGrantedWait
        Logger.LogDebug("SEED_GRANTED_WAIT------------------");

        Dictionary<string, string> grantedCapabilities = await seedCapabilitiesTask;
        Logger.LogInfo($"Got capability grants. ({grantedCapabilities?.Count})");

        #endregion SeedGrantedWait

        #region SeedCapabilitiesGranted
        Logger.LogDebug("SEED_CAPABILITIES_GRANTED----------");

        CircuitCode = loginResponse.CircuitCode;
        region.Circuit = SlMessageSystem.Instance.EnableCircuit(loginResponse.SimIp, loginResponse.SimPort);
        await Region.CurrentRegion.Circuit.SendUseCircuitCode(loginResponse.CircuitCode, SessionId, loginResponse.AgentId);
        Logger.LogInfo("UseCircuitCode was acked.");
        #endregion SeedCapabilitiesGranted

        #region AgentSend
        Logger.LogDebug("AGENT_SEND-------------------------");

        await Region.CurrentRegion.Circuit.SendCompleteAgentMovement(loginResponse.AgentId, SessionId, loginResponse.CircuitCode);
        Logger.LogInfo("CompleteAgentMovement was acked.");
        #endregion AgentSend

        #region InventorySend
        Logger.LogDebug("INVENTORY_SEND---------------------");
        #endregion InventorySend

        #region Misc
        Logger.LogDebug("MISC-------------------------------");
        #endregion Misc

        #region Precache
        Logger.LogDebug("PRECACHE---------------------------");
        #endregion Precache

        #region Cleanup
        Logger.LogDebug("CLEANUP----------------------------");
        #endregion Cleanup
    }
}
