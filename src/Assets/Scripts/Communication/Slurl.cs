﻿
using System;
using UnityEngine;

public enum SlUrlType
{
    Invalid,
    Location,
    HomeLocation,
    LastLocation,
    App,
    Help,
    NumSlurlTypes // must be last
};

public class Slurl
{
    public static readonly string SLURL_HTTPS_SCHEME = "https";
    public static readonly string SLURL_HTTP_SCHEME = "http";
    public static readonly string SLURL_SL_SCHEME;
    public static readonly string SLURL_SECONDLIFE_SCHEME = "secondlife";
    public static readonly string SLURL_SECONDLIFE_PATH = "secondlife";
    public static readonly string SLURL_COM = "slurl.com";
    // For DnD - even though www.slurl.com redirects to slurl.com in a browser, you  can copy and drag
    // text with www.slurl.com or a link explicitly pointing at www.slurl.com so testing for this
    // version is required also.

    public static readonly string WWW_SLURL_COM = "www.slurl.com";
    public static readonly string SECONDLIFE_COM;
    public static readonly string MAPS_SECONDLIFE_COM = "maps.secondlife.com";
    public static readonly string SLURL_X_GRID_LOCATION_INFO_SCHEME = "x-grid-location-info";
    public static Slurl START_LOCATION;
    public static readonly string SIM_LOCATION_HOME = "home";
    public static readonly string SIM_LOCATION_LAST = "last";
    public static readonly string SLURL_APP_PATH = "app";
    public static readonly string SLURL_REGION_PATH = "region";

    public SlUrlType SlurlType { get; protected set; }
    public string GridIdentifier { get; protected set; }  // reference to grid manager grid

    // used for Apps and Help
    public string AppCmd { get; protected set; }
    //public LLSD AppPath { get; protected set; }
    //public LLSD AppQueryMap { get; protected set; }
    public string AppQuery { get; protected set; }

    public string Region { get; protected set; }
    public Vector3 Position { get; protected set; }

    public Slurl()
    {
        SlurlType = SlUrlType.Invalid;
    }

    public Slurl(string slurl)
    {
        SlurlType = SlUrlType.Invalid;

        if (slurl == SIM_LOCATION_HOME)
        {
            SlurlType = SlUrlType.HomeLocation;
            return;
        }

        if (string.IsNullOrEmpty(slurl) || slurl == SIM_LOCATION_LAST)
        {
            SlurlType = SlUrlType.LastLocation;
            return;
        }

        Uri uri = new Uri(slurl);
        // TODO: Check bits
    }
    public Slurl(string gridIdentifier, string region)
    {
        throw new NotImplementedException();
    }
    public Slurl(string region, Vector3 position)
    {
        throw new NotImplementedException();
    }
    public Slurl(string gridIdentifier, string region, Vector3 position)
    {
        throw new NotImplementedException();
    }
    //public Slurl(string grid, string region, LLVector3d global_position)
    //{
    //    throw new NotImplementedException();
    //}
    //public Slurl(string region, LLVector3d global_position)
    //{
    //    throw new NotImplementedException();
    //}
    //public Slurl(string command, LLUUID id, string verb)
    //{
    //    throw new NotImplementedException();
    //}

    public string GetSlurlString()
    {
        switch (SlurlType)
        {
            case SlUrlType.HomeLocation:
                return SIM_LOCATION_HOME;

            case SlUrlType.LastLocation:
                return SIM_LOCATION_LAST;

            case SlUrlType.Location:
                {
                    // lookup the grid
                    int x = (int)Math.Round(Position.x);
                    int y = (int)Math.Round(Position.y);
                    int z = (int)Math.Round(Position.z);
                    Grid grid = GridManager.Instance.GetGrid(GridIdentifier);
                    return $"{grid.SlurlBase}{Uri.EscapeDataString(Region)}/{x}/{y}/{z}";
                }

            //case SlUrlType.App:
            //    {
            //        string appUrl = GridManager.Instance.GetAppSlUrlBase(Grid) + "/" + AppCmd;
            //        foreach (LLSD item in AppPath.GetItems())
            //        {
            //            appUrl += $"/{item.AsString}";
            //        }

            //        if (string.IsNullOrEmpty(AppQuery) == false)
            //        {
            //            appUrl += $"?{AppQuery}";
            //        }
            //        return appUrl;
            //    }

            default:
                Logger.LogWarning($"SlUrl.GetSlUrlString: Unexpected SLURL type for SLURL string ({(int)SlurlType})");
                return "";
        }
    }

    public string GetLoginString()
    {
        string s = "";
        switch (SlurlType)
        {
            case SlUrlType.Location:
                int x = (int)Math.Round(Position.x);
                int y = (int)Math.Round(Position.y);
                int z = (int)Math.Round(Position.z);
                s = $"uri:{Region}&{x}&{y}&{z}";
                break;

            case SlUrlType.HomeLocation:
                s = "home";
                break;

            case SlUrlType.LastLocation:
                s = "last";
                break;

            default:
                Logger.LogWarning($"SlUrl.GetLoginString: Unexpected SLURL type ({(int)SlurlType} for login string");
                break;
        }
        return s;
    }

    public string GetLocationString()
    {
        int x = (int)Math.Round(Position.x);
        int y = (int)Math.Round(Position.y);
        int z = (int)Math.Round(Position.z);
        return $"{Region}/{x}/{y}/{z}";
    }

    public bool IsValid()
    {
        return SlurlType != SlUrlType.Invalid;
    }

    public bool IsSpatial()
    {
        return (SlurlType == SlUrlType.LastLocation) || (SlurlType == SlUrlType.HomeLocation) || (SlurlType == SlUrlType.Location);
    }

    public override string ToString()
    {
        return $"   Type:        {SlurlType}\n"
               + $"   Grid:        {GridIdentifier}\n"
               + $"   Region:      {Region}\n"
               + $"   Position:    {Position}\n"
               + $"   AppCmd:      {AppCmd}\n"
               //+ $"   AppPath:     {AppPath}\n"
               //+ $"   AppQueryMap: {AppQueryMap}\n"
               + $"   AppQuery:    {AppQuery}";
    }
}

