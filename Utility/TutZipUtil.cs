using UnityEngine;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using LitJson;

namespace TUT
{
	public class TutZipUtil
	{
		public class MemoryStreamDataSource : IStaticDataSource , System.IDisposable
		{
			public MemoryStreamDataSource( byte[] bytes )
			{
				memStream = new MemoryStream( bytes );
				memStream.Seek( 0 , SeekOrigin.Begin );
			}
			
			public Stream GetSource()
			{
				return memStream;
			}
			
			public void Dispose()
			{
				if( disposed )
					return;
				disposed = true;
				memStream.Dispose();
			}
			
			private bool disposed = false;
			private MemoryStream memStream;
		}

		private string mErrMsg = string.Empty;

		public string Error
		{
			get
			{
				return mErrMsg;
			}
		}

		private ZipFile mZipFile = null;

		public TutZipUtil(string zip_file)
		{
			if(TutFileUtil.IsFolder(zip_file))
			{
				throw new ZipException(zip_file +" isn't vaild zip file!!!! ");
			}
			if(TutFileUtil.FileExist(zip_file))
			{
				mZipFile = new ZipFile(zip_file);
			}
			else
			{
				TutFileUtil.CreateFolder(TutFileUtil.GetFilePath(zip_file));
				mZipFile = ZipFile.Create(zip_file);
			}
		}

		public bool UnzipToFile(string file,string export_file)
		{
			if(mZipFile == null)
				return false;
			if(TutFileUtil.FileExist(export_file))
			{
				TutFileUtil.DeleteFile(export_file);
			}

			ZipEntry entry = mZipFile.GetEntry( file );
			if( null == entry )
			{	
				Debug.LogError(TutNorm.LogErrFormat("Zip" , " [Fatal :] get entry in zip faild, entry name:" + file ));
//                UnityEngine.Debug.LogError(TutNorm.LogErrFormat("Zip" , " [Fatal :] get entry in zip faild, entry name:" + file ));
				return false;
			}

			try
			{
	            TutFileUtil.CreateFolder(TutFileUtil.GetFilePath(export_file));
				Stream inStream = mZipFile.GetInputStream( entry );
				FileStream outStream = new FileStream(export_file,FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				int size = System.Convert.ToInt32( entry.Size );
				byte[] bytes = new byte[ size ];
				if( inStream.Read( bytes , 0 , size )> 0)
				{
					outStream.Write(bytes, 0, bytes.Length);
				}
				outStream.Flush();
				outStream.Close();
				inStream.Close();
			}
			catch(System.Exception e)
			{
				mErrMsg = e.Message;
				return false;
			}
			return true;
		}

		public T UnzipToJsonObj<T>(string file)
		{
			if(mZipFile == null)
				return default(T);
			ZipEntry entry = mZipFile.GetEntry( file );
			if( null == entry )
			{	
				Debug.LogError(TutNorm.LogErrFormat("Zip" , " [Fatal :] get entry in zip faild, entry name:" + file ));
//                UnityEngine.Debug.LogError(TutNorm.LogErrFormat("Zip" , " [Fatal :] get entry in zip faild, entry name:" + file ));
				return default(T);
			}
			return JsonMapper.ToObject<T>(new StreamReader(mZipFile.GetInputStream( entry )));
		}

		public void BeginAddFile()
		{
			if(mZipFile == null)
				return ;
			mZipFile.BeginUpdate();
		}

		public void EndAddFile()
		{
			if(mZipFile == null)
				return;
			mZipFile.CommitUpdate();
		}

		public void Close()
		{
			mZipFile.Close ();
		}

		public bool AddFile(string import_file,string entry_name)
		{
			if(mZipFile == null)
				return false;
			if(!TutFileUtil.FileExist(import_file))
				return false;
			ZipEntry entry = mZipFile.GetEntry( import_file );
			if(entry != null)
			{
				mZipFile.Delete(entry);
//                UnityEngine.Debug.LogWarning(TutNorm.LogErrFormat("Zip" , "<color = blue> [miss :] replace entry in zip, entry name:" + import_file+"</color>" ));
				Debug.LogWarning(TutNorm.LogErrFormat("Zip" , "<color = blue> [miss :] replace entry in zip, entry name:" + import_file+"</color>" ));
			}
			MemoryStreamDataSource memSource = new MemoryStreamDataSource( File.ReadAllBytes(import_file) );
			mZipFile.Add( memSource , entry_name , CompressionMethod.Stored);
			return true;
		}
	}
}


