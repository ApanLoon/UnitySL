using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Scripts.Primitives
{
    public enum PathType
    {
        Line     = 1,
        Circle   = 2,
        Circle2  = 3,
        Test     = 4
    }

    [Serializable]
    public class PathParameters
    {
        public const float MIN_CUT_DELTA = 0.02f;

        public const float TWIST_MIN = -1f;
        public const float TWIST_MAX =  1f;

        public const float RATIO_MIN = 0f;
        public const float RATIO_MAX = 2f; // Tom Y: Inverted sense here: 0 = top taper, 2 = bottom taper

        public const float HOLE_X_MIN = 0.05f;
        public const float HOLE_X_MAX = 1f;

        public const float HOLE_Y_MIN = 0.05f;
        public const float HOLE_Y_MAX = 0.5f;

        public const float SHEAR_MIN = -0.5f;
        public const float SHEAR_MAX =  0.5f;

        public const float REVOLUTIONS_MIN = 1f;
        public const float REVOLUTIONS_MAX = 4f;

        public const float TAPER_MIN = -1f;
        public const float TAPER_MAX =  1f;

        public const float SKEW_MIN = -0.95f;
        public const float SKEW_MAX =  0.95f;

        public PathType              PathType     = PathType.Line;
        public bool                  IsFlexible   = false;
    
        [Range(0f, 1f)]              public float   Begin        = 0f;
        [Range(0f, 1f)]              public float   End          = 1f;
        public Vector2 Scale        = new Vector2(1f, 1f);
        public Vector2 Shear        = new Vector2(0f, 0f);
        [Range(TWIST_MIN, TWIST_MAX)] public float   TwistBegin   = 0f;
        [Range(TWIST_MIN, TWIST_MAX)] public float   TwistEnd     = 0f;
        public float   RadiusOffset = 0f;
        public Vector2 Taper        = new Vector2(0f, 0f);
        public float   Revolutions  = 1f;
        [Range(SKEW_MIN, SKEW_MAX)]  public float   Skew         = 0f;

        public Vector2 BeginScale => new Vector2 (Scale.x > 1f ? 2f - Scale.x : 1f, Scale.y > 1f ? 2f - Scale.y : 1f);
        public Vector2 EndScale   => new Vector2 (Scale.x < 1f ?      Scale.x : 1f, Scale.y < 1f ?      Scale.y : 1f);

        public void Clamp()
        {
            float begin = Begin;
            float end = End;
            Volume.LimitRange(ref begin, 0f, 1f - MIN_CUT_DELTA);

            if (end >= 0.0149f && end < MIN_CUT_DELTA)
            {
                end = MIN_CUT_DELTA; // eliminate warning for common rounding error
            }
            Volume.LimitRange(ref end, MIN_CUT_DELTA, 1f);
            Volume.LimitRange(ref begin, 0f, end - MIN_CUT_DELTA, 0.01f);
            Begin = begin;
            End = end;

            Volume.LimitRange(ref TwistBegin, TWIST_MIN, TWIST_MAX);
            Volume.LimitRange(ref TwistEnd, TWIST_MIN, TWIST_MAX);

            Volume.LimitRange(ref Shear.x, SHEAR_MIN, SHEAR_MAX);
            Volume.LimitRange(ref Shear.y, SHEAR_MIN, SHEAR_MAX);

            Volume.LimitRange(ref Taper.x, TAPER_MIN, TAPER_MAX);
            Volume.LimitRange(ref Taper.y, TAPER_MIN, TAPER_MAX);

            Volume.LimitRange(ref Revolutions, REVOLUTIONS_MIN, REVOLUTIONS_MAX);

            #region Skew
            // Check the skew value against the revolutions.
            float skew = Skew;
            Volume.LimitRange(ref skew, SKEW_MIN, SKEW_MAX);
            float skew_mag = Mathf.Abs (skew);
            float revolutions = Revolutions;
            float scale_x = Scale.x;
            float min_skew_mag = 1.0f - 1.0f / (revolutions * scale_x + 1.0f);
            // Discontinuity; A revolution of 1 allows skews below 0.5.
            if (Mathf.Abs (revolutions - 1.0f) < 0.001)
                min_skew_mag = 0.0f;

            // Clip skew.
            float delta = skew_mag - min_skew_mag;
            if (delta < 0f)
            {
                // Check skew sign.
                if (skew < 0.0f)
                {
                    skew = -min_skew_mag;
                }
                else
                {
                    skew = min_skew_mag;
                }
            }
            Skew = skew;
            #endregion Skew

        }

        public override string ToString()
        {
            return $"                     PathType={PathType}, Path=({Begin}-{End}), Scale={Scale}, Shear={Shear}, Twist=({TwistBegin}-{TwistEnd}), RadiusOffset={RadiusOffset}, Taper={Taper}), Revolutions={Revolutions}, Skew={Skew}";
        }
    }
}