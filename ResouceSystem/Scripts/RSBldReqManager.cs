//using UnityEngine;
//using System.Collections;
//
//namespace TUT
//{
//    public class RSBldReqMgr : SingletonBase<RSBldReqMgr>
//    {
//        
//        public delegate void OnBlockFinish_Callback();
//        public OnBlockFinish_Callback OnBlockFinishCallBack = null;
//        
//        private List<string> mRSRQueueRefs = new List<string>();
//        private List<RSBldReqInfo> mRSRQueue = new List<RSBldReqInfo>();
//        private List<RSBldRequester> mRSBldReqers = new List<RSBldRequester>();
//        private Dictionary<string,int> mRepairList = new Dictionary<string, int>();
//        
//        private bool is_block = false;
//        
//        protected override void Init ()
//        {
//            base.Init ();
//            InitRequest();
//            update_time = -1;
//        }
//        
//        private void InitRequest()
//        {
//            mRepairList.Clear();
//            mRSBldReqers.Clear();
//            for(int i = 0; i < OhConst.AB_LoaderCount; i++)
//                mRSBldReqers.Add(new RSBldRequester());
//        }
//        
//        public void ResetManager()
//        {
//            mRepairList.Clear();
//            for(int i = 0;i<mRSRQueue.Count;i++)
//            {
//                mRSRQueue[i].on_finish = null;
//            }
//            mRSRQueueRefs.Clear();
//            mRSRQueue.Clear();
//            for(int i = 0;i<mRSBldReqers.Count;i++)
//            {
//                mRSBldReqers[i].ResetRequest();
//            }
//            update_time = -1;
//        }
//        
//        public RSBldReqInfo RequestAB(string url,RSFileInfo _info,RSBldRequester.RequestFinish finish,object param = null)
//        {
//            if(is_block)
//                return null;    
//            if(_info != null)
//            {
//                return CreateNewReq(url,_info,finish,param);
//            }
//            return null;
//        }       
//        
//        public void DownLoadAB(string url,RSFileInfo _info,RSBldRequester.RequestFinish finish,object param = null)
//        {
//            if(is_block)
//                return; 
//            if(_info != null)
//            {
//                RSBldReqInfo rinfo = CreateNewReq(url,_info,finish,param);
//                if(rinfo != null)
//                    rinfo.is_download = true;
//            }
//        }   
//        
//        public RSBldReqInfo RequestAsset(RSFileInfo _info,RSBldRequester.RequestFinish finish,object param = null)
//        {
//            if(_info == null)
//                return null;
//            switch(_info.rstype)
//            {
//                case RSType.RT_BUNDLE:
//                    return RequestAB(asset_url,version,false,finish,param,guid);
//                case RSType.RT_RESOURCES:
//                    RequsetRes(asset_url,finish,param);
//                    return null;
//                case RSType.RT_STREAM:
//                    return RequestAB(asset_url,version,true,finish,param,guid);
//            }
//            return null;
//        }       
//        
//        static public void RequsetRes(string asset_url,RSBldRequester.RequestFinish finish,object param = null)
//        {
//            
//            if(asset_url != "")
//            {
//                //          if(asset_url.EndsWith(".bat"))
//                //          {
//                //              asset_url = asset_url.Replace(".bat",".asset");
//                //          }
//                //          if(asset_url.Contains("/Tables") || asset_url.Contains("/Main"))
//                //          {
//                //              asset_url = asset_url.Replace(".bat",".txt");
//                //          }           
//                //int file_index = asset_url.LastIndexOf("/")+1 ;
//                //string url = asset_url.Substring(file_index,asset_url.LastIndexOf(".")  - file_index);
//                //AssetRequesterHodler.RequestPrefixResURL +
//                string url = asset_url;
//                if(asset_url.Contains("."))
//                {
//                    url = asset_url.Substring(0,asset_url.LastIndexOf("."));
//                    if(url.Contains("&"))
//                    {
//                        string load_path = url.Substring(0,url.LastIndexOf("/")+1);
//                        string load_file = url.Substring(url.LastIndexOf("/") + 1);
//                        load_file = load_file.Substring(load_file.LastIndexOf("&")+1);
//                        url = load_path + load_file;
//                    }
//                }
//                
//                UnityEngine.Object obj = Resources.Load( url);
//                if(obj == null)
//                {
//                    Debug.LogError("###Requset Fail!###-> The asset \""+url+"\" cannt be download! ");
//                }
//                if(finish != null)
//                {
//                    finish(asset_url,obj,param);
//                }
//            }
//        }
//        
//        public void BlockABRequest(string asset_url)
//        {
//            if(is_block)
//                return; 
//            if(mRSRQueueRefs.Contains(asset_url))
//            {
//                int index = mRSRQueueRefs.IndexOf(asset_url);           
//                mRSRQueueRefs.RemoveAt(index);
//                mRSRQueue.RemoveAt(index);           
//                return;
//            }
//            else
//            {
//                for(int i = 0;i<mRSBldReqers.Count;i++)
//                {
//                    if(mRSBldReqers[i].isLoading && mRSBldReqers[i].url == asset_url)
//                    {
//                        mRSBldReqers[i].Block();
//                        return;
//                    }
//                }
//            }
//        }
//        
//        public void Block()
//        {
//            TUT.TutProfiler.GetMemorySample();
//            is_block = true;
//            mRSRQueueRefs.Clear();
//            for(int i = 0;i<mRSRQueue.Count;i++)
//            {
//                mRSRQueue[i].on_finish = null;
//            }
//            mRSRQueue.Clear();
//            for(int i = 0;i<mRSBldReqers.Count;i++)
//            {
//                mRSBldReqers[i].Block(OnBlockFinish);
//            }       
//            OnBlockFinish();
//        }
//        
//        void OnBlockFinish()
//        {       
//            for(int i = 0;i<mRSBldReqers.Count;i++)
//            {
//                if(mRSBldReqers[i].isCoroutining)
//                    return;
//            }
//            for(int i = 0;i<mRSBldReqers.Count;i++)
//            {
//                mRSBldReqers[i].ResetRequest();
//            }   
//            is_block = false;
//            if(OnBlockFinishCallBack != null)
//                OnBlockFinishCallBack();
//        }   
//        
//        private float update_time = 0;
//        private RSBldRequester request = null;
//        private void UpdateLoadAssets()
//        {
//            if(is_block)
//                return;
//            if(Time.realtimeSinceStartup >= update_time)
//            {
//                update_time = Time.realtimeSinceStartup + OhConst.AB_UpdateLoadTime;
//                if(mRSRQueue.Count == 0)
//                {
//                    if(mHadCoroutineReq)
//                    {
//                        request = GetFreeCoroutineReq();
//                        if(request != null)
//                        {
//                            request.CancelCoroutine();
//                        }
//                        request = null;
//                    }
//                    return;
//                }           
//                
//                
//                for(int i =0;i<mRSRQueue.Count;i++)
//                {
//                    request = GetFreeRequest();
//                    if(request != null)
//                    {
//                        request.Request(mRSRQueue[i],mRSRQueue[i].on_finish);
//                        mHadCoroutineReq = true;
//                        mRSRQueueRefs.Remove(mRSRQueue[i].load_url);
//                        mRSRQueue.RemoveAt(i);
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }
//            }
//            request = null;
//        }
//        
//        public void AddReqCoroutine(RSBldRequester req)
//        {
//            if(req != null)
//            {
//                StartCoroutine( req.DoLoad());
//            }
//        }
//        
//        private RSBldRequester GetFreeRequest()
//        {
//            if(mRSBldReqers.Count == 0)
//                return null;
//            for(int i = 0;i<mRSBldReqers.Count;i++)
//            {
//                if(!mRSBldReqers[i].isLoading)
//                    return mRSBldReqers[i];
//            }
//            return null;
//        }   
//        
//        private bool mHadCoroutineReq = false;
//        
//        public RSBldRequester GetFreeCoroutineReq()
//        {
//            if(mRSBldReqers.Count == 0)
//                return null;
//            for(int i = 0;i < mRSBldReqers.Count;i++)
//            {
//                if(!mRSBldReqers[i].isLoading && mRSBldReqers[i].isCoroutining)
//                    return request;
//            }
//            mHadCoroutineReq = false;
//            return null;
//        }
//        
//        private RSBldReqInfo CreateNewReq(string url,RSFileInfo _info,RSBldRequester.RequestFinish finish,object param,bool download = false)
//        {   
//            RSBldReqInfo info = null;
//            if(!mRSRQueueRefs.Contains(_info.path))
//            {
//                mRSRQueueRefs.Add(_info.path);
//                info = new RSBldReqInfo(url,_info,finish,param);
//                mRSRQueue.Add(info);
//            }
//            return info;
//        }
//        
//        public void TryRepairAsset(string url,int version,bool stream,RSBldRequester.RequestFinish finish,object param,bool download = false,string guid = "")
//        {
//            if(stream)
//            {
//                if(AssetRequesterHodler.Instance.OnReqError != null)
//                {
//                    AssetRequesterHodler.Instance.OnReqError(AssetRequesterHodler.ReqErrorType.RET_STEAM_BUNDLE_ERR,url,version);
//                }
//                return;
//            }
//            int times = 0;
//            if(!mRepairList.TryGetValue(url,out times))
//            {
//                mRepairList.Add(url,1);
//            }
//            else
//            {
//                if(times > AssetRequesterHodler.TryRepairMaxTimes)
//                {
//                    if(AssetRequesterHodler.Instance.OnReqError != null)
//                    {
//                        AssetRequesterHodler.Instance.OnReqError(AssetRequesterHodler.ReqErrorType.RET_TRY_REPAIR_BUNDLE_ERR,url,version);
//                    }
//                    if(finish != null)
//                    {
//                        finish(url,null,param);
//                    }
//                    return;
//                }
//                else
//                    mRepairList[url] = times+1;
//            }
//            
//            string file = url;
//            if(url.Contains("/"))
//                file = url.Substring(url.LastIndexOf('/')+1);
//            if(File.Exists(AssetRequesterHodler.GetStoreFolder()+file))
//            {
//                File.Delete(AssetRequesterHodler.GetStoreFolder()+file);
//            }
//            
//            CreateNewReq(url,version,stream,finish,param,download,guid);
//        }
//        
//        void Update()
//        {
//            UpdateLoadAssets();
//        }   
//        
//        
//        public class AB_Version
//        {
//            public int version;
//            public string file;
//            public string guid;
//            
//            public AB_Version(string f,string g,int v)
//            {
//                version = v;
//                file = f;
//                guid = g;
//            }
//            
//            public AB_Version()
//            {
//                version = 0;
//                file = "";
//                guid = "";
//            }
//        }
//        
//        private Dictionary<string,AB_Version> AB_Versions = new Dictionary<string, AB_Version>();
//        private bool mNeedVersionListSave = false;
//        
//        public bool NeedVersionListSave
//        {
//            get
//            {
//                return mNeedVersionListSave;
//            }
//        }
//        
//        public bool CheckAssetbundle(string local_path,int version,string guid)
//        {
//            if(!File.Exists(local_path))
//                //      if(!AssetRequesterHodler.Instance.CacheFileExist(local_path))
//            {
//                return false;
//            }
//            else
//            {
//                if(!RSBldReqMgr.Instance.CheckVersion(local_path,version,guid))
//                {
//                    File.Delete(local_path);
//                    return false;
//                }
//            }
//            return true;
//        }
//        public bool CheckVersion(string local_path,int v,string guid)
//        {
//            if(AB_Versions.Count == 0)
//            {
//                if(!ReadABVersion())
//                {
//                    return false;
//                }
//            }
//            AB_Version version = null;
//            if(AB_Versions.TryGetValue(local_path,out version))
//            {
//                if(version == null)
//                    return false;
//                if(version.version == v && version.guid == guid)
//                    return true;
//                return false;
//            }
//            return false;
//        }
//        
//        public void RefrushVersionFile(string local_path,int v,string g)
//        {
//            if(AB_Versions.Count == 0)
//            {
//                if(!ReadABVersion())
//                {
//                    return;
//                }
//            }
//            AB_Version version = null;
//            if(AB_Versions.TryGetValue(local_path,out version))
//            {
//                if(version == null)
//                {
//                    AB_Versions[local_path] = new AB_Version(local_path,g,v);
//                }
//                else
//                {
//                    AB_Versions[local_path].version = v;
//                    AB_Versions[local_path].guid = g;
//                }
//                mNeedVersionListSave = true;
//            }
//            else
//            {
//                mNeedVersionListSave = true;
//                AB_Versions.Add(local_path,new AB_Version(local_path,g,v));
//            }
//        }
//        
//        public bool ReadABVersion()
//        {
//            string version_file = AssetRequesterHodler.GetStoreFolder()+"abv.bat";
//            if(!File.Exists(version_file))
//            {
//                AB_Versions.Clear();
//                return WriteABVersion();
//            }
//            try
//            {
//                using ( StreamReader reader =  File.OpenText(version_file))
//                {
//                    if(reader == null)
//                        return false;
//                    string text =  reader.ReadToEnd();
//                    AB_Versions = LitJson.JsonMapper.ToObject<Dictionary<string,AB_Version>>(text);
//                    if(AB_Versions == null)
//                    {
//                        AB_Versions = new Dictionary<string, AB_Version>();
//                    }
//                    reader.Close();
//                }
//            }
//            catch(LitJson.JsonException e)
//            {
//                AB_Versions.Clear();
//                return WriteABVersion();
//            }
//            catch (Exception e) 
//            {
//                Debug.LogError("read version failed: "+e.ToString());
//                return false;
//            }
//            return true;
//        }
//        
//        public bool WriteABVersion()
//        {
//            string file = AssetRequesterHodler.GetStoreFolder() + "abv.bat";
//            try
//            {
//                if(!Directory.Exists(AssetRequesterHodler.GetStoreFolder()))
//                    Directory.CreateDirectory(AssetRequesterHodler.GetStoreFolder());
//                
//                System.IO.TextWriter writer = new System.IO.StreamWriter(file, false);
//                LitJson.JsonWriter jw = new JsonWriter( writer as System.IO.TextWriter );   
//                LitJson.JsonMapper.ToJson(AB_Versions, jw );
//                writer.Close();
//            }
//            catch (Exception e) 
//            {
//                Debug.LogError("write version failed: "+e.ToString());
//                return false;
//            }
//            mNeedVersionListSave = false;
//            return true;
//        }
//    }
//}
