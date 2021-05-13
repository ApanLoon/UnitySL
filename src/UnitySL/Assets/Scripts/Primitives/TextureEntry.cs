using System;

namespace Assets.Scripts.Primitives
{
    public class TextureEntry
    {
        public const int MAX_FACES = 32; // NOTE: LL code uses another constant MAX_TES = 45, why is it not the same?
        public static readonly Guid WHITE_TEXTURE = new Guid ("5748decc-f629-461c-9a36-a35a221fe21f");

        public TextureEntryFace DefaultTexture = new TextureEntryFace();
        public TextureEntryFace[] FaceTextures = new TextureEntryFace[MAX_FACES];

        public TextureEntry()
        {
            for (int i = 0; i < FaceTextures.Length; i++)
            {
                FaceTextures[i] = new TextureEntryFace();
            }
        }

        public override string ToString()
        {
            return $"{{DefaultTexture={DefaultTexture}}}";
        }
    }
}
