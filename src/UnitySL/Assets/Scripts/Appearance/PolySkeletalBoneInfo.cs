using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class PolySkeletalBoneInfo
    {
        public string BoneName { get; set; }
        public Vector3 ScaleDeformation { get; set; }
        public Vector3 PositionDeformation { get; set; }
        public bool HasPositionDeformation { get; set; }

        public PolySkeletalBoneInfo(string name, Vector3 scale, Vector3 pos, bool hasPos)
        {
            BoneName = name;
            ScaleDeformation = scale;
            PositionDeformation = pos;
            HasPositionDeformation = hasPos;
        }
    }
}