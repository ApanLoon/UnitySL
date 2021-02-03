using UnityEngine;

public class TaskController : MonoBehaviour
{
    private void OnEnable()
    {
        SlMessageSystem.Instance.Start();
    }

    private void OnDisable()
    {
        SlMessageSystem.Instance.Stop();
    }
}
