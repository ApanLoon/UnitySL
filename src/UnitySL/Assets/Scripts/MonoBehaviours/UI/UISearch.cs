using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using HttpAPI;

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
    public TMP_Text title;
    public TMP_Text description;
    public RawImage map;
    public ButtonTemplate resultButtons;
    public readonly List<Place> resultPlaces = new List<Place>();

    private void Start()
    {
        items.Initialize();
        resultButtons.Initialize();
    }

    public void Search()
    {
        items.Clear();

        MaturityRating rating = 0;
        if (pg) rating |= MaturityRating.PG;
        if (mature) rating |= MaturityRating.Mature;
        if (adult) rating |= MaturityRating.Adult;
        WWWFormPlus form = new WWWFormPlus();
        string url = $"http://search.secondlife.com/client_search.php?q={searchInput.text.ToLower()}&start={start}&mat={(int)rating}&output=xml_no_dtd&client=raw_xml_frontend&s={category.ToString()}";
        Debug.Log(url);
        form.Request(url, OnSearchFail, OnSearchSuccess);
    }

    public void OnSearchFail(string msg)
    {
        Debug.LogWarning("Fail: " + msg);
    }

    public void OnSearchSuccess(string text)
    {
        // Result comes with some symbols that cannot be parsed.
        // Manually pull these out for now till we figure what else to do.
        text = Regex.Replace(text, "&nbsp;", "");

        XmlDocument document = new XmlDocument();
        document.Load(new StringReader(text));

        items.Clear();
        resultPlaces.Clear();
        foreach (XmlNode node in document["html"]["body"]["div"].ChildNodes.Cast<XmlNode>().First(x => x.Attributes["class"].InnerText == "results_container"))
        {
            UISearchItem item = items.InstantiateTemplate();
            item.label.text = node["h3"].InnerText.Trim();

            if (node.Attributes["class"].InnerText == "result place_icon")
            {
                item.label.text = "[P] " + item.label.text;

                Uri uri = new Uri(node["h3"]["a"].Attributes["href"].InnerText);
                string guid = uri.Segments.Last();

                Place place = new Place(guid, node["h3"].InnerText.Trim(), node["p"].InnerText.Trim());
                resultPlaces.Add(place);
                item.button.onClick.AddListener(() => PreviewPlace(place));
            }
            else
            {
                Debug.LogWarning("Search result of type '" + node.Attributes["class"].InnerText + "' not supported yet.");
            }
        }
    }

    public void PreviewPlace(Place place)
    {
        place.FetchDetails(Debug.Log, PreviewPlaceDetailed);
    }

    private void PreviewPlaceDetailed(Place place)
    {
        // Title
        title.text = place.title;

        // Description
        description.text = place.description;

        // Image
        HttpAPI.Region region = new HttpAPI.Region(place.region);
        region.GetMap(Debug.LogWarning, x => map.texture = x);

        // Buttons
        resultButtons.Clear();
        Button linkButton = resultButtons.InstantiateTemplate();
        linkButton.onClick.AddListener(() => Application.OpenURL($"https://world.secondlife.com/place/{place.guid}"));
        linkButton.GetComponentInChildren<TMP_Text>().text = "Link to page";
        Button findButton = resultButtons.InstantiateTemplate();
        findButton.onClick.AddListener(() => Application.OpenURL($"https://maps.secondlife.com/secondlife/{place.region}/{place.location.x}/{place.location.y}/{place.location.z}/"));
        findButton.GetComponentInChildren<TMP_Text>().text = "Find on map";
        Button tpButton = resultButtons.InstantiateTemplate();
        findButton.onClick.AddListener(() => Application.OpenURL($"secondlife:///app/teleport/{place.region}/{place.location.x}/{place.location.y}/{place.location.z}/"));
        findButton.GetComponentInChildren<TMP_Text>().text = "Teleport";
    }

    public void SetCategory(Category category) {
        this.category = category;
        Search();
    }

    [Serializable] public class UISearchItemTemplate : Template<UISearchItem> { };
    [Serializable] public class ButtonTemplate : Template<Button> { };
}