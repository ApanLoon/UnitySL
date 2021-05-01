using System.Collections.Generic;

public class GridManager
{
    // defines slurl formats associated with various grids.
    // we need to continue to support existing forms, as slurls
    // are shared between viewers that may not understand newer
    // forms.
    protected const string DEFAULT_LOGIN_PAGE = "https://viewer-splash.secondlife.com/";
    protected const string MAIN_GRID_LOGIN_URI = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
    protected const string SL_UPDATE_QUERY_URL = "https://update.secondlife.com/update";
    protected const string MAIN_GRID_SLURL_BASE = "http://maps.secondlife.com/secondlife/";
    protected const string SYSTEM_GRID_APP_SLURL_BASE = "secondlife:///app";
    protected const string MAIN_GRID_WEB_PROFILE_URL = "https://my.secondlife.com/";

    protected const string SYSTEM_GRID_SLURL_BASE = "secondlife://{0}/secondlife/";
    protected const string DEFAULT_SLURL_BASE = "https://{0}/region/";
    protected const string DEFAULT_APP_SLURL_BASE = "x-grid-location-info://{0}/app";

    protected const string MAINGRID = "util.agni.lindenlab.com";

    public static GridManager Instance { get; } = new GridManager();

    public Grid CurrentGrid { get; protected set; }

    protected List<Grid> Grids = new List<Grid>();
    protected Dictionary<string, Grid> GridByLabel = new Dictionary<string, Grid>();
    protected Dictionary<string, Grid> GridByKey = new Dictionary<string, Grid>();
    protected Dictionary<string, Grid> GridById = new Dictionary<string, Grid>();

    public GridManager()
    {
        AddSystemGrid (MAINGRID,
                       "Agni", // TODO: Localization - LLTrans::getString("AgniGridLabel"),
                       MAIN_GRID_LOGIN_URI,
                       "https://secondlife.com/helpers/",
                       DEFAULT_LOGIN_PAGE,
                       SL_UPDATE_QUERY_URL,
                       MAIN_GRID_WEB_PROFILE_URL,
                       "Agni");

        AddSystemGrid("util.aditi.lindenlab.com", 
                      "Aditi", // TODO: Localization - LLTrans::getString("AditiGridLabel"),
                      "https://login.aditi.lindenlab.com/cgi-bin/login.cgi",
                      "https://secondlife.aditi.lindenlab.com/helpers/",
                      DEFAULT_LOGIN_PAGE,
                      SL_UPDATE_QUERY_URL,
                      "https://my.aditi.lindenlab.com/",
                      "Aditi");

        // TODO: Load grids from gridFile

        // TODO: Load a grid from command line

        // TODO: Get last grid from settings

        SelectGrid(MAINGRID); // TODO: Get most recent grid from settings
    }

    public void SelectGrid(string identifier)
    {
        Grid grid = GetGrid(identifier);
        if (grid == null)
        {
            Logger.LogWarning("GridManager.SelectGrid", $"Unknown grid \"{identifier}\".");
            return;
        }

        CurrentGrid = grid;
        // TODO: Save current grid as last grid in settings
    }

    public Grid GetGrid(string identifier)
    {
        if (GridByKey.ContainsKey(identifier))
        {
            return GridByKey[identifier];
        }

        if (GridById.ContainsKey(identifier))
        {
            return GridById[identifier];
        }

        return null;
    }

    protected void AddSystemGrid (
        string key,
        string label,
        string loginUri,
        string helperUri,
        string loginPageUri,
        string updateUri,
        string webProfileUri,
        string loginId = "")
    {
        string id = loginId == "" ? key : loginId;
        string slurlBase = key == MAINGRID ? MAIN_GRID_SLURL_BASE : string.Format(SYSTEM_GRID_SLURL_BASE, id);

        Grid grid = new Grid(id, key, label, loginUri, helperUri, loginPageUri, updateUri, webProfileUri, slurlBase, true);
        AddGrid(grid);
    }

    public void AddGrid(Grid grid)
    {
        Grids.Add(grid);
        GridById[grid.Id] = grid;
        GridByKey[grid.Key] = grid;
        GridByLabel[grid.Label] = grid;
    }
}
