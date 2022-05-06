using Assets.Scripts.Appearance;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class PolyMeshTests
    {
        [Test]
        public void LoadMesh()
        {
            PolyMesh mesh = PolyMesh.LoadMesh(System.IO.Path.Combine(Application.streamingAssetsPath, "Character", "avatar_head.llm"));
        }

        [Test]
        public void AvatarAppearance_InitClass()
        {
            AvatarAppearance.InitClass();
        }

    }
}
