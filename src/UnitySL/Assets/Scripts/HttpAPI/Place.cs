using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HttpAPI
{
    public class Place
    {
        public const string WORLD_API_FETCH = "http://world.secondlife.com/place/";

        public string guid { get; private set; }
        public string title { get; private set; }
        public string description { get; private set; }
        public string region { get; private set; }
        public Vector3Int location { get; private set; }
        public string parcel { get; private set; }
        public string parcelid { get; private set; }
        public string area { get; private set; }
        public string ownerid { get; private set; }
        public string ownertype { get; private set; }
        public string owner { get; private set; }
        public string category { get; private set; }

        private bool fetched;

        public Place(string guid)
        {
            this.guid = guid;
        }

        public Place(string guid, string title, string description)
        {
            this.guid = guid;
            this.title = title;
            this.description = description;
        }

        /// <summary> Perform a http request to fetch the details about this place. Results are cached and will be returned immediately if called again. </summary>
        public void FetchDetails(Action<string> onError, Action<Place> onSuccess)
        {
            if (fetched) onSuccess(this);
            else
            {
                WWWFormPlus form = new WWWFormPlus();
                form.Request(WORLD_API_FETCH + guid, onError, x => { ApplyFetchResult(x); onSuccess(this); });
            }
        }

        private void ApplyFetchResult(string result)
        {

            title = FindTitle(result);
            region = FindMeta(result, "region");
            if (TryParseLocation(FindMeta(result, "location"), out Vector3Int location)) this.location = location;
            parcel = FindMeta(result, "parcel");
            parcelid = FindMeta(result, "parcelid");
            area = FindMeta(result, "area");
            ownerid = FindMeta(result, "ownerid");
            ownertype = FindMeta(result, "ownertype");
            owner = FindMeta(result, "owner");
            category = FindMeta(result, "category");
        }

        private string FindTitle(string text)
        {
            Match match = Regex.Match(text, $"<title>\\s*(.+?)\\s*<\\/title>");
            return match.Groups[1].Value;
        }

        private string FindMeta(string text, string name)
        {
            Match match = Regex.Match(text, $"<meta name=\"{name}\" content=\"(.+?)\"\\s*\\/>");
            return match.Groups[1].Value;
        }

        private bool TryParseLocation(string location, out Vector3Int result)
        {
            result = Vector3Int.zero;
            if (string.IsNullOrEmpty(location)) return false;

            string[] parts = location.Split('/');
            if (parts.Length != 3) return false;
            if (int.TryParse(parts[0], out int x)) result.x = x;
            if (int.TryParse(parts[1], out int y)) result.y = y;
            if (int.TryParse(parts[2], out int z)) result.z = z;
            return true;
        }
    }
}