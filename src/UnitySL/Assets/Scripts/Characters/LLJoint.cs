using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions.MathExtensions;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    // priority levels, from highest to lowest
    public enum JointPriority
    {
        UseMotion = -1,
        Low = 0,
        Medium,
        High,
        Higher,
        Highest,
        Additive = LLJoint.MaxPriority
    };

    [Flags]
    public enum DirtyFlags
    {
        MatrixIsDirty     = 1,
        RotationIsDirty   = 2,
        PositionIsDirty   = 4,
        EverythingIsDirty = 7
    };

    public enum SupportCategory
    {
        Base,
        Extended
    };

    public class LLJoint
    {
        public const int MaxJointsPerMesh = 15;
        
        // Need to set this to count of animate-able joints,
        // currently = #bones + #collision_volumes + #attachments + 2,
        // rounded to next multiple of 4.
        public const UInt32 MaxAnimatedJoints = 216; // must be divisible by 4!
        public const UInt32 MaxJointsPerMeshObject = 110;

        // These should be higher than the joint_num of any
        // other joint, to avoid conflicts in updateMotionsByType()
        public const UInt32 HandJointNum = (MaxAnimatedJoints - 1);
        public const UInt32 FaceJointNum = (MaxAnimatedJoints - 2);
        public const int MaxPriority = 7;
        public const float MaxPelvisOffset = 5f;

        public const float ThresholdPosOffset = 0.0001f; //0.1 mm

        public string Name { get; set; }

        public SupportCategory Support { get; set; }

        // parent joint
        public LLJoint Parent { get; protected set; }

        ///// <summary>
        ///// Explicit transformation members
        ///// </summary>
        //public XFormMatrix Xform { get; protected set; }

        /// <summary>
        /// Tracks the default position defined by the skeleton
        /// </summary>
        protected Vector3 DefaultPosition { get; set; }

        /// <summary>
        /// Tracks the default scale defined by the skeleton
        /// </summary>
        protected Vector3 DefaultScale { get; set; }

        public DirtyFlags DirtyFlags;
        public bool UpdateXform;

        /// <summary>
        /// Describes the skin binding pose
        /// </summary>
        public Vector3 SkinOffset { get; set; }

        /// <summary>
        /// Endpoint of the bone, if applicable. This is only relevant for
        /// external programs like Blender, and for diagnostic display.
        /// </summary>
        public Vector3 End { get; set; }

        public int JointNum { get; set; }

        // child joints
        public List<LLJoint> Children;

        // debug statics
        public static int NumTouches;
        public static int NumUpdates;
        public static HashSet<string> DebugJointNames;

        public static void SetDebugJointNames(HashSet<string> names)
        {
            DebugJointNames = names;
        }

        public static void SetDebugJointNames(string names)
        {
            SetDebugJointNames(new HashSet<string>(names.Split(' ', ':', ',')));
        }

        // Position overrides
        public Vector3OverrideMap AttachmentPosOverrides = new Vector3OverrideMap();
        public Vector3 PosBeforeOverrides;

        // Scale overrides
        public Vector3OverrideMap AttachmentScaleOverrides = new Vector3OverrideMap();
        public Vector3 ScaleBeforeOverrides;

        public void UpdatePos(string avatarInfo)
        {
            Vector3 pos;
            if (AttachmentPosOverrides.FindActiveOverride(out var meshId, out var foundPos))
            {
                if (do_debug_joint(Name))
                {
                    // TODO: It looks as if the LL code provides separate logs, this should be sent to the "Avatar" debug log.
                    Logger.LogDebug("LLJoint.UpdatePos", $"Avatar {avatarInfo} joint {Name}, winner of {AttachmentPosOverrides.Count} is mesh {meshId} pos {foundPos}");
                }
                pos = foundPos;
            }
            else
            {
                if (do_debug_joint(Name))
                {
                    // TODO: It looks as if the LL code provides separate logs, this should be sent to the "Avatar" debug log.
                    Logger.LogDebug("LLJoint.UpdatePos", $"Avatar {avatarInfo} joint {Name}, winner is posBeforeOverrides {PosBeforeOverrides}");
                }
                pos = PosBeforeOverrides;
            }
            SetPosition(pos);
        }

        public void UpdateScale(string avatarInfo)
        {
            Vector3 scale;
            if (AttachmentPosOverrides.FindActiveOverride(out var meshId, out var foundScale))
            {
                if (do_debug_joint(Name))
                {
                    // TODO: It looks as if the LL code provides separate logs, this should be sent to the "Avatar" debug log.
                    Logger.LogDebug("LLJoint.UpdateScale", $"Avatar {avatarInfo} joint {Name}, winner of {AttachmentPosOverrides.Count} is mesh {meshId} scale {foundScale}");
                }
                scale = foundScale;
            }
            else
            {
                if (do_debug_joint(Name))
                {
                    // TODO: It looks as if the LL code provides separate logs, this should be sent to the "Avatar" debug log.
                    Logger.LogDebug("LLJoint.UpdateScale", $"Avatar {avatarInfo} joint {Name}, winner is ScaleBeforeOverrides {ScaleBeforeOverrides}");
                }
                scale = PosBeforeOverrides;
            }
            SetScale(scale);
        }

        public LLJoint()
        {
            JointNum = -1;
            Init();
            Touch();
        }

        /// <summary>
        /// TODO: Only used for LLVOAvatarSelf::mScreenp.  *DOES NOT INITIALIZE mResetAfterRestoreOldXform
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public LLJoint(string name, LLJoint parent)
        {
            JointNum = -2;
            Init();
            UpdateXform = false;
            Name = name;
            parent?.AddChild(this);
            Touch();
        }

        private void Init()
        {
            Name = "unnamed";
            Parent = null;
            //Xform.setScaleChildOffset(true);
            //Xform.setScale(new Vector3(1.0f, 1.0f, 1.0f));
            DirtyFlags = DirtyFlags.MatrixIsDirty | DirtyFlags.RotationIsDirty | DirtyFlags.PositionIsDirty;
            UpdateXform = true;
            Support = SupportCategory.Base;
            End = new Vector3(0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// set name and parent
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public void Setup(string name, LLJoint parent = null)
        {
            Name = name;
            parent?.AddChild(this);
        }

        /// <summary>
        /// Sets all dirty flags for all children, recursively.
        /// </summary>
        /// <param name="flags"></param>
        public void Touch(DirtyFlags flags = DirtyFlags.EverythingIsDirty)
        {
            if ((flags | DirtyFlags) != DirtyFlags)
            {
                NumTouches++;
                DirtyFlags |= flags;
                DirtyFlags childFlags = flags;
                if ((flags & DirtyFlags.RotationIsDirty) != 0)
                {
                    childFlags |= DirtyFlags.PositionIsDirty;
                }

                foreach (LLJoint child in Children)
                {
                    child.Touch(childFlags);
                }
            }
        }



        /// <summary>
        /// GetRoot
        /// </summary>
        /// <returns>The root joint above this joint</returns>
        public LLJoint GetRoot()
        {
            return Parent == null ? this : Parent.GetRoot();
        }

        /// <summary>
        /// Recursively searches for a child joint by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The found joint or null if none is found</returns>
        public LLJoint FindJoint(string name)
        {
            if (name == Name)
            {
                return this;
            }

            foreach (LLJoint child in Children)
            {
                LLJoint found = child.FindJoint(name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public void AddChild(LLJoint joint)
        {
            joint.Parent?.RemoveChild(joint);

            Children.Add(joint);
            //joint.Xform.setParent(Xform);
            joint.Parent = this;
            joint.Touch();
        }

        public void RemoveChild(LLJoint joint)
        {
            if (Children.Contains(joint) == false)
            {
                return;
            }

            Children.Remove(joint);
            //joint.Xform.setParent(null);
            joint.Parent = null;
            joint.Touch();
        }

        public void RemoveAllChildren()
        {
            foreach (LLJoint joint in Children)
            {
                //joint.Xform.setParent(null);
                joint.Parent = null;
                joint.Touch();
            }
            Children.Clear();
        }

        /// <summary>
        /// Returns true if the given joint name exists in the
        /// HashSet that determines whether or not this joint
        /// should be debugged.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool do_debug_joint(string name)
        {
            return DebugJointNames.Contains(name);
        }

        /// <summary>
        /// Get local position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return Vector3.zero; //Xform.getPosition();
        }

        /// <summary>
        /// Set local position
        /// </summary>
        /// <returns></returns>
        public void SetPosition(Vector3 requestedPos, bool applyAttachmentOverrides = false)
        {
            Vector3 pos = new Vector3(requestedPos.x, requestedPos.y, requestedPos.z);

            if (applyAttachmentOverrides && AttachmentPosOverrides.FindActiveOverride(out _, out var activeOverride))
            {
                if (pos != activeOverride && do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.SetPosition", $"Avatar joint {Name} requested_pos {requestedPos} overridden by attachment {activeOverride}");
                }
                pos = activeOverride;
            }
            if (pos != GetPosition())
            {
                if (do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.SetPosition", $"Avatar joint {Name} set pos {pos}");
                }
                //Xform.SetPosition(pos);
                Touch(DirtyFlags.MatrixIsDirty | DirtyFlags.PositionIsDirty);
            }
        }

        /// <summary>
        ///  Get/set world position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldPosition()
        {
            UpdateWorldPRSParent();
            return Vector3.zero; //Xform.getWorldPosition();
        }

        /// <summary>
        ///  Get last world position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLastWorldPosition()
        {
            return Vector3.zero; //Xform.getWorldPosition();
        }

        /// <summary>
        ///  Set world position
        /// </summary>
        /// <returns></returns>
        public void SetWorldPosition(Vector3 pos)
        {
            if (Parent == null)
            {
                SetPosition(pos);
                return;
            }

            Matrix4x4 temp_matrix = GetWorldMatrix();
            temp_matrix.m30 = pos.x;
            temp_matrix.m31 = pos.y; // TODO: Should I compensate for handedness here? I probably should but haven't for now
            temp_matrix.m32 = pos.z;

            Matrix4x4 parentWorldMatrix = Parent.GetWorldMatrix();
            Matrix4x4 invParentWorldMatrix = parentWorldMatrix.inverse;

            temp_matrix *= invParentWorldMatrix;

            Vector3 localPos = new Vector3(temp_matrix.m30, temp_matrix.m31, temp_matrix.m32); // TODO: Should I compensate for handedness here? I probably should but haven't for now

            SetPosition(localPos);
        }

        /// <summary>
        ///  Get local rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRotation()
        {
            return Quaternion.identity; //TODO: Xform.GetRotation();
        }

        /// <summary>
        ///  Set local rotation
        /// </summary>
        /// <returns></returns>
        public void SetRotation(Quaternion rot)
        {
            if (rot.IsFinite())
            {
                //	if (mXform.getRotation() != rot)
                {
                    // TODO: Xform.SetRotation(rot);
                    Touch(DirtyFlags.MatrixIsDirty | DirtyFlags.RotationIsDirty);
                }
            }
        }

        /// <summary>
        /// Get world rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion GetWorldRotation()
        {
            UpdateWorldPRSParent();

            return Quaternion.identity;  // TODO: Xform.GetWorldRotation();
        }

        /// <summary>
        /// Get last world rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion GetLastWorldRotation()
        {
            return Quaternion.identity;  // TODO: Xform.GetWorldRotation();
        }

        /// <summary>
        /// Set world rotation
        /// </summary>
        /// <returns></returns>
        public void SetWorldRotation(Quaternion rot)
        {
            if (Parent == null)
            {
                SetRotation(rot);
                return;
            }

            Matrix4x4 temp_mat = Matrix4x4.Rotate(rot);

            Matrix4x4 parentWorldMatrix = Parent.GetWorldMatrix();
            parentWorldMatrix.m30 = 0;
            parentWorldMatrix.m31 = 0; // TODO: Should probably be compensated for handedness
            parentWorldMatrix.m32 = 0;

            Matrix4x4 invParentWorldMatrix = parentWorldMatrix.inverse;

            temp_mat *= invParentWorldMatrix;

            SetRotation(temp_mat.rotation);
        }

        /// <summary>
        /// Get local scale
        /// </summary>
        /// <returns></returns>
        public Vector3 GetScale()
        {
            return Vector3.one;  //TODO: Xform.GetScale();
        }

        /// <summary>
        /// Set local scale
        /// </summary>
        /// <returns></returns>
        public void SetScale(Vector3 requestedScale, bool applyAttachmentOverrides = false)
        {
            Vector3 scale = new Vector3(requestedScale.x,  requestedScale.y, requestedScale.z);
            if (applyAttachmentOverrides && AttachmentScaleOverrides.FindActiveOverride(out _, out var activeOverride))
            {
                if (scale != activeOverride && do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.SetScale", $"Avatar joint {Name} requested_scale {requestedScale} overridden by attachment {activeOverride}");
                }
                scale = activeOverride;
            }
            // TODO: Xform
            //if ((Xform.getScale() != scale) && do_debug_joint(Name))
            //{
            //    Logger.LogDebug("LLJoint.SetScale", $"Avatar joint {Name} set scale {scale}");
            //}
            //Xform.SetScale(scale);
            Touch();
        }

        public void UpdateWorldMatrixChildren()
        {
            if (UpdateXform == false)
            {
                return;
            }

            if (DirtyFlags.HasFlag(DirtyFlags.MatrixIsDirty))
            {
                UpdateWorldMatrix();
            }

            foreach (LLJoint joint in Children)
            {
                joint.UpdateWorldMatrixChildren();
            }
        }

        public void UpdateWorldMatrixParent()
        {
            if (DirtyFlags.HasFlag(DirtyFlags.MatrixIsDirty))
            {
                Parent?.UpdateWorldMatrixParent();
                UpdateWorldMatrix();
            }
        }

        public void UpdateWorldPRSParent()
        {
            if ((DirtyFlags & (DirtyFlags.RotationIsDirty | DirtyFlags.PositionIsDirty)) != 0)
            {
                Parent?.UpdateWorldPRSParent();
                //TODO: Xform.Update();
                DirtyFlags &= ~(DirtyFlags.RotationIsDirty | DirtyFlags.PositionIsDirty);
            }

        }

        public void UpdateWorldMatrix()
        {
            if (DirtyFlags.HasFlag(DirtyFlags.MatrixIsDirty))
            {
                NumUpdates++;
                //TODO: Xform.UpdateMatrix(false);
                DirtyFlags = 0;
            }
        }

        /// <summary>
        /// Get world matrix
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetWorldMatrix()
        {
            UpdateWorldMatrixParent();

            return Matrix4x4.identity; //TODO: Xform.GetWorldMatrix();
        }

        /// <summary>
        /// Set world matrix
        /// </summary>
        /// <returns></returns>
        public void SetWorldMatrix(Matrix4x4 mat)
        {
            Logger.LogInfo("LLJoint.SetWorldMatrix", "Not correctly implemented yet");
            // extract global translation
            Vector3 trans = new Vector3(mat.m30, mat.m31, mat.m32); // TODO: Should probably be compensated for handedness

            // extract global rotation
            Quaternion rot = mat.rotation;

            SetWorldPosition(trans);
            SetWorldRotation(rot);
        }


        public void ClampRotation(Quaternion old_rot, out Quaternion new_rot)
        {
            Vector3 main_axis = new Vector3(1f, 0f, 0f);

            foreach (LLJoint joint in Children)
            {
                if (joint.IsAnimatable())
                {
                    main_axis = joint.GetPosition().normalized;
                    // only care about first animatable child
                    break;
                }
            }
            //TODO: WTF? The LL code doesn't do anything here!
            new_rot.x = old_rot.x;
            new_rot.y = old_rot.y;
            new_rot.z = old_rot.z;
            new_rot.w = old_rot.w;
        }

        public virtual bool IsAnimatable()
        {
            return true;
        }

        public void AddAttachmentPosOverride(Vector3 pos, Guid meshId, string avInfo, ref bool activeOverrideChanged)
        {
            activeOverrideChanged = false;
            if (meshId == Guid.Empty)
            {
                return;
            }

            bool hasActiveOverrideBefore = HasAttachmentPosOverride(out var beforePos, out _);
            if (AttachmentPosOverrides.Count > 0)
            {
                if (do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.AddAttachmentPosOverride", $"Avatar {avInfo} joint {Name} saving PosBeforeOverrides {GetPosition()}");
                }
                PosBeforeOverrides = GetPosition();
            }
            AttachmentPosOverrides.Add(meshId, pos);
            HasAttachmentPosOverride(out var afterPos, out _);
            if (!hasActiveOverrideBefore || (afterPos != beforePos))
            {
                activeOverrideChanged = true;
                if (do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.AddAttachmentPosOverride", $"Avatar {avInfo} joint {Name} addAttachmentPosOverride for mesh {meshId} pos {GetPosition()}");
                }
                UpdatePos(avInfo);
            }
        }

        public void RemoveAttachmentPosOverride(Guid meshId, string avInfo, ref bool activeOverrideChanged)
        {
            activeOverrideChanged = false;
            if (meshId == Guid.Empty)
            {
                return;
            }

            HasAttachmentPosOverride(out var beforePos, out _);
            if (AttachmentPosOverrides.Remove(meshId))
            {
                bool hasActiveOverrideAfter = HasAttachmentPosOverride(out var afterPos, out _);
                if (!hasActiveOverrideAfter || (afterPos != beforePos))
                {
                    activeOverrideChanged = true;
                    if (do_debug_joint(Name))
                    {
                        Logger.LogDebug("LLJoint.RemoveAttachmentPosOverride", $"Avatar {avInfo} joint {Name} removeAttachmentPosOverride for {meshId}");
                        ShowJointPosOverrides(this, "remove", avInfo);
                    }
                    UpdatePos(avInfo);
                }
            }
        }

        public bool HasAttachmentPosOverride(out Vector3 pos, out Guid meshId)
        {
            return AttachmentPosOverrides.FindActiveOverride(out meshId, out pos);
        }

        public void ClearAttachmentPosOverrides()
        {
            if (AttachmentPosOverrides.Count > 0)
            {
                AttachmentPosOverrides.Clear();
                SetPosition(PosBeforeOverrides);
            }
        }

        public void ShowAttachmentPosOverrides(string avInfo)
        {
            bool hasActiveOverride;
            hasActiveOverride = AttachmentPosOverrides.FindActiveOverride(out _, out var activeOverride);
            int count = AttachmentPosOverrides.Count;
            if (count == 1)
            {
                Vector3 pos = AttachmentPosOverrides.Map.Values.First();
                string highlight = (hasActiveOverride && (pos == activeOverride)) ? "*" : "";
                Logger.LogDebug("LLJoint.ShowAttachmentPosOverrides", $"Avatar {avInfo} joint {Name} has single attachment pos override {highlight}{pos} default {DefaultPosition}");
            }
            else if (count > 1)
            {
                Logger.LogDebug("LLJoint.ShowAttachmentPosOverrides", $"Avatar {avInfo} joint {Name} has {count} attachment pos overrides");
                HashSet<Vector3> distinctOffsets = new HashSet<Vector3>();
                foreach (KeyValuePair<Guid, Vector3> kv in AttachmentPosOverrides.Map)
                {
                    distinctOffsets.Add(kv.Value);
                }
                if (distinctOffsets.Count > 1)
                {
                    Logger.LogDebug("LLJoint.ShowAttachmentPosOverrides", $"Avatar {avInfo} CONFLICTS, {distinctOffsets.Count} different values");
                }
                else
                {
                    Logger.LogDebug("LLJoint.ShowAttachmentPosOverrides", $"Avatar {avInfo} no conflicts");
                }

                foreach (Vector3 pos in distinctOffsets)
                {
                    string highlight = (hasActiveOverride && pos == activeOverride) ? "*" : "";
                    Logger.LogDebug("LLJoint.ShowAttachmentPosOverrides", $"Avatar {avInfo} POS {highlight}{pos} default {DefaultPosition}");
                }
            }
        }

        public void AddAttachmentScaleOverride(Vector3 scale, Guid meshId, string avInfo)
        {
            if (meshId == Guid.Empty)
            {
                return;
            }

            if (AttachmentScaleOverrides.Count > 0)
            {
                if (do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.AddAttachmentScaleOverride", $"Avatar {avInfo} joint {Name} saving ScaleBeforeOverrides {GetScale()}");
                }
                ScaleBeforeOverrides = GetScale();
            }
            AttachmentScaleOverrides.Add(meshId, scale);
            if (do_debug_joint(Name))
            {
                Logger.LogDebug("LLJoint.AddAttachmentScaleOverride", $"Avatar {avInfo} joint {Name} addAttachmentScaleOverride for mesh {meshId} scale {scale}");
            }
            UpdateScale(avInfo);
        }

        public void RemoveAttachmentScaleOverride(Guid meshId, string avInfo)
        {
            if (meshId == Guid.Empty)
            {
                return;
            }
            if (AttachmentScaleOverrides.Remove(meshId))
            {
                if (do_debug_joint(Name))
                {
                    Logger.LogDebug("LLJoint.RemoveAttachmentScaleOverride", $"Avatar {avInfo} joint {Name} removeAttachmentScaleOverride for mesh {meshId}");
                    ShowJointScaleOverrides(this, "remove", avInfo);
                }
                UpdateScale(avInfo);
            }
        }

        public bool HasAttachmentScaleOverride(out Vector3 scale, out Guid mesh_id)
        {
            return AttachmentScaleOverrides.FindActiveOverride(out mesh_id, out scale);
        }

        public void ClearAttachmentScaleOverrides()
        {
            if (AttachmentScaleOverrides.Count > 0)
            {
                AttachmentScaleOverrides.Clear();
                SetScale(ScaleBeforeOverrides);
            }
        }

        public void ShowAttachmentScaleOverrides(string avInfo)
        {
            bool hasActiveOverride;
            hasActiveOverride = AttachmentScaleOverrides.FindActiveOverride(out _, out var activeOverride);
            int count = AttachmentScaleOverrides.Count;
            if (count == 1)
            {
                Vector3 scale = AttachmentScaleOverrides.Map.Values.First();
                string highlight = (hasActiveOverride && (scale == activeOverride)) ? "*" : "";
                Logger.LogDebug("LLJoint.ShowAttachmentScaleOverrides", $"Avatar {avInfo} joint {Name} has single attachment scale override {highlight}{scale} default {DefaultScale}");
            }
            else if (count > 1)
            {
                Logger.LogDebug("LLJoint.ShowAttachmentScaleOverrides", $"Avatar {avInfo} joint {Name} has {count} attachment scale overrides");
                HashSet<Vector3> distinctOffsets = new HashSet<Vector3>();
                foreach (KeyValuePair<Guid, Vector3> kv in AttachmentScaleOverrides.Map)
                {
                    distinctOffsets.Add(kv.Value);
                }
                if (distinctOffsets.Count > 1)
                {
                    Logger.LogDebug("LLJoint.ShowAttachmentScaleOverrides", $"Avatar {avInfo} CONFLICTS, {distinctOffsets.Count} different values");
                }
                else
                {
                    Logger.LogDebug("LLJoint.ShowAttachmentScaleOverrides", $"Avatar {avInfo} no conflicts");
                }

                foreach (Vector3 pos in distinctOffsets)
                {
                    string highlight = (hasActiveOverride && pos == activeOverride) ? "*" : "";
                    Logger.LogDebug("LLJoint.ShowAttachmentScaleOverrides", $"Avatar {avInfo} POS {highlight}{pos} default {DefaultPosition}");
                }
            }
        }

        public void GetAllAttachmentPosOverrides(out int numOverrides, HashSet<Vector3> distinctOverrides)
        {
            numOverrides = AttachmentPosOverrides.Count;
            foreach (Vector3 pos in AttachmentPosOverrides.Map.Values)
            {
                distinctOverrides.Add(pos);
            }
        }

        public void GetAllAttachmentScaleOverrides(out int numOverrides, HashSet<Vector3> distinctOverrides)
        {
            numOverrides = AttachmentScaleOverrides.Count;
            foreach (Vector3 scale in AttachmentScaleOverrides.Map.Values)
            {
                distinctOverrides.Add(scale);
            }
        }

        public void ShowJointPosOverrides(LLJoint joint, string note, string avInfo)
        {
            Logger.LogDebug("LLJoint.ShowJointPosOverrides", $"Avatar {avInfo} joint {joint.Name} {note} {joint.PosBeforeOverrides} {joint.AttachmentPosOverrides.ShowJointVector3Overrides()}");
        }

        public void ShowJointScaleOverrides(LLJoint joint, string note, string avInfo)
        {
            Logger.LogDebug("LLJoint.ShowJointScaleOverrides", $"Avatar {avInfo} joint {joint.Name} {note} {joint.ScaleBeforeOverrides} {joint.AttachmentScaleOverrides.ShowJointVector3Overrides()}");
        }

        /// <summary>
        ///  Used in checks of whether a pos override is considered significant.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool AboveJointPosThreshold(Vector3 pos)
        {
            Vector3 diff = pos - DefaultPosition;
            const float maxJointPosOffset = ThresholdPosOffset;
            return diff.sqrMagnitude > maxJointPosOffset * maxJointPosOffset;
        }

        /// <summary>
        ///  Used in checks of whether a scale override is considered significant.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public bool AboveJointScaleThreshold(Vector3 scale)
        {
            Vector3 diff = scale - DefaultScale;
            const float maxJointScaleOffset = 0.0001f; // 0.1 mm
            return diff.sqrMagnitude > maxJointScaleOffset * maxJointScaleOffset;
        }
    }
}
