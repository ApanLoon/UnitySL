using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public enum MaturityRating { PG = 1, Mature = 2, Adult = 4 }

public class UISearch : MonoBehaviour
{
    public enum Category { All, Events, Groups, People, Places, Wiki }

    public TMP_InputField searchInput;
    public int start = 0;
    public Category category;
    public bool pg;
    public bool mature;
    public bool adult;

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
        Debug.Log("Fail: " + msg);
    }

    public void OnSuccess(string text)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SearchResultXML.Html));
        // convert string to stream
        byte[] byteArray = Encoding.UTF8.GetBytes(text);
        //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
        MemoryStream stream = new MemoryStream(byteArray);
        SearchResultXML.Html results = serializer.Deserialize(stream) as SearchResultXML.Html;
        Debug.Log(results.Body.Wrapper.Divs.Count);

    }
}