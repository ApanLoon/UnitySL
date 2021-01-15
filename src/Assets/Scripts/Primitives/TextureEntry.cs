using System;

public class TextureEntry
{
    public const int MAX_FACES = 32;
    public static readonly Guid WHITE_TEXTURE = new Guid ("5748decc-f629-461c-9a36-a35a221fe21f");

    public TextureEntryFace DefaultTexture;
    public TextureEntryFace[] FaceTextures = new TextureEntryFace[MAX_FACES];
}
