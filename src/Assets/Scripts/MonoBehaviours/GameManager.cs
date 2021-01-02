using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Region.Initialise();
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        // TODO: Popup confirmation
        // TODO: Log out
        // TODO: Perform cleanup
        Application.Quit();
#endif
    }
}
