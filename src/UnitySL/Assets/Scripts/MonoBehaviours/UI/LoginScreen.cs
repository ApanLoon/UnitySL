using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    [SerializeField] protected TMP_Text TimerText;

    [SerializeField] protected TMP_InputField NameText;
    [SerializeField] protected TMP_InputField PasswordText;
    [SerializeField] protected RectTransform NamesContent;
    [SerializeField] protected float NameHeight = 20f;
    [SerializeField] protected GameObject NamePrefab;
    [SerializeField] protected Toggle SaveNameToggle;
    [SerializeField] protected Toggle SavePasswordToggle;
    [SerializeField] protected TMP_InputField LocationText;
    [SerializeField] protected RectTransform LocationsContent;

    protected string HomeLocation = "Home location";
    protected string LastLocation = "Last location";

    protected float Timer = 0f;

    private void Start()
    {
        SaveNameToggle.isOn     = Settings.Instance.login.saveName;
        SavePasswordToggle.isOn = Settings.Instance.login.savePassword;

        UpdateNames();
        UpdateLocations();
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
    }

    #region Names
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
    #endregion Names

    #region Locations
    protected void UpdateLocations()
    {
        // Clear list in case there are debug values in the scene:
        foreach (Transform child in LocationsContent.transform)
        {
            Destroy(child.gameObject);
        }
        AddLocation(LastLocation);
        AddLocation(HomeLocation);
        foreach (string location in Settings.Instance.login.recentLocations)
        {
            AddLocation(location);
        }
        LocationsContent.sizeDelta = new Vector2(0, Settings.Instance.login.recentLocations.Count * NameHeight);
    }

    protected void AddLocation(string location)
    {
        GameObject go = Instantiate(NamePrefab, LocationsContent);

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
            Debug.LogError("LoginScreen.UpdateLocations: NamePrefab doesn't have ItemButton or DeleteButton.");
            return;
        }

        itemButton.onClick.AddListener(() => { OnLocationClick(location); });
        deleteButton.onClick.AddListener(() => { OnLocationDeleteClick(location); });


        TMP_Text itemText = itemButton.GetComponentInChildren<TMP_Text>();
        if (itemText == null)
        {
            Debug.LogError(
                "LoginScreen.UpdateLocations: NamePrefab does not have a TMP_Text component under the ItemButton.");
            return;
        }

        itemText.text = location;
    }

    protected void OnLocationClick(string location)
    {
        LocationText.text = location;
    }

    protected void OnLocationDeleteClick(string location)
    {
        Settings.Instance.login.recentLocations.Remove(location);
        UpdateLocations();
    }
    #endregion Locations

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
        Logger.LogDebug("LoginScreen.OnLogin", $"{NameText.text} ****");

        // TODO: Make it possible to select grid

        string location = LocationText.text.Trim();
        List<string> recentLocations = Settings.Instance.login.recentLocations;
        if (location != "" && location != LastLocation && location != HomeLocation && recentLocations.Contains(location) == false)
        {
            recentLocations.Add(location);
            Settings.Instance.Save();
        }

        Credential credential = new Credential(NameText.text, PasswordText.text);
        if (location == HomeLocation)
        {
            location = Slurl.SIM_LOCATION_HOME;
        }
        else if (location == LastLocation || location == "")
        {
            location = Slurl.SIM_LOCATION_LAST;
        }
        Slurl slurl = Slurl.FromLocationString(location);
        Logger.LogDebug($"LoginScreen.OnLogin", $"slurl={slurl.ToString()}");

        await Session.Instance.Start(credential, slurl);
       
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
        Settings.Instance.login.saveName = SaveNameToggle.isOn;
        SavePasswordToggle.interactable = SaveNameToggle.isOn;
    }

    public void OnSavePasswordToggle()
    {
        Settings.Instance.login.savePassword = SavePasswordToggle.isOn;
    }

    public async void OnLogout()
    {
        // TODO: This does not clean up completely but it should be made to.
        await Session.Instance.Stop(); // Logout
    }
}
