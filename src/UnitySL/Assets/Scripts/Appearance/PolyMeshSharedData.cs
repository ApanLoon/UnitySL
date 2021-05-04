using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class PolyMeshSharedData
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public int NumVertices { get; set; }
        public Vector3[] BaseCoords { get; set; }
        public Vector3[] BaseNormals { get; set; }
        public Vector3[] BaseBiNormals { get; set; }
        public Vector2[] TexCoords { get; set; }
        public Vector2[] DetailTexCoords { get; set; }
        public float[] Weights { get; set; }

        public bool HasWeights { get; set; }
        public bool HasDetailTexCoords { get; set; }

        public int NumFaces { get; set; }
        public PolyFace[] Faces { get; set; }

        public int NumJointNames { get; set; }
        public string[] JointNames { get; set; }

        public HashSet<PolyMorphData> MorphData { get; set; } = new HashSet<PolyMorphData>();

        public Dictionary<int, int> SharedVertices { get; set; } = new Dictionary<int, int>();

        public PolyMeshSharedData ReferenceData { get; set; }

        public int LastIndexOffset { get; set; }

        public int NumTriangleIndices { get; set; }
        public int[] TriangleIndices { get; set; }

        public void AllocateVertexData(int numVertices)
        {
            NumVertices = numVertices;
            BaseCoords      = new Vector3[NumVertices];
            BaseNormals     = new Vector3[NumVertices];
            BaseBiNormals   = new Vector3[NumVertices];
            TexCoords       = new Vector2[NumVertices];
            DetailTexCoords = new Vector2[NumVertices];
            Weights         = new float[NumVertices];
        }

        public void AllocateFaceData(int numFaces)
        {
            NumFaces = numFaces;
            Faces = new PolyFace[numFaces];
            NumTriangleIndices = NumFaces * 3;
        }

        public void AllocateJointNames(int numJointNames)
        {
            NumJointNames = numJointNames;
            JointNames = new string[NumJointNames];
        }
    }
}
