using UnityEngine;
using System;
using System.IO;

[Serializable]
public class Settings
{
	public static readonly string FILEPATH = Application.persistentDataPath + "/settings.json";

	public static Settings instance { get { return _instance != null ? _instance : _instance = Load(); } }
	private static Settings _instance;

	public Login   login   = new Login();
	public General general = new General();
	public Chat    chat    = new Chat();

    [Serializable]
    public class Login
    {
        public bool saveName = true;
        public bool savePassword = false;
    }

    [Serializable]
    public class General
    {
        public bool useDisplayNames = true;
        public bool useUserNames = false;
    }

    [Serializable]
    public class Chat
    {
        public bool notifyOnlineStatus = true;
    }
	
	#region LoadSave
	public void Save()
	{
		new FileInfo(FILEPATH).Directory.Create(); // Make sure the dir exists
		string json = JsonUtility.ToJson(this);
		File.WriteAllText(FILEPATH, json);
		Debug.Log("Wrote settings.json to " + FILEPATH);
	}

	public static Settings Load()
	{
		// Create a new settings file if it doesn't exist
		if (!File.Exists(FILEPATH)) new Settings().Save();
		// Load from file
		string json = File.ReadAllText(FILEPATH);
		return JsonUtility.FromJson<Settings>(json);
	}
    #endregion LoadSave
}
