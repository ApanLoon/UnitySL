
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public bool IsOpen { get; protected set; }
    public int PointCount => Points.Count;

    protected float Step;

    public class PathPoint
    {
        public Vector3 Position;
        public Matrix4x4 Rotation;
        public Vector2 Scale;
        public float ExtrusionT;
    }
    public List<PathPoint> Points = new List<PathPoint>();

    /// <summary>
    /// Regenerates the path given the parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="lod"></param>
    /// <param name="split"></param>
    /// <param name="isSculpted"></param>
    /// <param name="sculptSize"></param>
    /// <returns>true if the path has actually changed.</returns>
    public bool Generate(PathParameters parameters, float lod = 1f, int split = 0, bool isSculpted = false, int sculptSize = 1)
    {
        // TODO: Possibly do some testing that would enable us to quit early here if no generation is needed.

        Points.Clear();
        IsOpen = true;

        switch (parameters.PathType)
        {
            case PathType.Line:
                GenerateLine(parameters, lod, split, isSculpted, sculptSize);
                break;

            case PathType.Circle:
                GenerateCircle(parameters, lod, split, isSculpted, sculptSize);
                break;

            case PathType.Circle2:
                GenerateCircle2(parameters, lod, split, isSculpted, sculptSize);
                break;

            case PathType.Test:
                GenerateTest(parameters, lod, split, isSculpted, sculptSize);
                break;
        }
        return true;
    }

    protected void GenerateLine (PathParameters parameters, float lod, int split, bool isSculpted, int sculptSize)
    {
        // Take the begin/end twist into account for detail.
        int nPoints = Mathf.FloorToInt(Mathf.Abs(parameters.TwistBegin - parameters.TwistEnd) * 3.5f * (lod - 0.5f)) + 2;
        if (nPoints < split + 2)
        {
            nPoints = split + 2;
        }

        Step = 1.0f / (nPoints - 1);

        Vector2 startScale = parameters.BeginScale;
        Vector2 endScale = parameters.EndScale;

        for (int i = 0; i < nPoints; i++)
        {
            float t = Mathf.Lerp(parameters.Begin, parameters.End, i * Step);
            Points.Add (new PathPoint
            {
                Position = new Vector3 (Mathf.Lerp (0, parameters.Shear.x, t),
                                        Mathf.Lerp(0, parameters.Shear.y, t),
                                        t - 0.5f),
                Rotation = Matrix4x4.Rotate (Quaternion.AngleAxis (Mathf.Lerp (Mathf.PI * parameters.TwistBegin, Mathf.PI * parameters.TwistEnd, t) * Mathf.Rad2Deg,
                                                                    new Vector3(0f, 0f, 1f))),
                Scale = new Vector2 (Mathf.Lerp (startScale.x, endScale.x, t),
                                     Mathf.Lerp (startScale.y, endScale.y, t)),
                ExtrusionT = t
            });
        }
    }

    protected void GenerateCircle (PathParameters parameters, float lod, int split, bool isSculpted, int sculptSize)
    {
        float twistMag = Mathf.Abs(parameters.TwistBegin - parameters.TwistEnd);
        int nSides = Mathf.FloorToInt (Mathf.Floor (Volume.MIN_DETAIL_FACES * lod + twistMag * 3.5f * (lod - 0.5f)) * parameters.Revolutions);
        if (isSculpted)
        {
            nSides = Mathf.Max(sculptSize, 1);
        }

        if (nSides > 0)
        {
            GenNGon (parameters, nSides);
        }
    }

    protected void GenerateCircle2 (PathParameters parameters, float lod, int split, bool isSculpted, int sculptSize)
    {
        if (parameters.End - parameters.Begin >= 0.99f && parameters.Scale.x >= 0.99f)
        {
            IsOpen = false;
        }

        GenNGon (parameters, Mathf.FloorToInt (Volume.MIN_DETAIL_FACES * lod));

        float x = 0.5f;
        foreach (PathPoint point in Points)
        {
            point.Position.x = x;
            x *= -1f;
        }
    }

    protected void GenerateTest (PathParameters parameters, float lod, int split, bool isSculpted, int sculptSize)
    {
        int nPoints = 5;
        Step = 1.0f / (nPoints - 1);

        for (int i = 0; i < nPoints; i++)
        {
            float t = i * Step;
            Points.Add (new PathPoint
            {
                Position = new Vector3 (0f,
                                        Mathf.Lerp (0,    -Mathf.Sin (Mathf.PI * parameters.TwistEnd * t) * 0.5f, t),
                                        Mathf.Lerp (-0.5f, Mathf.Cos (Mathf.PI * parameters.TwistEnd * t) * 0.5f, t)),
                Scale = new Vector2 (Mathf.Lerp (1f, parameters.Scale.x, t),
                                     Mathf.Lerp (1f, parameters.Scale.y, t)),
                ExtrusionT = t,
                Rotation = Matrix4x4.Rotate (Quaternion.AngleAxis (Mathf.PI * parameters.TwistEnd * t * Mathf.Rad2Deg, new Vector3 (1f, 0f, 0f)))
            });
        }
    }

    protected static readonly float[] ScaleTable = { 1f, 1f, 1f, 0.5f, 0.707107f, 0.53f, 0.525f, 0.5f };

    /// <summary>
    /// Generates a circular path, starting at (1, 0, 0), counterclockwise along the xz plane.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="nSides"></param>
    /// <param name="startOff"></param>
    /// <param name="endScale"></param>
    /// <param name="twistScale"></param>
    protected void GenNGon (PathParameters parameters, int nSides, float endScale = 1f, float twistScale = 1f)
    {
        float skew = parameters.Skew;
        float skewMag = Mathf.Abs (skew);
        float holeX = parameters.Scale.x * (1.0f - skewMag);
        float holeY = parameters.Scale.y;

        // Calculate taper begin/end for x,y (Negative means taper the beginning)
        float taperXBegin = 1.0f;
        float taperXEnd = 1.0f - parameters.Taper.x;
        float taperYBegin = 1.0f;
        float taperYEnd = 1.0f - parameters.Taper.y;

	    if (taperXEnd > 1.0f )
	    {
		    // Flip tapering.
		    taperXBegin	= 2.0f - taperXEnd;
		    taperXEnd	= 1.0f;
	    }
	    if (taperYEnd > 1.0f )
	    {
		    // Flip tapering.
		    taperYBegin	= 2.0f - taperYEnd;
		    taperYEnd	= 1.0f;
	    }

        // For spheres, the radius is usually zero.
        float radiusStart = 0.5f;
	    if (nSides < 8)
	    {
		    radiusStart = ScaleTable[nSides];
	    }

        // Scale the radius to take the hole size into account.
        radiusStart *= 1.0f - holeY;

        // Now check the radius offset to calculate the start,end radius.  (Negative means
        // decrease the start radius instead).
        float radiusEnd = radiusStart;
        float radiusOffset = parameters.RadiusOffset;
	    if (radiusOffset < 0.0f)
	    {
		    radiusStart *= 1.0f + radiusOffset;
	    }
	    else
	    {
		    radiusEnd   *= 1.0f - radiusOffset;
	    }	
	    // Is the path NOT a closed loop?
	    IsOpen = (   (parameters.End * endScale - parameters.Begin < 1.0f)
                  || (skewMag > 0.001f)
                  || (Mathf.Abs (taperXEnd - taperXBegin) > 0.001f)
                  || (Mathf.Abs (taperYEnd - taperYBegin) > 0.001f)
                  || (Mathf.Abs (radiusEnd  - radiusStart)  > 0.001f) );
        
        // We run through this once before the main loop, to make sure
        // the path begins at the correct cut.
        float step = 1.0f / nSides;
        float t = parameters.Begin;

        AddNGonPoint(parameters, radiusStart, radiusEnd, holeX, holeY, taperXBegin, taperXEnd, taperYBegin, taperYEnd, twistScale, t);
        t += step;

	    // Snap to a quantized parameter, so that cut does not
	    // affect most sample points.
	    t = (int)(t * nSides) / (float)nSides;

	    // Run through the non-cut dependent points.
	    while (t < parameters.End)
	    {
            AddNGonPoint(parameters, radiusStart, radiusEnd, holeX, holeY, taperXBegin, taperXEnd, taperYBegin, taperYEnd, twistScale, t);
            t += step;
	    }

	    // Make one final pass for the end cut.
	    t = parameters.End;
        AddNGonPoint(parameters, radiusStart, radiusEnd, holeX, holeY, taperXBegin, taperXEnd, taperYBegin, taperYEnd, twistScale, t);
    }

    protected void AddNGonPoint(PathParameters parameters,
                                float radiusStart, float radiusEnd,
                                float holeX,       float holeY,
                                float taperXBegin, float taperXEnd,
                                float taperYBegin, float taperYEnd,
                                float twistScale,
                                float t)
    {
        float ang = 2.0f * Mathf.PI * parameters.Revolutions * t;
        float c = Mathf.Cos(ang) * Mathf.Lerp(radiusStart, radiusEnd, t);
        float s = Mathf.Sin(ang) * Mathf.Lerp(radiusStart, radiusEnd, t);

        // Twist rotates the path along the x,y plane (I think) - DJS 04/05/02
        Quaternion twist = Quaternion.AngleAxis ((2f * Mathf.PI
                                                       * Mathf.Lerp (parameters.TwistBegin * twistScale,
                                                                     parameters.TwistEnd * twistScale,
                                                                       t))
                                                       * Mathf.Rad2Deg, // Note: Indra subtracts PI from this angle, but that makes torus shapes weird
                                                 new Vector3(0f, 0f, 1f));
        // Rotate the point around the circle's center.
        Quaternion qang = Quaternion.AngleAxis(ang * Mathf.Rad2Deg, new Vector3(1f, 0f, 0f));

        Points.Add(new PathPoint
        {
            Position = new Vector3(0f + Mathf.Lerp(0, parameters.Shear.x, s) + Mathf.Lerp(-parameters.Skew, parameters.Skew, t) * 0.5f,
                c + Mathf.Lerp(0, parameters.Shear.y, s),
                s),
            Scale = new Vector2(holeX * Mathf.Lerp(taperXBegin, taperXEnd, t),
                holeY * Mathf.Lerp(taperYBegin, taperYEnd, t)),
            Rotation = Matrix4x4.Rotate(twist * qang),
            ExtrusionT = t
        });

    }
}

