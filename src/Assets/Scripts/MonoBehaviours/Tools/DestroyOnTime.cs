using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    public float LifeTime;

    protected float StartTime;

    protected void Start()
    {
        StartTime = Time.time;
    }

    protected void Update()
    {
        if (LifeTime > 0f && Time.time >= StartTime + LifeTime)
        {
            Destroy(gameObject);
        }
    }
}
