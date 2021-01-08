using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Session
{
    public static Session Instance = new Session();

    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }
    public UInt32 CircuitCode { get; set; } // TODO: Should not be here

    public bool IsLoggedIn { get; protected set; }
    public bool IsLogoutPending { get; protected set; }

    public async Task Start (Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        string uri = GridManager.Instance.CurrentGrid.LoginUri;
        await Start(uri, credential, slurl, getInventoryLibrary, godMode);
    }

    public async Task Start (string uri, Credential credential, Slurl slurl = null, bool getInventoryLibrary = true, bool godMode = false)
    {
        List<Task> awaitables = new List<Task>();

        IsLoggedIn = false;
        IsLogoutPending = false;

        #region Login

        Logger.LogDebug("LOGIN------------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Logging in...", 0.2f);

        Login login = new Login();
        LoginResponse loginResponse = await login.Connect(uri, credential, slurl, getInventoryLibrary, godMode);


        if (loginResponse.LoginSucceeded == false)
        {
            EventManager.Instance.RaiseOnProgressUpdate("Login", "Login failed.", 0.29f, true);

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
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Initializing world...", 0.3f);

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
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Requesting capability grants...", 0.32f);
        Task<Dictionary<string, Capability>> seedCapabilitiesTask = SeedCapabilities.RequestCapabilities(region.SeedCapability);

        agent.CurrentRegion = region;

        #endregion WorldInit

        #region MultimediaInit
        Logger.LogDebug("MULTIMEDIA_INIT--------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Initializing multimedia...", 0.42f);

        #endregion MultimediaInit

        #region FontInit
        Logger.LogDebug("FONT_INIT--------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Initializing fonts...", 0.45f);

        #endregion FontInit

        #region SeedGrantedWait
        Logger.LogDebug("SEED_GRANTED_WAIT------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Waiting for region capabilities...", 0.47f);

        region.Capabilities = await seedCapabilitiesTask;
        Logger.LogInfo($"Got capability grants. ({region.Capabilities?.Count})");

        #endregion SeedGrantedWait

        #region SeedCapabilitiesGranted
        Logger.LogDebug("SEED_CAPABILITIES_GRANTED----------");

        CircuitCode = loginResponse.CircuitCode;
        region.Circuit = SlMessageSystem.Instance.EnableCircuit(loginResponse.SimIp, loginResponse.SimPort);

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Waiting for region handshake...", 0.59f);
        await Region.CurrentRegion.Circuit.SendUseCircuitCode(loginResponse.CircuitCode, SessionId, loginResponse.AgentId);
        Logger.LogInfo("UseCircuitCode was acked.");
        #endregion SeedCapabilitiesGranted

        #region AgentSend
        Logger.LogDebug("AGENT_SEND-------------------------");

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Connecting to region...", 0.6f);
        await Region.CurrentRegion.Circuit.SendCompleteAgentMovement(loginResponse.AgentId, SessionId, loginResponse.CircuitCode);
        Logger.LogInfo("CompleteAgentMovement was acked.");
        #endregion AgentSend

        #region InventorySend
        Logger.LogDebug("INVENTORY_SEND---------------------");

        //TODO: Fill in inventory skeleton and request details
        //TODO: Fill in buddy list skeleton and request details
        //TODO: Request mute list
        //TODO: Request money balance

        awaitables.Add(region.Circuit.SendAgentDataUpdateRequest(agent.Id, SessionId));

        #endregion InventorySend

        #region Misc
        Logger.LogDebug("MISC-------------------------------");

        //TODO: Calculate max bandwidth
        awaitables.Add (region.Circuit.SendAgentThrottle());

        //TODO: Download audio
        //TODO: Download active gestures
        
        awaitables.Add (region.Circuit.SendAgentHeightWidth(1080, 1920)); // TODO: This should take the title and status bars into account.

        #endregion Misc

        #region Precache
        Logger.LogDebug("PRECACHE---------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Loading world...", 0.9f);

        //TODO: Send AgentIsNowWearing

        await Task.WhenAll(awaitables.ToArray());
        awaitables.Clear();
        #endregion Precache

        #region Cleanup
        Logger.LogDebug("CLEANUP----------------------------");

        //TODO: Make map view observe inventory
        //TODO: Make map view observe friends
        //TODO: Stop Away animation
        //TODO: Clear control flag Away
        //TODO: Observe friends
        //TODO: Retrieve land description
        //TODO: Send hover height to capability "AgentPreferences"

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Complete", 1f);
        await Task.Delay(1000); // Wait to let player see the "Complete" message.
        EventManager.Instance.RaiseOnProgressUpdate("Login", "", 1f, true);
        #endregion Cleanup

        IsLoggedIn = true;
    }

    public async Task Stop()
    {
        if (IsLoggedIn == false)
        {
            return;
        }

        Logger.LogDebug("LOGOUT-----------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Logging out...", 0.2f);

        EventManager.Instance.OnLogoutReplyMessage += OnLogoutReplyMessage;

        IsLogoutPending = true;
        await Region.CurrentRegion.Circuit.SendLogoutRequest(AgentId, SessionId);

        // Wait for LogOutReply:
        int frequency = 10;
        int timeout = 1000;
        var waitTask = Task.Run(async () =>
        {
            while (IsLogoutPending)
            {
                await Task.Delay(frequency);
            }
        });

        if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
        {
            Logger.LogError("LogoutReply took too long.");
        }

        IsLogoutPending = false;
        IsLoggedIn = false;

        EventManager.Instance.OnLogoutReplyMessage -= OnLogoutReplyMessage;

        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Complete", 1f);
        await Task.Delay(1000); // Wait to let player see the "Complete" message.
        EventManager.Instance.RaiseOnProgressUpdate("Logout", "", 1f, true);
    }

    protected void OnLogoutReplyMessage(LogoutReplyMessage message)
    {
        if (message.AgentId != AgentId || message.SessionId != SessionId)
        {
            Logger.LogWarning($"Received LogoutReply for unknown agent or session. (AgentId={message.AgentId}, sessionId={message.SessionId})");
            return;
        }

        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Updating inventory items...", 0.8f);

        // TODO: Do something with the inventory items.

        IsLogoutPending = false;
    }
}
