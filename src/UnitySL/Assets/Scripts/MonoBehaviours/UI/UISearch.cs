using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public enum MaturityRating { PG = 1, Mature = 2, Adult = 4 }

public class UISearch : MonoBehaviour
{
    public enum Category { All, Events, Groups, People, Places, Wiki }

    public TMP_InputField searchInput;
    public UISearchItemTemplate items;
    public int start = 0;
    public Category category;
    public bool pg;
    public bool mature;
    public bool adult;

    private void Start()
    {
        items.Initialize();
    }

    public void Search()
    {
        MaturityRating rating = 0;
        if (pg) rating |= MaturityRating.PG;
        if (mature) rating |= MaturityRating.Mature;
        if (adult) rating |= MaturityRating.Adult;
        WWWFormPlus form = new WWWFormPlus();
        string url = $"http://search.secondlife.com/client_search.php?q=hippo&start={start}&mat={(int)rating}&output=xml_no_dtd&client = raw_xml_frontend&s={category.ToString()}";
        Debug.Log(url);
        form.Request(url, OnFail, OnSuccess);
    }

    public void OnFail(string msg)
    {
        Debug.LogWarning("Fail: " + msg);
    }

    public void OnSuccess(string text)
    {
        // Result comes with some symbols that cannot be parsed.
        // Manually pull these out for now till we figure what else to do.
        text = Regex.Replace(text, "&nbsp;", "");

        XmlSerializer serializer = new XmlSerializer(typeof(SearchResultXML.Html));

        // convert string to stream
        byte[] byteArray = Encoding.UTF8.GetBytes(text);
        //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
        MemoryStream stream = new MemoryStream(byteArray);
        SearchResultXML.Html results = serializer.Deserialize(stream) as SearchResultXML.Html;
        SearchResultXML.ResultsDiv resultsContainer = results.Body.Wrapper.Divs.Find(x => x.Class == "results_container");
        if (resultsContainer == null)
        {
            OnFail("No results found in XML");
            return;
        }
		foreach(var r in resultsContainer.Results) {
			Debug.LogWarning(r.Class);

		}
        List<SearchResultXML.ResultDiv> resultsPlaces = resultsContainer.Results.FindAll(x => x.Class == "result place_icon");
        List<SearchResultXML.ResultDiv> resultsGroups = resultsContainer.Results.FindAll(x => x.Class == "result group_icon");
        List<SearchResultXML.ResultDiv> resultsResidents = resultsContainer.Results.FindAll(x => x.Class == "result resident_icon");
        List<SearchResultXML.ResultDiv> resultsRegions = resultsContainer.Results.FindAll(x => x.Class == "result region_icon");

        items.Clear();
        Debug.Log(resultsPlaces.Count);
        foreach (SearchResultXML.ResultDiv div in resultsPlaces)
        {
            UISearchItem item = items.InstantiateTemplate();
            item.label.text = div.a.Href.Trim();
        }
        Debug.Log(resultsGroups.Count);
        foreach (SearchResultXML.ResultDiv div in resultsGroups)
        {
            UISearchItem item = items.InstantiateTemplate();
            item.label.text = div.a.Href.Trim();
        }
        Debug.Log(resultsResidents.Count);
        foreach (SearchResultXML.ResultDiv div in resultsResidents)
        {
            UISearchItem item = items.InstantiateTemplate();
            item.label.text = div.a.Href.Trim();
        }
        Debug.Log(resultsRegions.Count);
        foreach (SearchResultXML.ResultDiv div in resultsRegions)
        {
            UISearchItem item = items.InstantiateTemplate();
            item.label.text = div.a.Href.Trim();
        }
    }

    [Serializable] public class UISearchItemTemplate : Template<UISearchItem> { };
}