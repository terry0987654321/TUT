using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUT.RSystem;

namespace TUT
{
    public class TutResAdapter : RSBldReqAdapter
    {
        public override void CaptureErr(RSFileInfo info, RSBldRequester.ReqErrorType type)
        {
            base.CaptureErr(info, type);
        }

        public override void RegisterRequest(RSBldRequester requester)
        {
            if (requester == null)
                return;
            TUT.TutCoroutine.Instance.Oh_StartCoroutine(requester.DoLoad());
        }

        public override bool SaveAssetToLocalPath(RSFileInfo info, byte[] bytes)
        {
            return base.SaveAssetToLocalPath(info, bytes);
        }

        public override bool TestAssetValidness(RSFileInfo info)
        {
            return base.TestAssetValidness(info);
        }

        public override void TryRepairAsset(RSBldReqInfo info)
        {
            base.TryRepairAsset(info);
        }
    }



    public class TutResources : TutSingletonBehaviour<TutResources>
    {
		public delegate IEnumerator PreCacheCallBack( Object obj);

        private List<RSBldRequester> mRSBldReqers = new List<RSBldRequester>();

		private Dictionary<string,RSType> mManifestList = new Dictionary<string, RSType> ();

        private TutResAdapter mAdapter = new TutResAdapter();

        private Dictionary<string,RSBldReqInfo> mReqInfoMap = new Dictionary<string, RSBldReqInfo>();

        private List<string> mReqQueue = new List<string>();

        private RSBldRequester mTemReqer = null;

        private int mTmpIndex = -1;

        private RSManifest mReleaseManifest = null;

        private bool mUpdatingQueue = false;

		private bool mIsBlocking = false;

        public static readonly string SteamManifestName = "steam_manifest.txt";
        public static readonly string ResManifestName = "res_manifest.txt";

#if UNITY_EDITOR
        private RSManifest mDevelopManifest = null;

        private RSManifest DevelopManifest
        {
            get
            {
                if(mDevelopManifest == null)
                {
                    mDevelopManifest = TutFileUtil.ReadJsonFile<RSManifest>(TutResourceCfg.Instance.GetDevelopManifestPath());
                    if(mDevelopManifest == null)
                        mDevelopManifest = new RSManifest();
                }
                return mDevelopManifest;
            }
        }
#endif

        public RSManifest Manifest
        {
            get
            {
#if UNITY_EDITOR
                if(TutResourceCfg.Instance.SupportDevelopMode())
                {
                    return DevelopManifest;
                }
                return mReleaseManifest;
#else
                return mReleaseManifest;
#endif
            }
        }

        public override bool IsGlobal()
        {
            return true;
        }

        public override bool IsAutoInit()
        {
            return false;
        }

        public override void Initialize(InitializeFinishHandle finish = null,object param = null)
        {
            if (Initialized)
                return;
            mFinishHandle = finish; 
            InitRequester();
            mManifestList.Add ("Assets/OH_Build_Tmp/Resources/"+ResManifestName , RSType.RT_RESOURCES);
            mManifestList.Add ("Assets/OH_Build_Streamanifest/Resources/"+SteamManifestName , RSType.RT_RESOURCES);
			TutRoutine routine = TUT.TutCoroutine.Instance.Oh_StartCoroutine (LoadAllManifest ());
			routine.ResultCB = (object result) => {

                InitializeFinishHandle handle = mFinishHandle;
                mFinishHandle = null;
                if (handle != null)
                {
                    handle();
                }
                mValid = true;
			};
        }

        public override IEnumerator InitializeCoroutine(InitializeFinishHandle finish = null,object param = null)
        {
            if (Initialized)
                yield break;
            Initialize(finish, param);
            yield break;
        }

		public void AdditionManifest(string manifest_guid)
		{
            if(mManifestList.ContainsKey(manifest_guid))
				return;
            mManifestList.Add (TutResourceCfg.Instance.ExternalUrl() + manifest_guid, RSType.RT_BUNDLE);
		}

		private IEnumerator LoadAllManifest()
		{
			TutRoutine routine = null;
			foreach(KeyValuePair<string,RSType> m in mManifestList)
			{
				routine = TUT.TutCoroutine.Instance.Oh_StartCoroutine(AdditionManifestCoroutine(m.Key,m.Value));
				yield return routine.Waiting;
			}
		}

		public IEnumerator AdditionManifestCoroutine(string manifest_path,RSType type)
        {
//            if (!Initialized)
//                yield break;
			TextAsset manifest = null;
			RSManifest rsm = null;
			switch(type)
			{
			case RSType.RT_RESOURCES:
            case RSType.RT_STREAM:
				manifest = Resources.Load<TextAsset>( RSFileInfo.GetResourceLoadPath(manifest_path));
				if(manifest != null)
				{
					rsm =TutFileUtil.ReadJsonString<RSManifest>(manifest.text);
					if(rsm != null)
					{
						if(Manifest == null)
						{
							mReleaseManifest = new RSManifest();
						}
						Manifest.CombineManifest(rsm);
					}
					else
					{
						Debug.LogError(TutNorm.LogErrFormat("AdditionManifest",manifest_path+"  : Cannt be load"));
					}
				}
				break;
			case RSType.RT_BUNDLE:
				RSFileInfo info = new RSFileInfo();
				info.path = manifest_path;
				info.type = (int)type;
                string p = manifest_path.Replace("\\", "/");
                int las = p.LastIndexOf("/");
                info.guid = p.Substring(las+1);

				if(Manifest == null)
				{
					mReleaseManifest = new RSManifest();
				}
				Manifest.PackFileInfo(info);
				OhResResult result =  Load(manifest_path);
				yield return result.Waiting;
				if(result[0] == null)
				{
					Debug.LogError(TutNorm.LogErrFormat("AdditionManifest",manifest_path+"  : Cannt be load"));
				}
				else
				{
					manifest = result[0].result as TextAsset;
					if(manifest != null)
					{
						rsm =TutFileUtil.ReadJsonString<RSManifest>(manifest.text);
						if(rsm != null)
						{
							if(Manifest == null)
							{
								mReleaseManifest = new RSManifest();
							}
							Manifest.CombineManifest(rsm);
						}
						else
						{
							Debug.LogError(TutNorm.LogErrFormat("AdditionManifest",manifest_path+"  : Cannt be parse"));
						}
					}
				}
				break;
			}
            yield break;
        }

        public IEnumerator UnloadExternalManifest(string guid)
        {
            TextAsset manifest = null;
            RSManifest rsm = null;
            if (Manifest == null)
            {
                yield return false;
                yield break;
            }
            string path = TutResourceCfg.Instance.ExternalUrl() + guid;
            RSFileInfo info = new RSFileInfo();
            info.path = path;
            info.type = (int)RSType.RT_BUNDLE;
            info.guid = guid;
            Manifest.PackFileInfo(info);
            OhResResult result =  Load(path);
            yield return result.Waiting;

            if(result[0] == null)
            {
                Debug.LogError(TutNorm.LogErrFormat("UnloadExternalManifest",path+"  : unload failed!"));
            }
            else
            {
                manifest = result[0].result as TextAsset;
                if(manifest != null)
                {
                    rsm =TutFileUtil.ReadJsonString<RSManifest>(manifest.text);
                    if(rsm != null)
                    {
                        RSFileInfo finfo = null;
                        RSFileInfo bfinfo = null;
                        for(int i = 0;i<rsm.files.Count;i++)
                        {
                            bfinfo = rsm.files[i];
                            if(bfinfo.rstype != RSType.RT_BUNDLE)
                            {
                                continue;
                            }
                            finfo = Manifest.GetInfo(bfinfo.path);
                            if(finfo != null)
                                continue;
                            TutFileUtil.DeleteFile(TutResourceCfg.Instance.ExternalPath() + bfinfo.guid);
                        }

                        TutFileUtil.DeleteFile(TutResourceCfg.Instance.ExternalPath() + guid);
                        yield return true;
                        yield break;
                    }
                    else
                    {
                        Debug.LogError(TutNorm.LogErrFormat("UnloadExternalManifest",path+"  : Cannt be parse"));

                    }
                }
            }
            yield return false;
        }

        public OhResResult Load(string path)
        {
			if (!Initialized || mIsBlocking)
                return null;
            if (string.IsNullOrEmpty(path))
                return null;
            OhResResult result = new OhResResult(new string[]{path});
            result._InitRSBldReq(TutCoroutine.Instance.Oh_StartCoroutine(_LoadCoroutine(result)));
            return result;
        }

        public OhResResult Load(string[] paths)
        {
			if (!Initialized || mIsBlocking)
                return null;
            if(paths == null || paths.Length == 0)
                return null;
            OhResResult result = new OhResResult(paths);
            result._InitRSBldReq( TutCoroutine.Instance.Oh_StartCoroutine(_LoadCoroutine(result)));
            return result;
        }

        private void _Load(OhResResult result)
        {
            RSBldReqInfo req_info = null;
            Object asset = null;
            string path = string.Empty;
            RSFileInfo file_info = null;
            bool need_update = false;
            for(int i = 0;i<result.ReqPaths.Length;i++)
            {

                req_info = null;
                path = result.ReqPaths[i];
//				Debug.Log(RSFileInfo.GetResourceLoadPath(path));
//				Debug.LogWarning(path);
                if(!LoadLocalAssets(path,out asset))
                {
                    file_info = Manifest.GetInfo(path);
                    if(file_info == null)
                    {
                        result._RequestFinish(path,asset,null);
                        Debug.LogError(TutNorm.LogErrFormat("Resource Load",path+"  Not Defined !!!!"));
                        continue;
                    }
                    req_info= GetQueueReqInfo(path);
                    if(req_info == null)
                    {
                        req_info = new RSBldReqInfo();
                        req_info.info = file_info;
                    }
                    req_info.on_finish += result._RequestFinish;
                }
                else
                {
                    result._RequestFinish(path,asset,null);
                }
                need_update = PushQueue(req_info)?true:need_update;
            }
            if (need_update)
            {
                StartUpdateQueue();
            }
            req_info = null;
            asset = null;
            path = string.Empty;
            file_info = null;
        }

		public float LoadAssetTotalTime = 0;

        private IEnumerator _LoadCoroutine(OhResResult result)
        {
            _Load(result);
            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine(result._WaittingResult()).Waiting;
            yield return result;
        }

        private bool LoadLocalAssets(string path,out Object result)
        {
            result = null;
#if UNITY_EDITOR
            if(TutResourceCfg.Instance.SupportDevelopMode())
            {
                if(RSInfo.isResTypeFromPath(path))
                {
                    result = Resources.Load(RSFileInfo.GetResourceLoadPath(path));
                    return true;
                }
                else
                {
                    RSFileInfo info =  Manifest.GetInfo(path);
                    if(info == null)
                        return false;
                    if(info.rstype == RSType.RT_RESOURCES)
                    {
                        result = UnityEditor.AssetDatabase.LoadAssetAtPath(path,typeof(Object));
                        return result != null;
                    }
                    return false;
                }
            }
            else
            {
                if(RSInfo.isResTypeFromPath(path))
                {
                    result = Resources.Load(RSFileInfo.GetResourceLoadPath(path));
                    return result != null;
                }
                else
                {
					RSFileInfo info =  Manifest.GetInfo(path);
					if(info == null)
						return false;
					if(info.rstype == RSType.RT_RESOURCES)
					{
						result = Resources.Load(RSFileInfo.GetResourceLoadPath(path));//.LoadAssetAtPath(path,typeof(Object));
						return result != null;
					}
                    return false;
                }
            }
            return false;
#else
			Debug.Log(path);
			if(RSInfo.isResTypeFromPath(path))
			{
				result = Resources.Load(RSFileInfo.GetResourceLoadPath(path));
				return result != null;
			}
			else
			{
				result = Resources.Load(RSFileInfo.GetResourceLoadPath(path));
				if(result != null)
				{
					return true;
				}
				else
				{
					RSFileInfo info =  Manifest.GetInfo(path);
					if(info == null)
						return false;
					if(info.rstype == RSType.RT_RESOURCES)
					{
						result = Resources.LoadAssetAtPath(path,typeof(Object));
						return result != null;
					}
					return false;
				}
			}

#endif
        }

        public OhResResult InstantiateOrLoad(string path,string temp_tag = null)
        {
			if (!Initialized|| mIsBlocking)
                return null;
            if (string.IsNullOrEmpty(path))
                return null;
            OhResResult result = new OhResResult(new string[]{path},true,string.IsNullOrEmpty(temp_tag)?null:new string[]{temp_tag});
            result._InitRSBldReq(TutCoroutine.Instance.Oh_StartCoroutine(_InstantiateOrLoadCoroutine(result)));
            return result;
        }
        
		public OhResResult InstantiateOrLoad(string[] paths,string[] temp_tags = null,bool keep_active_state = true,PreCacheCallBack per_cache = null)
        {
			if (!Initialized|| mIsBlocking)
                return null;
            if(paths == null || paths.Length == 0)
                return null;
            OhResResult result = null;
            if (temp_tags == null || temp_tags.Length == 0)
            {
                result = new OhResResult(paths,true);
            }
            else
            {
                result = new OhResResult(paths,true,temp_tags);
            }
			result.KeepActiveState = keep_active_state;
			result._InitRSBldReq(TutCoroutine.Instance.Oh_StartCoroutine(_InstantiateOrLoadCoroutine(result,per_cache)));
            return result;
        }
        
		private IEnumerator _InstantiateOrLoadCoroutine(OhResResult result,PreCacheCallBack per_cache = null)
        {
            string[] caches = result.ReqCachePaths;
            string[] non_caches = result.ReqNoCachePaths;
            TutRoutine routine = null;
            if (non_caches != null)
            {

				for(int i = 0;i<non_caches.Length;i++)
				{
					if(non_caches[i].Contains(".prefab"))
						GOPoolManager.Instance.NewPool_NoPrefab(non_caches[i]);
				}

                OhResResult sub = TutResources.Instance.Load(non_caches);
				sub.ResultCB=(OhResResult sub_result)=>{
					foreach(TutResResultData data in sub)
					{



						if(data.result != null && data.result.GetType() == typeof (GameObject))
						{
							GOPoolManager.Instance.SetPoolSource(data.path,data.result as GameObject);
							//                        GOPoolManager.Instance.NewPool(data.result as GameObject,data.path);
							//						Debug.Log("[     LOAD      ] "+data.path);
						}
						else
						{
							GOPoolManager.Instance.RemoveInvalidPool(data.path);
						}
					}
				};
				yield return sub.Waiting;
              
                routine  =  GOPoolManager.Instance.Instantiate(non_caches,result.ReqNoCacheTags);
				yield return routine;
                yield return routine.Waiting;
                List<GameObject> objs = routine.Result as List<GameObject>;
                if(objs != null)
                {
					for(int i = 0;i<objs.Count;i++)
					{
						if(per_cache != null)
						{
							TUT.TutRoutine r = TUT.TutCoroutine.Instance.Oh_StartCoroutine( per_cache(objs[i] as Object));
							yield return r.Waiting ;
							if( r.Result != null)
							{
								GameObject o = r.Result as GameObject;
								if(o != null)
								{
									objs[i] = o;
								}
							}
						}
					}
                    result.PushNonCacheResults(objs.ToArray());
                }
            }
            
            if (caches != null)
            {
                routine  =  GOPoolManager.Instance.Instantiate(caches,result.ReqCacheTags);
				yield return routine;
                yield return routine.Waiting;
                List<GameObject> objs = routine.Result as List<GameObject>;
                if(objs != null)
                {
                    result.PushCacheResults(objs.ToArray());
                }
            }
            result.PushEnd();
        }

		public TutRoutine Cache(string path,PreCacheCallBack per_cache = null)
        {
			if (!Initialized|| mIsBlocking)
                return null;
            if (string.IsNullOrEmpty(path))
                return null;
            OhResResult result = new OhResResult(new string[]{path},true);
			return TutCoroutine.Instance.Oh_StartCoroutine(_CacheCoroutine(result,per_cache));
        }

		public TutRoutine Cache(string[] paths,PreCacheCallBack per_cache = null)
        {
			if (!Initialized|| mIsBlocking)
                return null;
            if(paths == null || paths.Length == 0)
                return null;
            OhResResult result = new OhResResult(paths,true);
            return TutCoroutine.Instance.Oh_StartCoroutine(_CacheCoroutine(result,per_cache));
        }

		private IEnumerator _CacheCoroutine(OhResResult result,PreCacheCallBack per_cache = null)
        {
            string[] non_caches = result.ReqNoCachePaths;
            bool success = true;
            if (non_caches != null)
            {
                OhResResult sub = TutResources.Instance.Load(non_caches);
                yield return sub.Waiting;
                foreach(TutResResultData data in sub)
                {
                    if(data.result != null && data.result.GetType() == typeof (GameObject))
					{
						if(per_cache != null)
						{
							TUT.TutRoutine r = TUT.TutCoroutine.Instance.Oh_StartCoroutine( per_cache(data.result));
							yield return r.Waiting ;
							if( r.Result != null)
							{
								Object o = r.Result as Object;
								if(o != null)
								{
									data.result = o;
								}
							}
						}
                        GOPoolManager.Instance.NewPool(data.result as GameObject,data.path);
					}
                    else
                        success = false;
                }
            }
            result.PushEnd();
            yield return success; 
        }

        public GameObject Instantiate(string path,string temp_tag = null)
        {
			if(mIsBlocking)
				return null;
            return GOPoolManager.Instance.Instantiate(path,temp_tag);
        }

        public GameObject InstantiateNoCache(string path)
        {
			if(mIsBlocking)
				return null;
            return GOPoolManager.Instance.InstantiateNoCache(path);
        } 

        public OhResResult Instantiate(string[] paths,string[] temp_tags = null)
        {
			if (!Initialized|| mIsBlocking)
                return null;
            if(paths == null || paths.Length == 0)
                return null;
            OhResResult result = null;
            if (temp_tags == null || temp_tags.Length == 0)
            {
                result = new OhResResult(paths,true);
            }
            else
            {
                result = new OhResResult(paths,true,temp_tags);
            }
            result._InitRSBldReq( TutCoroutine.Instance.Oh_StartCoroutine(InstantiateCoroutine(result)));
            return result;
        }

        private IEnumerator InstantiateCoroutine(OhResResult result)
        {
            string[] caches = result.ReqCachePaths;
            TutRoutine routine = null;

            if (caches != null)
            {
                routine  =  GOPoolManager.Instance.Instantiate(caches,result.ReqCacheTags);
                yield return routine.Waiting;
                List<GameObject> objs = routine.Result as List<GameObject>;
                if(objs != null)
                {
                    result.PushCacheResults(objs.ToArray());
                }
            }
            result.PushEnd();
        }

        public static void Destroy(GameObject obj ,string temp_tag)
        {
			if(GOPoolManager.isValid)
            	GOPoolManager.Instance.Destroy(obj,temp_tag);
        }

		public static void DestroyAndRemoveCache(GameObject obj)
		{
			if(GOPoolManager.isValid)
				GOPoolManager.Instance.RealDestroy(obj);
		}

        private void InitRequester()
        {
            int count = TutResourceCfg.Instance.MaxRequesterCount <= 0 ? 1 : TutResourceCfg.Instance.MaxRequesterCount;
            for (int i = 0; i < count; i++)
            {
                mRSBldReqers.Add(new RSBldRequester(mAdapter));
            }
            mInitialized = true;
        }

        private RSBldRequester GetFreeReqer()
        {
            mTmpIndex = mRSBldReqers.FindIndex(i => !i.isLoading);
            if (mTmpIndex < 0)
                return null;
            return mRSBldReqers [mTmpIndex];
        }

        private bool PushQueue(RSBldReqInfo info)
        {
            if (info == null || info.info == null)
                return false;
            if (mReqQueue.Contains(info.info.path))
                return false;
            mReqInfoMap.Add(info.info.path,info);
            mReqQueue.Add(info.info.path);
            return true;
        }

        private RSBldReqInfo GetQueueReqInfo(string path)
        {
            RSBldReqInfo info = null;
            mReqInfoMap.TryGetValue(path, out info);
            return info;
        }

        private IEnumerator UpdateQueue()
        {
			if(mIsBlocking)
				yield break;
            while (mReqQueue.Count != 0)
            {
				if(mIsBlocking)
					yield break;
#if UNITY_EDITOR
                if(TutResourceCfg.Instance.SupportDevelopMode())
                {
                    yield return TutTimeUtil.WaitForSeconds((float)TutResourceCfg.Instance.DevelopSimulateLoadTime).Waiting;
					if(mIsBlocking)
						yield break;
                    mReqInfoMap[mReqQueue[0]].on_finish(mReqInfoMap[mReqQueue[0]].info.path,UnityEditor.AssetDatabase.LoadAssetAtPath( mReqInfoMap[mReqQueue[0]].info.path,typeof(Object)),null);
                    mReqInfoMap.Remove(mReqQueue[0]);
                    mReqQueue.RemoveAt(0);
					yield return TutTimeUtil.WaitForSeconds((float)TutResourceCfg.Instance.RequestRefreshCycle).Waiting;
                }
                else
                {
                    mTemReqer = GetFreeReqer();
                    if (mTemReqer != null)
                    {
                        mTemReqer.Request(mReqInfoMap[mReqQueue[0]]);
                        mReqInfoMap.Remove(mReqQueue[0]);
                        mReqQueue.RemoveAt(0);
                    }
					yield return TutTimeUtil.WaitForSeconds((float)TutResourceCfg.Instance.RequestRefreshCycle).Waiting;
                }
#else
                mTemReqer = GetFreeReqer();
                if (mTemReqer != null)
                {
                    mTemReqer.Request(mReqInfoMap[mReqQueue[0]]);
                    mReqInfoMap.Remove(mReqQueue[0]);
                    mReqQueue.RemoveAt(0);
                }
				yield return TutTimeUtil.WaitForSeconds((float)TutResourceCfg.Instance.RequestRefreshCycle);
#endif
            }
        }

        private void StartUpdateQueue()
        {
            if(!mUpdatingQueue)
            {
                TUT.TutCoroutine.Instance.Oh_StartCoroutine(ExeUpdateQueue());
            }
        }

        private IEnumerator ExeUpdateQueue()
        {
            mUpdatingQueue = true;
			yield return TutTimeUtil.WaitForSeconds((float)TutResourceCfg.Instance.RequestRefreshCycle).Waiting;
            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine(UpdateQueue()).Waiting;
            mUpdatingQueue = false;
			mIsBlocking = false;
        }

		public TutRoutine Block()
		{
			if (!Initialized || mIsBlocking)
				return null;
			return TutCoroutine.Instance.Oh_StartCoroutine(BlockCoroutine());
		}

		private IEnumerator BlockCoroutine()
		{
			mIsBlocking = true;

			if(mReqInfoMap.Count != 0)
			{
				mReqInfoMap.Clear();
			}
			if(mReqQueue.Count != 0)
			{
				mReqQueue.Clear();
			}
			int block_count = mRSBldReqers.Count;
			for(int i = 0;i<mRSBldReqers.Count;i++)
			{
				mRSBldReqers[i].Block(()=>{
					block_count --;
				});
			}
			while(block_count != 0)
			{
				yield return false;
			}

            TUT.RSystem.GoInstantiate.Instance.BlockInsQueue();
            if (!mUpdatingQueue)
            {
                mIsBlocking = false;
                //TUT.RSystem.GoInstantiate.Instance.BlockInsQueueDone();
                yield return true;
                yield break;
            }

			yield return true;
		}

//        private RSBldReqInfo NewReqInfo(string path)
//        {
//            RSBldReqInfo info = new RSBldReqInfo();
//            info.info = RSManifest
//        }
    }
}