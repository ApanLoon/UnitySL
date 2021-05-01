using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Agents;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Region;
using Assets.Scripts.Regions;

public class Session
{
    public static Session Instance = new Session();

    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }
    public UInt32 CircuitCode { get; set; } // TODO: Should not be here
    public Host FirstSimHost { get; set; }

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
        Logger.LogDebug("Session.Start", "LOGIN------------------------------");
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
            Logger.LogWarning("Session.Start", $"Login.Connect returned {loginResponse.MessageId} {loginResponse.Message}");
            return;
        }

        SessionId = loginResponse.SessionId;
        #endregion Login

        #region WorldInit
        Logger.LogDebug("Session.Start", "WORLD_INIT-------------------------");
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

        // Before we create the first region, we need to set the agent's OriginGlobal
        // This is necessary because creating objects before this is set will result in a
        // bad PositionAgent cache.
        agent.InitOriginGlobal (loginResponse.RegionHandle.ToVector3Double());

        FirstSimHost = new Host(loginResponse.SimIp, loginResponse.SimPort);
        Region region = World.Instance.AddRegion(loginResponse.RegionHandle, FirstSimHost);

        Logger.LogInfo("Session.Start", "Requesting capability grants...");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Requesting capability grants...", 0.32f);
        
        Task seedCapabilitiesTask = region.SetSeedCapability(loginResponse.SeedCapability);

        agent.Region = region;
        #endregion WorldInit

        #region MultimediaInit
        Logger.LogDebug("Session.Start", "MULTIMEDIA_INIT--------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Initializing multimedia...", 0.42f);

        #endregion MultimediaInit

        #region FontInit
        Logger.LogDebug("Session.Start", "FONT_INIT--------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Initializing fonts...", 0.45f);

        #endregion FontInit

        #region SeedGrantedWait
        Logger.LogDebug("Session.Start", "SEED_GRANTED_WAIT------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Waiting for region capabilities...", 0.47f);

        await seedCapabilitiesTask;
        Logger.LogInfo("Session.Start", "Got capability grants.");

        #endregion SeedGrantedWait

        #region SeedCapabilitiesGranted
        Logger.LogDebug("Session.Start", "SEED_CAPABILITIES_GRANTED----------");

        RegisterEventListeners();

        CircuitCode = loginResponse.CircuitCode;
        region.Circuit = SlMessageSystem.Instance.EnableCircuit(new Host(loginResponse.SimIp, loginResponse.SimPort));

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Waiting for region handshake...", 0.59f);
        await region.Circuit.SendUseCircuitCode(loginResponse.CircuitCode, SessionId, loginResponse.AgentId);
        Logger.LogInfo("Session.Start", "UseCircuitCode was acked.");

        AvatarNameCache.Instance.Start();
        #endregion SeedCapabilitiesGranted

        #region AgentSend
        Logger.LogDebug("Session.Start", "AGENT_SEND-------------------------");

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Connecting to region...", 0.6f);
        await region.Circuit.SendCompleteAgentMovement(loginResponse.AgentId, SessionId, loginResponse.CircuitCode);
        Logger.LogInfo("Session.Start", "CompleteAgentMovement was acked.");
        #endregion AgentSend

        #region InventorySend
        Logger.LogDebug("Session.Start", "INVENTORY_SEND---------------------");

        //TODO: Fill in inventory skeleton and request details
        
        //Fill in buddy list skeleton and request names:
        AvatarTracker.Instance.AddBuddyList(loginResponse.BuddyList);

        // Set up callbacks:
        AvatarTracker.Instance.RegisterCallbacks();

        //TODO: Request mute list
        //TODO: Request money balance

        awaitables.Add(region.Circuit.SendAgentDataUpdateRequest(agent.Id, SessionId));

        #endregion InventorySend

        #region Misc
        Logger.LogDebug("Session.Start", "MISC-------------------------------");

        //TODO: Calculate max bandwidth
        awaitables.Add (region.Circuit.SendAgentThrottle());

        //TODO: Download audio
        //TODO: Download active gestures

        awaitables.Add(region.Circuit.SendAgentHeightWidth(1080, 1920)); // TODO: This should take the title and status bars into account.

        #endregion Misc

        #region Precache
        Logger.LogDebug("Session.Start", "PRECACHE---------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Login", "Loading world...", 0.9f);

        //TODO: Send AgentIsNowWearing

        await Task.WhenAll(awaitables.ToArray());
        awaitables.Clear();
        #endregion Precache

        #region Cleanup
        Logger.LogDebug("Session.Start", "CLEANUP----------------------------");

        //TODO: Make map view observe inventory
        //TODO: Make map view observe friends
        //TODO: Stop Away animation
        //TODO: Clear control flag Away
        
        // Observe friends
        Agent.CurrentPlayer.ObserveFriends();
        
        //TODO: Retrieve land description
        //TODO: Send hover height to capability "AgentPreferences"

        EventManager.Instance.RaiseOnProgressUpdate("Login", "Complete", 1f);
        await Task.Delay(1000); // Wait to let player see the "Complete" message.
        EventManager.Instance.RaiseOnProgressUpdate("Login", "", 1f, true);
        #endregion Cleanup

        IsLoggedIn = true;

        await Task.Delay(3000);
        Logger.LogDebug("Session.Start", "POST----------------");

        // TODO: This is in the application loop in Indra:
        VolumeLayerManager.UnpackLayerData();

    }

    public async Task Stop()
    {
        if (IsLoggedIn == false)
        {
            return;
        }

        Logger.LogDebug("Session.Start", "LOGOUT-----------------------------");
        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Logging out...", 0.2f);

        AvatarTracker.Instance.ClearBuddyList();

        UnregisterEventListeners();

        IsLogoutPending = true;

        if (   Agent.CurrentPlayer                != null
            && Agent.CurrentPlayer.Region         != null
            && Agent.CurrentPlayer.Region.Circuit != null)
        {
            EventManager.Instance.OnLogoutReplyMessage += OnLogoutReplyMessage;

            Circuit circuit = Agent.CurrentPlayer.Region.Circuit;
            await circuit.SendLogoutRequest(AgentId, SessionId);

            // Wait for LogOutReply:
            int frequency = 10;
            int timeout = 2000;
            var waitTask = Task.Run(async () =>
            {
                while (IsLogoutPending)
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                Logger.LogError("Session.Stop", "LogoutReply took too long.");
            }

            circuit.Stop();
            EventManager.Instance.OnLogoutReplyMessage -= OnLogoutReplyMessage;
        }
        else
        {
            Logger.LogWarning("Session.Stop", $"Unable to send logout request. ({(Agent.CurrentPlayer == null ? "CurrentPlayer=null" : "")} {(Agent.CurrentPlayer.Region == null ? "Region=null" : "")} {(Agent.CurrentPlayer.Region.Circuit == null ? "Circuit=null" : "")})");
        }
        IsLogoutPending = false;

        IsLoggedIn = false;

        await Task.Delay(500); // Give the circuit a chance to shut down
        EventManager.Instance.RaiseOnLogout();

        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Complete", 1f);
        await Task.Delay(1000); // Wait to let player see the "Complete" message.
        EventManager.Instance.RaiseOnProgressUpdate("Logout", "", 1f, true);
    }

    protected void OnLogoutReplyMessage(LogoutReplyMessage message)
    {
        if (message.AgentId != AgentId || message.SessionId != SessionId)
        {
            Logger.LogWarning("Session.OnLogoutReplyMessage", $"Received LogoutReply for unknown agent or session. (AgentId={message.AgentId}, sessionId={message.SessionId})");
            return;
        }

        EventManager.Instance.RaiseOnProgressUpdate("Logout", "Updating inventory items...", 0.8f);

        // TODO: Do something with the inventory items.

        IsLogoutPending = false;
    }


    protected void RegisterEventListeners()
    {
        EventManager.Instance.OnLayerDataMessage += ProcessLayerData;
        //msg->setHandlerFuncFast(_PREHASH_ImageData, LLViewerTextureList::receiveImageHeader);
        //msg->setHandlerFuncFast(_PREHASH_ImagePacket, LLViewerTextureList::receiveImagePacket);
        //msg->setHandlerFuncFast(_PREHASH_ObjectUpdate, process_object_update);

        EventManager.Instance.OnObjectUpdateMessage += ProcessObjectUpdate;
        //msg->setHandlerFunc("ObjectUpdateCached", process_cached_object_update);
        //msg->setHandlerFuncFast(_PREHASH_ImprovedTerseObjectUpdate, process_terse_object_update_improved);
        //msg->setHandlerFunc("SimStats", process_sim_stats);
        //msg->setHandlerFuncFast(_PREHASH_HealthMessage, process_health_message);
        //msg->setHandlerFuncFast(_PREHASH_EconomyData, process_economy_data);
        //msg->setHandlerFunc("RegionInfo", LLViewerRegion::processRegionInfo);

        //msg->setHandlerFuncFast(_PREHASH_ChatFromSimulator, process_chat_from_simulator);
        //msg->setHandlerFuncFast(_PREHASH_KillObject, process_kill_object, NULL);
        //msg->setHandlerFuncFast(_PREHASH_SimulatorViewerTimeMessage, process_time_synch, NULL);
        //msg->setHandlerFuncFast(_PREHASH_EnableSimulator, process_enable_simulator);
        //msg->setHandlerFuncFast(_PREHASH_DisableSimulator, process_disable_simulator);
        //msg->setHandlerFuncFast(_PREHASH_KickUser, process_kick_user, NULL);

        //msg->setHandlerFunc("CrossedRegion", process_crossed_region);
        //msg->setHandlerFuncFast(_PREHASH_TeleportFinish, process_teleport_finish);

        //msg->setHandlerFuncFast(_PREHASH_AlertMessage, process_alert_message);
        //msg->setHandlerFunc("AgentAlertMessage", process_agent_alert_message);
        //msg->setHandlerFuncFast(_PREHASH_MeanCollisionAlert, process_mean_collision_alert_message, NULL);
        //msg->setHandlerFunc("ViewerFrozenMessage", process_frozen_message);

        //msg->setHandlerFuncFast(_PREHASH_NameValuePair, process_name_value);
        //msg->setHandlerFuncFast(_PREHASH_RemoveNameValuePair, process_remove_name_value);
        //msg->setHandlerFuncFast(_PREHASH_AvatarAnimation, process_avatar_animation);
        //msg->setHandlerFuncFast(_PREHASH_ObjectAnimation, process_object_animation);
        //msg->setHandlerFuncFast(_PREHASH_AvatarAppearance, process_avatar_appearance);
        //msg->setHandlerFuncFast(_PREHASH_CameraConstraint, process_camera_constraint);
        //msg->setHandlerFuncFast(_PREHASH_AvatarSitResponse, process_avatar_sit_response);
        //msg->setHandlerFunc("SetFollowCamProperties", process_set_follow_cam_properties);
        //msg->setHandlerFunc("ClearFollowCamProperties", process_clear_follow_cam_properties);

        //msg->setHandlerFuncFast(_PREHASH_ImprovedInstantMessage, process_improved_im);
        //msg->setHandlerFuncFast(_PREHASH_ScriptQuestion, process_script_question);
        //msg->setHandlerFuncFast(_PREHASH_ObjectProperties, LLSelectMgr::processObjectProperties, NULL);
        //msg->setHandlerFuncFast(_PREHASH_ObjectPropertiesFamily, LLSelectMgr::processObjectPropertiesFamily, NULL);
        //msg->setHandlerFunc("ForceObjectSelect", LLSelectMgr::processForceObjectSelect);

        //msg->setHandlerFuncFast(_PREHASH_MoneyBalanceReply, process_money_balance_reply, NULL);
        //msg->setHandlerFuncFast(_PREHASH_CoarseLocationUpdate, LLWorld::processCoarseUpdate, NULL);
        //msg->setHandlerFuncFast(_PREHASH_ReplyTaskInventory, LLViewerObject::processTaskInv, NULL);
        //msg->setHandlerFuncFast(_PREHASH_DerezContainer, process_derez_container, NULL);
        //msg->setHandlerFuncFast(_PREHASH_ScriptRunningReply,
        //                    &LLLiveLSLEditor::processScriptRunningReply);

        //msg->setHandlerFuncFast(_PREHASH_DeRezAck, process_derez_ack);

        //msg->setHandlerFunc("LogoutReply", process_logout_reply);

        ////msg->setHandlerFuncFast(_PREHASH_AddModifyAbility,
        ////					&LLAgent::processAddModifyAbility);
        ////msg->setHandlerFuncFast(_PREHASH_RemoveModifyAbility,
        ////					&LLAgent::processRemoveModifyAbility);
        //msg->setHandlerFuncFast(_PREHASH_AgentDataUpdate,
        //                    &LLAgent::processAgentDataUpdate);
        //msg->setHandlerFuncFast(_PREHASH_AgentGroupDataUpdate,
        //                    &LLAgent::processAgentGroupDataUpdate);
        //msg->setHandlerFunc("AgentDropGroup",
        //                    &LLAgent::processAgentDropGroup);
        //// land ownership messages
        //msg->setHandlerFuncFast(_PREHASH_ParcelOverlay,
        //                    LLViewerParcelMgr::processParcelOverlay);
        //msg->setHandlerFuncFast(_PREHASH_ParcelProperties,
        //                    LLViewerParcelMgr::processParcelProperties);
        //msg->setHandlerFunc("ParcelAccessListReply",
        //    LLViewerParcelMgr::processParcelAccessListReply);
        //msg->setHandlerFunc("ParcelDwellReply",
        //    LLViewerParcelMgr::processParcelDwellReply);

        //msg->setHandlerFunc("AvatarPropertiesReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarPropertiesReply);
        //msg->setHandlerFunc("AvatarInterestsReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarInterestsReply);
        //msg->setHandlerFunc("AvatarGroupsReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarGroupsReply);
        //// ratings deprecated
        ////msg->setHandlerFuncFast(_PREHASH_AvatarStatisticsReply,
        ////					LLPanelAvatar::processAvatarStatisticsReply);
        //msg->setHandlerFunc("AvatarNotesReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarNotesReply);
        //msg->setHandlerFunc("AvatarPicksReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarPicksReply);
        //msg->setHandlerFunc("AvatarClassifiedReply",
        //                    &LLAvatarPropertiesProcessor::processAvatarClassifiedsReply);

        //msg->setHandlerFuncFast(_PREHASH_CreateGroupReply,
        //                    LLGroupMgr::processCreateGroupReply);
        //msg->setHandlerFuncFast(_PREHASH_JoinGroupReply,
        //                    LLGroupMgr::processJoinGroupReply);
        //msg->setHandlerFuncFast(_PREHASH_EjectGroupMemberReply,
        //                    LLGroupMgr::processEjectGroupMemberReply);
        //msg->setHandlerFuncFast(_PREHASH_LeaveGroupReply,
        //                    LLGroupMgr::processLeaveGroupReply);
        //msg->setHandlerFuncFast(_PREHASH_GroupProfileReply,
        //                    LLGroupMgr::processGroupPropertiesReply);

        //// ratings deprecated
        //// msg->setHandlerFuncFast(_PREHASH_ReputationIndividualReply,
        ////					LLFloaterRate::processReputationIndividualReply);

        //msg->setHandlerFunc("ScriptControlChange",
        //                    LLAgent::processScriptControlChange);

        //msg->setHandlerFuncFast(_PREHASH_ViewerEffect, LLHUDManager::processViewerEffect);

        //msg->setHandlerFuncFast(_PREHASH_GrantGodlikePowers, process_grant_godlike_powers);

        //msg->setHandlerFuncFast(_PREHASH_GroupAccountSummaryReply,
        //                        LLPanelGroupLandMoney::processGroupAccountSummaryReply);
        //msg->setHandlerFuncFast(_PREHASH_GroupAccountDetailsReply,
        //                        LLPanelGroupLandMoney::processGroupAccountDetailsReply);
        //msg->setHandlerFuncFast(_PREHASH_GroupAccountTransactionsReply,
        //                        LLPanelGroupLandMoney::processGroupAccountTransactionsReply);

        //msg->setHandlerFuncFast(_PREHASH_UserInfoReply,
        //    process_user_info_reply);

        //msg->setHandlerFunc("RegionHandshake", process_region_handshake, NULL);

        //msg->setHandlerFunc("TeleportStart", process_teleport_start);
        //msg->setHandlerFunc("TeleportProgress", process_teleport_progress);
        //msg->setHandlerFunc("TeleportFailed", process_teleport_failed, NULL);
        //msg->setHandlerFunc("TeleportLocal", process_teleport_local, NULL);

        //msg->setHandlerFunc("ImageNotInDatabase", LLViewerTextureList::processImageNotInDatabase, NULL);

        //msg->setHandlerFuncFast(_PREHASH_GroupMembersReply,
        //                    LLGroupMgr::processGroupMembersReply);
        //msg->setHandlerFunc("GroupRoleDataReply",
        //                    LLGroupMgr::processGroupRoleDataReply);
        //msg->setHandlerFunc("GroupRoleMembersReply",
        //                    LLGroupMgr::processGroupRoleMembersReply);
        //msg->setHandlerFunc("GroupTitlesReply",
        //                    LLGroupMgr::processGroupTitlesReply);
        //// Special handler as this message is sometimes used for group land.
        //msg->setHandlerFunc("PlacesReply", process_places_reply);
        //msg->setHandlerFunc("GroupNoticesListReply", LLPanelGroupNotices::processGroupNoticesListReply);

        //msg->setHandlerFunc("AvatarPickerReply", LLFloaterAvatarPicker::processAvatarPickerReply);

        //msg->setHandlerFunc("MapBlockReply", LLWorldMapMessage::processMapBlockReply);
        //msg->setHandlerFunc("MapItemReply", LLWorldMapMessage::processMapItemReply);
        //msg->setHandlerFunc("EventInfoReply", LLEventNotifier::processEventInfoReply);

        //msg->setHandlerFunc("PickInfoReply", &LLAvatarPropertiesProcessor::processPickInfoReply);
        ////	msg->setHandlerFunc("ClassifiedInfoReply", LLPanelClassified::processClassifiedInfoReply);
        //msg->setHandlerFunc("ClassifiedInfoReply", LLAvatarPropertiesProcessor::processClassifiedInfoReply);
        //msg->setHandlerFunc("ParcelInfoReply", LLRemoteParcelInfoProcessor::processParcelInfoReply);
        //msg->setHandlerFunc("ScriptDialog", process_script_dialog);
        //msg->setHandlerFunc("LoadURL", process_load_url);
        //msg->setHandlerFunc("ScriptTeleportRequest", process_script_teleport_request);
        //msg->setHandlerFunc("EstateCovenantReply", process_covenant_reply);

        //// calling cards
        //msg->setHandlerFunc("OfferCallingCard", process_offer_callingcard);
        //msg->setHandlerFunc("AcceptCallingCard", process_accept_callingcard);
        //msg->setHandlerFunc("DeclineCallingCard", process_decline_callingcard);

        //msg->setHandlerFunc("ParcelObjectOwnersReply", LLPanelLandObjects::processParcelObjectOwnersReply);

        //msg->setHandlerFunc("InitiateDownload", process_initiate_download);
        //msg->setHandlerFunc("LandStatReply", LLFloaterTopObjects::handle_land_reply);
        //msg->setHandlerFunc("GenericMessage", process_generic_message);
        //msg->setHandlerFunc("LargeGenericMessage", process_large_generic_message);

        //msg->setHandlerFuncFast(_PREHASH_FeatureDisabled, process_feature_disabled_message);
    }

    private void UnregisterEventListeners()
    {
        EventManager.Instance.OnLayerDataMessage -= ProcessLayerData;
        EventManager.Instance.OnObjectUpdateMessage -= ProcessObjectUpdate;
    }

    protected void ProcessObjectUpdate (ObjectUpdateMessage obj)
    {
        
    }

    protected void ProcessLayerData(LayerDataMessage message) // TODO: This should not be here
    {
        //Logger.LogDebug("Session.ProcessLayerData", "");

        // TODO: Verify that the sending endpoint is associated with the current region

        VolumeLayerData vlData = new VolumeLayerData
        {
            LayerType = message.LayerType,
            Data = message.Data,
            Size = message.Data.Length,
            Region = Agent.CurrentPlayer?.Region
        };

        VolumeLayerManager.AddLayerData(vlData);
    }
}
