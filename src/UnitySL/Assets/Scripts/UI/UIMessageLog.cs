using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary> High performance pooled system for displaying a scrollable list of Text </summary>
[RequireComponent(typeof(ScrollRect))]
public class UIMessageLog : MonoBehaviour
{
    public TextTemplate labels;
    public ScrollRect scrollRect { get { return _scrollRect != null ? _scrollRect : _scrollRect = GetComponent<ScrollRect>(); } }
    private ScrollRect _scrollRect;
    public RectTransform rectTransform { get { return _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>(); } }
    private RectTransform _rectTransform;

    public List<Message> messages = new List<Message>();

    private TextGenerator textGen = new TextGenerator();
    private TextGenerationSettings generationSettings;

    public struct Message
    {
        public float yPos;
        public string text;

        public Message(float yPos, string text)
        {
            this.yPos = yPos;
            this.text = text;
        }
    }

    private void Awake()
    {
        labels.Initialize();
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        generationSettings = labels.template.GetGenerationSettings(scrollRect.content.rect.size);
    }

    private void OnScroll(Vector2 pos)
    {
        Debug.Log(pos.y);
    }

    public Text AppendMessage(string text)
    {
        float y = 0;
        if (messages.Count > 0)
        {
            Message previous = messages[messages.Count - 1];
            float previousHeight = textGen.GetPreferredHeight(previous.text, generationSettings);
            y = previous.yPos + previousHeight;
        }

        messages.Add(new Message(y, text));

        Text label = labels.InstantiateTemplate();
        label.text = text;
        float height = textGen.GetPreferredHeight(text, generationSettings);
        label.rectTransform.anchoredPosition = new Vector2(0, -y);
        label.rectTransform.sizeDelta = new Vector2(600, height);
        scrollRect.content.sizeDelta = new Vector2(0, y+height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        return label;
    }

    [Serializable] public class TextTemplate : Template<Text> { }
}
