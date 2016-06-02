using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUT.RSystem;

namespace TUT
{
    public class TutResResultData
    {
        public string path;
        public Object result;
        public object param;
        
        public TutResResultData(string _path,Object _result,object _param)
        {
            path = _path;
            result = _result;
            param = _param;
        }
        
        public void Clear()
        {
            path = string.Empty;
            result = null;
            param = null;
        }
    }
    /// <summary>
    ///  资源请求结果反馈
    /// 
    ///     请求结果反馈支持以同步的回调方式 获取结果 
    ///     也支持以协同的方式获取结果
    ///     默认情况下反馈结果在结果被调用完毕时会自动销毁资源索引关系
    ///     或者手动调用Destroy函数  来清理 资源索引关系
    /// </summary>
    public class OhResResult : IEnumerable,IEnumerator,System.IDisposable
    {
        ~OhResResult()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Destroy();
            }
        }
        
        public delegate void ReqResultCallback(OhResResult result);
        
        private List<TutResResultData> mData = null;

        private List<int> mCacheds = null;

        private List<int> mNotCacheds = null;

        private string[] mReqPaths = null;

		public bool KeepActiveState = true; 

        public TutResResultData this[int index]
        {
            get
            {
                if(mData == null || mData.Count <= index)
                    return null;
                return mData[index];
            }
        }


        public string[] ReqPaths
        {
            get
            {
                return mReqPaths;
            }
        }

        private string[] mCacheTempTags = null;

        public string[] CacheTempTags
        {
            get
            {
                return mCacheTempTags;
            }
        }

        public string[] ReqCachePaths
        {
            get
            {
                if(!mIsCache)
                    return null;

                if(mCacheds == null || mCacheds.Count == 0)
                    return null;

                string[] result = new string[mCacheds.Count];
                for(int i = 0; i< mCacheds.Count;i++)
                {
                    result[i] = mReqPaths[mCacheds[i]];
                }
                return result;
            }
        }

        public string[] ReqCacheTags
        {
            get
            {
                if(!mIsCache)
                    return null;
                if(mCacheds == null || mCacheds.Count == 0)
                    return null;
                
                if(mCacheTempTags == null || mCacheTempTags.Length == 0)
                    return null;

                string[] result = new string[mCacheds.Count];
                for(int i = 0; i< mCacheds.Count;i++)
                {
                    result[i] = mCacheTempTags[mCacheds[i]];
                }
                return result;
            }
        }

        public string[] ReqNoCachePaths
        {
            get
            {
                if(!mIsCache)
                    return null;

                if(mNotCacheds == null || mNotCacheds.Count == 0)
                    return null;

                string[] result = new string[mNotCacheds.Count];
                for(int i = 0; i< mNotCacheds.Count;i++)
                {
                    result[i] = mReqPaths[mNotCacheds[i]];
                }
                return result;
            }
        }

        public string[] ReqNoCacheTags
        {
            get
            {
                if(!mIsCache)
                    return null;
                
                if(mNotCacheds == null || mNotCacheds.Count == 0)
                    return null;

                if(mCacheTempTags == null || mCacheTempTags.Length == 0)
                    return null;
                
                string[] result = new string[mNotCacheds.Count];
                for(int i = 0; i< mNotCacheds.Count;i++)
                {
                    result[i] = mCacheTempTags[mNotCacheds[i]];
                }
                return result;
            }
        }
        
        private string mResultTmpPath = string.Empty;

        private object mResultTmpParam = null;

        private Object mResultTmp = null;
        
        private bool mWaitReqFinish = true;
        
        private ReqResultCallback mResult = null;
        //        private bool mAutoDestroy = true;
        private int mPosition = -1;
        
        private TutRoutine mRoutine = null;

        private IEnumerator mPushResultIEnum = null;

        private bool mIsCache = false;
        
        public bool IsDone
        {
            get
            {
                if(mRoutine == null)
                    return true;
                if(mRoutine.IsDone)
                {
                    mRoutine = null;
                    return true;
                }
                return false;
            }
        }

		public void Block()
		{
			if(mRoutine == null)
				return;
			mRoutine.Block ();
			mRoutine = null;
		}
        
        public Coroutine Waiting
        {
            get
            {
                if(mRoutine == null)
                    return null;
                return mRoutine.Waiting;
            }
        }
        
        public ReqResultCallback ResultCB
        {
            get
            {
                return mResult;
            }
            set
            {
                if(mRoutine == null)
                    return;
                mResult = value;
				if(mResult != null)
				{
					mRoutine.ResultCB = (object result)=>{
						mResult(this);
						mResult = null;
						mRoutine = null;
					};
				}
				else
				{
					mRoutine.ResultCB = null;
				}
            }
        }
        
        /// <summary>
        ///  初始化请求结果（用于协同方式）
        /// </summary>
        /// <param name="paths">Paths.</param>
        public OhResResult(string[] paths,bool is_cache = false,string[] tempTag = null)
        {
            mIsCache = is_cache;
            if (tempTag != null && mIsCache)
            {
                string[] tags = new string[paths.Length];
                for(int i = 0; i<tags.Length;i++)
                {
                    if(i<tempTag.Length)
                    {
                        tags[i] = tempTag[i];
                    }
                    else
                        tags[i] = string.Empty;
                }
                mCacheTempTags = tags;
            }
            else
                mCacheTempTags = null;
            initResult(paths, false);
        }
        
        public void _InitRSBldReq(TutRoutine routine)
        {
            mRoutine = routine;
        }  
        
        private void _Destroy()
        {
            //            mWaitReqFinish = true;
            if (mData != null)
            {
                for (int i = 0; i<mData.Count; i++)
                {
                    mData [i].Clear();
                }
                mData.Clear();
            }
            mData = null;

            if (mCacheds != null)
            {
                mCacheds.Clear();
            }
            mCacheds = null;

            if (mNotCacheds != null)
            {
                mNotCacheds.Clear();
            }
            mNotCacheds = null;

            mIsCache = false;
            mResult = null;
            mPushResultIEnum = null;
            mRoutine = null;
        }
        
        private void initResult(string[] paths,bool auto_destroy)
        {
            mResult = null;
            //            mAutoDestroy = auto_destroy;
            mWaitReqFinish = true;
            mReqPaths = new string[paths.Length];
            paths.CopyTo(mReqPaths,0);
            mData = new List<TutResResultData>();
            if (mIsCache)
            {
                mCacheds = new List<int>();
                mNotCacheds = new List<int>();
            }
            for (int i = 0; i<mReqPaths.Length; i++)
            {
                if(mIsCache)
                {
                    if(GOPoolManager.Instance.IsContainPool(mReqPaths[i]))
                    {
                        mCacheds.Add(i);
                    }
                    else
                    {
                        mNotCacheds.Add(i);
                    }
                }
                mData.Add(new TutResResultData(mReqPaths[i],null,null));
            }
        }


        public void  PushCacheResults(GameObject[] objs)
        {
            if (!mIsCache)
                return;

            if (objs.Length != mCacheds.Count)
                return;
            mWaitReqFinish = true;
            for (int i = 0; i<mCacheds.Count; i++)
            {
				objs[i].SetActive(KeepActiveState);
                mData[mCacheds[i]].result = objs[i];
            }
        }

        public void PushNonCacheResults(GameObject[] objs)
        {
            if (!mIsCache)
                return;
            
            if (objs.Length != mNotCacheds.Count)
                return;
            mWaitReqFinish = true;
            for (int i = 0; i<mNotCacheds.Count; i++)
            {
				if(objs[i] != null)
				{
					objs[i].SetActive(true);
				}
				else
				{
					Debug.LogError(TutNorm.LogErrFormat("Resource Load","Cant cache asset : "+mData[mNotCacheds[i]].path));
				}
                mData[mNotCacheds[i]].result = objs[i];
				
            }
        }

        public void PushEnd()
        {
            mWaitReqFinish = false;
        }

        private IEnumerator PushResult()
        {
            if(mData == null)
                yield break;
            int index = -1;
            int total = mData.Count;
            for(int count = 0;count < total;count ++)
            {
                if(mData == null)
                    yield break;
                
                index = mData.FindIndex(i=>i.path==mResultTmpPath);
                if(index >=0)
                {
                    mData[index].result = mResultTmp;
                    mData[index].param = mResultTmpParam;
                }
                else
                {
                    Debug.Log(TutNorm.LogErrFormat("Bundle Request"," request failed: miss a asset : "+mResultTmpPath));
                }

                if(count >= total -1)
                    break;
                yield return 0;
            }
            
            if(mData == null)
                yield break;
            
            //            if (mResult != null)
            //            {
            //                mResult(mData.ToArray());
            ////                if(mAutoDestroy)
            ////                    Destroy();
            //            }
            mWaitReqFinish = false;
        }
        
        public void _RequestFinish(string path,UnityEngine.Object obj,object param)
        {
            mResultTmpPath = path;
            mResultTmpParam = param;
            mResultTmp = obj;
			if(mResultTmp == null)
			{
				Debug.LogError(TutNorm.LogErrFormat("Resource Load Failed",path+"  Cannt be Loaded !!!!"));
			}
            if (mPushResultIEnum == null)
            {
                mPushResultIEnum = PushResult();
            }
            mPushResultIEnum.MoveNext();
            mResultTmpPath = string.Empty;
            mResultTmpParam = null;
            mResultTmp = null;
        }
        
        public IEnumerator _WaittingResult()
        {
            if (mResult != null)
            {
                mResult = null;
            }
            
            while (mWaitReqFinish)
                yield return 0;

            GameObject obj = null;
            RSystem.RSAssetReflex reflex = null;
            for (int i = 0; i<ReqPaths.Length; i++)
            {
                if(ReqPaths[i].Contains(".prefab"))
                {
                    if(mData[i].result != null)
                    {
                        obj = mData[i].result as GameObject;
						if(obj == null)
						{
							Debug.LogError(TutNorm.LogErrFormat("Load Asset"," Miss Obj "+mData[i].path));
						}
						else
						{
	                        reflex = obj.GetComponent<RSystem.RSAssetReflex>();
	                        if(reflex != null && reflex.ColDep != null)
	                        {
	                            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine(reflex.ColDep.Assemble()).Waiting;
	                        }
						}
                    }
                }
            }
#if UM
			TUT.TutProfiler.UsageMemoryDetail usage = TUT.TutProfiler.GetAssetUsageMemory();
			string log = " AAAAAAAAAAAAAAAAAAAAAsset Load UM: "+TUT.TutProfiler.toMemoryString(usage.total_size) +'\n';
			for (int i = 0; i<ReqPaths.Length; i++)
			{
				if(mData[i].result != null)
				{
					log+= ReqPaths[i]+'\n';
				}
			}
			log+= usage.ToString();
			Debug.Log(log.ToString());
#endif
        }
        
        public IEnumerator GetEnumerator ()
        {
            return (IEnumerator)this;
        }
        
        public object Current
        {
            get
            {
                if(mData == null)
                    return null;
                try
                {
                    return mData[mPosition];
                }
                catch (System. IndexOutOfRangeException)
                {
                    throw new System.InvalidOperationException();
                }
            }
        }
        
        public bool MoveNext ()
        {
            if (mData == null)
            {
                //                if(mAutoDestroy)
                //                    Destroy();
                return false;
            }
            mPosition++;
            if(mPosition >= mData.Count)
            {
                //                if(mAutoDestroy)
                //                    Destroy();
                return false;
            }
            return true;
        }
        
        public void Reset ()
        {
            //            if(mAutoDestroy)
            //                Destroy();
            mPosition = -1;
        }
        
    }
}