using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{
    public class RSBldRequester
    {
        public enum ReqErrorType
        {
            RET_NIL,
            RET_WWW_ERROR,
            RET_SAVE_FAILED,
            RET_INVAILD_BUNDLE
        }

        public delegate void RequestFinish(string path,UnityEngine.Object obj,object param);
        public delegate void RequestBlock();

        public bool isLoading
        {
            get{return mLoading;}
        }

        private bool mIs_block = false;
        private bool mLoading = false;
        private RSBldReqInfo mCurinfo = null;
        private RequestFinish mOnFinish = null;
        private RequestBlock mOnBlock = null;
        private bool mNeedSaveAsset = false;
        private string mReqUrl = string.Empty;
        private RSBldReqAdapter mAdapter = null;

        public string url
        {   
            get
            {
                return mReqUrl;
            }
        }

        public RSBldRequester(RSBldReqAdapter adapter)
        {
            mAdapter = adapter;
        }

        public void Block(RequestBlock callback = null)
        {
			if(!mLoading)
			{
				mIs_block = false;
				if(mCurinfo != null && mOnFinish != null)
					mOnFinish(mCurinfo.info.path,null,mCurinfo.info_param);
				mOnFinish = null;
				if(callback!=null)
				{
					callback();
				}
			}
			else
			{
				mIs_block = true;
				mOnBlock = callback;
				if(mCurinfo != null && mOnFinish != null)
					mOnFinish(mCurinfo.info.path,null,mCurinfo.info_param);
				mOnFinish = null;
			}
        }

        public void Request(RSBldReqInfo info)
        {
            if(info == null || info.on_finish == null || mAdapter == null )
                return;
            mCurinfo = info;
            mOnFinish = info.on_finish;
            mNeedSaveAsset = false;
            mReqUrl = mCurinfo.loadPath;
            if(! mAdapter.TestAssetValidness(mCurinfo.info))
            {
                mReqUrl = mCurinfo.downloadPath;
                mNeedSaveAsset = true;
            }
            if(mCurinfo.is_not_save)
            {
                mNeedSaveAsset = false;
            }
            mAdapter.RegisterRequest(this);

            mLoading = true;
        }

        private void DisposeAssetbundle(ReqErrorType error,ref AssetBundle bundle)
        {   
            mLoading = false;
            if(mIs_block)
            {   
                if(mOnFinish != null)
                    mOnFinish(mCurinfo.info.path,null,null);
                BlockDispose(ref bundle);
                return;
            }   
            if(error != ReqErrorType.RET_NIL)
            {
                mAdapter.CaptureErr(mCurinfo.info,error);
            }
            else
            {
                if(mCurinfo.is_download)
                {
                    DownLoadDispose(ref bundle);
                }
                else
                    BundleDispose(ref bundle);
            }
        }
        
        private void DownLoadDispose(ref AssetBundle bundle)
        {
            bool is_bad_bundle = false;
            mLoading = false;
            if(bundle != null)
            {
                if(mOnFinish != null)
                    mOnFinish(mCurinfo.info.path,null,1);
                bundle.Unload(true);
            }
            else
            {
                is_bad_bundle = true;
            }
            if(is_bad_bundle)
            {
                mAdapter.TryRepairAsset(mCurinfo);
            }
            bundle = null;
            ResetRequest();
        }
        
        private void BlockDispose(ref AssetBundle bundle)
        {
            mLoading = false;
            RequestBlock block = mOnBlock;
            if(block != null)
            {
                block();
            }
            mOnBlock = null;
            ResetRequest();
            if(bundle != null)
                bundle.Unload(false);
            bundle = null;
        }
        
        private void BundleDispose(ref AssetBundle bundle)
        {
            bool is_bad_bundle = false;
            if(mOnFinish != null && mCurinfo != null )
            {
                if(bundle!=null)
                {
                    if(bundle.mainAsset != null)
                    {
                        UnityEngine.Object obj = bundle.mainAsset;
                        mOnFinish(mCurinfo.info.path,obj,mCurinfo.info_param);
                        obj = null;
                    }
                    else
                        is_bad_bundle = true;
                    
                    bundle.Unload(false);
                    bundle = null;
                }
                else
                {
                    is_bad_bundle = true;
                }
            }
            if(is_bad_bundle)
            {
                mAdapter.TryRepairAsset(mCurinfo);
            }
            ResetRequest();
        }
        
        public void ResetRequest()
        {
            mReqUrl = string.Empty;
            mIs_block = false;
            mLoading = false;
            mNeedSaveAsset = false;
            if(mCurinfo != null)
                mCurinfo.Reset();       
            mCurinfo = null;
            mOnFinish = null;
            mOnBlock = null;
        }
        
        public IEnumerator DoLoad()
        {
            AssetBundle bundle = null;
            if (mCurinfo == null)
            {
                BlockDispose(ref bundle);
                yield break;
            }

            string req_url = mCurinfo.loadPath;

            if(mIs_block)
            {
                BlockDispose(ref bundle);
                yield break;
            }

            if (string.IsNullOrEmpty(req_url))
            {
                mAdapter.CaptureErr(mCurinfo.info,ReqErrorType.RET_INVAILD_BUNDLE);
                BlockDispose(ref bundle);
                yield break;
            }

            Dictionary<string,string> headers = new Dictionary<string, string>();
            headers.Add("time", Time.realtimeSinceStartup.ToString());
            WWW www = new WWW(req_url, null, headers);
            yield return www;
                
            if (mIs_block)
            {
                if (www.assetBundle != null)
                {
                    if (mNeedSaveAsset)
                        mAdapter.SaveAssetToLocalPath(mCurinfo.info, www.bytes);
                    www.assetBundle.Unload(true);
                }
                www.Dispose();
                www = null;
                BlockDispose(ref bundle);
                yield break;
            }
                
            if (www.error != null)
            {
                mLoading = false;
                DisposeAssetbundle(ReqErrorType.RET_WWW_ERROR, ref bundle);
                www.Dispose();
                www = null;
                yield break;
            }

            if (www.assetBundle != null)
            {   
                bool save_success = true;
                if (mNeedSaveAsset)
                    save_success = mAdapter.SaveAssetToLocalPath(mCurinfo.info, www.bytes);
                bundle = www.assetBundle;
                    
                DisposeAssetbundle((!save_success) ? ReqErrorType.RET_SAVE_FAILED : ReqErrorType.RET_NIL, ref bundle);
                bundle = null;
                if (www.assetBundle != null)
                    www.assetBundle.Unload(false);
            } else
            {
                DisposeAssetbundle(ReqErrorType.RET_INVAILD_BUNDLE, ref bundle);
            }
            www.Dispose();
            www = null;
            mLoading = false;
        }   
    }
}