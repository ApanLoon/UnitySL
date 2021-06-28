using System;
using System.Collections.Generic;
using Assets.Scripts.Appearance;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    public abstract class Character
    {
        protected Character()
        {
            PreferredPelvisHeight = 0f;
            Sex = CharacterSex.SEX_FEMALE;
            AppearanceSerialNum = 0;
            SkeletonSerialNum = 0;

			//MotionController.SetCharacter(this);
			//PauseRequest = new LLPauseRequestHandle();
		}

        /// <summary>
        /// get the prefix to be used to lookup motion data files
        /// from the viewer data directory
        /// </summary>
        /// <returns></returns>
        public abstract string GetAnimationPrefix();

		/// <summary>
		/// get the root joint of the character
		/// </summary>
		/// <returns></returns>
		public abstract LLJoint GetRootJoint();

		/// <summary>
		/// get the specified joint default implementation does
		/// recursive search, subclasses may optimize/cache results.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
	    public virtual LLJoint GetJoint(string name)
        {
            LLJoint joint = null;

            LLJoint root = GetRootJoint();
            if (root != null)
            {
                joint = root.FindJoint(name);
            }

            if (joint == null)
            {
                Logger.LogWarning("Character.GetJoint", $"Failed to find joint.({name})");
            }
            return joint;
        }

		/// <summary>
		/// get the position of the character
		/// </summary>
		/// <returns></returns>
		public abstract Vector3 GetCharacterPosition();

		/// <summary>
		/// get the rotation of the character
		/// </summary>
		/// <returns></returns>
		public abstract Quaternion GetCharacterRotation();

		/// <summary>
		/// get the velocity of the character
		/// </summary>
		/// <returns></returns>
		public abstract Vector3 GetCharacterVelocity();

		/// <summary>
		/// get the angular velocity of the character
		/// </summary>
		/// <returns></returns>
		public abstract Vector3 GetCharacterAngularVelocity();

		/// <summary>
		/// get the height & normal of the ground under a point
		/// </summary>
		/// <param name="inPos"></param>
		/// <param name="outPos"></param>
		/// <param name="outNorm"></param>
		public abstract void GetGround(Vector3 inPos, out Vector3 outPos, out Vector3 outNorm);

		/// <summary>
		/// skeleton joint accessor to support joint subclasses
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract LLJoint GetCharacterJoint(UInt32 i);

		/// <summary>
		/// get the physics time dilation for the simulator
		/// </summary>
		/// <returns></returns>
		public abstract float GetTimeDilation();

		/// <summary>
		/// gets current pixel area of this character
		/// </summary>
		/// <returns></returns>
		public abstract float GetPixelArea();

		/// <summary>
		/// gets the head mesh of the character
		/// </summary>
		/// <returns></returns>
		public abstract PolyMesh GetHeadMesh();

		/// <summary>
		/// gets the upper body mesh of the character
		/// </summary>
		/// <returns></returns>
		public abstract PolyMesh GetUpperBodyMesh();

		/// <summary>
		/// gets global coordinates from agent local coordinates
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public abstract Vector3Double GetPosGlobalFromAgent(Vector3 position);

		/// <summary>
		/// gets agent local coordinates from global coordinates
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public abstract Vector3 GetPosAgentFromGlobal(Vector3Double position);

		/// <summary>
		/// updates all visual parameters for this character
		/// </summary>
		public abstract void UpdateVisualParams();

		public abstract void AddDebugText(string text );

	public abstract Guid GetID();

		/*
			//-------------------------------------------------------------------------
			// End Interface
			//-------------------------------------------------------------------------
			// registers a motion with the character
			// returns true if successfull
			BOOL registerMotion( const LLUUID& id, LLMotionConstructor create );

			void removeMotion( const LLUUID& id );

		// returns an instance of a registered motion, creating one if necessary
		LLMotion* createMotion( const LLUUID &id );

		// returns an existing instance of a registered motion
		LLMotion* findMotion( const LLUUID &id );

		// start a motion
		// returns true if successful, false if an error occurred
		virtual BOOL startMotion( const LLUUID& id, F32 start_offset = 0.f);

			// stop a motion
			virtual BOOL stopMotion( const LLUUID& id, BOOL stop_immediate = FALSE );

			// is this motion active?
			BOOL isMotionActive( const LLUUID& id );

		// Event handler for motion deactivation.
		// Called when a motion has completely stopped and has been deactivated.
		// Subclasses may optionally override this.
		// The default implementation does nothing.
		virtual void requestStopMotion(LLMotion* motion);

			// periodic update function, steps the motion controller
			enum e_update_t { NORMAL_UPDATE, HIDDEN_UPDATE, FORCE_UPDATE };
			void updateMotions(e_update_t update_type);

			LLAnimPauseRequest requestPause();
			BOOL areAnimationsPaused() const { return mMotionController.isPaused(); }
		void setAnimTimeFactor(F32 factor) { mMotionController.setTimeFactor(factor); }
		void setTimeStep(F32 time_step) { mMotionController.setTimeStep(time_step); }

		LLMotionController& getMotionController() { return mMotionController; }

		// Releases all motion instances which should result in
		// no cached references to character joint data.  This is 
		// useful if a character wants to rebuild it's skeleton.
		virtual void flushAllMotions();

		// Flush only wipes active animations. 
		virtual void deactivateAllMotions();

		// dumps information for debugging
		virtual void dumpCharacter(LLJoint* joint = NULL);

		virtual F32 getPreferredPelvisHeight() { return mPreferredPelvisHeight; }

		virtual LLVector3 getVolumePos(S32 joint_index, LLVector3& volume_offset) { return LLVector3::zero; }

		virtual LLJoint* findCollisionVolume(S32 volume_id) { return NULL; }

		virtual S32 getCollisionVolumeID(std::string &name) { return -1; }

		void setAnimationData(std::string name, void* data);

		void* getAnimationData(std::string name);

		void removeAnimationData(std::string name);

		void addVisualParam(LLVisualParam* param);
		void addSharedVisualParam(LLVisualParam* param);

		virtual BOOL setVisualParamWeight(const LLVisualParam* which_param, F32 weight);
		virtual BOOL setVisualParamWeight(const char* param_name, F32 weight);
		virtual BOOL setVisualParamWeight(S32 index, F32 weight);

		// get visual param weight by param or name
		F32 getVisualParamWeight(LLVisualParam* distortion);
		F32 getVisualParamWeight(const char* param_name);
		F32 getVisualParamWeight(S32 index);

		// set all morph weights to defaults
		void clearVisualParamWeights();

		// visual parameter accessors
		LLVisualParam* getFirstVisualParam()
		{
			mCurIterator = mVisualParamIndexMap.begin();
			return getNextVisualParam();
		}
		LLVisualParam* getNextVisualParam()
		{
			if (mCurIterator == mVisualParamIndexMap.end())
				return 0;
			return (mCurIterator++)->second;
		}

		S32 getVisualParamCountInGroup(const EVisualParamGroup group) const
		{
			S32 rtn = 0;
			for (visual_param_index_map_t::const_iterator iter = mVisualParamIndexMap.begin();
				 iter != mVisualParamIndexMap.end();
				)
			{
				if ((iter++)->second->getGroup() == group)
				{
					++rtn;
				}
			}
			return rtn;
		}

		LLVisualParam* getVisualParam(S32 id) const
		{
			visual_param_index_map_t::const_iterator iter = mVisualParamIndexMap.find(id);
	return (iter == mVisualParamIndexMap.end()) ? 0 : iter->second;
		}
		S32 getVisualParamID(LLVisualParam* id)
	{
		visual_param_index_map_t::iterator iter;
		for (iter = mVisualParamIndexMap.begin(); iter != mVisualParamIndexMap.end(); iter++)
		{
			if (iter->second == id)
				return iter->first;
		}
		return 0;
	}
	S32 getVisualParamCount() const { return (S32)mVisualParamIndexMap.size(); }
		LLVisualParam* getVisualParam(const char* name);


	ESex getSex() const         { return mSex; }
		void setSex(ESex sex) { mSex = sex; }

	U32 getAppearanceSerialNum() const      { return mAppearanceSerialNum; }
		void setAppearanceSerialNum(U32 num) { mAppearanceSerialNum = num; }

	U32 getSkeletonSerialNum() const        { return mSkeletonSerialNum; }
		void setSkeletonSerialNum(U32 num) { mSkeletonSerialNum = num; }

	static std::vector<LLCharacter*> sInstances;
	static BOOL sAllowInstancesChange; //debug use

	virtual void setHoverOffset(const LLVector3& hover_offset, bool send_update = true) { mHoverOffset = hover_offset; }
	const LLVector3& getHoverOffset() const { return mHoverOffset; }

					*/

		//protected MotionController MotionController;

		protected Dictionary<string, object> AnimationData; // TODO: Can we make the type more specific?

		protected float					PreferredPelvisHeight;
		protected CharacterSex				Sex;
		UInt32					AppearanceSerialNum;
		UInt32					SkeletonSerialNum;
		//AnimPauseRequest	PauseRequest;

        private Dictionary<int, VisualParameter> VisualParameterByIndex;
        private Dictionary<string, VisualParameter> VisualParameterByName;

        private static HashSet<string> VisualParamNames;

        private Vector3 _hoverOffset;

	}
}
