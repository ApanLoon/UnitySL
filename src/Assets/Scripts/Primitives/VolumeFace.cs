
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[Flags]
public enum VolumeFaceMask
{
    Single = 0x0001,
    Cap = 0x0002,
    End = 0x0004,
    Side = 0x0008,
    Inner = 0x0010,
    Outer = 0x0020,
    Hollow = 0x0040,
    Open = 0x0080,
    Flat = 0x0100,
    Top = 0x0200,
    Bottom = 0x0400
}

public class VolumeFace
{
    public int Id { get; set; }
    public VolumeFaceMask TypeMask { get; set; }

    // Only used for INNER/OUTER faces
    public int BeginS { get; set; }
    public int BeginT { get; set; }
    public int NumS { get; set; }
    public int NumT { get; set; }

    public Vector3 ExtentsMin { get; set; }
    public Vector3 ExtentsMax { get; set; }
    public Vector3 Centre { get; set; }
    public Vector3 TexCoordExtentsMin { get; set; }
    public Vector3 TexCoordExtentsMax { get; set; }
    public int NumVertices { get; set; }
    public int NumAllocatedVertices { get; set; }
    public int NumIndices { get; set; }

    public List<Vector3> Positions { get; set; } = new List<Vector3>();
    public List<Vector3> Normals { get; set; } = new List<Vector3>();
    public List<Vector3> Tangents { get; set; } = new List<Vector3>();
    public List<Vector2> TexCoords { get; set; } = new List<Vector2>();
    public List<int> Indices { get; set; } = new List<int>();
    public List<int> Edge { get; set; } = new List<int>();

    public bool IsOptimised { get; set; }

    public class VertexData
    {
        public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector2 TexCoord { get; set; }
    }

    public bool Create(Volume volume, bool partial_build)
    {
        //tree for this face is no longer valid
        //delete mOctree;
        //mOctree = NULL;

        bool ret = false;
        if ((TypeMask & VolumeFaceMask.Cap) != 0)
        {
            ret = CreateCap (volume, partial_build);
        }
        else if ((TypeMask & VolumeFaceMask.End) != 0 || (TypeMask & VolumeFaceMask.Side) != 0)
        {
            ret = CreateSide (volume, partial_build);
        }
        else
        {
            Debug.LogError("Unknown/uninitialized face type!");
        }
        return ret;
    }

    protected bool CreateCap (Volume volume, bool partialBuild)
    {
		if ((TypeMask & VolumeFaceMask.Hollow) == 0
            && (TypeMask & VolumeFaceMask.Open) == 0
            && (   (volume.Parameters.PathParameters.Begin == 0.0f)
                && (volume.Parameters.PathParameters.End == 1.0f))
                && (   volume.Parameters.ProfileParameters.ProfileType == ProfileType.Square
                    && volume.Parameters.PathParameters.PathType == PathType.Line)
	        )
		{
			return CreateUnCutCubeCap (volume, partialBuild);
		}

		int numVertices = 0, numIndices = 0;

		List<Vector3> mesh = volume.Points;
		List<Vector3> profile = volume.Profile.Points;

		// All types of caps have the same number of vertices and indices
		numVertices = profile.Count;
		numIndices = (profile.Count - 2) * 3;

		//if ((TypeMask & VolumeFaceMask.Hollow) == 0 && (TypeMask & VolumeFaceMask.Open) == 0)
		//{
		//	resizeVertices(num_vertices + 1);

		//	//if (!partial_build)
		//	{
		//		resizeIndices(num_indices + 3);
		//	}
		//}
		//else
		//{
		//	resizeVertices(num_vertices);
		//	//if (!partial_build)
		//	{
		//		resizeIndices(num_indices);
		//	}
		//}

		Positions.Clear();
        Normals.Clear();
        Tangents.Clear();
        TexCoords.Clear();
        Indices.Clear();
        Edge.Clear();

		int maxS = volume.Profile.PointCount;
		int maxT = volume.Path.PointCount;

		Centre = Vector3.zero;

		int offset = ((TypeMask & VolumeFaceMask.Top) != 0)
            ? (maxT - 1) * maxS
            : BeginS;

		// Figure out the normal, assume all caps are flat faces.
		// Cross product to get normals.

		Vector2 cuv;
		Vector2 min_uv, max_uv;
		// VFExtents change
		Vector3 min = ExtentsMin;
		Vector3 max = ExtentsMax;

		// Copy the vertices into the array

		int srcIndex = offset; // Index in mesh
		int endIndex = srcIndex + numVertices; // Index in mesh

		min = mesh[srcIndex];
		max = min;

		int pIndex = 0; // Index in profile

		if ((TypeMask & VolumeFaceMask.Top) != 0)
		{
			min_uv.x = profile[pIndex].x + 0.5f;
			min_uv.y = profile[pIndex].y + 0.5f;

			max_uv = min_uv;

			while (srcIndex < endIndex)
            {
                Vector2 tc = new Vector2(profile[pIndex].x + 0.5f, profile[pIndex].y + 0.5f);
                UpdateMinMax (ref min_uv, ref max_uv, tc);
				TexCoords.Add(tc);

				UpdateMinMax (ref min, ref max, mesh[srcIndex]);
				Positions.Add (mesh[srcIndex]);

				++pIndex;
				++srcIndex;
			}
		}
		else
        {
            min_uv.x = profile[pIndex].x + 0.5f;
            min_uv.y = 0.5f - profile[pIndex].y;
			max_uv = min_uv;

			while (srcIndex < endIndex)
			{
				// Mirror for underside.
                Vector2 tc = new Vector2(profile[pIndex].x + 0.5f, 0.5f - profile[pIndex].y);
                UpdateMinMax(ref min_uv, ref max_uv, tc);
                TexCoords.Add(tc);

                UpdateMinMax(ref min, ref max, mesh[srcIndex]);
                Positions.Add(mesh[srcIndex]);

                ++pIndex;
                ++srcIndex;
			}
		}

        Centre = (min + max) * 0.5f;
        cuv = (min_uv + max_uv) * 0.5f;

        VertexData vd = new VertexData
        {
            Position = Centre,
            TexCoord = cuv
        };

		if ((TypeMask & VolumeFaceMask.Hollow) == 0 && (TypeMask & VolumeFaceMask.Open) == 0)
        {
            Positions.Add(Centre);
			TexCoords.Add(cuv);
			numVertices++;
		}

	    //if (partial_build)
	    //{
	    //	return TRUE;
	    //}

		if ((TypeMask & VolumeFaceMask.Hollow) != 0)
        {
            CreateHollowCap (numVertices, profile, (TypeMask & VolumeFaceMask.Top) == 0);
        }
		else
		{
			// Not hollow, generate the triangle fan.
            CreateSolidCap (numVertices);
		}

	    Vector3 d0 = Positions[Indices[1]] - Positions[Indices[0]];
		Vector3 d1 = Positions[Indices[2]] - Positions[Indices[0]];

		Vector3 normal = Vector3.Cross(d0, d1);

		if (Vector3.Dot(normal, normal) > 0.00001f)
		{
			normal.Normalize();
		}
		else
        {
            //degenerate, make up a value
            normal = normal.z >= 0 ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, -1f);
        }

		//llassert(llfinite(normal.getF32ptr()[0]));
		//llassert(llfinite(normal.getF32ptr()[1]));
		//llassert(llfinite(normal.getF32ptr()[2]));

		//llassert(!llisnan(normal.getF32ptr()[0]));
		//llassert(!llisnan(normal.getF32ptr()[1]));
		//llassert(!llisnan(normal.getF32ptr()[2]));

		for (int i = 0; i < numVertices; i++)
		{
			Normals.Add(normal);
		}

		return true;
    }

	protected bool CreateUnCutCubeCap (Volume volume, bool partialBuild)
	{
        List<Vector3> mesh = volume.Points;
		List<Vector3> profile = volume.Profile.Points;
		int maxS = volume.Profile.PointCount;
		int maxT = volume.Path.PointCount;

		int gridSize = (profile.Count - 1) / 4;
		// VFExtents change
		Vector3 min = ExtentsMin;
		Vector3 max = ExtentsMax;

		int offset = ((TypeMask & VolumeFaceMask.Top) != 0)
            ? (maxT - 1) * maxS 
            : BeginS;

        {
			VertexData[] corners = new VertexData[4];
			VertexData baseVert = new VertexData();
			for (int t = 0; t < 4; t++)
			{
                corners[t] = new VertexData
                {
                    Position = mesh[offset + (gridSize * t)],
                    TexCoord = new Vector2(profile[gridSize * t].x + 0.5f, 0.5f - profile[gridSize * t].y)
                };
            }

			{
				Vector3 lhs = corners[1].Position - corners[0].Position;
				Vector3 rhs = corners[2].Position - corners[1].Position;
				baseVert.Normal = Vector3.Cross (lhs, rhs);
				baseVert.Normal.Normalize();
			}

			if ((TypeMask & VolumeFaceMask.Top) == 0)
			{
				baseVert.Normal *= -1.0f;
			}
			else
			{
				//Swap the UVs on the U(X) axis for top face
				Vector2 swap;
				swap = corners[0].TexCoord;
				corners[0].TexCoord = corners[3].TexCoord;
				corners[3].TexCoord = swap;
				swap = corners[1].TexCoord;
				corners[1].TexCoord = corners[2].TexCoord;
				corners[2].TexCoord = swap;
			}

			int size = (gridSize + 1) * (gridSize + 1);
			//resizeVertices(size);
            Positions.Clear();
            Normals.Clear();
            Tangents.Clear();
            TexCoords.Clear();
            Indices.Clear();
            Edge.Clear();

			for (int gx = 0; gx < gridSize + 1; gx++)
			{
				for (int gy = 0; gy < gridSize + 1; gy++)
				{
					VertexData newVert = LerpPlanarVertex (corners[0],
						                                   corners[1],
                                                           corners[3],
						                                   (float)gx / (float)gridSize,
						                                   (float)gy / (float)gridSize);

                    Positions.Add (newVert.Position);
					Normals.Add(baseVert.Normal);
					TexCoords.Add(newVert.TexCoord);

					if (gx == 0 && gy == 0)
					{
						min = newVert.Position;
						max = min;
					}
					else
					{
                        UpdateMinMax (ref min, ref max, newVert.Position);
					}
				}
			}

			Centre = (min + max) * 0.5f;
            ExtentsMin = min;
            ExtentsMax = max;
        }

		if (!partialBuild)
		{
			int[] idxs = { 0, 1, (gridSize + 1) + 1, (gridSize + 1) + 1, (gridSize + 1), 0 };

			int curEdge = 0;

			for (int gx = 0; gx < gridSize; gx++)
			{

				for (int gy = 0; gy < gridSize; gy++)
				{
					if ((TypeMask & VolumeFaceMask.Top) != 0)
					{
						for (int i = 5; i >= 0; i--)
                        {
                            Indices.Add((gy * (gridSize + 1)) + gx + idxs[i]);
						}

						int edgeValue = gridSize * 2 * gy + gx * 2;

						if (gx > 0)
						{
							Edge.Add(edgeValue);
						}
						else
						{
							Edge.Add(-1); // Mark face to higlight it
						}

						if (gy < gridSize - 1)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add (-1);
						}

						Edge.Add (edgeValue);

						if (gx < gridSize - 1)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add (-1);
						}

						if (gy > 0)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add(-1);
						}

						Edge.Add (edgeValue);
					}
					else
					{
						for (int i = 0; i < 6; i++)
                        {
                            Indices.Add ((gy * (gridSize + 1)) + gx + idxs[i]);
						}

						int edgeValue = gridSize * 2 * gy + gx * 2;

						if (gy > 0)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add (-1);
						}

						if (gx < gridSize - 1)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add (-1);
						}

						Edge.Add (edgeValue);

						if (gy < gridSize - 1)
						{
							Edge.Add (edgeValue);
						}
						else
                        {
                            Edge.Add(-1);
                        }

						if (gx > 0)
						{
							Edge.Add (edgeValue);
						}
						else
						{
							Edge.Add (-1);
						}

						Edge.Add (edgeValue);
					}
				}
			}
		}

		return true;
	}

	protected void CreateHollowCap(int numVertices, List<Vector3> profile, bool bottom)
    {
		// HOLLOW TOP
		// Does it matter if it's open or closed? - djs

		int pt1 = 0, pt2 = numVertices - 1;
		int i = 0;
		while (pt2 - pt1 > 1)
		{
			// Use the profile points instead of the mesh, since you want
			// the un-transformed profile distances.
			Vector3 p1 = profile[pt1];
			Vector3 p2 = profile[pt2];
			Vector3 pa = profile[pt1 + 1];
			Vector3 pb = profile[pt2 - 1];

			// Use area of triangle to determine backfacing
			float area_1a2, area_1ba, area_21b, area_2ab;
			area_1a2 =   (p1.x * pa.y - pa.x * p1.y)
                       + (pa.x * p2.y - p2.x * pa.y)
                       + (p2.x * p1.y - p1.x * p2.y);

			area_1ba =   (p1.x * pb.y - pb.x * p1.y)
                       + (pb.x * pa.y - pa.x * pb.y)
                       + (pa.x * p1.y - p1.x * pa.y);

			area_21b =   (p2.x * p1.y - p1.x * p2.y)
                       + (p1.x * pb.y - pb.x * p1.y)
                       + (pb.x * p2.y - p2.x * pb.y);

			area_2ab =   (p2.x * pa.y - pa.x * p2.y)
                       + (pa.x * pb.y - pb.x * pa.y)
                       + (pb.x * p2.y - p2.x * pb.y);

			bool use_tri1a2 = true;
			bool tri_1a2 = true;
			bool tri_21b = true;

			if (area_1a2 < 0)
			{
				tri_1a2 = false;
			}
			if (area_2ab < 0)
			{
				// Can't use, because it contains point b
				tri_1a2 = false;
			}
			if (area_21b < 0)
			{
				tri_21b = false;
			}
			if (area_1ba < 0)
			{
				// Can't use, because it contains point b
				tri_21b = false;
			}

			if (!tri_1a2)
			{
				use_tri1a2 = false;
			}
			else if (!tri_21b)
			{
				use_tri1a2 = true;
			}
			else
			{
				Vector3 d1 = p1 - pa;
				Vector3 d2 = p2 - pb;
				use_tri1a2 = Vector3.Dot(d1, d1) < Vector3.Dot(d2, d2);
			}

			if (use_tri1a2)
			{
                if (bottom)
                {
                    Indices.Add(pt1);
                    Indices.Add(pt2);
                    Indices.Add(pt1 + 1);
				}
				else
                {
                    Indices.Add(pt1);
                    Indices.Add(pt1 + 1);
                    Indices.Add(pt2);
				}
				i += 3;
				pt1++;
			}
			else
			{
                if (bottom)
                {
                    Indices.Add(pt1);
                    Indices.Add(pt2);
                    Indices.Add(pt2 - 1);
                }
				else
                {
                    Indices.Add(pt1);
                    Indices.Add(pt2 - 1);
                    Indices.Add(pt2);
                }
				i += 3;
				pt2--;
			}
		}
    }

    protected void CreateSolidCap(int numVertices)
    {
        bool top = (TypeMask & VolumeFaceMask.Top) != 0;

        for (int i = 0; i < (numVertices - 2); i++)
        {
            if (top)
            {
				Indices.Add(numVertices - 1);
				Indices.Add(i);
                Indices.Add(i + 1);
            }
			else
            {
                Indices.Add(numVertices - 1);
                Indices.Add(i + 1);
                Indices.Add(i);
            }
        }
    }

	protected bool CreateSide (Volume volume, bool partialBuild)
    {
        bool flat = TypeMask.HasFlag(VolumeFaceMask.Flat); //TypeMask & VolumeFaceMask.Flat) != 0;

		SculptType sculptStitching = volume.Parameters.SculptType;
        SculptFlags sculptFlags = volume.Parameters.SculptFlags;
        bool sculptInvert = sculptFlags.HasFlag(SculptFlags.Invert); //sculptFlags & SculptFlags.Invert) != 0;
        bool sculptMirror = sculptFlags.HasFlag(SculptFlags.Mirror); //sculptFlags & SculptFlags.Mirror) != 0;
		bool sculptReverseHorizontal = (sculptInvert ? !sculptMirror : sculptMirror);  // XOR

		int numVertices, numIndices;

		List<Vector3> mesh = volume.Points;
		List<Vector3> profile = volume.Profile.Points;
		List<Path.PathPoint> pathData = volume.Path.Points;

		int maxS = volume.Profile.PointCount;

		int s, t, i;
		float ss, tt;

		numVertices = NumS * NumT;
		numIndices = (NumS - 1) * (NumT - 1) * 6;

		// TODO: How does partial builds work?
		//partial_build = (num_vertices > NumVertices || num_indices > NumIndices) ? false : partial_build;

		//if (!partial_build)
		//{
		//	resizeVertices(num_vertices);
		//	resizeIndices(num_indices);

		//	if (!volume->isMeshAssetLoaded())
		//	{
		//		mEdge.resize(num_indices);
		//	}
		//}
		Positions.Clear();
		Indices.Clear();
		Edge.Clear();


		float beginStex = Mathf.Floor (profile[BeginS][2]);
		int numS = (TypeMask.HasFlag(VolumeFaceMask.Inner | VolumeFaceMask.Flat) && NumS > 2) //(TypeMask & VolumeFaceMask.Inner) != 0 && (TypeMask & VolumeFaceMask.Flat) != 0 && NumS > 2)
			? NumS / 2
            : NumS;

		int curVertex = 0;
		int endT = BeginT + NumT;
        bool test = TypeMask.HasFlag(VolumeFaceMask.Inner | VolumeFaceMask.Flat) && NumS > 2; //(TypeMask & VolumeFaceMask.Inner) != 0 && (TypeMask & VolumeFaceMask.Flat) != 0 && NumS > 2;

		// Copy the vertices into the array
		for (t = BeginT; t < endT; t++)
		{
			tt = pathData[t].ExtrusionT;
			for (s = 0; s < numS; s++)
			{
				if (TypeMask.HasFlag(VolumeFaceMask.End)) //(TypeMask & VolumeFaceMask.End) != 0)
                {
                    ss = s > 0 ? 1f : 0f;
                }
				else
				{
					// Get s value for tex-coord.
					if (!flat)
					{
						ss = profile[BeginS + s][2];
					}
					else
					{
						ss = profile[BeginS + s][2] - beginStex;
					}
				}

				if (sculptReverseHorizontal)
				{
					ss = 1f - ss;
				}

				// Check to see if this triangle wraps around the array.
				if (BeginS + s >= maxS)
				{
					// We're wrapping
					i = BeginS + s + maxS * (t - 1);
				}
				else
				{
					i = BeginS + s + maxS * t;
				}

                Positions.Add (mesh[i]);
				TexCoords.Add (new Vector2(ss, tt));

				curVertex++;

				if (test && s > 0)
                {
                    Positions.Add (mesh[i]);
                    TexCoords.Add(new Vector2(ss, tt));
					curVertex++;
				}
			}

			if (TypeMask.HasFlag(VolumeFaceMask.Inner | VolumeFaceMask.Flat) && NumS > 2) //(TypeMask & VolumeFaceMask.Inner) != 0 && (TypeMask & VolumeFaceMask.Flat) != 0 && NumS > 2)
			{
				s = TypeMask.HasFlag(VolumeFaceMask.Open) //((TypeMask & VolumeFaceMask.Open) != 0)
					? s = numS - 1
					: 0;

				i = BeginS + s + maxS * t;
				ss = profile[BeginS + s][2] - beginStex;

				Positions.Add(mesh[i]);
                TexCoords.Add(new Vector2(ss, tt));

				curVertex++;
			}
		}

        Centre = Vector3.zero;

//		int cur_pos = Positions.Count;
//		int end_pos = cur_pos + NumVertices;
        int curPos = 0;
        int endPos = Positions.Count;

		//get bounding box for this side
		Vector3 faceMin;
		Vector3 faceMax;

		faceMin = faceMax = Positions[curPos++];

		while (curPos < endPos)
		{
			UpdateMinMax(ref faceMin, ref faceMax, Positions[curPos++]);
		}
		// VFExtents change
		ExtentsMin = faceMin;
		ExtentsMax = faceMax;

		int tcCount = NumVertices;
		if (tcCount % 2 == 1)
		{ //odd number of texture coordinates, duplicate last entry to padded end of array
			tcCount++;
			TexCoords.Add(TexCoords[NumVertices - 1]);
		}

//		int cur_tc = TexCoords.Count;
//		int end_tc = cur_tc + tc_count;
        int curTc = 0;
        int endTc = TexCoords.Count;

		Vector3 tcMin;
		Vector3 tcMax;

		tcMin = tcMax = TexCoords[curTc++];

		while (curTc < endTc)
		{
			UpdateMinMax (ref tcMin, ref tcMax, TexCoords[curTc++]);
		}

		//TODO: TexCoordExtents are weird this assumes Vector4
		//TexCoordExtentsMin.x = llmin(minp[0], minp[2]);
		//TexCoordExtentsMin.y = llmin(minp[1], minp[3]);
		//TexCoordExtentsMax.x = llmax(maxp[0], maxp[2]);
		//TexCoordExtentsMax.y = llmax(maxp[1], maxp[3]);

		Centre = (faceMin + faceMax) * 0.5f;

		int curIndex = 0;
        bool flatFace = TypeMask.HasFlag(VolumeFaceMask.Flat); //(TypeMask & VolumeFaceMask.Flat) != 0;

		if (!partialBuild)
		{
			// Now we generate the indices.
			for (t = 0; t < (NumT - 1); t++)
			{
				for (s = 0; s < (NumS - 1); s++)
				{
					Indices.Add (s     + NumS *  t);       //bottom left
					Indices.Add (s + 1 + NumS * (t + 1));  //top right
					Indices.Add (s     + NumS * (t + 1));  //top left
					Indices.Add (s     + NumS *  t);       //bottom left
					Indices.Add (s + 1 + NumS *  t);       //bottom right
					Indices.Add (s + 1 + NumS * (t + 1));  //top right
                    curIndex += 6;

					Edge.Add ((NumS - 1) * 2 * t + s * 2 + 1);    //bottom left/top right neighbor face 
					if (t < NumT - 2)
					{                                               //top right/top left neighbor face 
						Edge.Add ((NumS - 1) * 2 * (t + 1) + s * 2 + 1);
					}
					else if (NumT <= 3 || volume.Path.IsOpen == true)
					{ //no neighbor
						Edge.Add (-1);
					}
					else
					{ //wrap on T
						Edge.Add(s * 2 + 1);
					}
					if (s > 0)
					{                                                   //top left/bottom left neighbor face
						Edge.Add ((NumS - 1) * 2 * t + s * 2 - 1);
					}
					else if (flatFace || volume.Profile.IsOpen == true)
					{ //no neighbor
						Edge.Add (-1);
					}
					else
					{   //wrap on S
						Edge.Add ((NumS - 1) * 2 * t + (NumS - 2) * 2 + 1);
					}

					if (t > 0)
					{                                                   //bottom left/bottom right neighbor face
						Edge.Add ((NumS - 1) * 2 * (t - 1) + s * 2);
					}
					else if (NumT <= 3 || volume.Path.IsOpen == true)
					{ //no neighbor
						Edge.Add (-1);
					}
					else
					{ //wrap on T
						Edge.Add ((NumS - 1) * 2 * (NumT - 2) + s * 2);
					}
					if (s < NumS - 2)
					{                                               //bottom right/top right neighbor face
						Edge.Add ((NumS - 1) * 2 * t + (s + 1) * 2);
					}
					else if (flatFace || volume.Profile.IsOpen == true)
					{ //no neighbor
						Edge.Add (-1);
					}
					else
					{ //wrap on S
						Edge.Add ((NumS - 1) * 2 * t);
					}
					Edge.Add ((NumS - 1) * 2 * t + s * 2);                            //top right/bottom left neighbor face	
				}
			}
		}

		//      //clear normals
		//int dst = Normals.Count;
		//int end = dst + NumVertices;
		//Vector3 zero = Vector3.zero;

		//while (dst < end)
		//      {
		//          Normals.Add(zero);
		//	dst++;
		//}

		//generate normals 

        return true;

		//TODO: Normal calculation is broken - Could it be that Indra supports a normal per index while re-using positions?

		int count = Indices.Count / 3;
		Debug.Log($"Generate normals. {count}");
		List<Vector3> triangle_normals = new List<Vector3>();
		int output = 0;
		int end_output = count;
        int idx = 0;
		while (output < end_output)
        {
            Vector3 b  = Positions[Indices[idx + 0]];
            Vector3 v1 = Positions[Indices[idx + 1]];
            Vector3 v2 = Positions[Indices[idx + 2]];

			//calculate triangle normal
			Vector3 a = b - v1;
			b -= v2;

            Vector3 d0 = v1 - b;
            Vector3 d1 = v2 - b;

            Vector3 normal = Vector3.Cross(d0, d1);

            if (Vector3.Dot(normal, normal) > 0.00001f)
            {
                normal.Normalize();
            }
            else
            {
                //degenerate, make up a value
                normal = normal.z >= 0 ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, -1f);
            }

            Normals.Add(normal);
            Normals.Add(normal);
			//LLQuad & vector1 = *((LLQuad*)&v1);
			//LLQuad & vector2 = *((LLQuad*)&v2);

			//LLQuad & amQ = *((LLQuad*)&a);
			//LLQuad & bmQ = *((LLQuad*)&b);

			////v1.setCross3(t,v0);
			////setCross3(const LLVector4a& a, const LLVector4a& b)
			//// Vectors are stored in memory in w, z, y, x order from high to low
			//// Set vector1 = { a[W], a[X], a[Z], a[Y] }
			//vector1 = _mm_shuffle_ps(amQ, amQ, _MM_SHUFFLE(3, 0, 2, 1));
			//// Set vector2 = { b[W], b[Y], b[X], b[Z] }
			//vector2 = _mm_shuffle_ps(bmQ, bmQ, _MM_SHUFFLE(3, 1, 0, 2));
			//// mQ = { a[W]*b[W], a[X]*b[Y], a[Z]*b[X], a[Y]*b[Z] }
			//vector2 = _mm_mul_ps(vector1, vector2);
			//// vector3 = { a[W], a[Y], a[X], a[Z] }
			//amQ = _mm_shuffle_ps(amQ, amQ, _MM_SHUFFLE(3, 1, 0, 2));
			//// vector4 = { b[W], b[X], b[Z], b[Y] }
			//bmQ = _mm_shuffle_ps(bmQ, bmQ, _MM_SHUFFLE(3, 0, 2, 1));
			//// mQ = { 0, a[X]*b[Y] - a[Y]*b[X], a[Z]*b[X] - a[X]*b[Z], a[Y]*b[Z] - a[Z]*b[Y] }
			//vector1 = _mm_sub_ps(vector2, _mm_mul_ps(amQ, bmQ));

			//llassert(v1.isFinite3());

			//v1.store4a((F32*)output);

			output++;
			idx += 3;
		}

		//idx = 0;

		//for (int i = 0; i < count; i++) //for each triangle
		//{
		//	Vector3 c = triangle_normals[i];

        //  LLVector4a* n0p = Normals[Indices[idx + 0]];
		//	LLVector4a* n1p = norm + idx[1];
		//	LLVector4a* n2p = norm + idx[2];

		//	idx += 3;

		//	LLVector4a n0, n1, n2;
		//	n0.load4a((F32*)n0p);
		//	n1.load4a((F32*)n1p);
		//	n2.load4a((F32*)n2p);

		//	n0.add(c);
		//	n1.add(c);
		//	n2.add(c);

		//	llassert(c.isFinite3());

		//	//even out quad contributions
		//	switch (i % 2 + 1)
		//	{
		//		case 0: n0.add(c); break;
		//		case 1: n1.add(c); break;
		//		case 2: n2.add(c); break;
		//	};

		//	n0.store4a((F32*)n0p);
		//	n1.store4a((F32*)n1p);
		//	n2.store4a((F32*)n2p);
		//}


		// adjust normals based on wrapping and stitching

		Vector3 top = (Positions[0] - Positions[NumS * (NumT - 2)]);
		bool s_bottom_converges = UnityEngine.Vector3.Dot(top, top) < 0.000001f;

		top = Positions[NumS - 1] - Positions[NumS * (NumT - 2) + NumS - 1];
		bool s_top_converges = UnityEngine.Vector3.Dot (top, top) < 0.000001f;

		if (sculptStitching == SculptType.None)  // logic for non-sculpt volumes
		{
			if (volume.Path.IsOpen == false)
			{ //wrap normals on T
				for (int j = 0; j < NumS; j++)
				{
					Vector3 n = Normals[j] + Normals[NumS * (NumT - 1) + j];
					Normals[j] = n;
					Normals[NumS * (NumT - 1) + j] = n;
				}
			}

			if ((volume.Profile.IsOpen == false) && !(s_bottom_converges))
			{ //wrap normals on S
				for (int j = 0; j < NumT; j++)
				{
					Vector3 n = Normals[NumS * j] + Normals[NumS * j + NumS - 1];
					Normals[NumS * j] = n;
					Normals[NumS * j + NumS - 1] = n;
				}
			}

			if (   volume.Parameters.PathParameters.PathType == PathType.Circle
                && volume.Parameters.ProfileParameters.ProfileType == ProfileType.CircleHalf)
			{
				if (s_bottom_converges)
				{ //all lower S have same normal
					for (int j = 0; j < NumT; j++)
					{
						Normals[NumS * j] = new Vector3(1f, 0f, 0f);
					}
				}

				if (s_top_converges)
				{ //all upper S have same normal
					for (int j = 0; j < NumT; j++)
					{
						Normals[NumS * j + NumS - 1] = new Vector3(-1f, 0f, 0f);
					}
				}
			}
		}
		else  // logic for sculpt volumes
		{
			//BOOL average_poles = FALSE;
			//BOOL wrap_s = FALSE;
			//BOOL wrap_t = FALSE;

			//if (sculpt_stitching == LL_SCULPT_TYPE_SPHERE)
			//	average_poles = TRUE;

			//if ((sculpt_stitching == LL_SCULPT_TYPE_SPHERE) ||
			//	(sculpt_stitching == LL_SCULPT_TYPE_TORUS) ||
			//	(sculpt_stitching == LL_SCULPT_TYPE_CYLINDER))
			//	wrap_s = TRUE;

			//if (sculpt_stitching == LL_SCULPT_TYPE_TORUS)
			//	wrap_t = TRUE;


			//if (average_poles)
			//{
			//	// average normals for north pole

			//	LLVector4a average;
			//	average.clear();

			//	for (S32 i = 0; i < mNumS; i++)
			//	{
			//		average.add(norm[i]);
			//	}

			//	// set average
			//	for (S32 i = 0; i < mNumS; i++)
			//	{
			//		norm[i] = average;
			//	}

			//	// average normals for south pole

			//	average.clear();

			//	for (S32 i = 0; i < mNumS; i++)
			//	{
			//		average.add(norm[i + mNumS * (mNumT - 1)]);
			//	}

			//	// set average
			//	for (S32 i = 0; i < mNumS; i++)
			//	{
			//		norm[i + mNumS * (mNumT - 1)] = average;
			//	}
   //         }

			//if (wrap_s)
			//{
			//	for (S32 i = 0; i < mNumT; i++)
			//	{
			//		LLVector4a n;
			//		n.setAdd(norm[mNumS * i], norm[mNumS * i + mNumS - 1]);
			//		norm[mNumS * i] = n;
			//		norm[mNumS * i + mNumS - 1] = n;
			//	}
			//}

			//if (wrap_t)
			//{
			//	for (S32 i = 0; i < mNumS; i++)
			//	{
			//		LLVector4a n;
			//		n.setAdd(norm[i], norm[mNumS * (mNumT - 1) + i]);
			//		norm[i] = n;
			//		norm[mNumS * (mNumT - 1) + i] = n;
			//	}
			//}
        }

    	return true;
	}

	protected void UpdateMinMax(ref Vector2 min, ref Vector2 max, Vector2 pos)
    {
        if (pos.x < min.x)
        {
            min.x = pos.x;
        }
        if (pos.x > max.x)
        {
            max.x = pos.x;
        }
        if (pos.y < min.y)
        {
            min.y = pos.y;
        }
        if (pos.y > max.y)
        {
            max.y = pos.y;
        }
    }

    protected void UpdateMinMax (ref Vector3 min, ref Vector3 max, Vector3 pos)
    {
        if (pos.x < min.x)
        {
            min.x = pos.x;
        }
        if (pos.x > max.x)
        {
            max.x = pos.x;
        }
        if (pos.y < min.y)
        {
            min.y = pos.y;
        }
        if (pos.y > max.y)
        {
            max.y = pos.y;
        }
		if (pos.z < min.z)
        {
            min.z = pos.z;
        }
        if (pos.z > max.z)
        {
            max.z = pos.z;
        }
    }

	protected VertexData LerpPlanarVertex (VertexData v0, VertexData v1, VertexData v2, float coef01, float coef02)
    {
        return new VertexData()
        {
            Position = (v1.Position - v0.Position) * coef01 + (v2.Position - v0.Position) * coef02 + v0.Position,
            TexCoord = (v1.TexCoord - v0.TexCoord) * coef01 + (v2.TexCoord - v0.TexCoord) * coef02 + v0.TexCoord,
			Normal = v0.Normal
        };
    }
}
