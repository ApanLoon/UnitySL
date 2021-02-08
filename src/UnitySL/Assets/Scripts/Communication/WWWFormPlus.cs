using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
/// <summary> A replacement for unity's WWWForm. Provides simpler API for making POST and GET requests. </summary>
public class WWWFormPlus
{
    [SerializeField]
    private List<WWWFormPlusField> fields = new List<WWWFormPlusField>();

    public WWWFormPlus()
    {
        fields = new List<WWWFormPlusField>();
    }

    public void AddField(string key, string value)
    {
        if (value == null)
        {
            Debug.LogWarning("Null value added to field " + key);
            return;
        }
        if (Contains(key)) SetValue(key, value);
        else fields.Add(new WWWFormPlusField(key, value));
    }

    public void AddField(string key, int value)
    {
        if (Contains(key)) SetValue(key, value.ToString());
        else fields.Add(new WWWFormPlusField(key, value.ToString()));
    }

    public void AddField(string key, float value)
    {
        if (Contains(key)) SetValue(key, value.ToString());
        else fields.Add(new WWWFormPlusField(key, value.ToString()));
    }

    public void AddBinaryField(string key, byte[] value)
    {
        if (Contains(key)) SetValue(key, value.ToString());
        else fields.Add(new WWWFormPlusField(key, value));
    }

    public void Update(string key, string value)
    {
        AddField(key, value);
    }

    public void Remove(string key)
    {
        WWWFormPlusField item = null;
        foreach (WWWFormPlusField f in fields)
        {
            if (f.key == key)
            {
                item = f;
                break;
            }
        }

        if (item != null)
            fields.Remove(item);
    }

    public bool Contains(string key)
    {
        foreach (WWWFormPlusField f in fields)
        {
            if (f.key == key)
                return true;
        }
        return false;
    }

    public string GetValue(string key, bool silenceError = true)
    {
        foreach (WWWFormPlusField f in fields)
        {
            if (f.key == key) return f.value;
        }

        if (!silenceError)
        {
            Debug.LogError("ERROR: Key \"" + key + "\" not found!");
        }
        return "";
    }

    public byte[] GetBytes(string key, bool silenceError = true)
    {
        foreach (WWWFormPlusField f in fields)
        {
            if (f.key == key) return f.bytes;
        }

        if (!silenceError)
        {
            Debug.LogError("ERROR: Key \"" + key + "\" not found!");
        }
        return null;
    }

    public override string ToString()
    {
        string text = "WWWFormPlus:";
        foreach (WWWFormPlusField f in fields)
        {
            if (f.binary)
            {
                text += string.Format("\n[{0}] \"{1}\"", f.key, f.bytes + " " + f.bytes.Length);
            }
            else
            {
                if (f.value.Length < 1000)
                    text += string.Format("\n[{0}] \"{1}\"", f.key, f.value);
                else
                {
                    text += string.Format("\n[{0}] \"{1}... (Too Long)\"", f.key, f.value.Substring(0, 1000));
                }
            }
        }
        return text;
    }

    private void SetValue(string key, string value)
    {
        foreach (WWWFormPlusField f in fields)
        {
            if (f.key == key)
            {
                f.value = value;
                return;
            }
        }
    }

    public void Request(string url, Action<string> onError, Action<string> onSuccess)
    {
        if (fields.Count == 0) GetRequest(url, onError, onSuccess).RunCoroutine();
        else PostRequest(url, onError, onSuccess).RunCoroutine();
    }

    private IEnumerator PostRequest(string url, Action<string> onError, Action<string> onSuccess)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, this))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                onError?.Invoke($"({webRequest.responseCode}) {webRequest.error}");
            }
            else
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetRequest(string url, Action<string> onError, Action<string> onSuccess)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                onError?.Invoke($"({webRequest.responseCode}) {webRequest.error}");
            }
            else
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    public static implicit operator WWWForm(WWWFormPlus plus)
    {
        if (plus == null) return null;
        WWWForm form = new WWWForm();
        foreach (WWWFormPlusField f in plus.fields)
        {
            if (f.binary) form.AddBinaryData(f.key, f.bytes);
            else form.AddField(f.key, f.value);
        }
        return form;
    }
}

[Serializable]
public class WWWFormPlusField
{
    public string key;
    public string value;
    public byte[] bytes;
    public bool binary;

    public WWWFormPlusField(string key, string value)
    {
        binary = false;
        this.key = key;
        this.value = value;
    }

    public WWWFormPlusField(string key, byte[] value)
    {
        binary = true;
        this.key = key;
        this.bytes = value;
    }
}