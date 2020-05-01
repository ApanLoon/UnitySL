using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public enum PrimitiveShape
{
    Cube          =  1,
    Prism         =  2,
    Tetrahedron   =  3,
    Pyramid       =  4,
    Cylinder      =  5,
    Cone          =  6,
    Sphere        =  7,
    Torus         =  8,
    SquareTorus   =  9,   //Not compliant with Indra
    TriangleTorus = 10,   //Not compliant with Indra
    CylinderHemi  = 11,   //Not compliant with Indra
    ConeHemi      = 12,   //Not compliant with Indra
    SphereHemi    = 13,   //Not compliant with Indra
    TorusHemi     = 14,   //Not compliant with Indra
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Volume : MonoBehaviour
{
    public const int MIN_DETAIL_FACES = 6;
    public const float MIN_LOD = 0f;
    public const float MAX_LOD = 3f;

    [Flags]
    public enum GizmoVisibility
    {
        Path    = 0x01,
        Profile = 0x02,
        Points  = 0x04,
        Edge    = 0x08,
        Normals = 0x10,
    }
    public GizmoVisibility ShowGizmos;

    public int NumVolumeFaces => VolumeFaces.Count;

    public FaceId FaceMask;
    public Vector3 LodScaleBias;

    [SerializeField] protected bool IsUnique;
    [SerializeField] protected float Lod = 3f;
    [SerializeField] protected int SculptLevel;
    [SerializeField] protected float SurfaceArea;
    [SerializeField] protected bool IsMeshAssetLoaded;
    
    public VolumeParameters Parameters = new VolumeParameters();
    public Path Path;
    public Profile Profile;
    public List<Vector3> Points = new List<Vector3>();

    [SerializeField] protected bool GenerateSingleFace = false;
    protected List<VolumeFace> VolumeFaces = new List<VolumeFace>();

    protected static readonly Dictionary<FaceId, int> FaceIdToMaterialIndex = new Dictionary<FaceId, int>
    {
        {FaceId.PathBegin,    0},
        {FaceId.PathEnd,      1},
        {FaceId.InnerSide,    2},
        {FaceId.ProfileBegin, 3},
        {FaceId.ProfileEnd,   4},
        {FaceId.Outer0,       5},
        {FaceId.Outer1,       6},
        {FaceId.Outer2,       7},
        {FaceId.Outer3,       8}
    };
    [SerializeField] Material[] Materials = new Material[9];

    protected MeshFilter MeshFilter;
    protected MeshRenderer MeshRenderer;

    private void OnDrawGizmos()
    {
        if (ShowGizmos.HasFlag(GizmoVisibility.Path) && Path != null)
        {
            for (var i = 0; i < Path.Points.Count; i++)
            {
                Path.PathPoint point = Path.Points[i];
                Gizmos.color = Color.black;
                if (i > 0)
                {
                    Gizmos.DrawLine(Path.Points[i - 1].Position, point.Position);
                }

                Gizmos.color = Color.red;
                Gizmos.DrawLine(point.Position, point.Position + point.Rotation.MultiplyPoint3x4 (Vector3.right * 0.25f));
                Gizmos.color = Color.green;
                Gizmos.DrawLine(point.Position, point.Position + point.Rotation.MultiplyPoint3x4(Vector3.up * 0.25f));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(point.Position, point.Position + point.Rotation.MultiplyPoint3x4(Vector3.forward * 0.25f));
            }

            if (!Path.IsOpen && Path.PointCount > 1)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(Path.Points[Path.PointCount - 1].Position, Path.Points[0].Position);
            }
        }

        if (ShowGizmos.HasFlag(GizmoVisibility.Profile) && Profile != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < Profile.PointCountOutside; i++)
            {
                if (i > 0)
                {
                    Vector3 p1 = Profile.Points[i - 1];
                    Vector3 p2 = Profile.Points[i];
                    
                    // z is the t-value, not actually the position
                    p1.z = 0f;
                    p2.z = 0f;
                    Gizmos.DrawLine(p1, p2);
                }
            }

            if (Profile.PointCount > Profile.PointCountOutside)
            {
                // We are hollow
                for (int i = Profile.PointCountOutside; i < Profile.PointCount; i++)
                {
                    if (i > Profile.PointCountOutside)
                    {
                        Vector3 p1 = Profile.Points[i - 1];
                        Vector3 p2 = Profile.Points[i];

                        // z is the t-value, not actually the position
                        p1.z = 0f;
                        p2.z = 0f;
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }
        }

        if (ShowGizmos.HasFlag(GizmoVisibility.Points))
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Vector3 point = Points[i];
                if (i > 0)
                {
                    Gizmos.DrawLine(Points[i - 1], point);
                }

                Gizmos.color = Color.Lerp(Color.black, Color.white, (float) i / Points.Count);
                Gizmos.DrawSphere(point, 0.025f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(point, point + Vector3.right * 0.25f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(point, point + Vector3.up * 0.25f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(point, point + Vector3.forward * 0.25f);
            }
        }

        if (ShowGizmos.HasFlag(GizmoVisibility.Edge))
        {
            Gizmos.color = Color.magenta;
            foreach (VolumeFace face in VolumeFaces)
            {
                foreach (int i in face.Edge)
                {
                    //TODO: I don't know what the Edge list contains...
                    //Gizmos.DrawCube(face.Positions[face.Indices[i]], Vector3.one * 0.025f);
                }
            }
        }

        if (ShowGizmos.HasFlag(GizmoVisibility.Normals))
        {
            Gizmos.color = Color.cyan;
            foreach (VolumeFace face in VolumeFaces)
            {
                for (var i = 0; i < face.Positions.Count; i++)
                {
                    Vector3 position = face.Positions[i];
                    Vector3 normal = face.Normals[i];
                    Gizmos.DrawSphere(position, 0.0125f);
                    Gizmos.DrawLine(position, position + normal * 0.1f);
                }
            }
        }
    }

    private void Start()
    {
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<MeshRenderer>();

        Path = new Path();
        Profile = new Profile();
        GenerateMesh();
    }

    private void Update()
    {
        GenerateMesh();
    }

    public void ResetToDefaultShape(int shape)
    {
        ResetToDefaultShape ((PrimitiveShape)shape);
    }

    public void ResetToDefaultShape (PrimitiveShape shape)
    {
        Parameters.PathParameters.TwistBegin = 0f;
        Parameters.PathParameters.TwistEnd = 0f;
        Parameters.PathParameters.RadiusOffset = 0f;
        Parameters.PathParameters.Taper = new Vector2(0f, 0f);
        Parameters.PathParameters.Revolutions = 1f;
        Parameters.PathParameters.Skew = 0f;
        switch (shape)
        {
            case PrimitiveShape.Sphere:
                //rotation.setQuat(90.f* DEG_TO_RAD, LLVector3::y_axis);
                Parameters.SetType (ProfileType.CircleHalf, HoleType.HoleSame, PathType.Circle);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (1f, 1f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.Torus:
                //	rotation.setQuat(90.f* DEG_TO_RAD, LLVector3::y_axis);
                Parameters.SetType (ProfileType.Circle, HoleType.HoleSame, PathType.Circle);
                Parameters.SetBeginAndEndS(0f, 1f);
                Parameters.SetBeginAndEndT(0f, 1f);
                Parameters.SetRatio(1f, 0.25f);
                Parameters.SetShear(0f, 0f);
                break;

            case PrimitiveShape.SquareTorus:
                //	rotation.setQuat(90.f* DEG_TO_RAD, LLVector3::y_axis);
                Parameters.SetType (ProfileType.Square, HoleType.HoleSame, PathType.Circle);
                Parameters.SetBeginAndEndS(0f, 1f);
                Parameters.SetBeginAndEndT(0f, 1f);
                Parameters.SetRatio(1f, 0.25f);
                Parameters.SetShear(0f, 0f);
                break;

            case PrimitiveShape.TriangleTorus:
                //	rotation.setQuat(90.f* DEG_TO_RAD, LLVector3::y_axis);
                Parameters.SetType (ProfileType.EqualTri, HoleType.HoleSame, PathType.Circle);
                Parameters.SetBeginAndEndS(0f, 1f);
                Parameters.SetBeginAndEndT(0f, 1f);
                Parameters.SetRatio(1f, 0.25f);
                Parameters.SetShear(0f, 0f);
                break;

            case PrimitiveShape.SphereHemi:
                Parameters.SetType (ProfileType.CircleHalf, HoleType.HoleSame, PathType.Circle);
                //Parameters.SetBeginAndEndS(0.5f, 1f);
                Parameters.SetBeginAndEndT(0f, 0.5f);
                Parameters.SetRatio(1f, 0.25f);
                Parameters.SetShear(0f, 0f);
                break;

            case PrimitiveShape.Cube:
                Parameters.SetType (ProfileType.Square, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (1f, 1f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.Prism:
                Parameters.SetType(ProfileType.Square, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (0f, 1f);
                Parameters.SetShear (-0.5f, 0);
                break;

            case PrimitiveShape.Pyramid:
                Parameters.SetType (ProfileType.Square, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (0f, 0f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.Tetrahedron:
                Parameters.SetType (ProfileType.EqualTri, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (0f, 0f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.Cylinder:
                Parameters.SetType(ProfileType.Circle, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (1f, 1f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.CylinderHemi:
                Parameters.SetType (ProfileType.Circle, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0.25f, 0.75f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (1f, 1f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.Cone:
                Parameters.SetType (ProfileType.Circle, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0f, 1f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (0f, 0f);
                Parameters.SetShear (0f, 0f);
                break;

            case PrimitiveShape.ConeHemi:
                Parameters.SetType (ProfileType.Circle, HoleType.HoleSame, PathType.Line);
                Parameters.SetBeginAndEndS (0.25f, 0.75f);
                Parameters.SetBeginAndEndT (0f, 1f);
                Parameters.SetRatio (0f, 0f);
                Parameters.SetShear (0f, 0f);
                break;
        }
    }
    
    public void GenerateMesh()
    {
        Parameters.Clamp();
        Generate();
        CreateVolumeFaces();

        Debug.Log($"Profile.Faces.Count: {Profile.Faces.Count}");
        Debug.Log($"VolumeFaces.Count: {VolumeFaces.Count}");
        Debug.Log($"FaceMask: {FaceMask}");

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Material> materials = new List<Material>();

        Mesh mesh = new Mesh();
        List<SubMeshDescriptor> subMeshes = new List<SubMeshDescriptor>();
        foreach (var face in VolumeFaces)
        {
            Vector3 size = face.ExtentsMax - face.ExtentsMin;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size.z = Mathf.Abs(size.z);
            subMeshes.Add (new SubMeshDescriptor
            {
                baseVertex = vertices.Count,
                bounds = new Bounds(face.Centre, size),
                firstVertex = vertices.Count,
                indexCount = face.Indices.Count,
                indexStart = triangles.Count,
                topology = MeshTopology.Triangles,
                vertexCount = face.Positions.Count
            });
            
            vertices.AddRange(face.Positions);
            normals.AddRange(face.Normals);
            triangles.AddRange(face.Indices);
            materials.Add (Materials[FaceIdToMaterialIndex[Profile.Faces[face.Id].FaceId]]);

            Debug.Log($"Id: {face.Id}, FaceId: {Profile.Faces[face.Id].FaceId}, TypeMask {face.TypeMask}, nVertices: {face.Positions.Count}, nNormals: {face.Normals.Count}, nIndices: {face.Indices.Count}");
            //SubMeshDescriptor sd = subMeshes[subMeshes.Count - 1];
            //Debug.Log($"bounds: {sd.bounds}, baseVertex: {sd.baseVertex}, firstVertex: {sd.firstVertex}, vertexCount: {sd.vertexCount}, indexStart: {sd.indexStart}, indexCount: {sd.indexCount}");
            string m;
            m = "";
            //foreach (Vector3 p in face.Positions)
            //{
            //    m += $"{p}, ";
            //}
            //Debug.Log($"Positions: {m}");
            //m = "";
            //foreach (int faceIndex in face.Indices)
            //{
            //    m += $"{faceIndex}, ";
            //}
            //Debug.Log($"Indices: {m}");
            //m = "";
            //foreach (int edgeIndex in face.Edge)
            //{
            //    m += $"{edgeIndex}, ";
            //}
            //Debug.Log($"Edge: {m}");
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.subMeshCount = subMeshes.Count;
        for (int i = 0; i < subMeshes.Count; i++)
        {
            mesh.SetSubMesh(i, subMeshes[i]);
        }

        MeshFilter.sharedMesh = mesh;
        MeshRenderer.materials = materials.ToArray();
    }

    protected bool Generate()
    {
        //Added 10.03.05 Dave Parks
        // Split is a parameter to LLProfile::generate that tesselates edges on the profile 
        // to prevent lighting and texture interpolation errors on triangles that are 
        // stretched due to twisting or scaling on the path.  
        int split = (int)(Lod * 0.66f);

        if (Parameters.PathParameters.PathType == PathType.Line
            && (Parameters.PathParameters.Scale.x != 1.0f || Parameters.PathParameters.Scale.y != 1.0f)
            && (   Parameters.ProfileParameters.ProfileType == ProfileType.Square
                || Parameters.ProfileParameters.ProfileType == ProfileType.IsoTri
                || Parameters.ProfileParameters.ProfileType == ProfileType.EqualTri
                || Parameters.ProfileParameters.ProfileType == ProfileType.RightTri))
        {
            split = 0;
        }

        LodScaleBias = new Vector3(0.5f, 0.5f, 0.5f);

        float profile_detail = Lod;
        float path_detail = Lod;

        if (Parameters.SculptType != SculptType.Mesh)
        {
            PathType path_type = Parameters.PathParameters.PathType;
            ProfileType profile_type = Parameters.ProfileParameters.ProfileType;
            if (path_type == PathType.Line && profile_type == ProfileType.Circle)
            {
                //cylinders don't care about Z-Axis
                LodScaleBias = new Vector3(0.6f, 0.6f, 0.0f);
            }
            else if (path_type == PathType.Circle)
            {
                LodScaleBias = new Vector3(0.6f, 0.6f, 0.6f);
            }
        }

        bool pathChanged = Path.Generate(Parameters.PathParameters, path_detail, split);
        bool profileChanged = Profile.Generate(Parameters.ProfileParameters, Path.IsOpen, profile_detail, split);
        if (pathChanged || profileChanged)
        {
            Points.Clear();

            //Generate vertex positions:

            // Apply rotation, scaling and position:
            for (int s = 0; s < Path.PointCount; s++)
            {
                //Compute matrix for this step on the path:
                //NOTE: The order of applying the matrices is different from original Indra code.
                Matrix4x4 matrix = Path.Points[s].Rotation;
                Vector2 scale = Path.Points[s].Scale;
                matrix *= new Matrix4x4 (new Vector4(scale.x, 0f,      0f, 0f),
                                         new Vector4(0f,      scale.y, 0f, 0f),
                                         new Vector4(0f,    0f,      0f, 0f),
                                         new Vector4(0f,    0f,      0f, 1f));
                Vector3 offset = Path.Points[s].Position;
                for (int t = 0; t < Profile.PointCount; t++)
                {
                    Points.Add (offset + matrix.MultiplyPoint3x4 (Profile.Points[t]));
                }
            }

            foreach (Profile.Face face in Profile.Faces)
            {
                FaceMask |= face.FaceId;
            }

            return true;
        }

        return false;
    }

    protected int GetNumFaces()
    {
        return IsMeshAssetLoaded ? NumVolumeFaces : Profile.Faces.Count;
    }

    protected void CreateVolumeFaces()
    {
        if (GenerateSingleFace)
        {
            // do nothing
            return;
        }

        int numFaces = GetNumFaces();
        bool partialBuild = true;
        //TODO: How does partial build work?
        //if (numFaces != VolumeFaces.Count)
        //{
            partialBuild = false;
            VolumeFaces.Clear();
        //}

        // Initialize volume faces with parameter data
        for (int i = 0; i < numFaces; i++)
        {
            VolumeFace vf = new VolumeFace();
            Profile.Face face = Profile.Faces[i];
            vf.BeginS = face.Index;
            vf.NumS = face.Count;
            if (vf.NumS < 0)
            {
                Debug.LogError("Volume face corruption detected.");
            }

            vf.BeginT = 0;
            vf.NumT = Path.PointCount;
            vf.Id = i;

            // Set the type mask bits correctly
            if (Parameters.ProfileParameters.Hollow > 0f)
            {
                vf.TypeMask |= VolumeFaceMask.Hollow;
            }
            if (Profile.IsOpen)
            {
                vf.TypeMask |= VolumeFaceMask.Open;
            }
            if (face.Cap)
            {
                vf.TypeMask |= VolumeFaceMask.Cap;
                if (face.FaceId == FaceId.PathBegin)
                {
                    vf.TypeMask |= VolumeFaceMask.Top;
                }
                else
                {
                    //llassert(face.mFaceID == LL_FACE_PATH_END);
                    vf.TypeMask |= VolumeFaceMask.Bottom;
                }
            }
            else if ((face.FaceId & (FaceId.ProfileBegin | FaceId.ProfileEnd)) != 0)
            {
                vf.TypeMask |= VolumeFaceMask.Flat | VolumeFaceMask.End;
            }
            else
            {
                vf.TypeMask |= VolumeFaceMask.Side;
                if (face.Flat)
                {
                    vf.TypeMask |= VolumeFaceMask.Flat;
                }
                if ((face.FaceId & FaceId.InnerSide) != 0)
                {
                    vf.TypeMask |= VolumeFaceMask.Inner;
                    if (face.Flat && vf.NumS > 2)
                    { //flat inner faces have to copy vert normals
                        vf.NumS = vf.NumS * 2;
                        if (vf.NumS < 0)
                        {
                            Debug.LogError("Volume face corruption detected.");
                        }
                    }
                }
                else
                {
                    vf.TypeMask |= VolumeFaceMask.Outer;
                }
            }
            VolumeFaces.Add (vf);
        }

        foreach (VolumeFace vf in VolumeFaces)
        {
            vf.Create (this, partialBuild);
        }
    }

    // Less restricitve approx 0 for volumes
    protected static readonly float APPROXIMATELY_ZERO = 0.001f;
    public static bool ApproxZero (float f, float tolerance = -1f)
    {
        if (tolerance < 0f)
        {
            tolerance = APPROXIMATELY_ZERO;
        }
        return (f >= -tolerance) && (f <= tolerance);
    }

    // return true if in range (or nearly so)
    public static bool LimitRange (ref float v, float min, float max, float tolerance = -1f)
    {
        if (tolerance < 0f)
        {
            tolerance = APPROXIMATELY_ZERO;
        }

        float minDelta = v - min;
        if (minDelta < 0f)
        {
            v = min;
            if (!ApproxZero (minDelta, tolerance))
            {
                return false;
            }
        }

        float maxDelta = max - v;
        if (maxDelta < 0f)
        {
            v = max;
            if (!ApproxZero (maxDelta, tolerance))
            {
                return false;
            }
        }
        return true;
    }


}