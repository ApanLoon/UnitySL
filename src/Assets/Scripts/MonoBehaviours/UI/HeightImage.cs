using UnityEngine;
using UnityEngine.UI;

public class HeightImage : MonoBehaviour
{
    [SerializeField] protected RawImage Image;
    private void Start()
    {
        EventManager.Instance.OnHeightsDecoded += OnHeightsDecoded;
    }

    protected void OnHeightsDecoded(float[] heights, uint width)
    {
        Texture2D texture = new Texture2D((int)width, (int)width, TextureFormat.RGBA32, false);
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float height = heights[y * width + x];
                float scaledHeight = height / 30f;
                pixels[y * width + x] = new Color(scaledHeight, scaledHeight, scaledHeight, 1f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        Image.texture = texture;
    }
}
