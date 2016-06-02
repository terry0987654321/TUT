using UnityEngine;
using System.Collections;
using TUT;
namespace TUT.RSystem
{
    public  abstract class RSBldReqAdapter
    {
        public virtual bool TestAssetValidness(RSFileInfo info)
        {
            return true;
        }

        public virtual void RegisterRequest(RSBldRequester requester)
        {

        }

        public virtual void CaptureErr(RSFileInfo info,RSBldRequester.ReqErrorType type)
        {

        }

        public virtual void TryRepairAsset(RSBldReqInfo info)
        {

        }

        public virtual bool SaveAssetToLocalPath(RSFileInfo info,byte[] bytes)
        {
            return false;
        }
    }
}
