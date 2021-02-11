using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;


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

        XmlDocument document = new XmlDocument();
        document.Load(new StringReader(text));

        items.Clear();
        foreach (XmlNode node in document["html"]["body"]["div"].ChildNodes.Cast<XmlNode>().First(x => x.Attributes["class"].InnerText == "results_container"))
        {

            if (node.Attributes["class"].InnerText == "result place_icon")
            {
                UISearchItem item = items.InstantiateTemplate();
                item.label.text = node["h3"].InnerText.Trim();

                Uri uri = new Uri(node["h3"]["a"].Attributes["href"].InnerText);
                string guid = uri.Segments.Last();
                Debug.Log(uri + " = " + guid);

                Place place = new Place(guid, node["h3"].InnerText.Trim(), node["p"].InnerText.Trim());
                item.button.onClick.AddListener(() => PreviewPlace(place));
            }
        }
    }

    public void PreviewPlace(Place place)
    {
        place.FetchDetails(Debug.Log, x => Debug.Log(x.snapshot));
    }

    [Serializable] public class UISearchItemTemplate : Template<UISearchItem> { };
}