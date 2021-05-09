
using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class PolyMesh
    {
        protected const string ExpectedBinaryIdentifier = "Linden Binary Mesh 1.0";

        public bool IsLod { get; protected set; } = false;
        public PolyMeshSharedData SharedData { get; protected set; }

        public static PolyMesh LoadMesh(string fileName, bool isLod = false)
        {
            // Open the file:
            if (string.IsNullOrEmpty(fileName))
            {
                Logger.LogError("PolyMesh.LoadMesh", "Filename is empty!");
                return null;
            }

            try
            {
                byte[] buffer = File.ReadAllBytes(fileName);
                int length = buffer.Length;
                int o = 0;

                return DeSerializePolyMesh(buffer, ref o, length, isLod);
            }
            catch (Exception e)
            {
                Logger.LogError("PolyMesh.LoadMesh", e.Message + $" In {fileName}.");
                return null;
            }
        }

        public static PolyMesh DeSerializePolyMesh(byte[] buffer, ref int o, int length, bool isLod)
        {
            if (length < 128)
            {
                throw new Exception("Not enough bytes in file for header.");
            }

            PolyMesh mesh = new PolyMesh {IsLod = isLod, SharedData = new PolyMeshSharedData()};

            string identifier = BinarySerializer.DeSerializeString(buffer, ref o, 24, -1);
            if (identifier.Trim() != ExpectedBinaryIdentifier)
            {
                throw new Exception($"Invalid mesh file header.");
            }

            string logMessage = "";

            mesh.SharedData.HasWeights          = BinarySerializer.DeSerializeBool(buffer, ref o, length);
            mesh.SharedData.HasDetailTexCoords = BinarySerializer.DeSerializeBool(buffer, ref o, length);
            mesh.SharedData.Position            = BinarySerializer.DeSerializeVector3(buffer, ref o, length);

            Vector3 rotationAngles              = BinarySerializer.DeSerializeVector3(buffer, ref o, length);
            byte rotationOrder                  = BinarySerializer.DeSerializeUInt8(buffer, ref o, length);
            rotationOrder = 0; // NOTE: This is what the LL code does
            mesh.SharedData.Rotation = Quaternion.Euler(rotationAngles);

            mesh.SharedData.Scale               = BinarySerializer.DeSerializeVector3(buffer, ref o, length);

            logMessage = $"\n"
                         + $"    identifier:         \"{identifier}\"\n"
                         + $"    hasWeights:         {mesh.SharedData.HasWeights}\n"
                         + $"    hasDetailTexCoords: {mesh.SharedData.HasDetailTexCoords}\n"
                         + $"    position:           {mesh.SharedData.Position}\n"
                         + $"    rotationAngles:     {rotationAngles}\n"
                         + $"    rotationOrder:      {rotationOrder}\n"
                         + $"    scale:              {mesh.SharedData.Scale}\n"
                ;

            if (isLod == false)
            {
                int numVertices = BinarySerializer.DeSerializeUInt16_Le(buffer, ref o, length);
                logMessage += $"    nVertices:          0x{numVertices:x4}\n";
                mesh.SharedData.AllocateVertexData(numVertices);

                #region Vertices
                if (length - o < mesh.SharedData.NumVertices * 12)
                {
                    throw new Exception($"Not enough bytes to read vertices.");
                }

                for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                {
                    mesh.SharedData.BaseCoords[i] = BinarySerializer.DeSerializeVector3(buffer, ref o, length);
                }
                #endregion Vertices
                #region Normals
                if (length - o < mesh.SharedData.NumVertices * 12)
                {
                    throw new Exception($"Not enough bytes to read normals.");
                }

                for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                {
                    mesh.SharedData.BaseNormals[i] = BinarySerializer.DeSerializeVector3(buffer, ref o, length);
                }
                #endregion Normals
                #region BiNormals
                if (length - o < mesh.SharedData.NumVertices * 12)
                {
                    throw new Exception($"Not enough bytes to read bi-normals.");
                }

                for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                {
                    mesh.SharedData.BaseBiNormals[i] = BinarySerializer.DeSerializeVector3(buffer, ref o, length);
                }
                #endregion BiNormals
                #region TexCoords
                if (length - o < mesh.SharedData.NumVertices * 8)
                {
                    throw new Exception($"Not enough bytes to read tex-coords.");
                }

                for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                {
                    mesh.SharedData.TexCoords[i] = BinarySerializer.DeSerializeVector2(buffer, ref o, length);
                }
                #endregion TexCoords
                #region DetailedTexCoords
                if (mesh.SharedData.HasDetailTexCoords)
                {
                    if (length - o < mesh.SharedData.NumVertices * 8)
                    {
                        throw new Exception($"Not enough bytes to read detailed tex-coords.");
                    }

                    for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                    {
                        mesh.SharedData.DetailTexCoords[i] = BinarySerializer.DeSerializeVector2(buffer, ref o, length);
                    }
                }
                #endregion DetailedTexCoords
                #region Weights
                if (mesh.SharedData.HasWeights)
                {
                    if (length - o < mesh.SharedData.NumVertices * 4)
                    {
                        throw new Exception($"Not enough bytes to read weights.");
                    }

                    for (int i = 0; i < mesh.SharedData.NumVertices; i++)
                    {
                        mesh.SharedData.Weights[i] = BinarySerializer.DeSerializeFloat_Le(buffer, ref o, length);
                    }
                }
                #endregion Weights
            }

            #region Faces
            UInt16 nFaces = BinarySerializer.DeSerializeUInt16_Le(buffer, ref o, length);
            logMessage += $"    nFaces:               0x{nFaces:x4}\n";
            mesh.SharedData.AllocateFaceData(nFaces);

            int numTris = 0;
            if (length - o < nFaces * 3 * 2)
            {
                throw new Exception($"Not enough bytes to read faces.");
            }

            for (int i = 0; i < nFaces; i++)
            {
                Int16 a = BinarySerializer.DeSerializeInt16_Le(buffer, ref o, length);
                Int16 b = BinarySerializer.DeSerializeInt16_Le(buffer, ref o, length);
                Int16 c = BinarySerializer.DeSerializeInt16_Le(buffer, ref o, length);
                if (mesh.SharedData.ReferenceData != null && 
                    (
                       a >= mesh.SharedData.ReferenceData.NumVertices
                    || b >= mesh.SharedData.ReferenceData.NumVertices
                    || c >= mesh.SharedData.ReferenceData.NumVertices
                    ))
                {
                    throw new Exception("DeSerializePolyMesh: Face index is out of range of the reference mesh.");
                }

                PolyFace face = new PolyFace(a, c, b); // Swizzled order for Unity
                if (isLod)
                {
                    // Store largest index in case of LODs
                    for (int j = 0; j < 3; j++)
                    {
                        if (face[j] > mesh.SharedData.NumVertices - 1)
                        {
                            mesh.SharedData.NumVertices = face[j] + 1;
                        }
                    }
                }

                mesh.SharedData.Faces[i] = face;
                numTris++;
            }

            logMessage += $"    NumTriangles: {numTris}\n";
            #endregion Faces

            if (isLod == false)
            {
                UInt16 nSkinJoints = 0;
                if (mesh.SharedData.HasWeights)
                {
                    nSkinJoints = BinarySerializer.DeSerializeUInt16_Le(buffer, ref o, length);
                    logMessage += $"    nSkinJoints:        0x{nSkinJoints:x4}\n";
                    mesh.SharedData.AllocateJointNames(nSkinJoints);
                }

                #region SkinJoints
                for (int i = 0; i < nSkinJoints; i++)
                {
                    mesh.SharedData.JointNames[i] = BinarySerializer.DeSerializeString(buffer, ref o, 64, -1);
                    logMessage += $"        jointName: {mesh.SharedData.JointNames[i]}\n";
                }
                #endregion SkinJoints
                #region MorphSections
                while (true)
                {
                    string morphName = BinarySerializer.DeSerializeString(buffer, ref o, 64, -1);
                    if (morphName == "End Morphs")
                    {
                        break;
                    }

                    PolyMorphData morphData = DeSerializePolyMorphData(buffer, ref o, length);
                    morphData.Name = morphName;
                    logMessage += $"        Morph name: {morphName}\n";

                    mesh.SharedData.MorphData.Add(morphData);

                    // Insert jiggle physics morphs:
                    switch (morphName)
                    {
                        case "Breast_Female_Cleavage":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamCleavage (morphData, 0.75f, "Breast_Physics_LeftRight_Driven"));
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDuplicate(morphData,        "Breast_Physics_InOut_Driven"));
                            break;

                        case "Breast_Gravity":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDuplicate(morphData, "Breast_Physics_UpDown_Driven"));
                            break;

                        case "Big_Belly_Torso":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDirection(morphData, new Vector3(0f, 0.05f, 0f), "Belly_Physics_Torso_UpDown_Driven"));
                            break;

                        case "Big_Belly_Legs":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDirection(morphData, new Vector3(0f, 0.05f, 0f), "Belly_Physics_Legs_UpDown_Driven"));
                            break;

                        case "skirt_belly":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDirection(morphData, new Vector3(0f, 0.05f, 0f), "Belly_Physics_Skirt_UpDown_Driven"));
                            break;

                        case "Small_Butt":
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDirection(morphData, new Vector3(0f, 0.05f, 0f), "Butt_Physics_UpDown_Driven"));
                            //mesh.SharedData.MorphData.Add(CloneMorphParamDirection(morphData, new Vector3(0f, 0.03f, 0f), "Butt_Physics_LeftRight_Driven"));
                            break;

                    }
                }
                #endregion MorphSections
                #region Remaps
                Int32 nRemaps = BinarySerializer.DeSerializeInt32_Le(buffer, ref o, length);
                for (int i = 0; i < nRemaps; i++)
                {
                    Int32 src = BinarySerializer.DeSerializeInt32_Le(buffer, ref o, length);
                    Int32 dst = BinarySerializer.DeSerializeInt32_Le(buffer, ref o, length);
                    mesh.SharedData.SharedVertices[src] = dst;
                }
                #endregion Remaps
            }

            if (mesh.SharedData.NumJointNames == 0)
            {
                mesh.SharedData.AllocateJointNames(1);
            }

            logMessage += $"{length - o} bytes left in buffer.";
            Logger.LogDebug("PolyMesh.DeSerializePolyMesh", logMessage);

            return mesh;
        }

        public static PolyMorphData DeSerializePolyMorphData(byte[] buffer, ref int offset, int length)
        {
            PolyMorphData data = new PolyMorphData();

            Int32 nVertices = BinarySerializer.DeSerializeInt32_Le(buffer, ref offset, length);

            for (int i = 0; i < nVertices; i++)
            {
                UInt32 vertexIndex = BinarySerializer.DeSerializeUInt32_Le(buffer, ref offset, length);
                if (vertexIndex > 10000)
                {
                    throw new Exception($"Bad morph index: {vertexIndex}");
                }

                Vector3 coordinate = BinarySerializer.DeSerializeVector3(buffer, ref offset, length);
                Vector3 normal = BinarySerializer.DeSerializeVector3(buffer, ref offset, length);
                Vector3 biNormal = BinarySerializer.DeSerializeVector3(buffer, ref offset, length);
                Vector2 texCoord = BinarySerializer.DeSerializeVector2(buffer, ref offset, length);
            }

            return data;
        }
    }
}
