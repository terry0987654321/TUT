using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TUT
{
	public class GuideToNextInfo
	{
		public string toNextTag;
		public int NextId;
	}

	public class GuideUnitBase
	{
		public bool is_unlimited = false;
		public int judge_unlimited_index = -1;

		public string active_name;
		public int Id;
		public int stage;
		public int point;
		public int level =-1;

		public string pre_param = null;
		public System.Action<string> pre_action = null;

		public string post_param = null;
		public System.Action<string> post_action = null;

		public List<GuideToNextInfo> NextInfos = new List<GuideToNextInfo>();

		public bool activeGlobalLock = false;

	}

	public class GuideInfo
	{
		public string active_name;
		public int stage;
		public int point;
		public int level =-1;
		public string next_tag = string.Empty;

		public string PreGuideParam = null;
		public System.Action<string> PreGuideAction = null;

		public string PostGuideParam = null;
		public System.Action<string> PostGuideAction = null;
		
		public GuideInfo(string _active_name,int _stage,int _point)
		{
			active_name = _active_name;
			stage = _stage;
			point = _point;
		}
		
		public GuideInfo SetLevel(int _level)
		{
			level = _level;
			return this;
		}
		
		public GuideInfo SetToNextTag(string _next_tag)
		{
			next_tag = _next_tag;
			return this;
		}

		public GuideInfo SetPreAction(System.Action<string> action,string param)
		{
			PreGuideParam = param;
			PreGuideAction = action;
			return this;
		}

		public GuideInfo SetPostAction(System.Action<string> action,string param)
		{
			PostGuideParam = param;
			PostGuideAction = action;
			return this;
		}
	}

	public interface IGuideUIController
	{
		IEnumerator ShowGuide(TUT.GuideUnitBase unit);

		void FullScreenLock();

		void FullScreenUnlock();
	}

	public interface IGuideArchive
	{
		IEnumerator SyncServerArchive (GuideUnitBase unit);
		void SyncDone ();
	}

	public interface IGuideCfg
	{
		int Count();
		void AddNewPoint(string active_name);
		void InsertPoint(int index,string active_name);
		void RemovePoint(int index);
		void MovePoint(int sourceIndex, int destIndex);
		void Clear();
		GuideUnitBase GetUnitFromeIndex(int index);

		GuideUnitBase GetNextUnit (GuideUnitBase cur, string next_tag);
		GuideUnitBase GetValidUnit (GuideInfo info);
	}
	
	public class GuideController : TUT.TutSingletonBehaviour<GuideController>
	{
		private int mUserId = -1;

		private IGuideUIController mUIController = null;

		private IGuideArchive mArchive = null;

		private IGuideCfg mGuideCfg = null;

		private GuideUnitBase mCurShowUnit = null;

		private GuideInfo mCurInfo = null;

		private TutRoutine mShowUnitState = null;

		public void Reset()
		{
			if(mShowUnitState != null)
			{
				mShowUnitState.Block();
				mShowUnitState = null;
			}
		}

//		public override void Initialize (InitializeFinishHandle finish, object param)
//		{
//			base.Initialize (finish, param);
//			mUserId = param as int;
//		}

		public void SetParam(int user_id,IGuideUIController ui_controller,IGuideArchive archive,IGuideCfg cfg)
		{
			mUserId = user_id;
			mUIController = ui_controller;
			mArchive = archive;
			mGuideCfg = cfg;
		}

		public override bool IsGlobal ()
		{
			return true;
		}

		public override bool IsAutoInit ()
		{
			return true;
		}

		public string UserStageName
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return "guide_check_stage"+"_"+mUserId.ToString();
			}
		}

		public string UserUnlimitedName
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return "guide_unlimited"+"_"+mUserId.ToString();
			}
		}

		public string UserUnlimitedIndexName
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return "guide_unlimited_index"+"_"+mUserId.ToString();
			}
		}

		public string UserEnableName
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return "guide_enable_stage"+"_"+mUserId.ToString();
			}
		}

		public string UserPointName
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return "guide_check_point"+"_"+mUserId.ToString();
			}
		}

		public bool EnableGuide
		{
			get
			{
				if(!Initialized)
					return false;
				return PlayerPrefs.GetInt(UserEnableName,-1)>=0;
			}
			set
			{
				if(!Initialized)
					return ;
				if(value)
				{
					PlayerPrefs.SetInt(UserEnableName,1);
				}
				else
				{
					PlayerPrefs.SetInt(UserEnableName,-1);
				}
			}
		}

		public int CurStage
		{
			get
			{
				if(!Initialized)
					return -1;
				return PlayerPrefs.GetInt(UserStageName,-1);
			}
			set
			{
				if(!Initialized)
					return;
				PlayerPrefs.SetInt(UserStageName,value);
			}
		}
		
		public int CurCPoint
		{
			get
			{
				if(!Initialized)
					return -1;
				return PlayerPrefs.GetInt(UserPointName,-1);
			}
			set
			{
				if(!Initialized)
					return;
				PlayerPrefs.SetInt(UserPointName,value);
			}
		}

		public string CurUnlimltedArchive
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return PlayerPrefs.GetString(UserUnlimitedName,string.Empty);
			}
			set
			{
				if(!Initialized)
					return;
				PlayerPrefs.SetString(UserUnlimitedName,value);
			}
		}

		public string CurUnlimltedArchiveIndex
		{
			get
			{
				if(!Initialized)
					return string.Empty;
				return PlayerPrefs.GetString(UserUnlimitedIndexName,string.Empty);
			}
			set
			{
				if(!Initialized)
					return;
				PlayerPrefs.SetString(UserUnlimitedIndexName,value);
			}
		}

		private void ClearGuideSchedule()
		{
			if(PlayerPrefs.HasKey(UserPointName))
			{
				PlayerPrefs.DeleteKey(UserPointName);
			}

			if(PlayerPrefs.HasKey(UserStageName))
			{
				PlayerPrefs.DeleteKey(UserStageName);
			}
		}

		private IEnumerator UpdateShowUnit()
		{
			if(mUIController == null || mCurShowUnit == null || mGuideCfg == null)
				yield break;

			if(mCurShowUnit.pre_action != null)
			{
				mCurShowUnit.pre_action(mCurShowUnit.pre_param);
			}

			bool global_lock = false;
			if(mCurShowUnit.activeGlobalLock)
			{
//				mUIController.FullScreenLock();
				global_lock = true;
			}

			TutRoutine routine = TutCoroutine.Instance.Oh_StartCoroutine(mUIController.ShowGuide(mCurShowUnit));
			yield return routine.Waiting;


			System.Action<string> post = mCurShowUnit.post_action;
			string param = mCurShowUnit.post_param;

			int stage = mCurShowUnit.stage;
 			mCurShowUnit = mGuideCfg.GetNextUnit (mCurShowUnit,mCurInfo.next_tag);
			if(mCurShowUnit != null)
			{
				if(mCurShowUnit.activeGlobalLock&&global_lock)
				{
					mUIController.FullScreenLock();
				}
				else
				{
					mUIController.FullScreenUnlock();
				}
				CurStage = mCurShowUnit.stage;
				CurCPoint = mCurShowUnit.point;
				if(mArchive != null && stage != CurStage)
				{
					routine = TutCoroutine.Instance.Oh_StartCoroutine(mArchive.SyncServerArchive(mCurShowUnit));
				}
			}
			else
			{
				if(mArchive != null)
				{
					mArchive.SyncDone();
				}
			}

			if( post!= null)
			{
				post(param);
			}

			mCurInfo = null;
			mCurShowUnit = null;
			mShowUnitState = null;

		}

		public GuideUnitBase GetStageAndPoint(GuideInfo info)
		{
			if(mGuideCfg == null)
				return null;
			return mGuideCfg.GetValidUnit (info);
		}

		public IEnumerator CheckStageAndPointCoroutine(GuideInfo info)
		{
			if(mGuideCfg == null)
				yield break;
			
			if(mShowUnitState != null && !mShowUnitState.IsDone)
			{
				yield break;
			}
			
			GuideUnitBase unit = mGuideCfg.GetValidUnit (info);
			if(unit == null)
				yield break;
			else
			{
				mCurInfo = info;
				mCurShowUnit = unit;
				mShowUnitState = TutCoroutine.Instance.Oh_StartCoroutine(UpdateShowUnit());
				yield return mShowUnitState.Waiting;
				mShowUnitState = null;
			}
		}
	}
}