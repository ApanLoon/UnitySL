using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets.Scripts.Characters;
using Assets.Scripts.Extensions.SystemExtensions;
using Assets.Scripts.Types;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public abstract class AvatarAppearance : Character
    {
        protected const string AVATAR_DEFAULT_CHAR = "avatar";
        protected static readonly Color DUMMY_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        #region Initialisation
        protected AvatarAppearance(WearableData wearableData) : base()
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

            // TODO: Initialise baked texture data
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
            Root = CreateAvatarJoint();
            Root.Name = "mRoot";

            //TODO: Stopped here
        }

        protected abstract AvatarJoint CreateAvatarJoint();

        public static int InitFlags { get; set; }

        #endregion Initialisation

        #region Inherited
        #endregion Inherited

        #region State
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
