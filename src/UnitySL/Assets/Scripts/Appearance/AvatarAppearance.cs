using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets.Scripts.Characters;
using Assets.Scripts.Common;
using Assets.Scripts.Extensions.SystemExtensions;
using Assets.Scripts.Types;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class AvatarAppearance : Character
    {
        protected const string AVATAR_DEFAULT_CHAR = "avatar";
        protected static readonly Color DUMMY_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        #region Initialisation
        public AvatarAppearance(WearableData wearableData) : base() // Note: indra has a protected constructor since it subclasses this in VOAvatar
        {
            IsDummy = false;
            TexSkinColor = null; //TODO: Not implemented
            TexHairColor = null; //TODO: Not implemented
            TexEyeColor = null; //TODO: Not implemented
            PelvisToFoot = 0f;
            HeadOffset = Vector3.zero;
            Root = null;
            WearableData = wearableData;
            NumBones = 0;
            NumCollisionVolumes = 0;
            IsBuilt = false;
            InitFlags = 0;

            if (WearableData == null)
            {
                throw new Exception("AvatarAppearance.constructor: Can't create an avatar with a null wearableData.");
            }

            for (int i = 0; i < (int)AvatarAppearanceDictionary.BakedTextureIndex.NumIndices; i++)
            {
                BakedTextureDatas.Add(new BakedTextureData()
                {
                    LastTextureID = IndraConstants.IMG_DEFAULT_AVATAR,
                    TexLayerSet   = null,
                    IsLoaded      = false,
                    IsUsed        = false,
                    MaskTexName   = 0,
                    TextureIndex  = AvatarAppearanceDictionary.BakedToLocalTextureIndex((AvatarAppearanceDictionary.BakedTextureIndex)i)
                });
            }
        }

        public static void InitClass()
        {
            InitClass("", "");
        }

        public static void InitClass(string avatarFilename, string skeletonFilename)
        {
            if (string.IsNullOrEmpty(avatarFilename))
            {
                avatarFilename = $"{AVATAR_DEFAULT_CHAR}_lad.xml";
            }
            avatarFilename = System.IO.Path.Combine(Application.streamingAssetsPath, "Character", avatarFilename);
            AvatarConfigurationXmlTree = OpenAvatarXmlFile(avatarFilename, "linden_avatar", new[] { "1.0", "2.0" });

            // TODO: Extract the "wearable_definition_version" attribute of the root node and set it in the Wearable class

            if (string.IsNullOrEmpty(skeletonFilename))
            {
                skeletonFilename = "avatar_skeleton.xml";
            }
            skeletonFilename = System.IO.Path.Combine(Application.streamingAssetsPath, "Character", skeletonFilename);
            SkeletonXmlTree = OpenAvatarXmlFile(skeletonFilename, "linden_skeleton", new[] { "1.0", "2.0" });

            try
            {
                AvatarSkeletonInfo = null;
                AvatarSkeletonInfo = AvatarSkeletonInfo.ParseXml(SkeletonXmlTree.GetFirstChild());
            }
            catch (Exception e)
            {
                Logger.LogError("AvatarAppearance.InitClass", $"Error parsing skeleton file: {skeletonFilename} ({e})");
            }

            try
            {
                AvatarXmlInfo = null;
                AvatarXmlInfo = AvatarXmlInfo.ParseXml(AvatarConfigurationXmlTree.GetFirstChild());
            }
            catch (Exception e)
            {
                Logger.LogError("AvatarAppearance.InitClass", $"Error parsing avatar file: {avatarFilename} ({e})");
            }
        }

        /// <summary>
        /// Called after construction to initialise the instance.
        /// </summary>
        public virtual void InitInstance()
        {
            // Initialise joint, mesh and shape members

            Root = CreateAvatarJoint();
            Root.Name = "mRoot";

            foreach (var keyValuePair in AvatarAppearanceDictionary.Instance.MeshEntries)
            {
                AvatarAppearanceDictionary.MeshIndex meshIndex = keyValuePair.Key;
                AvatarAppearanceDictionary.MeshEntry meshEntry = keyValuePair.Value;

                AvatarJoint joint = CreateAvatarJoint();
                joint.Name = meshEntry.Name;
                joint.MeshId = (int)meshIndex;
                MeshLod.Add(joint);

                for (int lod = 0; lod < meshEntry.Lod; lod++)
                {
                    // TODO: Stopped here!
                    AvatarJointMesh mesh = CreateAvatarJointMesh();

                    //// We pre-pended an m - need to capitalise first character for camelCase
                    char first = Char.ToUpper(meshEntry.Name[0]);
                    string meshName = $"m{first}{meshEntry.Name.Substring(1)}{lod}"; // "m" + mesh_dict->mName + boost::lexical_cast < std::string> (lod); // TODO: What is lexical_cast()?
                    mesh.Name = meshName;
                    mesh.MeshId = (int)meshIndex;
                    //mesh->setPickName(mesh_dict->mPickName);
                    mesh.IsTransparent = false;
                    switch (meshIndex)
                    {
                        case AvatarAppearanceDictionary.MeshIndex.Hair:
                            mesh.IsTransparent = true;
                            break;

                        case AvatarAppearanceDictionary.MeshIndex.Skirt:
                            mesh.IsTransparent = true;
                            break;

                        case AvatarAppearanceDictionary.MeshIndex.EyeBallLeft:
                        case AvatarAppearanceDictionary.MeshIndex.EyeBallRight:
                            mesh.SetSpecular (new Color(1.0f, 1.0f, 1.0f, 1.0f), 1f);
                            break;
                    }

                    joint.MeshParts.Add(mesh);
                }
            }

            // Associate baked textures with meshes
            foreach (KeyValuePair<AvatarAppearanceDictionary.MeshIndex, AvatarAppearanceDictionary.MeshEntry> keyValuePair in AvatarAppearanceDictionary.Instance.MeshEntries)
            {
                AvatarAppearanceDictionary.MeshIndex meshIndex = keyValuePair.Key;
                AvatarAppearanceDictionary.MeshEntry meshEntry = keyValuePair.Value;
                AvatarAppearanceDictionary.BakedTextureIndex bakedTextureIndex = meshEntry.BakedTextureIndex;

                // Skip it if there's no associated baked texture.
                if (bakedTextureIndex == AvatarAppearanceDictionary.BakedTextureIndex.NumIndices)
                {
                    continue;
                }

                foreach (AvatarJointMesh mesh in MeshLod[(int)meshIndex].MeshParts)
                {
                    BakedTextureDatas[(int)bakedTextureIndex].JointMeshes.Add(mesh);
                }
            }
            
            BuildCharacter();

            InitFlags |= 1 << 0;
        }

        protected AvatarJoint CreateAvatarJoint()
        {
            return new AvatarJoint(); // NOTE: indra has LLViewerJoint here in the VOAvatar subclass
        }

        protected AvatarJointMesh CreateAvatarJointMesh()
        {
            return new AvatarJointMesh(); // NOTE: indra has ViewerJointMesh here in the VOAvatar subclass
        }

        public static int InitFlags { get; set; }

        #endregion Initialisation

        #region Inherited
        #endregion Inherited

        #region State
        public bool IsSelf { get; protected set; } = false; // NOTE: In indra this is an virtual method where this returns false
        public bool IsValid { get; protected set; } = false;
        public bool IsUsingLocalAppearance { get; protected set; } = false; // NOTE: In indra this is an abstract method
        public bool IsEditingAppearance { get; protected set; } = false; // NOTE: In indra this is an abstract method

        public bool IsBuilt { get; protected set; } = false;
        #endregion State

        #region Skeleton
        public Vector3 HeadOffset { get; set; }
        public AvatarJoint Root { get; set; }
        public Dictionary<string, LLJoint> JointMap { get; protected set; } = new Dictionary<string, LLJoint>();
        public Dictionary<string, Vector3> LastBodySizeState { get; protected set; } = new Dictionary<string, Vector3>();
        public Dictionary<string, Vector3> CurrentBodySizeState { get; protected set; } = new Dictionary<string, Vector3>();
        protected List<AvatarJoint> Skeleton { get; set; } = new List<AvatarJoint>();
        protected Vector3OverrideMap PelvisFixups { get; set; }
        protected Dictionary<string, string> JointAliasMap { get; set; } = new Dictionary<string, string>();

        protected static bool ParseSkeletonFile(string fileName)
        {
            // TODO: Did I not implement this already? Is it somewhere else?
            return false;
        }

        protected void BuildCharacter() // NOTE: In indra, this is virtual
        {
            //-------------------------------------------------------------------------
            // remove all references to our existing skeleton
            // so we can rebuild it
            //-------------------------------------------------------------------------
            // FlushAllMotions(); // TODO: Should this be in the Character that this AvatarAppearance is connected to? Or will be...

            //-------------------------------------------------------------------------
            // remove all of mRoot's children
            //-------------------------------------------------------------------------
            Root.RemoveAllChildren();
            JointMap.Clear();
            IsBuilt = false;

            //-------------------------------------------------------------------------
            // clear mesh data
            //-------------------------------------------------------------------------
            foreach (AvatarJoint avatarJoint in MeshLod)
            {
                foreach (AvatarJointMesh avatarJointMesh in avatarJoint.MeshParts)
                {
                    avatarJointMesh.SetMesh(null);
                }
            }

            //-------------------------------------------------------------------------
            // (re)load our skeleton and meshes
            //-------------------------------------------------------------------------

            bool status = LoadAvatar(); // TODO: Indra times this and logs the time it took to load the avatar
            Logger.LogDebug("AvatarAppearance.BuildCharacter", "Avatar loaded.");

            if (status == false)
            {
                if (IsSelf)
                {
                    Logger.LogError("AvatarAppearance.BuildCharacter", "Unable to load user's avatar");
                }
                else
                {
                    Logger.LogWarning("AvatarAppearance.BuildCharacter", "Unable to load other's avatar");
                }
                return;
            }

            //-------------------------------------------------------------------------
            // initialize "well known" joint pointers
            //-------------------------------------------------------------------------
            Pelvis     = Root.FindJoint("mPelvis");
            Torso      = Root.FindJoint("mTorso");
            Chest      = Root.FindJoint("mChest");
            Neck       = Root.FindJoint("mNeck");
            Head       = Root.FindJoint("mHead");
            Skull      = Root.FindJoint("mSkull");
            HipLeft    = Root.FindJoint("mHipLeft");
            HipRight   = Root.FindJoint("mHipRight");
            KneeLeft   = Root.FindJoint("mKneeLeft");
            KneeRight  = Root.FindJoint("mKneeRight");
            AnkleLeft  = Root.FindJoint("mAnkleLeft");
            AnkleRight = Root.FindJoint("mAnkleRight");
            FootLeft   = Root.FindJoint("mFootLeft");
            FootRight  = Root.FindJoint("mFootRight");
            WristLeft  = Root.FindJoint("mWristLeft");
            WristRight = Root.FindJoint("mWristRight");
            EyeLeft    = Root.FindJoint("mEyeLeft");
            EyeRight   = Root.FindJoint("mEyeRight");

            //-------------------------------------------------------------------------
            // Make sure "well known" pointers exist
            //-------------------------------------------------------------------------
            if (!(   Pelvis     != null
                  && Torso      != null
                  && Chest      != null
                  && Neck       != null
                  && Head       != null
                  && Skull      != null
                  && HipLeft    != null
                  && HipRight   != null
                  && KneeLeft   != null
                  && KneeRight  != null
                  && AnkleLeft  != null
                  && AnkleRight != null
                  && FootLeft   != null
                  && FootRight  != null
                  && WristLeft  != null
                  && WristRight != null
                  && EyeLeft    != null
                  && EyeRight   != null))
            {
                Logger.LogError("AvatarAppearance.BuildCharacter", "Failed to create avatar.");
                return;
            }

            //-------------------------------------------------------------------------
            // Initialise the pelvis
            //-------------------------------------------------------------------------
            // SL-315
            Pelvis.SetPosition (new Vector3 (0.0f, 0.0f, 0.0f));

            IsBuilt = true;
        }

        protected virtual bool LoadAvatar()
        {
            Logger.LogError("AvatarAppearance.LoadAvatar", "Not implemented!"); //TODO: Not implemented
            //// avatar_skeleton.xml
            //if (!BuildSkeleton (AvatarSkeletonInfo))
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: buildSkeleton() failed");
            //    return false;
            //}

            //// avatar_lad.xml : <skeleton>
            //if (!LoadSkeletonNode())
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: loadNodeSkeleton() failed");
            //    return false;
            //}

            //// avatar_lad.xml : <mesh>
            //if (!LoadMeshNodes())
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: loadNodeMesh() failed");
            //    return false;
            //}

            //// avatar_lad.xml : <global_color>
            //if (AvatarXmlInfo.TexSkinColorInfo != null)
            //{
            //    TexSkinColor = new TexGlobalColor(this);
            //    if (!TexSkinColor.SetInfo(AvatarXmlInfo.TexSkinColorInfo))
            //    {
            //        Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: mTexSkinColor->setInfo() failed");
            //        return false;
            //    }
            //}
            //else
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "<global_color> name=\"skin_color\" not found");
            //    return false;
            //}
            //if (AvatarXmlInfo.TexHairColorInfo)
            //{
            //    TexHairColor = new TexGlobalColor(this);
            //    if (!TexHairColor.SetInfo(AvatarXmlInfo.TexHairColorInfo))
            //    {
            //        Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: mTexHairColor->setInfo() failed");
            //        return false;
            //    }
            //}
            //else
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "<global_color> name=\"hair_color\" not found");
            //    return false;
            //}
            //if (AvatarXmlInfo.TexEyeColorInfo != null)
            //{
            //    TexEyeColor = new TexGlobalColor(this);
            //    if (!TexEyeColor.SetInfo(AvatarXmlInfo.TexEyeColorInfo))
            //    {
            //        Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: mTexEyeColor->setInfo() failed");
            //        return false;
            //    }
            //}
            //else
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "<global_color> name=\"eye_color\" not found");
            //    return false;
            //}

            //// avatar_lad.xml : <layer_set>
            //if (AvatarXmlInfo.LayerInfoList.Count == 0)
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: missing <layer_set> node");
            //    return false;
            //}

            //if (AvatarXmlInfo.MorphMaskInfoList.Count == 0)
            //{
            //    Logger.LogError("AvatarAppearance.LoadAvatar", "avatar file: missing <morph_masks> node");
            //    return false;
            //}

            //// avatar_lad.xml : <morph_masks>
            //foreach (var info in AvatarXmlInfo.MorphMaskInfoList)
            //{
            //    AvatarAppearanceDictionary.BakedTextureIndex baked = AvatarAppearanceDictionary.FindBakedByRegionName(info.Region);
            //    if (baked != AvatarAppearanceDictionary.BakedTextureIndex.NumIndices)
            //    {
            //        VisualParameter morph_param = GetVisualParam(info.Name);
            //        if (morph_param != null)
            //        {
            //            bool invert = info.Invert;
            //            AddMaskedMorph(baked, morph_param, invert, info.Layer);
            //        }
            //    }
            //}

            //LoadLayersets();

            // avatar_lad.xml : <driver_parameters>
            // TODO: driver_parameters are ignored for now
            //foreach (var info in AvatarXmlInfo.DriverInfoList)
            //{
            //    DriverParam driver_param = new DriverParam(this);
            //    if (driver_param.SetInfo(info))
            //    {
            //        AddVisualParam(driver_param);
            //        driver_param.SetParamLocation(IsSelf() ? LOC_AV_SELF : LOC_AV_OTHER);
            //        VisualParameter (LLAvatarAppearance::* avatar_function)(S32)const = &LLAvatarAppearance::getVisualParam;
            //        if (!driver_param->linkDrivenParams(boost::bind(avatar_function, (LLAvatarAppearance*)this, _1), false))
            //        {
            //            LL_WARNS() << "could not link driven params for avatar " << getID().asString() << " param id: " << driver_param->getID() << LL_ENDL;
            //            continue;
            //        }
            //    }
            //    else
            //    {
            //        delete driver_param;
            //        LL_WARNS() << "avatar file: driver_param->parseData() failed" << LL_ENDL;
            //        return false;
            //    }
            //}

            return true;
        }

        public Vector3 BodySize { get; set; }
        public Vector3 AvatarOffset { get; set; }

        public float PelvisToFoot { get; protected set; }

        public LLJoint Pelvis { get; set; }
        public LLJoint Torso { get; set; }
        public LLJoint Chest { get; set; }
        public LLJoint Neck { get; set; }
        public LLJoint Head { get; set; }
        public LLJoint Skull { get; set; }
        public LLJoint EyeLeft { get; set; }
        public LLJoint EyeRight { get; set; }
        public LLJoint HipLeft { get; set; }
        public LLJoint HipRight { get; set; }
        public LLJoint KneeLeft { get; set; }
        public LLJoint KneeRight { get; set; }
        public LLJoint AnkleLeft { get; set; }
        public LLJoint AnkleRight { get; set; }
        public LLJoint FootLeft { get; set; }
        public LLJoint FootRight { get; set; }
        public LLJoint WristLeft { get; set; }
        public LLJoint WristRight { get; set; }

        protected static XmlDocument AvatarConfigurationXmlTree;
        protected static XmlDocument SkeletonXmlTree;
        public static AvatarSkeletonInfo AvatarSkeletonInfo { get; protected set; }
        public static AvatarXmlInfo AvatarXmlInfo { get; protected set; }
        #endregion Skeleton

        #region Rendering
        public bool IsDummy { get; protected set; } = false;
        #endregion Rendering

        #region Meshes
        protected MultiMap<string, PolyMesh> PolyMeshes { get; set; } = new MultiMap<string, PolyMesh>();
        protected List<AvatarJoint> MeshLod { get; set; } = new List<AvatarJoint>();
        #endregion Meshes

        #region Appearance
        protected TexGlobalColor TexSkinColor { get; set; }
        protected TexGlobalColor TexHairColor { get; set; }
        protected TexGlobalColor TexEyeColor { get; set; }
        #endregion Appearance

        #region Wearables
        protected WearableData WearableData { get; set; }
        #endregion Wearables

        #region BakedTextures

        public TexLayerSet GetAvatarLayerSet(AvatarAppearanceDictionary.BakedTextureIndex bakedIndex)
        {
            return BakedTextureDatas[(int)bakedIndex].TexLayerSet;
        }

        protected TexLayerSet CreateTextLayerSet()
        {
            return new TexLayerSet(this); // NOTE: indra has LLViewerTexLayerSet here in the VOAvatar subclass
        }

        protected class BakedTextureData
        {
            public Guid LastTextureID;
            public TexLayerSet TexLayerSet; // Only exists for self
            public bool IsLoaded;
            public bool IsUsed;
            public AvatarAppearanceDictionary.TextureIndex TextureIndex;
            public UInt32 MaskTexName;
            
            /// <summary>
            /// Stores pointers to the joint meshes that this baked texture deals with
            /// </summary>
            public List<AvatarJointMesh> JointMeshes = new List<AvatarJointMesh>();
            
            public List<MaskedMorph> MaskedMorphs = new List<MaskedMorph>();
        };
        protected List<BakedTextureData> BakedTextureDatas { get; } = new List<BakedTextureData>();
        #endregion BakedTextures

        #region Physics
        public int NumBones { get; set; }
        public int NumCollisionVolumes { get; set; }
        #endregion Physics

        /// <summary>
        /// Opens an XML file and checks the root node and version.
        /// </summary>
        /// <param name="filename">Path to the XML file to open</param>
        /// <param name="rootName">Expected name of the root node</param>
        /// <param name="versions">Acceptable version values</param>
        /// <returns>A parsed XmlDocument or null if errors occurred</returns>
        public static XmlDocument OpenAvatarXmlFile(string filename, string rootName, string[] versions)
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(filename);
            }
            catch (Exception e)
            {
                Logger.LogError("AvatarAppearance.OpenAvatarXmlFile", $"Can't parse {rootName} file: {filename}");
                return null;
            }

            // Sanity-check the XML file:
            XmlNode root = doc.GetFirstChild();
            if (root == null)
            {
                Logger.LogError("AvatarAppearance.OpenAvatarXmlFile", $"No root node found in {rootName} file: {filename}");
                return null;
            }

            if (root.Name != rootName || root.NodeType != XmlNodeType.Element)
            {
                Logger.LogError("AvatarAppearance.OpenAvatarXmlFile", $"Invalid root node in {rootName} file: {filename}");
                return null;
            }

            XmlElement rootElement = (XmlElement)root;
            string version = rootElement.GetAttribute("version");
            if (version == string.Empty || !versions.Contains(version))
            {
                Logger.LogError("AvatarAppearance.OpenAvatarXmlFile", $"Invalid version in {rootName} file: {filename}");
                return null;
            }

            return doc;
        }


        public override string GetAnimationPrefix()
        {
            throw new NotImplementedException();
        }

        public override LLJoint GetRootJoint()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetCharacterPosition()
        {
            throw new NotImplementedException();
        }

        public override Quaternion GetCharacterRotation()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetCharacterVelocity()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetCharacterAngularVelocity()
        {
            throw new NotImplementedException();
        }

        public override void GetGround(Vector3 inPos, out Vector3 outPos, out Vector3 outNorm)
        {
            throw new NotImplementedException();
        }

        public override LLJoint GetCharacterJoint(uint i)
        {
            throw new NotImplementedException();
        }

        public override float GetTimeDilation()
        {
            throw new NotImplementedException();
        }

        public override float GetPixelArea()
        {
            throw new NotImplementedException();
        }

        public override PolyMesh GetHeadMesh()
        {
            throw new NotImplementedException();
        }

        public override PolyMesh GetUpperBodyMesh()
        {
            throw new NotImplementedException();
        }

        public override Vector3Double GetPosGlobalFromAgent(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetPosAgentFromGlobal(Vector3Double position)
        {
            throw new NotImplementedException();
        }

        public override void UpdateVisualParams()
        {
            throw new NotImplementedException();
        }

        public override void AddDebugText(string text)
        {
            throw new NotImplementedException();
        }

        public override Guid GetID()
        {
            throw new NotImplementedException();
        }
    }
}
