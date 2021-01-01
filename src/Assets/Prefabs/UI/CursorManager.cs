using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public enum CursorMode
    {
        Normal,
        Alt,
        Help,
        Hand,
        Move,
        EastWest,
        NorthEastSouthWest,
        NorthSouth,
        NorthWestSouthEast,
        Pen,
        Add
    }

    [SerializeField] protected Sprite[] CursorSprites;
    protected Texture2D[] ConvertedTextures;

    public CursorMode CurrentMode { get; protected set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"CursorManager: Multiple instances. Disabling {name}");
            gameObject.SetActive(false);
            return;
        }

        Instance = this;

        ConvertSprites();

        SetCursorMode(CursorMode.Normal);
    }

    protected void ConvertSprites()
    {
        int n = CursorSprites.Length;
        ConvertedTextures = new Texture2D[n];
        for (int i = 0; i < n; i++)
        {
            Sprite sprite = CursorSprites[i];
            Rect rect = sprite.rect;
            Texture2D texture = sprite.texture;
            ConvertedTextures[i] = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            ConvertedTextures[i].SetPixels(texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            ConvertedTextures[i].Apply();
        }
    }

    public void SetCursorMode(CursorMode mode)
    {
        CurrentMode = mode;
        int i = (int)mode;
        if (ConvertedTextures.Length <= i && ConvertedTextures[i] != null)
        {
            Debug.LogError($"CursorManager: No sprite for mode {mode}.");
            return;
        }

        // TODO: The cursor moves when the image is changed!?
        Sprite sprite = CursorSprites[i];
        Vector2 pivot = sprite.pivot;
        pivot.y = sprite.rect.height - pivot.y;
        Texture2D texture = ConvertedTextures[i];
        Cursor.SetCursor(texture, pivot, UnityEngine.CursorMode.ForceSoftware);
    }
}
