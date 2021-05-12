using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary> High performance pooled system for displaying a scrollable list of Text </summary>
[RequireComponent(typeof(UIScrollRect))]
public class UIMessageLog : MonoBehaviour
{
    public TextTemplate labels;
    public UIScrollRect scrollRect { get; protected set; }
    public RectTransform rectTransform { get; protected set; }

    public List<Message> messages = new List<Message>();

    private TextGenerator textGen = new TextGenerator();
    private TextGenerationSettings generationSettings;

    private bool autoscroll;
    private float width;

    public class Message
    {
        public float yMin;
        public float yMax;
        public string text;
        public TMP_Text label;

        public Message(string text)
        {
            this.text = text;
        }
    }

    private void OnEnable()
    {
        labels.Initialize();

        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponent<UIScrollRect>();

        scrollRect.onValueChanged.AddListener(OnScroll);
        scrollRect.disableDirectDragging = true;

        Canvas.ForceUpdateCanvases();
        Vector2 size = scrollRect.content.rect.size;
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScroll);
        labels.Clear();
    }

    private void OnScroll(Vector2 pos)
    {
        // Attempt to determine whether autoscroll should be active or not
        if (scrollRect.horizontalScrollbar is UIScrollBar hScrollBar && hScrollBar.isDragging) autoscroll = Mathf.Abs(hScrollBar.value) < 0.001f;
        else if (scrollRect.verticalScrollbar is UIScrollBar vScrollBar && vScrollBar.isDragging) autoscroll = Mathf.Abs(vScrollBar.value) < 0.001f;

        // Update contents using pooled labels
        UpdateContent();
    }

    public TMP_Text AppendMessage(string text)
    {
        Vector2 size = scrollRect.content.rect.size;
        float y = 0;
        if (messages.Count > 0)
        {
            Message previous = messages[messages.Count - 1];
            y = previous.yMax;
        }

        Vector2 preferredValues = labels.template.GetPreferredValues(text, labels.template.rectTransform.rect.size.x, 500);
        float height = preferredValues.y;
        width = Mathf.Max(width, preferredValues.x);

        TMP_Text label = labels.InstantiateTemplate();
        Message m = new Message(text)
        {
            yMin = y,
            yMax = y + height,
            label = label
        };
        messages.Add(m);
        label.text = text;
        label.rectTransform.anchoredPosition = new Vector2(labels.template.rectTransform.anchoredPosition.x, -y);
        scrollRect.content.sizeDelta = new Vector2(width, y + height);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        if (autoscroll)
        {
            scrollRect.verticalNormalizedPosition = 0;
            scrollRect.velocity = Vector2.zero;
        }

        return label;
    }

    private void Clear()
    {
        messages.Clear();
        labels.Clear();
    }

    public void Load(IEnumerable<string> messages)
    {
        if (scrollRect == null)
        {
            return;
        }

        Clear();
        foreach (string msg in messages)
        {
            AppendMessage(msg);
        }
        UpdateContent();

        // Move the view to the bottom of the log:
        scrollRect.verticalNormalizedPosition = 0;
        scrollRect.velocity = Vector2.zero;
    }

    private void UpdateContent()
    {
        if (messages.Count == 0)
        {
            messages.Clear();
            return;
        }
        float totalHeight = messages[messages.Count - 1].yMax;
        float viewMin = scrollRect.content.anchoredPosition.y;
        float viewMax = scrollRect.viewport.rect.height + viewMin;
        foreach (Message m in messages)
        {
            if (m.yMax > viewMin && m.yMin < viewMax)
            {
                if (m.label == null)
                {
                    m.label = labels.InstantiateTemplate();
                    m.label.text = m.text;
                    m.label.rectTransform.anchoredPosition = new Vector2(labels.template.rectTransform.anchoredPosition.x, -m.yMin);
                }
            }
            else
            {
                if (m.label != null)
                {
                    labels.ReturnItemToPool(m.label);
                    m.label = null;
                }
            }
        }
    }

    [Serializable] public class TextTemplate : Template<TMP_Text> { }
}
