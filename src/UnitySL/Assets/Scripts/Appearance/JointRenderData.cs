using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class JointRenderData
    {
        public JointRenderData(Matrix4x4 worldMatrix, SkinJoint skinJoint)
        {
            WorldMatrix = worldMatrix;
            SkinJoint = skinJoint;
        }

        public Matrix4x4 WorldMatrix { get; set; }
        public SkinJoint SkinJoint { get; set; }
    }
}
