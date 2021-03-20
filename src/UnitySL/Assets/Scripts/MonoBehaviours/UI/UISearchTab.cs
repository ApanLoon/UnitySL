using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UISearchTab : MonoBehaviour
{
    public UISearch uiSearch { get { return _uiSearch != null ? _uiSearch : _uiSearch = GetComponentInParent<UISearch>(); } }
    private UISearch _uiSearch;
    public UISearch.Category category;

    private void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChange);
    }

    private void OnValueChange(bool on) {
        if (on) uiSearch.SetCategory(category);
    }
}
