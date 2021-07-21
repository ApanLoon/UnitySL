namespace Assets.Scripts.Appearance
{
    /// <summary>
    /// An ordered set of texture layers that gets composited into a single texture.
    /// Only exists for llvoavatarself.
    /// </summary>
    public class TexLayerSet
    {
        public TexLayerSet(AvatarAppearance appearance)
        {
            Appearance = appearance;

            // TODO: TexLayerSet is not implemented
        }

        public AvatarAppearance Appearance { get; set; }
    }
}
