using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3 (0f, 1f, -3.5f);
    public float Speed = 50f;
    protected float Angle = 0f;

    private void Start()
    {
        if (Target == null)
        {
            Debug.LogError("No target specified!");
            enabled = false;
        }
    }

    private void Update()
    {
        Vector3 pos = Offset;
        Quaternion q = Quaternion.AngleAxis(Angle, new Vector3(0f, 1f, 0f));
        Matrix4x4 m = Matrix4x4.Rotate(q);
        pos = m.MultiplyPoint3x4(pos);
        transform.position = Target.position + pos;

        transform.LookAt(Target.position);

        Angle += Speed * Time.deltaTime;
    }
}
