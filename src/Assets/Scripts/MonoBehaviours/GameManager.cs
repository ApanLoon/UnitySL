using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        // TODO: Popup confirmation
        // TODO: Perform cleanup
        Application.Quit();
#endif
    }
}
