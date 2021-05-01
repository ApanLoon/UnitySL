using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CredentialStorage : IEnumerable<Credential>
{
    protected static readonly string NameSettingPrefix = "Login_StoredUserName";
    protected static readonly string SecretSettingPrefix = "Login_StoredSecret";

    public static CredentialStorage Instance
    {
        get
        {
            if (_credentialStorage == null)
            {
                _credentialStorage = new CredentialStorage();
                _credentialStorage.Initialise();
            }
            return _credentialStorage;
        }
    }

    private static CredentialStorage _credentialStorage;

    protected List<Credential> Credentials;

    public void Initialise()
    {
        Credentials = new List<Credential>();

        int i = 0;
        while (PlayerPrefs.HasKey($"{NameSettingPrefix}{i}"))
        {
            string name = PlayerPrefs.GetString($"{NameSettingPrefix}{i}");

            string[] parts = name.Split(' ');
            string first = "";
            string last = "resident";
            switch (parts.Length)
            {
                case 1:
                    first = parts[0];
                    break;

                case 2:
                    first = parts[0];
                    last = parts[1];
                    break;
                
                default:
                    Logger.LogError("CredentialStorage.Initialise",$"Store contains invalid name: \"{name}\"");
                    break;
            }

            string secret = PlayerPrefs.HasKey($"{SecretSettingPrefix}{i}") ? PlayerPrefs.GetString($"{SecretSettingPrefix}{i}") : "";

            if (first != "")
            {
                Credentials.Add(new Credential(first, last, secret));
            }

            i++;
        }
    }

    public void Store(string name, string secret)
    {
        string[] parts = name.Split(' ');
        string first = "";
        string last = "resident";
        switch (parts.Length)
        {
            case 1:
                first = parts[0];
                break;

            case 2:
                first = parts[0];
                last = parts[1];
                break;

            default:
                Logger.LogError("CredentialStorage.Store", $"Invalid name: \"{name}\"");
                break;
        }

        Store(first, last, secret);
    }

    public void Store(string first, string last, string secret)
    {
        Credential credential = Credentials.Find(c => c.First.Equals(first) && c.Last.Equals(last));
        if (credential != null)
        {
            credential.Secret = secret;
        }
        else
        {
            Credentials.Add (new Credential(first, last, secret));
        }

        StoreCredentials();
    }

    public void Remove(Credential credential)
    {
        Credentials.Remove(credential);
        StoreCredentials();
    }

    protected void StoreCredentials()
    {
        int i = 0;
        foreach (Credential credential in Credentials)
        {
            PlayerPrefs.SetString($"{NameSettingPrefix}{i}", $"{credential.First} {credential.Last}");
            PlayerPrefs.SetString($"{SecretSettingPrefix}{i}", $"{credential.Secret}");
            i++;
        }

        while (PlayerPrefs.HasKey($"{NameSettingPrefix}{i}"))
        {
            PlayerPrefs.DeleteKey($"{NameSettingPrefix}{i}");
            if (PlayerPrefs.HasKey($"{SecretSettingPrefix}{i}"))
            {
                PlayerPrefs.DeleteKey($"{SecretSettingPrefix}{i}");
            }
            i++;
        }
    }

    public int Count => Credentials.Count;

    public IEnumerator<Credential> GetEnumerator()
    {
        return Credentials.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
