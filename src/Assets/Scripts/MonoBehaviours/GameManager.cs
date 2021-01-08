using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        await Session.Instance.Stop(); // Logout

        // TODO: Perform cleanup
    }
}
