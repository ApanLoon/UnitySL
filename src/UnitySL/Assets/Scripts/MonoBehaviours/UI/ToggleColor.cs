using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Extendes a UI.Toggle by letting you set a color for when the toggle is on </summary>
[RequireComponent(typeof(Toggle))]
public class ToggleColor : MonoBehaviour
{
    public Toggle toggle => _toggle != null ? _toggle : _toggle = GetComponent<Toggle>();
    private Toggle _toggle;
    public ColorBlock offColors;
    public ColorBlock onColors;

    private void Awake()
    {
        toggle.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(toggle.isOn);
    }

    private void OnValueChanged(bool on)
    {
        toggle.colors = on ? onColors : offColors;
    }
}
