
using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum FaceId
{
    PathBegin    = 0x01,
    PathEnd      = 0x02,
    InnerSide    = 0x04,
    ProfileBegin = 0x08,
    ProfileEnd   = 0x10,
    Outer0       = 0x20,
    Outer1       = 0x40,
    Outer2       = 0x80,
    Outer3       = 0x100
}

public class Profile
{
    public bool IsOpen { get; protected set; }
    public bool IsConcave { get; protected set; }

    public int PointCount => Points.Count;
    public int PointCountOutside { get; protected set; }

    public List<Vector3> Points = new List<Vector3>();

    public class Face
    {
        public int Index;
        public int Count;
        public float ScaleU;
        public bool Cap;
        public bool Flat;
        public FaceId FaceId;
    }
    public List<Face> Faces = new List<Face>();

    /// <summary>
    /// Regenerate the profile given the parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="pathIsOpen"></param>
    /// <param name="lod"></param>
    /// <param name="split"></param>
    /// <param name="isSculpted"></param>
    /// <param name="sculptSize"></param>
    /// <returns>true if the profile has actually changed</returns>
    public bool Generate(ProfileParameters parameters, bool pathIsOpen, float lod = 1f, int split = 0, bool isSculpted = false, int sculptSize = 1)
    {
        if (lod < Volume.MIN_LOD)
        {
            Debug.LogWarning("Generating profile with LOD < MIN_LOD.  CLAMPING");
            lod = Volume.MIN_LOD;
        }

        Points.Clear();
        Faces.Clear();

        switch (parameters.ProfileType)
        {
            case ProfileType.Square:
                GenerateSquare(parameters, pathIsOpen, lod, split, isSculpted, sculptSize);
                break;

            case ProfileType.IsoTri:
            case ProfileType.RightTri:
            case ProfileType.EqualTri:
                GenerateTriangle(parameters, pathIsOpen, lod, split, isSculpted, sculptSize);
                break;

            case ProfileType.Circle:
                GenerateCircle (parameters, pathIsOpen, lod, split, isSculpted, sculptSize);
                break;

            case ProfileType.CircleHalf:
                GenerateCircleHalf (parameters, pathIsOpen, lod, split, isSculpted, sculptSize);
                break;

            default:
                Debug.LogError($"Unknown profile: {parameters.ProfileType}");
                break;
        }


        if (pathIsOpen)
        {
            AddCap (FaceId.PathEnd); // bottom
        }

        if (IsOpen) // interior edge caps
        {
            AddFace (PointCount - 1, 2, 0.5f, FaceId.ProfileBegin, true);

            if (parameters.Hollow > 0f)
            {
                AddFace (PointCountOutside - 1, 2, 0.5f, FaceId.ProfileEnd, true);
            }
            else
            {
                AddFace (PointCount - 2, 2, 0.5f, FaceId.ProfileEnd, true);
            }
        }

        return true;
    }

    protected void GenerateSquare (ProfileParameters parameters, bool pathIsOpen, float lod, int split, bool isSculpted, int sculptSize)
    {
        int nFaces = 0;

        GenNGon (parameters, 4, -0.375f, 0, 1, split);

        if (pathIsOpen)
        {
            AddCap (FaceId.PathBegin);
        }

        for (int i = Mathf.FloorToInt(parameters.Begin * 4f); i < Mathf.FloorToInt (parameters.End * 4f + .999f); i++)
        {
            AddFace ((nFaces++) * (split + 1), split + 2, 1, (FaceId)((int)FaceId.Outer0 << i), true);
        }

        for (int i = 0; i < Points.Count; i++)
        {
            // Scale by 4 to generate proper tex coords.
            Points[i] = new Vector3(Points[i].x, Points[i].y, Points[i].z * 4f);
        }

        if (parameters.Hollow > 0f)
        {
            switch (parameters.HoleType)
            {
                case HoleType.HoleTriangle:
                    // This offset is not correct, but we can't change it now... DK 11/17/04
                    AddHole (parameters, true, 3, -0.375f, parameters.Hollow, 1f, split);
                    break;

                case HoleType.HoleCircle:
                    // TODO: Compute actual detail levels for cubes
                    AddHole (parameters, false, Volume.MIN_DETAIL_FACES * lod, -0.375f, parameters.Hollow, 1f);
                break;

                case HoleType.HoleSame:
                case HoleType.HoleSquare:
                default:
                    AddHole (parameters, true, 4, -0.375f, parameters.Hollow, 1f, split);
                break;
            }
        }

        if (pathIsOpen)
        {
            Faces[0].Count = Points.Count;
        }
    }

    protected void GenerateTriangle (ProfileParameters parameters, bool pathIsOpen, float lod, int split, bool isSculpted, int sculptSize)
    {
        int nFaces = 0;

        GenNGon (parameters, 3, 0, 0, 1, split);
        
        for (int i = 0; i < PointCount; i++)
        {
            // Scale by 3 to generate proper tex coords.
            Points[i] = new Vector3 (Points[i].x, Points[i].y, Points[i].z * 3f);
        }

        if (pathIsOpen)
        {
            AddCap (FaceId.PathBegin);
        }

        for (int i = Mathf.FloorToInt (parameters.Begin * 3f); i < Mathf.FloorToInt (parameters.End * 3f + .999f); i++)
        {
            AddFace ((nFaces++) * (split + 1), split + 2, 1, (FaceId)((int)FaceId.Outer0 << i), true);
        }
        if (parameters.Hollow > 0f)
        {
            // Swept triangles need smaller hollowness values,
            // because the triangle doesn't fill the bounding box.
            float triangleHollow = parameters.Hollow / 2f;

            switch (parameters.HoleType)
            {
                case HoleType.HoleCircle:
                    // TODO: Actually generate level of detail for triangles
                    AddHole (parameters, false, Volume.MIN_DETAIL_FACES * lod, 0, triangleHollow, 1f);
                    break;

                case HoleType.HoleSquare:
                    AddHole (parameters, true, 4, 0, triangleHollow, 1f, split);
                    break;

                case HoleType.HoleSame:
                case HoleType.HoleTriangle:
                default:
                    AddHole (parameters, true, 3, 0, triangleHollow, 1f, split);
                break;
            }
        }
    }

    protected void GenerateCircle (ProfileParameters parameters, bool pathIsOpen, float lod, int split, bool isSculpted, int sculptSize)
    {
        // If this has a square hollow, we should adjust the
        // number of faces a bit so that the geometry lines up.
        HoleType holeType = 0;
        float circleDetail = Volume.MIN_DETAIL_FACES * lod;
        if (parameters.Hollow > 0f)
        {
            holeType = parameters.HoleType;
            if (holeType == HoleType.HoleSquare)
            {
                // Snap to the next multiple of four sides,
                // so that corners line up.
                circleDetail = Mathf.Ceil (circleDetail / 4.0f) * 4.0f;
            }
        }

        int sides = (int)circleDetail;

        if (isSculpted)
        {
            sides = sculptSize;
        }

        GenNGon (parameters, sides);

        if (pathIsOpen)
        {
            AddCap (FaceId.PathBegin);
        }

        if (IsOpen && parameters.Hollow <= 0f)
        {
            AddFace (0, PointCount - 1, 0, FaceId.Outer0, false);
        }
        else
        {
            AddFace (0, PointCount, 0, FaceId.Outer0, false);
        }

        if (parameters.Hollow > 0f)
        {
            switch (holeType)
            {
                case HoleType.HoleSquare:
                    AddHole (parameters, true, 4, 0, parameters.Hollow, 1f, split);
                    break;

                case HoleType.HoleTriangle:
                    AddHole (parameters, true, 3, 0, parameters.Hollow, 1f, split);
                    break;

                case HoleType.HoleCircle:
                case HoleType.HoleSame:
                default:
                    AddHole (parameters, false, circleDetail, 0, parameters.Hollow, 1f);
                    break;
            }
        }
    }

    protected void GenerateCircleHalf (ProfileParameters parameters, bool pathIsOpen, float lod, int split, bool isSculpted, int sculptSize)
    {
        // If this has a square hollow, we should adjust the
        // number of faces a bit so that the geometry lines up.
        HoleType hole_type = 0;
        // Number of faces is cut in half because it's only a half-circle.
        float circle_detail = Volume.MIN_DETAIL_FACES * lod * 0.5f;
        if (parameters.Hollow > 0f)
        {
            hole_type = parameters.HoleType;
            if (hole_type == HoleType.HoleSquare)
            {
                // Snap to the next multiple of four sides (div 2),
                // so that corners line up.
                circle_detail = Mathf.Ceil (circle_detail / 2.0f) * 2.0f;
            }
        }

        GenNGon (parameters, Mathf.FloorToInt (circle_detail), 0.5f, 0f, 0.5f);
        
        if (pathIsOpen)
        {
            AddCap (FaceId.PathBegin);
        }

        if (IsOpen && parameters.Hollow <= 0f)
        {
            AddFace (0, PointCount - 1, 0, FaceId.Outer0, false);
        }
        else
        {
            AddFace (0, PointCount, 0, FaceId.Outer0, false);
        }

        if (parameters.Hollow > 0f)
        {
            switch (hole_type)
            {
                case HoleType.HoleSquare:
                    AddHole (parameters, true, 2, 0.5f, parameters.Hollow, 0.5f, split);
                    break;

                case HoleType.HoleTriangle:
                    AddHole (parameters, true, 3, 0.5f, parameters.Hollow, 0.5f, split);
                    break;

                case HoleType.HoleCircle:
                case HoleType.HoleSame:
                default:
                    AddHole (parameters, false, circle_detail, 0.5f, parameters.Hollow, 0.5f);
                    break;
            }
        }

        // Special case for openness of sphere
        if ((parameters.End - parameters.Begin) < 1f)
        {
            IsOpen = true;
        }
        else if (parameters.Hollow <= 0f)
        {
            IsOpen = false;
            Points.Add(Points[0]);
        }
    }

    protected void AddCap(FaceId faceId)
    {
        Faces.Add (new Face
        {
            Index = 0,
            Count = Points.Count,
            ScaleU = 1f,
            Cap = true,
            FaceId = faceId
        });
    }

    protected void AddFace(int i, int count, float scaleU, FaceId faceId, bool isFlat)
    {
        Faces.Add(new Face
        {
            Index = i,
            Count = count,
            ScaleU = scaleU,
            Flat = isFlat,
            Cap = false,
            FaceId = faceId
        });
    }

    protected static readonly float[] ScaleTable = { 1f, 1f, 1f, 0.5f, 0.707107f, 0.53f, 0.525f, 0.5f };

    /// <summary>
    /// Generate an n-sided "circular" path.
    /// 0 is (1,0), and we go counter-clockwise along a circular path from there.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="nSides"></param>
    /// <param name="offset"></param>
    /// <param name="bevel"></param>
    /// <param name="angScale"></param>
    /// <param name="split"></param>
    protected void GenNGon (ProfileParameters parameters, int nSides, float offset = 0f, float bevel = 0f, float angScale = 1f, int split = 0)
    {
        float scale = 0.5f;
        float t, tStep, tFirst, tFraction, ang, angStep;
        Vector3 pt1, pt2;

        float begin = parameters.Begin;
        float end = parameters.End;

        tStep = 1.0f / nSides;
	    angStep = 2.0f * Mathf.PI * tStep * angScale;

	    // Scale to have size "match" scale.  Compensates to get object to generally fill bounding box.

	    int nTotalSides = Mathf.RoundToInt (nSides / angScale);	// Total number of sides all around

	    if (nTotalSides < 8)
	    {
		    scale = ScaleTable[nTotalSides];
	    }

        tFirst = Mathf.Floor (begin * nSides) / nSides;

        // pt1 is the first point on the fractional face.
        // Starting t and ang values for the first face
        t = tFirst;
	    ang = 2.0f * Mathf.PI * (t * angScale + offset) + Mathf.PI; // Note: Indra does NOT add PI here, but this makes spheres correct
	    pt1 = new Vector3(Mathf.Cos (ang) * scale, Mathf.Sin (ang) * scale, t);

	    // Increment to the next point.
	    // pt2 is the end point on the fractional face
	    t += tStep;
	    ang += angStep;
	    pt2 = new Vector3 (Mathf.Cos (ang) * scale, Mathf.Sin (ang) * scale, t);

	    tFraction = (begin - tFirst) * nSides;

	    // Only use if it's not almost exactly on an edge.
	    if (tFraction < 0.9999f)
        {
            Points.Add (Vector3.Lerp(pt1, pt2, tFraction));
	    }

	    // There's lots of potential here for floating point error to generate unneeded extra points - DJS 04/05/02
	    while (t < end)
	    {
		    // Iterate through all the integer steps of t.
		    pt1 = new Vector3 (Mathf.Cos (ang) * scale, Mathf.Sin (ang) * scale, t);

		    if (Points.Count > 0)
            {
			    Vector3 p = Points[Points.Count - 1];
			    for (int i = 0; i < split && Points.Count > 0; i++)
                {
                    float factor = 1.0f / (float) (split + 1) * (float) (i + 1);
                    Points.Add (new Vector3 ((pt1.x - p.x) * factor + p.x,
                                                 (pt1.y - p.y) * factor + p.y,
                                                 (pt1.z - p.z) * factor + p.z));
			    }
		    }
		    Points.Add(pt1);

		    t += tStep;
		    ang += angStep;
	    }

	    tFraction = (end - (t - tStep)) * nSides;

        // pt1 is the first point on the fractional face
        // pt2 is the end point on the fractional face
        pt2 = new Vector3 (Mathf.Cos (ang) * scale, Mathf.Sin (ang) * scale, t);

	    // Find the fraction that we need to add to the end point.
	    tFraction = (end - (t - tStep)) * nSides;
	    if (tFraction > 0.0001f)
	    {
		    Vector3 newPoint = Vector3.Lerp (pt1, pt2, tFraction);
		    
		    if (Points.Count > 0)
            {
			    Vector3 p = Points[Points.Count - 1];
			    for (int i = 0; i < split && Points.Count > 0; i++)
                {
                    float factor = 1.0f / (float)(split + 1) * (float)(i + 1);
                    Points.Add (new Vector3 ((newPoint.x - p.x) * factor + p.x,
                                                 (newPoint.y - p.y) * factor + p.y,
                                                 (newPoint.z - p.z) * factor + p.z));
			    }
		    }
		    Points.Add (newPoint);
	    }

	    // If we're sliced, the profile is open.
	    if ((end - begin) * angScale < 0.99f)
	    {
		    IsConcave = (end - begin) * angScale > 0.5f;
            IsOpen = true;

		    if (parameters.Hollow <= 0f)
            {
                // put center point if not hollow.
                Points.Add (new Vector3 (0, 0, 0));
            }
        }
	    else
	    {
		    // The profile isn't open.
		    IsOpen = false;
		    IsConcave = false;
	    }
    }

    /// <summary>
    /// Hollow is percent of the original bounding box, not of this particular
    /// profile's geometry.  Thus, a swept triangle needs lower hollow values than
    /// a swept square.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="flat"></param>
    /// <param name="sides"></param>
    /// <param name="offset"></param>
    /// <param name="boxHollow"></param>
    /// <param name="angScale"></param>
    /// <param name="split"></param>
    protected void AddHole (ProfileParameters parameters, bool flat, float sides, float offset, float boxHollow, float angScale, int split = 0)
    {
        // Note that addHole will NOT work for non-"circular" profiles, if we ever decide to use them.

        // Total add has number of vertices on outside.
        PointCountOutside = PointCount;

        // Why is the "bevel" parameter -1? DJS 04/05/02
        GenNGon (parameters, Mathf.FloorToInt(sides), offset, -1, angScale, split);

        AddFace (PointCountOutside, PointCount - PointCountOutside, 0, FaceId.InnerSide, flat);

        // Scale the hole:
        List<Vector3> tmp = new List<Vector3>();
        for (int i = PointCountOutside; i < PointCount; i++)
        {
            tmp.Add (new Vector3 (Points[i].x * boxHollow,
                                      Points[i].y * boxHollow,
                                      Points[i].z * boxHollow));
        }

        // Reverse the order of the inner points:
        int j = PointCount - PointCountOutside - 1;
        for (int i = PointCountOutside; i < PointCount; i++)
        {
            Points[i] = tmp[j--];
        }

        foreach (var face in Faces)
        {
            if (face.Cap)
            {
                face.Count *= 2;
            }
        }
    }
}
