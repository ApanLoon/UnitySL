
using System.Linq;
using Assets.Scripts.Appearance;
using UnityEngine;

namespace Assets.Scripts.Primitives
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PolyMeshTest : MonoBehaviour
    {
        public string FileName;
        protected MeshFilter MeshFilter;
        protected MeshRenderer MeshRenderer;
        protected PolyMesh Mesh;

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            Mesh = PolyMesh.LoadMesh(System.IO.Path.Combine(Application.streamingAssetsPath, "Character", FileName));
            GenerateMesh();
        }

        protected void GenerateMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = Mesh.SharedData.BaseCoords,
                normals = Mesh.SharedData.BaseNormals,
                triangles = Mesh.SharedData.Faces.Select(x => new int[] {x.A, x.B, x.C}).SelectMany(x => x).ToArray()
            };


            //mesh.subMeshCount = subMeshes.Count;
            //for (int i = 0; i < subMeshes.Count; i++)
            //{
            //    mesh.SetSubMesh(i, subMeshes[i]);
            //}

            MeshFilter.sharedMesh = mesh;
            //MeshRenderer.materials = materials.ToArray();
        }
    }
}
