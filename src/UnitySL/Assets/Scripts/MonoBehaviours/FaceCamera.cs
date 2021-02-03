using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private void Update()
    {
        Vector3 fwd = Camera.main.transform.forward; // TODO: Add a CameraManager that can return the current camera
        transform.forward = fwd;
    }
}
