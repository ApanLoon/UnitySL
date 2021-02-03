using System;
using UnityEngine;


public enum ProfileType
{
    Circle     = 0,
    Square     = 1,
    IsoTri     = 2,
    EqualTri   = 3,
    RightTri   = 4,
    CircleHalf = 5,
}

public enum HoleType
{
    HoleSame     = 0, // Same as outside profile
    HoleCircle   = 1,
    HoleSquare   = 2,
    HoleTriangle = 3
}

[Serializable]
public class ProfileParameters
{
    public const float MIN_CUT_DELTA = 0.02f;

    public const float HOLLOW_MIN = 0f;
    public const float HOLLOW_MAX = 0.95f;
    public const float HOLLOW_MAX_SQUARE = 0.7f;


    public ProfileType ProfileType = ProfileType.Square;
    public HoleType HoleType = HoleType.HoleSame;

    [Range(0f, 1f)] public float Begin = 0f;
    [Range(0f, 1f)] public float End = 1f;
    [Range(HOLLOW_MIN, HOLLOW_MAX)] public float Hollow = HOLLOW_MIN;

    public void Clamp()
    {
        Begin  = Begin  >= 1f ? 0f : (int) (Begin * 100000) / 100000.0f;
        End    = End    <= 0f ? 1f : (int)(End    * 100000) / 100000.0f;

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

        Hollow = (int)(Hollow * 100000) / 100000.0f;
        if (Hollow < HOLLOW_MIN)
        {
            Hollow = HOLLOW_MIN;
        }
        if (Hollow > HOLLOW_MAX)
        {
            Hollow = HOLLOW_MAX;
        }
    }
}
