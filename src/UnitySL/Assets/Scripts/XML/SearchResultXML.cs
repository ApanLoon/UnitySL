using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;

public class SearchResultXML
{
    [XmlRoot(ElementName = "meta", Namespace = "http://www.w3.org/1999/xhtml")]
    public class Meta
    {
        [XmlAttribute(AttributeName = "http-equiv")]
        public string Httpequiv { get; set; }
        [XmlAttribute(AttributeName = "content")]
        public string Content { get; set; }
    }

    [XmlRoot(ElementName = "link", Namespace = "http://www.w3.org/1999/xhtml")]
    public class Link
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "rel")]
        public string Rel { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
    public class Wrapper
    {
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlElement(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
        public List<ResultsDiv> Divs { get; set; }
    }

    [XmlRoot(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
    public class ResultsDiv
    {
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlElement(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
        public List<ResultDiv> Results { get; set; }
    }

    [XmlRoot(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
    public class ResultDiv
    {
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlElement(ElementName = "a", Namespace = "http://www.w3.org/1999/xhtml")]
        public A a { get; set; }
    }

    [XmlRoot(ElementName = "a", Namespace = "http://www.w3.org/1999/xhtml")]
    public class A
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "body", Namespace = "http://www.w3.org/1999/xhtml")]
    public class Body
    {
        [XmlElement(ElementName = "div", Namespace = "http://www.w3.org/1999/xhtml")]
        public Wrapper Wrapper { get; set; }
    }

    [XmlRoot(ElementName = "html", Namespace = "http://www.w3.org/1999/xhtml")]
    public class Html
    {
        [XmlElement(ElementName = "body", Namespace = "http://www.w3.org/1999/xhtml")]
        public Body Body { get; set; }
    }
}
