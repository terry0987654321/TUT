using UnityEngine;
using System.Collections;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace TUT
{
    public class TutGuidUtil
    {
		public class ComputeGUID
		{
			private byte[] mBuffer;
			private Stream mInputStream;  
			private MD5CryptoServiceProvider mHashAlgorithm;
			public delegate void AsyncComputeResultHandle( long cur,long target,Guid guid );
			private AsyncComputeResultHandle mResult;
			private bool mWaitRead = false;
			private bool mIsComplete = true;
			private Guid mAsyncResult = Guid.Empty;

			public Guid AsyncResult
			{
				get
				{
					return mAsyncResult;
				}
			}

			private int mBufSize = 0;

			public ComputeGUID(int bufSize,AsyncComputeResultHandle result = null)
			{
				mResult = result;
				mBufSize = (bufSize<=0?1:bufSize)*1024;
			}

			public static Guid ComputeFile(string path)
			{
				if (!TutFileUtil.FileExist(path))
				{
					throw new ArgumentException(string.Format("<{0}>, ", path));  
				}
				MD5 md5Hasher = MD5.Create();
				byte[] data = md5Hasher.ComputeHash(File.ReadAllBytes(path));
				return new Guid(data);
			}

			public static Guid ComputeStr(string value)
			{
				MD5 md5Hasher = MD5.Create();
				byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
				return new Guid(data);
			}

			public IEnumerator ComputeFileAsync(string path)  
			{  
				if (!File.Exists(path))  
					throw new ArgumentException(string.Format("<{0}>, ", path));  
				mIsComplete = false;
				int bufferSize = mBufSize; 
				
				mBuffer = new byte[bufferSize];  
				mInputStream = File.Open(path, FileMode.Open);  
				mHashAlgorithm = new MD5CryptoServiceProvider();  
				IAsyncResult result = null;
				while(!mIsComplete)
				{
					mWaitRead = true;
					result = mInputStream.BeginRead(mBuffer, 0, mBufSize, AsyncComputeHashCallback, null); 

					while(mWaitRead)
						yield return null;

					int bytesRead = mInputStream.EndRead(result);  
					if (mInputStream.Position < mInputStream.Length)  
					{  
						if (null != mResult)  
							mResult(mInputStream.Position ,mInputStream.Length,Guid.Empty);  
						
						var output = new byte[mBuffer.Length];  
						mHashAlgorithm.TransformBlock(mBuffer, 0, mBuffer.Length, output, 0);  
						yield return null;
						continue;
					}  
					else  
					{  
						mHashAlgorithm.TransformFinalBlock(mBuffer, 0, bytesRead);  
					}  
					
					mIsComplete = true;
					mAsyncResult = new Guid(mHashAlgorithm.Hash);
					if (null != mResult)  
						mResult(mInputStream.Position ,mInputStream.Length,mAsyncResult);   
					mInputStream.Close();  
				}
			}  

			private void AsyncComputeHashCallback(IAsyncResult result)
			{
				mWaitRead = false;
			}
		}

        public static Guid TextToGUID(string value)
        {
			return ComputeGUID.ComputeStr(value);
        }

        public static Guid FileToGUID(string path)
        {
			return ComputeGUID.ComputeFile(path);
        }

        public static Guid StringToGuid(string guid)
        {
            return new Guid(guid);
        }

        public static string GuidToString(Guid guid)
        {
            return guid.ToString("N");
        }
    }
}