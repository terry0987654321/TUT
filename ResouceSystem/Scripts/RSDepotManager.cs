using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{
	public class RSDUnit<AssetType> where AssetType : Object
	{
		public delegate void AssetReadyCB(string path,AssetType asset);

		private AssetType mAsset = default(AssetType);

		private string mAssetPath = string.Empty;

		private OhResResult mReqResult = null;

		private AssetReadyCB mReadyCB = null;

		public bool isValid
		{
			get
			{
				return mAsset != default(AssetType);
			}
		}

		public AssetType UnitAsset
		{
			get
			{
				return mAsset;
			}
		}

		public AssetReadyCB ReadyCB
		{
			set
			{
				if(value != null)
				{
					if(isValid)
					{
						if(mReqResult != null)
						{
							mReqResult.ResultCB = null;
							mReqResult = null;
						}
						AssetReadyCB cb = mReadyCB;
						mReadyCB = null;
						if(cb != null)
						{
							cb(mAssetPath,mAsset);
						}
						value(mAssetPath,mAsset);
					}
					else
					{
						mReadyCB += value;
					}
				}
			}
		}

		public Coroutine Waitting
		{
			get
			{
				if(mReqResult == null)
				{
					return null;
				}
				return mReqResult.Waiting;
			}
		}

		public AssetReadyCB SubCB
		{
			set
			{
				if(value != null)
				{
					if(mReadyCB != null)
					{
						mReadyCB -= value;
					}
				}
			}
		}

		public void Init(OhResResult result)
		{
			if(mReqResult != null)
			{
				mReqResult.ResultCB = null;
				mReqResult = null;
			}
			mReqResult = result;
			mReqResult.ResultCB=(OhResResult _result)=>{
				_FillUnit(result[0].path,_result[0].result as AssetType);
			};
		}

		private void _FillUnit(string path,AssetType asset)
		{
			mAssetPath = path;
			mAsset = asset;
			AssetReadyCB cb = mReadyCB;
			mReadyCB = null;
			if(cb != null)
			{
				cb(mAssetPath,mAsset);
			}
			mReqResult = null;
		}

		public void Clear()
		{
			if(mReqResult != null)
			{
				mReqResult.ResultCB = null;
				mReqResult = null;
			}
			mReadyCB = null;
			mAsset = null;
			mAssetPath = string.Empty;
		}
	}

	public interface RSDepotInterface
	{
		void ClearDepot ();
	}

	public class RSDepot<T> : TutSingleton<RSDepot<T>> , RSDepotInterface where T : Object
	{
		protected override void Initialize ()
		{
			base.Initialize ();
			RSDepotManager.Instance._AddDepot (m_Instance);
		}

		private Dictionary<string,RSDUnit<T>> mRSCaches = new Dictionary<string, RSDUnit<T>>();

		public bool ContainAsset(string path)
		{
			return mRSCaches.ContainsKey (path);
		}

		public RSDUnit<T> GetAsset(string path)
		{
			RSDUnit<T> unit = null;
			if(!mRSCaches.ContainsKey(path))
			{
				unit = new RSDUnit<T>();
				mRSCaches.Add(path,unit);
				unit.Init(TutResources.Instance.Load(path));
			}
			else
			{
				unit = mRSCaches[path];
				if(!unit.isValid)
				{
					unit.Init(TutResources.Instance.Load(path));
				}
			}
			return unit;
		}

		public void ClearDepot()
		{
			foreach(RSDUnit<T> unit in mRSCaches.Values)
			{
				unit.Clear();
			}
			mRSCaches.Clear ();
		}
	}

	public class RSDepotManager  : TutSingletonBehaviour<RSDepotManager>
	{
		public override bool IsAutoInit ()
		{
			return true;
		}

		public override bool IsGlobal ()
		{
			return false;
		}

		private static bool mIsValid = true;

		public static bool IsValid
		{
			get
			{
				return mIsValid;
			}
		}

		private List<RSDepotInterface> mDepots = new List<RSDepotInterface>();

		public void _AddDepot(RSDepotInterface depot)
		{
			if(!mDepots.Contains(depot))
				mDepots.Add(depot);
		}

		public void ClearDepots()
		{
			foreach(RSDepotInterface depot in mDepots)
			{
				depot.ClearDepot();
			}
			mDepots.Clear ();

		}

		private void OnDestroy()
		{
			if(m_Instance != null)
			{
				m_Instance.ClearDepots ();
				mIsValid = false;
			}
			m_Instance = null;
		}

		protected virtual  void OnApplicationQuit()
		{
			if(m_Instance != null)
			{
				m_Instance.ClearDepots ();
				mIsValid = false;
			}
			m_Instance = null;
		}
	}

}