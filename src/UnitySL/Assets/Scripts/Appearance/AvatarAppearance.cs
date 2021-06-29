using System;
using System.IO;
using System.Linq;
using System.Xml;
using Assets.Scripts.Characters;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class AvatarAppearance : Character
    {
        protected const string AVATAR_DEFAULT_CHAR = "avatar";
        protected static readonly Color DUMMY_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        public static AvatarSkeletonInfo AvatarSkeletonInfo { get; protected set; }
        public static AvatarXmlInfo AvatarXmlInfo { get; protected set; }

        protected static XmlDocument AvatarConfigurationXmlTree;
        protected static XmlDocument SkeletonXmlTree;

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
            AvatarConfigurationXmlTree = OpenAvatarXmlFile(avatarFilename, "linden_avatar", new []{"1.0", "2.0"});

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
                AvatarSkeletonInfo = AvatarSkeletonInfo.ParseXml(SkeletonXmlTree.LastChild); // This skips any XmlDeclaration
            }
            catch (Exception e)
            {
                Logger.LogError("AvatarAppearance.InitClass", $"Error parsing skeleton file: {skeletonFilename} ({e})");
            }

            try
            {
                AvatarXmlInfo = null;
                AvatarXmlInfo = AvatarXmlInfo.ParseXml(AvatarConfigurationXmlTree.LastChild); // This skips any XmlDeclaration
            }
            catch (Exception e)            
            {
                Logger.LogError("AvatarAppearance.InitClass", $"Error parsing avatar file: {avatarFilename} ({e})");
            }
        }

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
            XmlNode root = doc.LastChild; // This skips any XmlDeclaration
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
