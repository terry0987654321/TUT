using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUT;
namespace TUT.RSystem
{
    public class RSBldReqInfo
    {
        public string loadPath
        {
            get
            {
                if(info == null)
                    return string.Empty;
                switch(info.rstype)
                {
                    case TUT.RSystem.RSType.RT_BUNDLE:
                        return TutResourceCfg.Instance.ExternalUrl() + info.guid;
                    case TUT.RSystem.RSType.RT_STREAM:
                        return TutResourceCfg.Instance.ExtralLocalUrl() +"bundles/"+ info.guid;
                    case TUT.RSystem.RSType.RT_RESOURCES:
                        return RSFileInfo.GetResourceLoadPath(info.path);
                }
                return info.path;
            }
        }

        public string downloadPath
        {
            get
            {
                if(info == null)
                    return string.Empty;
                if(string.IsNullOrEmpty( TutResourceCfg.Instance.ExternalDownLoadUrl ) )
                    return string.Empty;
                return TutResourceCfg.Instance.ExternalDownLoadUrl + info.buid;
            }
        }

        public RSFileInfo info;
        public object info_param = null;
        public RSBldRequester.RequestFinish on_finish = null;
        public bool is_download = false;
        public bool is_not_save = false;
        
        public void Reset()
        {
            info = null;
            on_finish = null;
            info_param = null;
            is_not_save = false;
            is_download = false;
        }
    }
}
