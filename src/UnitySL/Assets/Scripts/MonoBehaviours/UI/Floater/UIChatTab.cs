using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIChatTab : MonoBehaviour
{
    public Toggle toggle { get { return _toggle != null ? _toggle : _toggle = GetComponent<Toggle>(); } }
    private Toggle _toggle;
    public Text label;
    [NonSerialized] public readonly List<string> messageLog = new List<string>();
    [NonSerialized] public bool canClose;
}
