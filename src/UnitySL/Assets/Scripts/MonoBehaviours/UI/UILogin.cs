using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UILogin : MonoBehaviour
{
    public CanvasGroup group { get { return _group != null ? _group : _group = GetComponent<CanvasGroup>(); } }
    private CanvasGroup _group;

    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public Toggle saveNameToggle;
    public Toggle savePasswordToggle;
    public UICredentialTemplate savedCredentials;
    public Button loginButton;

    private void Start()
    {
        savedCredentials.Initialize();
        saveNameToggle.isOn = Settings.Instance.login.saveName;
        savePasswordToggle.isOn = Settings.Instance.login.savePassword;
        Open();
    }

    public void Open()
    {
        group.interactable = true;
        gameObject.SetActive(true);
        nameInput.text = "";
        passwordInput.text = "";
        ReloadCredentials();
        ValidateInput();
    }

    public void Close()
    {
        group.interactable = false;
        gameObject.SetActive(false);
    }

    public void ValidateInput()
    {
        loginButton.interactable = !string.IsNullOrEmpty(nameInput.text) && !string.IsNullOrEmpty(passwordInput.text);
    }

    public async void Login()
    {
        group.interactable = false;
        Credential credential = new Credential(nameInput.text, passwordInput.text);
        await Session.Instance.Start(credential);
        if (Session.Instance.IsLoggedIn)
        {
            Close();
            if (saveNameToggle)
            {
                CredentialStorage.Instance.Store(nameInput.text, savePasswordToggle.isOn ? passwordInput.text : "");
            }
        }
        else
        {
            // Error logging in, enable UI again
            group.interactable = true;
        }
    }

    public void SaveCredentialPreferences()
    {
        Settings.Instance.login.saveName = saveNameToggle.isOn;
        if (!saveNameToggle.isOn)
        {
            savePasswordToggle.isOn = false;
        }
        savePasswordToggle.interactable = saveNameToggle.isOn;
        Settings.Instance.login.saveName = saveNameToggle.isOn;
        Settings.Instance.login.savePassword = savePasswordToggle.isOn;
        Settings.Instance.Save();
    }

    private void ReloadCredentials()
    {
        savedCredentials.Clear();
        foreach (Credential c in CredentialStorage.Instance)
        {
            UICredential uic = savedCredentials.InstantiateTemplate();
            uic.label.text = $"{c.First} {c.Last}";
            uic.button.onClick.AddListener(() => { LoadCredential(c); });
            uic.buttonX.onClick.AddListener(() => { DeleteCredential(c); });
        }
    }

    protected void LoadCredential(Credential credential)
    {
        nameInput.text = $"{credential.First} {credential.Last}";
        passwordInput.text = credential.Secret;
    }

    protected void DeleteCredential(Credential credential)
    {
        CredentialStorage.Instance.Remove(credential);
        ReloadCredentials();
    }


    [Serializable] public class UICredentialTemplate : Template<UICredential> { }
}
