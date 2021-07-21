
using Assets.Scripts.Characters;

namespace Assets.Scripts.Appearance
{
    public class MaskedMorph
    {
        public MaskedMorph(VisualParameter morphTarget, bool invert, string layer)
        {
            MorphTarget = morphTarget;
            Invert = invert;
            Layer = layer;
        }

        public VisualParameter MorphTarget { get; set; }
        public bool Invert { get; set; }
        public string Layer { get; set; }
    }
}
