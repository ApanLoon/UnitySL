using System.Collections.Generic;
using Assets.Scripts.Characters;

namespace Assets.Scripts.Appearance
{
    public class AvatarJoint : LLJoint
    {
        public int MeshId { get; set; }
        public List<AvatarJointMesh> MeshParts { get; } = new List<AvatarJointMesh>();

        // TODO: AvatarJoint is not implemented
    }
}