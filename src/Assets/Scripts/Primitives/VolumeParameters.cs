using System;
using UnityEngine;

public enum SculptType
{
    None = 0,
    Sphere = 1,
    Torus = 2,
    Plane = 3,
    Cylinder = 4,
    Mesh = 5
}

[Flags]
public enum SculptFlags
{
    Invert = 64,
    Mirror = 128
}

[Serializable]
public class VolumeParameters
{
    public ProfileParameters ProfileParameters = new ProfileParameters();
    public PathParameters PathParameters = new PathParameters();

    public SculptType SculptType = SculptType.None;
    public SculptFlags SculptFlags = 0;
    public float SculptSize = 1f;

    public bool SetType(ProfileType profileType, HoleType holeType, PathType pathType)
    {
        bool valid = true;
        ProfileParameters.ProfileType = profileType;
        ProfileParameters.HoleType = holeType;
        PathParameters.PathType = pathType;
        return valid;
    }

    public bool SetBeginAndEndS (float begin, float end)
    {
        bool valid = true;
        if (end >= 0.0149f && end < ProfileParameters.MIN_CUT_DELTA)
        {
            end = ProfileParameters.MIN_CUT_DELTA; // eliminate warning for common rounding error
        }

        valid &= Volume.LimitRange (ref begin, 0f,                               1f  - ProfileParameters.MIN_CUT_DELTA);
        valid &= Volume.LimitRange (ref end,       ProfileParameters.MIN_CUT_DELTA, 1f);
        valid &= Volume.LimitRange (ref begin, 0f,                               end - ProfileParameters.MIN_CUT_DELTA, 0.01f);
        ProfileParameters.Begin = begin;
        ProfileParameters.End = end;
        return valid;
    }


    public bool SetBeginAndEndT (float begin, float end)
    {
        bool valid = true;
        valid &= Volume.LimitRange (ref begin, 0f,                        1f  - PathParameters.MIN_CUT_DELTA);
        valid &= Volume.LimitRange (ref end,   PathParameters.MIN_CUT_DELTA, 1f);
        valid &= Volume.LimitRange (ref begin, 0f,                        end - PathParameters.MIN_CUT_DELTA, 0.01f);
        PathParameters.Begin = begin;
        PathParameters.End = end;
        return valid;
    }

    public void SetRatio(float x, float y)
    {
        float minX = PathParameters.RATIO_MIN;
        float maxX = PathParameters.RATIO_MAX;
        float minY = PathParameters.RATIO_MIN;
        float maxY = PathParameters.RATIO_MAX;
        // If this is a circular path (and not a sphere) then 'ratio' is actually hole size.
        if (PathParameters.PathType == PathType.Circle
            && ProfileParameters.ProfileType != ProfileType.CircleHalf)
        {
            // Holes are more restricted...
            minX = PathParameters.HOLE_X_MIN;
            maxX = PathParameters.HOLE_X_MAX;
            minY = PathParameters.HOLE_Y_MIN;
            maxY = PathParameters.HOLE_Y_MAX;
        }

        PathParameters.Scale.x = x;
        PathParameters.Scale.y = y;
        Volume.LimitRange(ref PathParameters.Scale.x, minX, maxX);
        Volume.LimitRange(ref PathParameters.Scale.y, minY, maxY);
    }

    public bool SetShear(float x, float y)
    {
        bool valid = true;
        valid &= Volume.LimitRange (ref x, PathParameters.SHEAR_MIN, PathParameters.SHEAR_MAX);
        valid &= Volume.LimitRange (ref y, PathParameters.SHEAR_MIN, PathParameters.SHEAR_MAX);
        PathParameters.Shear = new Vector2 (x, y);
        return valid;
    }
    public void Clamp()
    {
        ProfileParameters.Clamp();
        PathParameters.Clamp();

        #region Hollow
        // Validate the hollow based on path and profile.
        float maxHollow = ProfileParameters.HOLLOW_MAX;
        // Only square holes have trouble.
        if (ProfileParameters.HoleType == HoleType.HoleSquare)
        {
            switch (ProfileParameters.ProfileType)
            {
                case ProfileType.Circle:
                case ProfileType.CircleHalf:
                case ProfileType.EqualTri:
                    maxHollow = ProfileParameters.HOLLOW_MAX_SQUARE;
                    break;
            }
        }
        Volume.LimitRange (ref ProfileParameters.Hollow, ProfileParameters.HOLLOW_MIN, maxHollow);
        #endregion Hollow

        #region Ratio
        SetRatio(PathParameters.Scale.x, PathParameters.Scale.y);
        #endregion Ratio

        #region RadiusOffset
        // If this is a sphere, just set it to 0 and get out.
        if (ProfileParameters.ProfileType == ProfileType.CircleHalf
            || PathParameters.PathType != PathType.Circle)
        {
            PathParameters.RadiusOffset = 0f;
            return;
        }
        // Limit radius offset, based on taper and hole size y.
        float radiusOffset = PathParameters.RadiusOffset;
        float taperY = PathParameters.Taper.y;
        float radiusMag = Mathf.Abs(radiusOffset);
        float holeYMag = Mathf.Abs(PathParameters.Scale.y);
        float taperYMag = Mathf.Abs(taperY);
        // Check to see if the taper effects us.
        if ((radiusOffset > 0f && taperY < 0f)
            || (radiusOffset < 0f && taperY > 0f))
        {
            // The taper does not help increase the radius offset range.
            taperYMag = 0f;
        }
        float maxRadiusMag = 1f - holeYMag * (1f - taperYMag) / (1f - holeYMag);
        // Enforce the maximum magnitude.
        float delta = maxRadiusMag - radiusMag;
        if (delta < 0f)
        {
            // Check radius offset sign.
            if (radiusOffset < 0f)
            {
                radiusOffset = -maxRadiusMag;
            }
            else
            {
                radiusOffset = maxRadiusMag;
            }
        }
        PathParameters.RadiusOffset = radiusOffset;
        #endregion RadiusOffset
    }
}
