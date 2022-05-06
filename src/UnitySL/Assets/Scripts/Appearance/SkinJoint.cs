using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class SkinJoint
    {
        public AvatarJoint Joint { get; set; }
        public Vector3 RootToJointSkinOffset { get; set; }
        public Vector3 RootToParentJointSkinOffset { get; set; }

        public bool SetupSkinJoint(AvatarJoint joint)
        {
            Joint = joint;
            if (joint == null)
            {
                Logger.LogInfo("SkinJoint.SetupSkinJoint", "Can't find joint");
            }

            // compute the inverse root skin matrix
            RootToJointSkinOffset = AvatarJointMesh.TotalSkinOffset(joint);
            RootToJointSkinOffset = -RootToJointSkinOffset;

            RootToParentJointSkinOffset = AvatarJointMesh.TotalSkinOffset(AvatarJointMesh.GetBaseSkeletonAncestor(joint));
            RootToParentJointSkinOffset = -RootToParentJointSkinOffset;

            return true;
        }
    }
}
