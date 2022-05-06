
using System;
using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Extensions.SystemExtensions;
using Assets.Scripts.NewView;
using UnityEngine;

namespace Assets.Scripts.Appearance
{
    public class AvatarJointMesh : AvatarJoint
    {
        // Utility functions added with Bento to simplify handling of extra
        // spine joints, or other new joints internal to the original
        // skeleton, and unknown to the system avatar.
        #region BentoUtility
        public static AvatarJoint GetBaseSkeletonAncestor(AvatarJoint joint)
        {
            LLJoint ancestor = joint.Parent;
            while (ancestor.Parent != null && (ancestor.Support != SupportCategory.Base))
            {
                Logger.LogDebug("AvatarJointMesh.GetBaseSkeletonAncestor", $"Skipping non-base ancestor {ancestor.Name}");
                ancestor = ancestor.Parent;
            }
            return (AvatarJoint)ancestor;
        }

        public static Vector3 TotalSkinOffset(AvatarJoint joint)
        {
            Vector3 totalOffset = Vector3.zero;
            while (joint != null)
            {
                if (joint.Support == SupportCategory.Base)
                {
                    totalOffset += joint.SkinOffset;
                }

                joint = (AvatarJoint)joint.Parent;
            }
            return totalOffset;
        }
        #endregion BentoUtility

        protected Color Color { get; set; }
        protected float Shiny { get; set; }
        
        /// <summary>
        /// Global texture
        /// </summary>
        protected Texture2D Texture { get; set; }

        /// <summary>
        /// LayerSet owned by the avatar
        /// </summary>
        protected TexLayerSet LayerSet { get; set; }

        /// <summary>
        /// Handle to a temporary texture for previewing uploads
        /// </summary>
        protected UInt32 TestImageName { get; set; }

        /// <summary>
        /// Global PolyMesh
        /// </summary>
        protected PolyMesh Mesh { get; set; }

        protected bool CullBackFaces { get; set; } = true;

        /// <summary>
        /// Face w/ AGP copy of mesh
        /// </summary>
        protected Face Face { get; set; }
        protected UInt32 FaceIndexCount { get; set; }
        protected UInt32 NumSkinJoints { get; set; }
        protected SkinJoint[] SkinJoints { get; set; }
        public bool IsTransparent { get; set; }

        protected bool AllocateSkinData(UInt32 numSkinJoints)
        {
            SkinJoints = new SkinJoint[numSkinJoints];
            NumSkinJoints = numSkinJoints;
            return true;
        }

        protected void FreeSkinData()
        {
            NumSkinJoints = 0;
            SkinJoints = null;
        }

        /// <summary>
        /// Set the shiny value
        /// </summary>
        /// <param name="color">WARNIG: This is ignored!</param>
        /// <param name="shiny"></param>
        public void SetSpecular(Color color, float shiny)
        {
            Shiny = shiny;
        }

        public void SetMesh(PolyMesh mesh)
        {
            Mesh = mesh;

            FreeSkinData();

            if (Mesh == null)
            {
                return;
            }

            // acquire the transform from the mesh object
            // SL-315
            SetPosition(Mesh.Position);
            SetRotation(Mesh.Rotation);
            SetScale(Mesh.Scale);

            // create skin joints if necessary
            if (Mesh.HasWeights && !Mesh.IsLod)
            {
                int numJointNames = Mesh.JointNames.Length;

                AllocateSkinData((UInt32)numJointNames);

                for (int i = 0; i < numJointNames; i++)
                {
                    AvatarJoint joint = (AvatarJoint)GetRoot().FindJoint(Mesh.JointNames[i]);
                    SkinJoints[i].SetupSkinJoint(joint);
                }
            }

            // setup joint array
            if (!Mesh.IsLod)
            {
                SetupJoint ((AvatarJoint)GetRoot());
                Logger.LogDebug("AvatarJointMesh.SetMesh", $"{Name} joint render entries: {Mesh.JointRenderData.Count}");
            }
        }

        public void SetupJoint(AvatarJoint currentJoint)
        {
            for (UInt32 sj = 0; sj < NumSkinJoints; sj++)
            {
                SkinJoint js = SkinJoints[sj];

                if (js.Joint != currentJoint)
                {
                    continue;
                }

                // we've found a skinjoint for this joint..
                Logger.LogDebug("AvatarJointMesh.SetupJoint", $"Mesh: {Name} joint {currentJoint.Name} matches skinjoint {sj}");

                // is the last joint in the array our parent?

                List<JointRenderData> jrd = Mesh.JointRenderData;

                // SL-287 - need to update this so the results are the same if
                // additional extended-skeleton joints lie between this joint
                // and the original parent.
                LLJoint ancestor = GetBaseSkeletonAncestor(currentJoint);
                if (jrd.Count != 0 && jrd.LastItem().WorldMatrix == ancestor.GetWorldMatrix())
                {
                    // ...then just add ourselves
                    AvatarJoint joint = js.Joint;
                    jrd.Add(new JointRenderData(joint.GetWorldMatrix(), js));
                    Logger.LogDebug("AvatarJointMesh.SetupJoint", $"add joint[{jrd.Count - 1}] = {js.Joint.Name}");
                }
                // otherwise add our ancestor and ourselves
                else
                {
                    jrd.Add(new JointRenderData(ancestor.GetWorldMatrix(), null));
                    Logger.LogDebug("AvatarJointMesh.SetupJoint", $"add2 ancestor joint[{jrd.Count - 1}] = {ancestor.Name}");
                    jrd.Add(new JointRenderData(currentJoint.GetWorldMatrix(), js));
                    Logger.LogDebug("AvatarJointMesh.SetupJoint", $"add2 joint[{jrd.Count - 1}] = {currentJoint.Name}");
                }
            }

            // depth-first traversal
            foreach (LLJoint child in currentJoint.Children)
            {
                SetupJoint((AvatarJoint)child);
            }
        }
    }
}
