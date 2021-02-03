using TMPro;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [SerializeField] protected TMP_Text NameText;
    [SerializeField] protected TMP_Text GroupTitleText;

    public void SetName(string name)
    {
        NameText.text = name;
    }
    public void SetGroupTitle(string title)
    {
        GroupTitleText.text = title;
    }
}
