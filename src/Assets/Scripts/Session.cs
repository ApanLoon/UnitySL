using System;
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

        Agent agent = new Agent(loginResponse.AgentId)
        {
            DisplayName = loginResponse.DisplayName,
            FirstName = loginResponse.FirstName,
            LastName = loginResponse.LastName
        };
        Agent.SetCurrentPlayer(agent);
        EventManager.Instance.RaiseOnAgentDataChanged(agent);

        Region region = new Region();
        region.Handle = loginResponse.RegionHandle;
        region.SeedCapability = loginResponse.SeedCapability;
        Region.SetCurrentRegion(region);

        agent.CurrentRegion = region;

        #endregion WorldInit

        #region MultimediaInit
        #endregion MultimediaInit

        #region FontInit
        #endregion FontInit

        #region SeedCapabilities
        #endregion SeedCapabilities

        #region SeedCapabilitiesGranted
        region.Circuit = SlMessageSystem.Instance.EnableCircuit(loginResponse.SimIp, loginResponse.SimPort);
        await Region.CurrentRegion.Circuit.SendUseCircuitCode(loginResponse.CircuitCode, SessionId, loginResponse.AgentId);
        Logger.LogInfo("UseCircuitCode was acked.");
        #endregion SeedCapabilitiesGranted

        #region AgentSend
        await Region.CurrentRegion.Circuit.SendCompleteAgentMovement(loginResponse.AgentId, SessionId, loginResponse.CircuitCode);
        Logger.LogInfo("CompleteAgentMovement was acked.");
        #endregion AgentSend

        #region InventorySend
        #endregion InventorySend

        #region Misc
        #endregion Misc

        #region Precache
        #endregion Precache

        #region Cleanup
        #endregion Cleanup
    }
}
