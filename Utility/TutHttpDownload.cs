using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.ComponentModel;
using System;

namespace TUT
{
	public class TutHttpDownload
	{
		public enum HDState
		{
			HDS_DAMAGED_FILE = -8,
			HDS_INTEGRITY_TEST_FAILD = -7,
			HDS_WRITING_BUF_FAILD = -6,
			HDS_WRITING_BUF_TIMEOUT = -5,
			HDS_CREATE_CONNECT_TIMEOUT = -4,
			HDS_CREATE_CONNECT_FAILD = -3,
			HDS_NET_ERR = -2,
			HDS_REMOTE_CHECK_ERR = -1,
			HDS_NIL = 0,
			HDS_REMOTE_CHECK,
			HDS_REMOTE_CHECK_SUC,
			HDS_CREATE_CONNECT,
			HDS_WRITING_BUF,
			HDS_INTEGRITY_TEST,
			HDS_FINISH,
		}

		public class ProgressInfo
		{
			public long current
			{
				get
				{
					return mCurrent;
				}
			}

			public long target
			{
				get
				{
					return mTraget;
				}
			}

			private long mCurrent;
			private long mTraget;

			public ProgressInfo(long cur,long tar)
			{
				mCurrent = cur;
				mTraget = tar;
			}

			public float progress
			{
				get
				{
					return (float)(System.Convert.ToDouble(mCurrent)/System.Convert.ToDouble(mTraget));
				}
			}
		}


		public int BufferSize = 1000;
		public int Timeout = 3;
		public int TryAgainNum = 3;

		private string mRemoteFile = string.Empty;
		private string mLocalFile = string.Empty;
        private string mLocalPath = string.Empty;
		private long mRemoteFileSize = 0;
		private string mRemoteGuid = string.Empty;
		private float mTimeoutCount = 0;
		private HttpWebResponse mAsynchResponse = null;
		private HttpWebRequest mDownloadRequest = null;
		private Stream mInStream = null;
		private FileStream mOutStream = null;
		private HDState mState = HDState.HDS_NIL;
		private bool mWriteBufDone = false;
		private string mErrorMsg = string.Empty;
		private bool mDownloading = false;
		private TutGuidUtil.ComputeGUID mComputeGUID = null;


		public delegate void DownloadHandle(HDState state,object result);
		
		private DownloadHandle mLoadListener = null;

		private delegate bool WaitingDoneHandle();
		private delegate void WaitTimeoutHandle();

		public HDState State
		{
			get
			{
				return mState;
			}
		}


		public long LocalFileSize
		{
			get
			{
				return  (File.Exists(mLocalFile))? (new FileInfo(mLocalFile)).Length : 0;
			}
		}

		private bool HadError
		{
			get
			{
				return ((int)mState)< 0;
			}
		}

		private bool IsDownloadingFinish
		{
			get
			{
				if(HadError)
					return false;
				return mRemoteFileSize == LocalFileSize;
			}
		}

		private int WriteBufferSize
		{
			get
			{
				int size = (BufferSize<=0?1:BufferSize);
				return 1024*size;
			}
		}

		public TutHttpDownload()
		{
		}

		private IEnumerator CheckFileGuid()
		{
			mComputeGUID = new TutGuidUtil.ComputeGUID(BufferSize,AsyncComputeResultHandle);
			yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( mComputeGUID.ComputeFileAsync(mLocalFile)).Waiting;

			if(!mComputeGUID.AsyncResult.Equals(TutGuidUtil.StringToGuid(mRemoteGuid)))
			{
				ChangeState(HDState.HDS_INTEGRITY_TEST_FAILD,false,mLocalFile + " out of date");
			}
		}

		private void AsyncComputeResultHandle( long cur,long target,Guid guid )
		{
			ChangeState(HDState.HDS_INTEGRITY_TEST,true,new ProgressInfo(cur,target));
		}

		private void ChangeState(HDState state,bool report,object param)
		{
			mState = state;
			if(report)
			{
				if(mLoadListener != null)
				{
					mLoadListener(mState,param);
				}
			}
		}

		public IEnumerator RequestDownload(string url,string guid,string localfile,DownloadHandle listener)
		{
			if(mDownloading)
				yield break;
			mDownloading = true;
			mRemoteFile = url;
			mRemoteGuid = guid;
			mLocalFile = localfile;
            mLocalPath = TutFileUtil.GetFilePath(mLocalFile);
			mLoadListener = listener;

			ChangeState(HDState.HDS_REMOTE_CHECK,false,null);
			HttpWebRequest request = null;
			HttpWebResponse resp = null;
			try 
			{
			 	request = (HttpWebRequest)System.Net.WebRequest.Create(mRemoteFile);
				request.Method = "HEAD";
				request.ContentType = "application/x-www-form-urlencoded";
				resp = (HttpWebResponse)request.GetResponse();
			}
			catch (System.Exception e) 
			{
				ChangeState(HDState.HDS_REMOTE_CHECK_ERR,true,e.Message);
                Debug.Log(TutNorm.LogWarFormat("HTTP Download", "ERROR: " + mRemoteFile));
				yield break;
			}
			mRemoteFileSize = resp.ContentLength;
			resp.Close();     
			resp = null;
			request.Abort ();
			request = null;
            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine(CheckLocalFile()).Waiting;
		}

		private IEnumerator CheckLocalFile()
		{
			if(!mDownloading)
				yield break;
			if(HadError)
			{
				Reset();
				yield break;
			}
			long localFileSize = LocalFileSize;
			if (localFileSize > mRemoteFileSize)
			{
				try 
				{
					TutFileUtil.DeleteFile(mLocalFile);
				}
				catch (System.Exception e) {
                    Debug.LogError(TutNorm.LogErrFormat("HTTP Download", "Could not delete local file!!! "+e.Message+" " +e.StackTrace));
				}
				while (TutFileUtil.FileExist(mLocalFile))
					yield return null;
			}
			else
				if(localFileSize == mRemoteFileSize)
			{
                yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( CheckFileGuid() ).Waiting;
				if(HadError)
				{
					if(mLoadListener != null)
					{
						mLoadListener(mState,mErrorMsg);
					}
					Reset();
					yield break;
				}
				ChangeState(HDState.HDS_FINISH,true,mLocalFile);
				Reset();
				yield break;
			}
			ChangeState(HDState.HDS_CREATE_CONNECT,true,mRemoteFileSize - localFileSize);
		}

		public IEnumerator Downloading()
		{
			if(!mDownloading)
				yield break;
			if(HadError)
			{
				Reset();
				yield break;
			}
            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( Connect() ).Waiting;
			Reset();
		}

		private void Reset()
		{
			BlockConnection();
			mDownloading = false;
		}

		private IEnumerator Connect()
		{
			bool tryAgain = true;
			int tryAgainNum = TryAgainNum<=0?0:TryAgainNum;
			while(tryAgain)
			{
                yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( CreateConnection()).Waiting;

				if(HadError)
				{
					if(tryAgainNum > 0)
					{
						yield return new WaitForEndOfFrame();
						tryAgainNum --;
						continue;
					}
					else
					{
						break;
					}
				}
                yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine(WritingBuffer()).Waiting;

				if(HadError)
				{
					if(tryAgainNum > 0)
					{
						yield return new WaitForEndOfFrame();
						tryAgainNum --;
						continue;
					}
					else
					{
						break;
					}
				}
				tryAgain = false;
			}
			if(HadError)
			{
				if(mLoadListener != null)
				{
					mLoadListener(mState,mErrorMsg);
				}
				yield break;
			}
			else
			{
				if(IsDownloadingFinish)
				{
                    yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( CheckFileGuid() ).Waiting;
					if(HadError)
					{
						if(mLoadListener != null)
						{
							mLoadListener(mState,mErrorMsg);
						}
						yield break;
					}
					ChangeState(HDState.HDS_FINISH,true,mLocalFile);
				}
				else
				{
					ChangeState(HDState.HDS_DAMAGED_FILE,true,mLocalFile);
				}
			}
		}



		private IEnumerator CreateConnection()
		{
			bool success = true;
			try
			{
				mDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(mRemoteFile);
				mDownloadRequest.Timeout = Timeout; 
				mDownloadRequest.AddRange((int)LocalFileSize, (int)mRemoteFileSize - 1);
				mDownloadRequest.BeginGetResponse(ConnectAsynchCallback, mDownloadRequest);
			}
			catch(System.Exception e)
			{
				BlockConnection();
				mErrorMsg = e.Message;
				ChangeState(HDState.HDS_CREATE_CONNECT_FAILD,false,e.Message);
				success = false;
			}
			if (!success) 
			{
				BlockConnection();
				ChangeState(HDState.HDS_CREATE_CONNECT_FAILD,false,null);
				success = false;								
				yield break;						
			}
			 
            yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( Wait(()=>{
				return mAsynchResponse != null;
			},()=>{
				BlockConnection();
				ChangeState(HDState.HDS_CREATE_CONNECT_TIMEOUT,false,null);
			})).Waiting;
		}

		private void ConnectAsynchCallback(IAsyncResult result)
		{
			try
			{
				if (result == null) 
                	Debug.LogError(TutNorm.LogErrFormat("HTTP Download" ,"Asynch result is null!"));
			
				HttpWebRequest webRequest = (HttpWebRequest)result.AsyncState;
				if (webRequest == null)
                	Debug.LogError(TutNorm.LogErrFormat("HTTP Download" ,"Could not cast to web request"));
			
				mAsynchResponse = webRequest.EndGetResponse(result) as HttpWebResponse;
				if (mAsynchResponse == null) 
                	Debug.LogError(TutNorm.LogErrFormat("HTTP Download" ,"Asynch response is null!"));
			}
			catch(System.Exception e)
			{
				Debug.LogError(e.Message);
				BlockConnection();
				ChangeState(HDState.HDS_NET_ERR,false,e.Message);
			}
		}

		private IEnumerator Wait(WaitingDoneHandle done,WaitTimeoutHandle timeout)
		{
			if(done == null || timeout == null)
				yield break;
			mTimeoutCount = Time.realtimeSinceStartup;
			bool wait =true;
			while (wait)
			{
				if(done())
				{
					wait = false;
				}
				else
				{
					if(Time.realtimeSinceStartup - mTimeoutCount >= Timeout)
					{
						timeout();
						wait = false;
						yield break;
					}
					else
						yield return null;
				}
			}
		}

		private void BlockConnection()
		{
			if(mOutStream != null)
			{
				mOutStream.Flush();
				mOutStream.Close();
			}
			mOutStream = null;

			if(mInStream != null)
				mInStream.Close();
			mInStream = null;

			if(mDownloadRequest != null)
				mDownloadRequest.Abort();
			mDownloadRequest = null;

			if(mAsynchResponse != null)
				mAsynchResponse.Close();
			mAsynchResponse = null;
		}

		private IEnumerator WritingBuffer()
		{
			try
			{
				mInStream = mAsynchResponse.GetResponseStream();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				BlockConnection();
				ChangeState(HDState.HDS_NET_ERR,false,e.Message);
				yield break;	
			}
			mOutStream = new FileStream(mLocalFile, (LocalFileSize > 0)? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			
			int count = 0;
			
			byte[] buff = new byte[WriteBufferSize]; 
			
			bool need_down =true;
			while(need_down)
			{
				mWriteBufDone = false;
				IAsyncResult result = null;
				try
				{
					result = mInStream.BeginRead(buff, 0, WriteBufferSize,WriteAsynchCallback,null);
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.Message);
					BlockConnection();
					ChangeState(HDState.HDS_NET_ERR,false,e.Message);
					yield break;
				}
                yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( Wait(()=>{
					return mWriteBufDone;
				},()=>{
					BlockConnection();
					ChangeState(HDState.HDS_WRITING_BUF_TIMEOUT,false,null);
					})).Waiting;
				if(HadError) yield break;
				try
				{
					count = mInStream.EndRead(result);
				}
				catch(System.Exception e)
				{
					Debug.LogError(e.Message);
					BlockConnection();
					ChangeState(HDState.HDS_NET_ERR,false,e.Message);
					yield break;
				}
				if(count > 0)
				{
					bool write_suc = true;
					try
					{
						mOutStream.Write(buff, 0, count);
						mOutStream.Flush();
						ChangeState(HDState.HDS_WRITING_BUF,true,new ProgressInfo(LocalFileSize,mRemoteFileSize));
					}
					catch(System.Exception e)
					{
						BlockConnection();
						mErrorMsg = e.Message;
						ChangeState(HDState.HDS_WRITING_BUF_FAILD,false,e.Message);
						write_suc = false;
					}
					if(!write_suc)
						yield break;
					yield return null;
				}
				else
				{
					need_down = false;
				}
			}
			BlockConnection();

			while(LocalFileSize != mRemoteFileSize) 
			{
				yield return null;
			}
		}

		protected void WriteAsynchCallback(IAsyncResult result) 
		{
			mWriteBufDone = true;
		}
	}
}