using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Assets.Scripts.Regions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    private void Start()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlaymodeChanged;
#endif
        Settings s = Settings.Instance; // Force loading of settings

        Region.Initialise();
    }

#if UNITY_EDITOR
    protected async void OnPlaymodeChanged(PlayModeStateChange obj)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode == false && EditorApplication.isPlaying && Session.Instance.IsLogoutPending == false)
        {
            await CleanUp(); //TODO: It appears as if the editor simply won't wait. It shuts down before the cleanup is complete.
        }
    }
#endif

    public async void ExitApplication()
    {
        // TODO: Popup confirmation
        await CleanUp();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    protected async Task CleanUp()
    {
        if (Session.Instance.IsLoggedIn && Session.Instance.IsLogoutPending == false)
        {
            await Session.Instance.Stop(); // Logout
        }

        // Perform cleanup:
        SlMessageSystem.Instance.Stop();

        Settings.Instance.Save(); // TODO: Is this the best place to save the settings?
    }
}
