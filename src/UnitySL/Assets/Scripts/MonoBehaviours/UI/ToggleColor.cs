using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Extendes a UI.Toggle by letting you set a color for when the toggle is on </summary>
[RequireComponent(typeof(Toggle))]
public class ToggleColor : MonoBehaviour
{
    public Toggle toggle { get { return _toggle != null ? _toggle : _toggle = GetComponent<Toggle>(); } }
    private Toggle _toggle;
    public Color onColor;
    private ColorBlock originalColors;

    private void Awake()
    {
        originalColors = toggle.colors;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool on)
    {
        if (on)
        {
            ColorBlock block = new ColorBlock()
            {
                normalColor = onColor,
                highlightedColor = onColor,
                selectedColor = onColor,
                pressedColor = onColor,
                colorMultiplier = 1
            };
            toggle.colors = block;
        }
        else
        {
            toggle.colors = originalColors;
        }
    }
}
