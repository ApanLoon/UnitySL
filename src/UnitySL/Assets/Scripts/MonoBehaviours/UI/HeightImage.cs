using Assets.Scripts;
using Assets.Scripts.Regions;
using UnityEngine;
using UnityEngine.UI;

public class HeightImage : MonoBehaviour
{
    [SerializeField] protected RawImage Image;
    private void OnEnable()
    {
        EventManager.Instance.OnHeightsDecoded += OnHeightsDecoded;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnHeightsDecoded -= OnHeightsDecoded;
    }

    protected void OnHeightsDecoded(Region region, Surface surface)
    {
        if (region != Agent.CurrentPlayer.Region)
        {
            return;
        }

        Texture2D texture = new Texture2D((int)surface.GridsPerEdge, (int)surface.GridsPerEdge, TextureFormat.RGBA32, false);
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < surface.GridsPerEdge; y++)
        {
            for (int x = 0; x < surface.GridsPerEdge; x++)
            {
                float height = surface.SurfaceZ[y * surface.GridsPerEdge + x];
                float scaledHeight = Mathf.InverseLerp(surface.MinZ, surface.MaxZ, height);
                pixels[y * surface.GridsPerEdge + x] = new Color(scaledHeight, scaledHeight, scaledHeight, 1f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        Image.texture = texture;
    }
}
