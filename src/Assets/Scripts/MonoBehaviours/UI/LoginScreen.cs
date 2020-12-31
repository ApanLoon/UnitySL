using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoginScreen : MonoBehaviour
{
    public bool IsRandomLogActive = false;
    [SerializeField] protected TMP_Text LogText;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;

    [SerializeField] protected TMP_Text TimerText;

    [SerializeField] protected TMP_InputField NameText;
    [SerializeField] protected TMP_InputField PasswordText;
    [SerializeField] protected RectTransform NamesContent;
    [SerializeField] protected float NameHeight = 20f;
    [SerializeField] protected GameObject NamePrefab;
    [SerializeField] protected Toggle SaveNameToggle;
    [SerializeField] protected Toggle SavePasswordToggle;

    protected string SaveNameSetting = "Login_SaveName";
    protected string SavePasswordSetting = "Login_SavePassword";

    protected float Timer = 0f;

    protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>()
    {
        { Logger.LogLevel.Debug, "<#777777>" },
        { Logger.LogLevel.Info, "<color=\"green\">" },
        { Logger.LogLevel.Warning, "<color=\"yellow\">" },
        { Logger.LogLevel.Error, "<color=\"red\">" }
    };

    private void Start()
    {
        Logger.OnLog += (level, message) =>
        {
            bool autoScroll = Mathf.Abs(LogVerticalScrollbar.value) < 0.01f;
            LogText.text += $"{LogLevelToColour[level]}{level} {message}";
            LogText.ForceMeshUpdate();
            if (autoScroll)
            {
                LogVerticalScrollbar.value = 0f;
            }
        };

        SaveNameToggle.isOn     = PlayerPrefs.HasKey(SaveNameSetting)     ? bool.Parse(PlayerPrefs.GetString(SaveNameSetting)) : true;
        SavePasswordToggle.isOn = PlayerPrefs.HasKey(SavePasswordSetting) ? bool.Parse(PlayerPrefs.GetString(SavePasswordSetting)) : true;

        UpdateNames();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool reverse = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            NavigateToNext(reverse);
        }

        Timer += Time.deltaTime;
        TimerText.text = Timer.ToString("F", CultureInfo.InvariantCulture);

        RandomLog();
    }

    protected float TimeToLog = 0f;
    protected void RandomLog()
    {
        if (IsRandomLogActive == false)
        {
            return;
        }

        if (TimeToLog < Timer)
        {
            bool autoScroll = Mathf.Abs(LogVerticalScrollbar.value) < 0.1f;
            LogText.text += $"{LogLevelToColour[Logger.LogLevel.Debug]}{Logger.LogLevel.Debug} {DateTime.Now:T}Random log AutoScroll={autoScroll}\n";
            if (autoScroll)
            {
                LogVerticalScrollbar.value = 0f;
            }
            LogText.ForceMeshUpdate();
            TimeToLog = Timer + Random.Range(0f, 1f);
        }
    }

    protected void UpdateNames()
    {
        // Clear names list in case there are debug values in the scene:
        foreach (Transform child in NamesContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Credential credential in CredentialStorage.Instance)
        {
            GameObject go = Instantiate(NamePrefab, NamesContent);

            Button itemButton = null;
            Button deleteButton = null;
            foreach (Button b in go.GetComponentsInChildren<Button>())
            {
                switch (b.name)
                {
                    case "ItemButton":
                        itemButton = b;
                        break;

                    case "DeleteButton":
                        deleteButton = b;
                        break;
                }
            }
            if (itemButton == null || deleteButton == null)
            {
                Debug.LogError("LoginScreen.UpdateNames: NamePrefab doesn't have ItemButton or DeleteButton.");
                return;
            }
            itemButton.onClick.AddListener(() => { OnNameClick(credential); });
            deleteButton.onClick.AddListener(() => { OnNameDeleteClick(credential); });


            TMP_Text itemText = itemButton.GetComponentInChildren<TMP_Text>();
            if (itemText == null)
            {
                Debug.LogError("LoginScreen.UpdateNames: NamePrefab does not have a TMP_Text component under the ItemButton.");
                return;
            }
            itemText.text = $"{credential.First} {credential.Last}";
        }
        NamesContent.sizeDelta = new Vector2(0, CredentialStorage.Instance.Count * NameHeight);
    }

    protected void OnNameClick(Credential credential)
    {
        NameText.text = $"{credential.First} {credential.Last}";
        PasswordText.text = credential.Secret;
    }

    protected void OnNameDeleteClick(Credential credential)
    {
        CredentialStorage.Instance.Remove(credential);
        UpdateNames();
    }

    protected void NavigateToNext(bool reverse = false)
    {
        EventSystem eventSystem = EventSystem.current;
        Selectable current = eventSystem.currentSelectedGameObject?.GetComponent<Selectable>();
        if (current == null)
        {
            return;
        }

        Selectable next = null;
        if (reverse)
        {
            next = current.FindSelectableOnLeft();
            if (next == null)
            {
                next = current.FindSelectableOnUp();
            }
        }
        else
        {
            next = current.FindSelectableOnRight();
            if (next == null)
            {
                next = current.FindSelectableOnDown();
            }
        }

        if (next == null)
        {
            return;
        }

        eventSystem.SetSelectedGameObject(next.gameObject, new BaseEventData(eventSystem));
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnNameChange()
    {
    }

    public void OnPasswordChange()
    {
    }

    public async void OnLogin()
    {
        Logger.LogDebug($"Login: {NameText.text} ****");

        // TODO: Make it possible to select grid

        Credential credential = new Credential(NameText.text, PasswordText.text);
        await Login.Instance.Connect(credential);

        // TODO: Saving name and password should only be done if the login is successful

        if (SaveNameToggle.isOn == false)
        {
            return;
        }

        CredentialStorage.Instance.Store(NameText.text, SavePasswordToggle.isOn ? PasswordText.text : "");

        UpdateNames();
    }

    public void OnSaveNameToggle()
    {
        PlayerPrefs.SetString(SaveNameSetting, SaveNameToggle.isOn ? "true" : "false");
        SavePasswordToggle.interactable = SaveNameToggle.isOn;
    }

    public void OnSavePasswordToggle()
    {
        PlayerPrefs.SetString(SavePasswordSetting, SavePasswordToggle.isOn ? "true" : "false");
    }
}
