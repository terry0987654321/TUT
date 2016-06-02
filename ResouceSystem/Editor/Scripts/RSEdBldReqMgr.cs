using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TUT;

namespace TUT.RSystem
{
    public class RSEdBldReqMgr : RSBldReqAdapter
    {
        private static RSEdBldReqMgr mInstance;

        public static RSEdBldReqMgr Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new RSEdBldReqMgr();
                }
                return mInstance;
            }
        }

        private RSBldRequester mRequester = null;

        private bool mNeedUpdate = false;

        private List<RSBldReqInfo> mReqInfos = new List<RSBldReqInfo>();

        private List<string> mReqPaths = new List<string>();

        private IEnumerator mReqIEnum = null;

        public RSEdBldReqMgr()
        {
            bool need_register = true;
            if (EditorApplication.update != null)
            {
                foreach (System.Delegate i in EditorApplication.update.GetInvocationList())
                {
                    if (i.Method.Name == "RSEdBldReqMgrUpdate")
                    {
                        need_register = false;
                        break;
                    }
                }
            }
            if(need_register)
            {
                mReqIEnum = mRequester.DoLoad();
                EditorApplication.update += RSEdBldReqMgrUpdate;
            }
            mRequester = new RSBldRequester(this);
        }

        public void Load(OhResResult result,OhResResult.ReqResultCallback finish)
        {
            if (finish == null)
                return;
            RSBldReqInfo req_info = null;
            for(int i = 0;i<result.ReqPaths.Length;i++)
            {
                req_info= GetQueueInfo(result.ReqPaths[i]);
                if(req_info == null)
                {
                    req_info = new RSBldReqInfo();
                    req_info.info = new RSFileInfo();
                    req_info.info.path = result.ReqPaths[i];
                }
                req_info.on_finish += result._RequestFinish;
                mReqInfos.Add(req_info);
                mReqPaths.Add(result.ReqPaths[i]);
            }
            UpdateReqQueue();
        }

        public RSBldReqInfo GetQueueInfo(string path)
        {
           int index = mReqPaths.FindIndex(i=>i==path);
            if(index < 0)
                return null;
            return mReqInfos[index];
        }

        void RSEdBldReqMgrUpdate()
        {
            if(!mNeedUpdate)
                return;

            if(!mReqIEnum.MoveNext())
            {
                UpdateReqQueue();
            }

            if(!mRequester.isLoading)
            {
                mNeedUpdate = false;
            }
        }

        public void UpdateReqQueue()
        {
            if(!mRequester.isLoading || mReqInfos.Count == 0)
            {
                return;
            }
            mRequester.Request( mReqInfos[0] );
            mReqInfos.RemoveAt(0);
            mReqPaths.RemoveAt(0);
        }

        public override void RegisterRequest(RSBldRequester requester)
        {
            if(requester == mRequester)
            {
                mNeedUpdate = true;
            }
        }
    }
}
