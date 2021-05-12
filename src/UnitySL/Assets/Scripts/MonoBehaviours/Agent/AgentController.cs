using Assets.Scripts.MonoBehaviours.UI.ToolTips;
using TMPro;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [SerializeField] protected ToolTipTarget ToolTipTarget;
    [SerializeField] protected TMP_Text NameText;
    [SerializeField] protected TMP_Text GroupTitleText;

    public void SetName(string name)
    {
        NameText.text = name;
        SetToolTip();
    }
    public void SetGroupTitle(string title)
    {
        GroupTitleText.text = title;
        SetToolTip();
    }

    protected void SetToolTip()
    {
        ToolTipTarget.Text = $"{GroupTitleText.text}\n{NameText.text}";
    }
}
