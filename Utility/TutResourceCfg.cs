using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TUT
{
    public class TutResourceCfg : TutCfgBase<TutResourceCfg>
    {
        public bool EnableDevelopMode = false;

        public bool SupportDevelopMode()
        {

#if UNITY_EDITOR
            return EnableDevelopMode;
#else
            return false;
#endif
        }

        public string GetDevelopManifestPath()
        {
#if UNITY_EDITOR
            return "Assets/_oohhoo_data/_manifest_depot/working.manifest";
#else
            return string.Empty;
#endif
        }


        public double RequestRefreshCycle = 0.06f;

        public double DevelopSimulateLoadTime = 1f;

        public int MaxRequesterCount = 1;

        public string StoreFolderName = "ExternalAssets";

        public string ExternalDownLoadUrl = string.Empty;

        private string mExternalUrl = string.Empty;

        private string mExternalPath = string.Empty;

        public string ExternalPath()
        {
            if(string.IsNullOrEmpty(mExternalPath))
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    mExternalPath =UnityEngine.Application.persistentDataPath +"/"+StoreFolderName+"/";
                }
                else
                    if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    mExternalPath = UnityEngine.Application.persistentDataPath +"/"+StoreFolderName+"/";
                }
                else
                {
                    mExternalPath = TutFileUtil.GetPathParent( UnityEngine.Application.dataPath)+"/"+StoreFolderName+"/";
                }
            }
            return mExternalPath;
        }

        public string ExternalUrl()
        {
            if(string.IsNullOrEmpty(mExternalUrl))
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    mExternalUrl = "jar:file://"+ExternalPath();
                }
                else
                if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    mExternalUrl = "file://"+ ExternalPath();
                }
                else
                {
                    mExternalUrl =  "file:///"+ExternalPath();
                }
            }
            return mExternalUrl;
        }

        private string mExtralLocalUrl = string.Empty;
        public string ExtralLocalUrl()
        {
            if(string.IsNullOrEmpty(mExtralLocalUrl))
            {
                mExtralLocalUrl = GetExtralLocalUrl();
            }
            return mExtralLocalUrl;
        }

        private static string GetExtralLocalUrl()
        {
            string dir ;
            if(Application.platform == RuntimePlatform.Android)
            {
                dir = "jar:file://" + Application.dataPath + "!/assets/";
            }
            else
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                dir = "file://" + Application.dataPath + "/Raw/";
            }
            else
            {
                dir = "file:///"+Application.dataPath + "/StreamingAssets/";
            }
            return dir;
        }
    }
}
