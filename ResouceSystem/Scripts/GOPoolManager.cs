using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TUT.RSystem
{

	public class GOPoolManager:TUT.TutSingletonBehaviour<GOPoolManager>
	{
		public readonly static float InsTime = 0.1f; 

		public readonly static Vector3 InsPosValue = new Vector3 (1000, 0, 1000);
		
		public delegate void InstantiateCallback(GameObject[] objs);


		private List<int> mInsIDs = new List<int>();

		public override bool IsAutoInit()
		{
			return true;
		}

		public override void Initialize (InitializeFinishHandle finish, object param)
		{
			m_Instance.transform.position = InsPosValue;
			base.Initialize (finish, param);
		}
		
		private Transform mTransform = null;
		
		public Transform _MyTransform
		{
			get
			{
				if(mTransform == null)
					mTransform = this.transform;
				return mTransform;
			}
		}
		
		private Dictionary<string,GOPoolUnit> mPools = new Dictionary<string, GOPoolUnit>();
		
		public GameObject Instantiate(string name,string temp_tag = null)
		{
			GOPoolUnit pool = null;
			
			if(mPools.TryGetValue(name,out pool))
			{
				return pool.Pop(temp_tag);
			}
			else
			{
				return null;
			}
		}
		
		public TutRoutine Instantiate(string[] names,string[] temp_tags = null)
		{
			return TutCoroutine.Instance.Oh_StartCoroutine(Instantiale_Coroutine(names,temp_tags));
		}
		
		private IEnumerator Instantiale_Coroutine(string[] names,string[] temp_tags)
		{
			List<GameObject> results = null;
			if (names == null || names.Length == 0)
			{
				yield break;
			}
			GOPoolUnit pool = null;
			TutRoutine routine = null;
			results = new List<GameObject>();
			string temp_tag = string.Empty;
			for (int i = 0; i<names.Length; i++)
			{
				if(temp_tags != null && i<temp_tags.Length)
				{
					temp_tag = temp_tags[i];
				}
				else
				{
					temp_tag = string.Empty;
				}
				
				if(mPools.TryGetValue(names[i],out pool))
				{
					routine = TutCoroutine.Instance.Oh_StartCoroutine( pool.Pop_Coroutine(temp_tag) );
					yield return routine;
					yield return routine.Waiting;
					if(routine.Result != null && routine.Result.GetType() == typeof(GameObject))
					{
						results.Add(routine.Result as GameObject);
					}
					else
						results.Add(null);
				}
				else
					results.Add(null);
			}
			
			for (int i = 0; i<results.Count; i++)
			{
				if(results[i] != null)
				{
//					results[i].SetActive(true);
					results[i].transform.parent = null;
					GOPoolUnit.ResetTransform(results[i].transform);
				}
			}

//			string log = string.Empty;
//			foreach(GOPoolUnit p in mPools.Values)
//			{
//				log += p.PoolTag+":"+p.CacheCount.ToString()+"\n";
//			}
//			Debug.LogError (log);
			yield return results;
		}
		
		public GameObject InstantiateNoCache( string name )
		{
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(name,out pool))
			{
				return GameObject.Instantiate(pool.UnitSource) as GameObject;
			}
			else
			{
				return null;
			}
		}
		
		public void Destroy( GameObject obj ,string temp_tag = null)
		{
			if(obj == null)
				return;
			GOPoolTag tag = obj.GetComponent<GOPoolTag>();
			if (tag == null)
				return;
			if(mPools.Count == 0)
			{
				obj = null;
				return;
			}
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(tag.PoolName,out pool))
			{
				pool.Push(obj,temp_tag);
			}
		}

		public void RealDestroy(GameObject obj)
		{
			if(obj == null)
				return;
			GOPoolTag tag = obj.GetComponent<GOPoolTag>();
			if (tag == null)
				return;
			if(mPools.Count == 0)
			{
				obj = null;
				return;
			}
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(tag.PoolName,out pool))
			{
				pool.DestroyInsUnit(obj);
			}	
		}
		
		public void NewPool(GameObject prefab,string pool_name = null)
		{
			if(prefab == null)
				return;
			GOPoolUnit pool = null;
			string pname = string.IsNullOrEmpty(pool_name) ? prefab.name : pool_name;
			if(!mPools.TryGetValue(pname,out pool))
			{
				pool = new GOPoolUnit(prefab,pname);
				mPools.Add(pool.PoolTag,pool);
			}
			else
			{
				if(prefab != pool.UnitSource)
					Debug.LogWarning(TutNorm.LogErrFormat("GOPoolUnit ","Warning:[Failed] Cannt Add New Pool,cause this's prefab "+ pool_name +" is Exist!"));
			}
		}

		public void NewPool_NoPrefab(string pool_name)
		{
			GOPoolUnit pool = null;
			if(!mPools.TryGetValue(pool_name,out pool))
			{
				pool = new GOPoolUnit(pool_name);
				mPools.Add(pool.PoolTag,pool);
			}
//			else
//			{					
//				Debug.LogWarning(TutNorm.LogErrFormat("GOPoolUnit ","Warning:[Failed] Cannt Add New Pool,cause this's tag "+ pool_name +" is Exist!"));
//			}
		}

		public void RemoveInvalidPool(string pool_name)
		{
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(pool_name,out pool))
			{
				if(!pool.IsValid)
				{
					mPools.Remove(pool.PoolTag);
				}
			}	
		}

		public void SetPoolSource(string pool_name,GameObject prefab)
		{
			if(prefab == null)
				return;
			GOPoolUnit pool = null;
			if(!mPools.TryGetValue(pool_name,out pool))
			{
				if(prefab != pool.UnitSource)
					Debug.LogWarning(TutNorm.LogErrFormat("GOPoolUnit ","Warning:[Failed] Cannt Set Pool Source,cause this "+ pool_name +" is not Exist!"));
			}
			else
			{
				pool.SetUnitSource(prefab);
			}
		}
		
		public void ClearAllTemp()
		{
			foreach (GOPoolUnit pool in mPools.Values)
			{
				pool.ClearTemp();
			}
		}
		
		public bool IsContainPool(string pool_name)
		{
			return mPools.ContainsKey(pool_name);
		}
		
		public void ClearPool(string pool_name)
		{
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(pool_name,out pool))
			{
				pool.ClearPool();
			}
		}
		
		public void DeletePool(string pool_name)
		{
			GOPoolUnit pool = null;
			if(mPools.TryGetValue(pool_name,out pool))
			{
				pool.DestroyPool();
				mPools.Remove(pool_name);
			}
		}
		
		public void ClearAllObjCache()
		{
			foreach(GOPoolUnit pool in mPools.Values)
			{
				pool.ClearPool();
			}
		}
		
		public void DestroyAllObjCache()
		{
			foreach(GOPoolUnit pool in mPools.Values)
			{
				pool.DestroyPool();
			}
			mPools.Clear();
		}
	}
	
	public class GOInsUnit
	{
		public bool Activated = false;
		public string TempTag = string.Empty;
		public GameObject Cache = null;
		public bool Manual_Destroy = false;
		
		public void init(GameObject cache,bool activated,Transform bind_node,bool manual = false)
		{
			Activated = activated;
			Cache = cache;
			//            Cache.name = Cache.name.Replace("(Clone)","");
			GOPoolUnit.Binding(Cache,bind_node);
			Manual_Destroy = manual;
			
		}
	}
	
	public class GoInstantiate : TUT.TutSingleton<GoInstantiate>
	{
		public class GoInsReq
		{
			private GOPoolUnit mPool = null;
			
			private bool mBlockReq = false;
			
			private bool mIsProcessing = false;
			
			public GOPoolUnit Pool
			{
				get
				{
					return mPool;
				}
			}
			
			public GoInsReq(GOPoolUnit pool)
			{
				mPool = pool;
			}
			
			public void ProcessReq()
			{
				mPool = null;
				mIsProcessing = true;
			}
			
			public  void Block()
			{
				mPool = null;
				mBlockReq = true;
			}
			
			public IEnumerator WaitProcess()
			{
				if (mBlockReq)
					yield break;
				while (!mIsProcessing)
				{
					if (mBlockReq)
						yield break;
					yield return 0;
				}
				mPool = null;
				yield return true;
			}
		}
		
		private bool mIsBlock = false;
		
		private List<GoInsReq> mInsQueue = new List<GoInsReq>();
		
		private bool mUpdateQueue = false;
		
		private TutRoutine mUpQueueRoutine = null;
		
		private GoInsReq CreateInsReq(GOPoolUnit pool)
        {
            GoInsReq req = new GoInsReq(pool);
            if (mIsBlock)
                return req;
            mInsQueue.Add(req);
            if (mUpQueueRoutine == null || mUpQueueRoutine.IsDone)
                mUpQueueRoutine = TUT.TutCoroutine.Instance.Oh_StartCoroutine(UpdateQueue());
            return req;
        }

		public void BlockReq(GoInsReq req)
		{
			if (mInsQueue.Count == 0 || mIsBlock || req == null)
			{
				return;
			}
			if(req.Pool == null)
				return;
			Debug.Log("   [      Block Req       ]  "+req.Pool.PoolTag);
			req.Block();
			mInsQueue.Remove (req);
			if(mUpQueueRoutine != null && mInsQueue.Count == 0)
			{
				mUpQueueRoutine.Block();
				mUpQueueRoutine = null;
			}
		}
		
		public void BlockPoolInQueue(GOPoolUnit pool)
        {
            if (mInsQueue.Count == 0 || mIsBlock)
            {
                return;
            }
            for (int i = 0; i<mInsQueue.Count;)
            {
                if(mInsQueue[i].Pool == pool)
                {
                    mInsQueue[i].Block();
                    mInsQueue.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private IEnumerator UpdateQueue()
        {
            if (mInsQueue.Count == 0 || mIsBlock)
            {
                yield break;
            }
            for (int i = 0; i < mInsQueue.Count; )
            {
                if (!mIsBlock)
                {
					if(!TUT.TutRoutine.IgnoreYRNull)
                    	yield return new WaitForEndOfFrame();
                    if (mInsQueue.Count == 0)
                    {
                        mIsBlock = false;
                        yield break;
                    }
                    //                    yield return new WaitForSeconds(GOPoolManager.InsTime);

					if(!TUT.TutRoutine.IgnoreYRNull)
                   		yield return TutTimeUtil.WaitForSeconds(GOPoolManager.InsTime).Waiting;

                    if (mInsQueue.Count != 0)
                    {
//                        Debug.Log("   [      Queue       ]  " + mInsQueue[0].Pool.PoolTag);
                        mInsQueue[0].ProcessReq();
                        mInsQueue.RemoveAt(0);
                    }
                    else
                    {
                        mIsBlock = false;
                        yield break;
                    }
                }
            }

            for (int i = 0; i < mInsQueue.Count; i++)
            {
                mInsQueue[i].Block();
            }

            mInsQueue.Clear();
            mIsBlock = false;
        }

        public void BlockInsQueueDone()
        {
            mIsBlock = false;
        }

        public void BlockInsQueue()
        {
            mIsBlock = true;
			if(mUpQueueRoutine != null)
			{
				mUpQueueRoutine.Block();
				mUpQueueRoutine = null;
			}

			for(int i= 0;i<mInsQueue.Count;i++)
			{
				mInsQueue[i].Block();
			}
			
			mInsQueue.Clear();
        }

        public GOInsUnit CreateInsUnit(GOPoolUnit pool)
        {
            if (pool == null || pool.UnitSource == null)
                return null;
            GameObject target =(GameObject) GameObject.Instantiate(pool.UnitSource);
            GOPoolTag tag = target.AddComponent<GOPoolTag>();
            tag.PoolName = pool.PoolTag;
            GOInsUnit unit = new GOInsUnit();
            unit.init(target, true, GOPoolManager.Instance._MyTransform);
			pool._AddCaches (unit);
			#if UM
			TUT.TutProfiler.UsageMemoryDetail usage = TUT.TutProfiler.GetAssetUsageMemory();
			string log = "IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIInstantiate Asset: "+TUT.TutProfiler.toMemoryString(usage.total_size) +'\n';
			log+= pool.PoolTag+'\n';
			log+= usage.ToString();
			Debug.Log(log);
			#endif

            return unit;
        }

        public IEnumerator CreateInsUnit_Coroutine(GOPoolUnit pool)
        {
            if (pool == null || pool.UnitSource == null||mIsBlock)
                yield break;
            GoInsReq req = CreateInsReq(pool);
            TutRoutine req_routine = TutCoroutine.Instance.Oh_StartCoroutine(req.WaitProcess());
			req_routine.ClientBlockObj = req;
			req_routine.BlockCB = (object obj) => {
				BlockReq (obj as GoInsReq);
			};
			yield return req_routine;
            yield return req_routine.Waiting;

            if (mIsBlock || req_routine.Result == null)
            {
                req = null;
                yield break;
            }
            yield return CreateInsUnit(pool);
        }
    }

    public class GOPoolUnit
    {
        private GameObject mSource = null;

        public GameObject UnitSource
        {
            get
            {
                return mSource;
            }
        }

		public bool IsValid
		{
			get
			{
				return mSource != null;
			}
		}
        
        private List<GOInsUnit> mCaches = null;

        private List<GOInsUnit> mTemps = new List<GOInsUnit>();

        private string mTag = string.Empty;

        public string PoolTag
        {
            get
            {
                return mTag;
            }
        }

        public GOPoolUnit(GameObject source,string pool_name= null)
        {
            mSource = source;
            mTag = pool_name;
        }

		public GOPoolUnit(string pool_name)
		{
			mSource = null;
			mTag = pool_name;
		}

		public void SetUnitSource(GameObject source)
		{
			mSource = source;
		}
		
		public GameObject Pop(string temp_tag)
        {
            if(string.IsNullOrEmpty(temp_tag))
            {
                return GetFreeCache();
            }
            else
            {
                GOInsUnit unit = GetTempUnit(temp_tag);
                if(unit == null)
                {
                    return GetFreeCache();
                }
                else
                {
                    return unit.Cache;
                }
            }
        }


        private GameObject GetFreeCache()
        {
            GameObject result = null;
            if (mCaches == null || mCaches.Count == 0)
            {
                result = AddInsUnit();  
            } 
            else
            {
                int index = mCaches.FindIndex(i => !i.Activated);
                if (index >= 0)
                {
                    mCaches [index].Activated = true;
                    result = mCaches [index].Cache;
                } 
                else
                {
                    if (result == null)
                    {
                        result = AddInsUnit();  
                    }
                }
            }
            if(result != null)
            {
                result.SetActive(true);
                result.transform.parent = null;
                ResetTransform(result.transform);
            }
            else
            {
                Debug.LogError(TutNorm.LogErrFormat("GOPoolUnit ",mSource.name +"  spawn failed !!!!"));
            }
            return result;
        }

        private IEnumerator GetFreeCache_Coroutine()
        {
			while(!IsValid)
			{
				yield return 0;
			}
            GameObject result = null;
            if(mCaches == null || mCaches.Count == 0)
            {
                TutRoutine rotine = TutCoroutine.Instance.Oh_StartCoroutine(AddInsUnit_Coroutine());
				yield return rotine;
                yield return rotine.Waiting;
                if((rotine.Result == null || rotine.Result.GetType() != typeof(GameObject)) )
                {
                    yield break;
                }
                else
                {
                    result = rotine.Result as GameObject;
                }
            }
            else
            {
                int index = mCaches.FindIndex(i=> !i.Activated);
                if( index >= 0)
                {
                    mCaches[index].Activated = true;
                    result = mCaches[index].Cache;
                }
                else
                {
                    TutRoutine rotine = TutCoroutine.Instance.Oh_StartCoroutine(AddInsUnit_Coroutine());
					yield return rotine;
                    yield return rotine.Waiting;
                    if((rotine.Result == null || rotine.Result.GetType() != typeof(GameObject)) )
                    {
                        yield break;
                    }
                    else
                    {
                        result = rotine.Result as GameObject;
                    }
                }
            }
            
            if(result != null)
            {
//                result.SetActive(true);
//                result.transform.parent = null;
//                ResetTransform(result.transform);
            }
            else
            {
                Debug.LogError(TutNorm.LogErrFormat("GOPoolUnit ",mSource.name +"  spawn failed !!!!"));
            }
            yield return result;
        }

        private GOInsUnit GetTempUnit(string temp_tag)
        {
            if (mTemps.Count == null)
                return null;
            int index = mTemps.FindIndex(i => i.TempTag == temp_tag);
            if (index >= 0)
            {
                GOInsUnit unit = mTemps[index];
                unit.TempTag = string.Empty;
                mTemps.RemoveAt(index);
                return unit;
            }
            return null;
        }

        private GameObject AddInsUnit()
        {
            if(mCaches == null)
            {
                mCaches = new List<GOInsUnit>();
            }
            GOInsUnit unit = GoInstantiate.Instance.CreateInsUnit(this);
//			unit.Activated = true;
            return unit.Cache;
        }

        public IEnumerator Pop_Coroutine(string temp_tag)
        {
            if(string.IsNullOrEmpty(temp_tag))
            {
                TutRoutine routine =  TutCoroutine.Instance.Oh_StartCoroutine(GetFreeCache_Coroutine());
				yield return routine;
                yield return routine.Waiting;
                yield return routine.Result;
                yield break;
            }
            else
            {
                GOInsUnit unit = GetTempUnit(temp_tag);
                if(unit == null)
                {
                    TutRoutine routine =  TutCoroutine.Instance.Oh_StartCoroutine(GetFreeCache_Coroutine());
					yield return routine;
                    yield return routine.Waiting;
                    yield return routine.Result;
                    yield break;
                }
                else
                {
                    yield return unit.Cache;
                    yield break;
                }
            }
        }

        private IEnumerator AddInsUnit_Coroutine()
        {
            if(mCaches == null)
            {
                mCaches = new List<GOInsUnit>();
            }
            TutRoutine rotine = TutCoroutine.Instance.Oh_StartCoroutine(GoInstantiate.Instance.CreateInsUnit_Coroutine(this));
			yield return rotine;
            yield return rotine.Waiting;
            if (rotine.Result != null)
            {
                GOInsUnit unit = rotine.Result as GOInsUnit;
                if(unit != null)
                {
//					unit.Activated = true;
                    yield return unit.Cache;
                    yield break;
                }
            }
        }

		public void _AddCaches(GOInsUnit unit)
		{
			if(unit != null)
				mCaches.Add(unit);
		}

		public int CacheCount
		{
			get
			{
				if(mCaches == null)
					return 0;
				return mCaches.Count;
			}
		}

        public bool Push( GameObject target,string temp_tag)
        {
            if(mCaches == null )
                return false;
            
            int index = mCaches.FindIndex(i => i.Cache == target);
            if (index >= 0)
            {
                if(string.IsNullOrEmpty(temp_tag))
                {
                    ResetInsUnit(mCaches[index],GOPoolManager.Instance._MyTransform);
                }
                else
                {
                    mCaches[index].TempTag = temp_tag;
                    mCaches[index].Cache.transform.parent = null;
                    if(!mTemps.Contains(mCaches[index]))
                        mTemps.Add(mCaches[index]);
                }
                return true;
            }
            return false;
        }

        private void ResetInsUnit(GOInsUnit unit,Transform bind_node)
        {
            if (unit == null)
                return;
            unit.Activated = false;
            unit.TempTag = string.Empty;
            Binding(unit.Cache,bind_node);
        }

		public void DestroyInsUnit(GameObject target)
		{
			if (target == null)
				return;
			GOInsUnit unit = null;
			int index = mCaches.FindIndex(i => i.Cache == target);
			if (index >= 0)
			{
				unit = mCaches[index];
				unit.Activated = false;
				unit.TempTag = string.Empty;	
				unit.Cache.SetActive(false);
				unit.Cache.transform.parent = null;
				GameObject.Destroy (unit.Cache);
				unit.Cache = null;
				mCaches.RemoveAt(index);
			}
		}

        public static void Binding(GameObject target,Transform bind_node)
        {
            target.SetActive(false);
            target.transform.parent = bind_node;
            ResetTransform(target.transform);
        }

        public void ClearTemp()
        {
            if (mTemps.Count == 0)
                return;
            for (int i = 0; i<mTemps.Count; i++)
            {
                ResetInsUnit(mTemps[i],GOPoolManager.Instance._MyTransform);
            }
            mTemps.Clear();
        }

        public void ClearPool()
        {
            GoInstantiate.Instance.BlockPoolInQueue(this);

            if(mCaches == null)
                return ;

            ClearTemp();

            for(int i = 0;i < mCaches.Count;i++)
            {
                if(mCaches[i].Cache != null)
                {
                    mCaches[i].Cache.transform.parent = null;
                    GameObject.Destroy( mCaches[i].Cache);
                }
                mCaches[i].Cache = null;
            }
            mCaches.Clear();
        }
        
        public void DestroyPool()
        {
            ClearPool();
            mSource = null;
        }

		public static void ResetTransform(Transform trf)
        {
			trf.localPosition = Vector3.zero;
            trf.localRotation = Quaternion.identity;
            trf.localScale = new Vector3(1,1,1);
        }
    }



}
