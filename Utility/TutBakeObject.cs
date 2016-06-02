using UnityEngine;
using System.Collections;

namespace TUT
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class TutBakeObject : MonoBehaviour
	{
		public class BakeAnimInfo
		{
			private Animation mAnim = null;
			private AnimationState mAnimState = null;
			private float mAnimTime = 0;
			private bool mValid = false;
			private BakeAnimInfo mCurPlayingInfo = null;
			private float mFadeTime = 0.5f;
			private float mFadeStartTime = 0;
			public bool isValid
			{
				get
				{
					return mValid;
				}
			}

			public string AnimName
			{
				get
				{ 
					if(mAnimState != null)
						return mAnimState.name;
					return string.Empty;
				}
			}

			public AnimationState AnimState
			{
				get
				{
					if(mAnimState != null)
						return mAnimState;
					return null;
				}
			}
				

			public BakeAnimInfo (string anim_name,Animation anim,BakeAnimInfo playing_info)
			{
				mAnim = anim;
				mCurPlayingInfo = playing_info;
				mAnimState = mAnim[anim_name];
				if(mAnimState != null)
					mValid = true;
				mAnimTime = 0;
				mFadeStartTime = 0;
			}

			public void _UpdateFade(float time,float fadeTime)
			{
				mAnimState.enabled = true;
				mAnimState.blendMode = AnimationBlendMode.Blend;
				mAnimState.weight = Mathf.Lerp(1,0,Mathf.Max(1-(mFadeStartTime)/fadeTime,0));
				mAnimState.time = mAnimTime;
				mAnimTime += time;
				mFadeStartTime += time;
				switch (mAnimState.wrapMode)
				{
				default:
					if (mAnimTime >= mAnimState.length)
					{
						mAnimTime = mAnimState.length;
					}
					break;
				}
			}

			public void _Update(float time)
			{
				if (!isValid)
					return;
				if(mCurPlayingInfo != null && mFadeStartTime <= mFadeTime && mCurPlayingInfo.AnimState != null)
				{
					mAnimState.enabled = true;
					mAnimState.blendMode = AnimationBlendMode.Blend;
					mAnimState.weight = Mathf.Lerp(0,1,( mFadeStartTime)/mFadeTime);
					mAnimState.layer = mCurPlayingInfo.AnimState.layer +1;
					mCurPlayingInfo._UpdateFade(time,mFadeTime);
					mFadeStartTime += time;
				}
				else
				{
					mAnimState.enabled = true;
					mAnimState.layer = 1;
					mAnimState.weight = 1;
				}
				mAnimState.time = mAnimTime;
				mAnim.Sample();
				mAnimTime += time;
				switch (mAnimState.wrapMode)
				{
				case WrapMode.Loop:
					if (mAnimTime >= mAnimState.length)
						mAnimTime = 0;
					break;
				default:
					if (mAnimTime >= mAnimState.length)
					{
						mAnimTime = mAnimState.length;
						mValid = false;
					}
					break;
				}
			}
		}

		public bool AlwaysUpdateAnim = true;

		public Animation mParentAnim = null;

		public SkinnedMeshRenderer mParentMesh;

		private MeshRenderer mRenderer = null;

		private MeshFilter mFiter = null;

		private Mesh mBakeMesh = null;

		private BakeAnimInfo mCurAnimInfo = null;

		private TutBakeTagBones mTagBones = null;

		void Start()
		{
			InitObject (mParentMesh, mParentAnim);
			Play ("idle01");
		}

		public void InitObject(SkinnedMeshRenderer renderer, Animation anim)
		{
			mParentAnim = anim;
			mParentMesh = renderer;
			if (mRenderer == null)
			{
				mRenderer = this.gameObject.GetComponent<MeshRenderer> ();
			}

			if (mFiter == null) {
				mFiter = this.gameObject.GetComponent<MeshFilter> ();
			}

			if (mBakeMesh == null) {
				mBakeMesh = new Mesh ();
			}

			if(mParentAnim !=null)
			{
				mTagBones = mParentAnim.GetComponent<TutBakeTagBones>();
			}

			if (mRenderer != null &&mFiter != null ) {
				mRenderer.sharedMaterials = mParentMesh.sharedMaterials;
				mFiter.mesh  = mBakeMesh;

			}
		}

		public void Play(string anim_name)
		{
			if (mCurAnimInfo != null && mCurAnimInfo.isValid && mCurAnimInfo.AnimName == anim_name) {
				return;
			}
			BakeAnimInfo info  = new BakeAnimInfo (anim_name, mParentAnim,mCurAnimInfo);

			mCurAnimInfo = null;
			mCurAnimInfo = info;
		}

		private bool mIsVisble = false;

		void OnBecameVisible()
		{
			mIsVisble = true;
		}

		void OnBecameInvisible()
		{
			if(!AlwaysUpdateAnim)
				mIsVisble = false;
		}

		public Transform TestBone = null;
		void Update()
		{
			if(!mIsVisble)
				return;
			if(Input.GetKeyDown(KeyCode.Alpha1))  
			{
				Play("run");
			}
			if(Input.GetKeyDown(KeyCode.Alpha2))  
			{
				Play("idle01");
			}

			if (mCurAnimInfo == null || !mCurAnimInfo.isValid) {
				return;
			}
			mParentAnim.Stop ();
			mCurAnimInfo._Update (Time.deltaTime);
			if(mTagBones != null && TestBone !=null)
			{
				//Test
				//TestBone.localPosition =mTagBones.GetBonesPos(0);
				//TestBone.localRotation =  mTagBones.GetBonesRotate(0);
			}
			mParentMesh.BakeMesh (mBakeMesh);
			mFiter.mesh  = mBakeMesh;
		}
	}
}
