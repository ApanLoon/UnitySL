using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPopup : MonoBehaviour
{
    [SerializeField] protected GameObject Container;
    [SerializeField] protected TMP_Text TitleText;
    [SerializeField] protected TMP_Text MessageText;
    [SerializeField] protected Image ProgressFillerImage;

    private void OnEnable()
    {
        EventManager.Instance.OnProgressUpdate += OnProgressUpdate;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnProgressUpdate -= OnProgressUpdate;
    }

    protected void OnProgressUpdate(string title, string message, float progress, bool close, float maxProgress)
    {
        TitleText.text = title;
        MessageText.text = message;

        float percentage = progress / maxProgress;
        ProgressFillerImage.fillAmount = percentage;

        Container.SetActive(!close);
    }
}
