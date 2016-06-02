using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TUT;


namespace TUT.RSystem
{
	public class RSAssetsTransfer 
	{
		public static readonly string TmpResPath = "Assets/OH_Build_Tmp/Resources/";

//		[MenuItem("File/Build Transfer to ...")]
		public static void TransferToBuildState()//RSType manifest_build_type = RSType.RT_NIL)
		{
			TutFileUtil.DeleteFolder (TmpResPath);
			RSAssetPostprocessor.IgnorePostprocess = true;
			RSFileInfo[] infos = RSEdManifest.GetFileInfos ();
            RSManifest rsManifest = new RSManifest();

			string exprot_path = Application.dataPath+"/OH_Build_Streamanifest/Resources/";
			TutFileUtil.CreateFolder(exprot_path);
			RSManifest smanifest = RSBuildPipelineUtil.LoadManifest(exprot_path+TutResources.SteamManifestName);
			if(smanifest == null)
				smanifest = new RSManifest();

			for(int i = 0;i< infos.Length;i++)
			{
				if(infos[i].rstype == RSType.RT_RESOURCES)
				{
					if(RSInfo.isResTypeFromPath(infos[i].path))
						continue;
					MoveAssetToTmpResPos(infos[i]);
                    if(rsManifest.files == null)
                        rsManifest.files = new List<RSFileInfo>();
                    rsManifest.files.Add((RSFileInfo)(infos[i].Clone()));
				}
				else
				if(infos[i].rstype == RSType.RT_STREAM)
				{
					if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
					{
						TutFileUtil.CopyFile(infos[i].path,"Assets/StreamingAssets/"+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
						if(smanifest.files == null)
							smanifest.files = new List<RSFileInfo>();
						smanifest.files.Add((RSFileInfo)(infos[i].Clone()));
					}
				}
				else
				if(infos[i].rstype == RSType.RT_BUNDLE)
				{
					if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
					{
						TutFileUtil.CopyFile(infos[i].path,Application.dataPath.Replace("Assets","")+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
					}
				}
			}

//			switch(manifest_build_type)
//			{
//			case RSType.RT_RESOURCES:
//				TutFileUtil.CopyFile(RSEdManifest.WoringFile(),TmpResPath+"main_manifest.txt");
//				break;
//			case RSType.RT_STREAM:
//				break;
//			case RSType.RT_BUNDLE:
//				break;
//			}
            string path = Application.dataPath +"/OH_Build_Tmp/Resources/";
            string file = path +TutResources.ResManifestName;
            TutFileUtil.WriteJsonFile(rsManifest,file);

			if(smanifest != null)
				RSBuildPipelineUtil.SaveStreamManifest(smanifest);

			RSAssetPostprocessor.IgnorePostprocess = false;
			AssetDatabase.Refresh ();
		}

        public static RSFileInfo[] TransferToBuildState( RSFileInfo[] infos )
        {
            List<RSFileInfo> result = new List<RSFileInfo>();
            RSAssetPostprocessor.IgnorePostprocess = true;
            for(int i = 0;i< infos.Length;i++)
            {
                if(infos[i].rstype == RSType.RT_RESOURCES)
                {
                    if(RSInfo.isResTypeFromPath(infos[i].path))
                        continue;
                    MoveAssetToTmpResPos(infos[i]);
                }
                else
                    if(infos[i].rstype == RSType.RT_STREAM)
                {
                    if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
                    {
                        TutFileUtil.CopyFile(infos[i].path,"Assets/StreamingAssets/"+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
                    }
                    else
                    {
                        result.Add(infos[i]);
                    }
                }
                else
                    if(infos[i].rstype == RSType.RT_BUNDLE)
                {
                    if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
                    {
                        TutFileUtil.CopyFile(infos[i].path,Application.dataPath.Replace("Assets","")+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
                    }
                    else
                    {
                        result.Add(infos[i]);
                    }
                }
            }

            RSAssetPostprocessor.IgnorePostprocess = false;
            AssetDatabase.Refresh ();
            return result.ToArray();
        }

		[MenuItem("File/Recover Build Transfer to ...")]
		public static void TransferToDepState()
		{
			RSAssetPostprocessor.IgnorePostprocess = true;
			RSFileInfo[] infos = RSEdManifest.GetFileInfos ();
			for(int i = 0;i< infos.Length;i++)
			{
				if(infos[i].rstype == RSType.RT_RESOURCES)
				{
					if(RSInfo.isResTypeFromPath(infos[i].path))
						continue;
					MoveAssetToDepPos(infos[i]);
				}
				else
				if(infos[i].rstype == RSType.RT_STREAM)
				{
					if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
					{
						TutFileUtil.DeleteFile("Assets/StreamingAssets/"+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
					}
				}
				else
				if(infos[i].rstype == RSType.RT_BUNDLE)
				{
					if(RSInspector.LimitedSuffixs.isDontNeedBundleAsset(infos[i].path))
					{
						TutFileUtil.DeleteFile(Application.dataPath.Replace("Assets","")+TutResourceCfg.Instance.StoreFolderName+"/"+infos[i].path);
					}
				}
			}
			RSAssetPostprocessor.IgnorePostprocess = false;
			AssetDatabase.Refresh ();
		}

		private static void CreateFolder(string path)
		{
			if( TUT.TutFileUtil.CreateFolder (path))
				AssetDatabase.Refresh ();
		}

		private static void MoveAssetToTmpResPos(RSFileInfo info)
		{
			if(info == null)
				return;
			if(!TutFileUtil.FileExist(info.path))
				return;
			string new_path = TmpResPath + info.path;
			CreateFolder (TutFileUtil.GetFilePath( new_path ));
			AssetDatabase.MoveAsset (info.path, new_path);
		}

		private static void MoveAssetToDepPos(RSFileInfo info)
		{
			if(info == null)
				return;
			string new_path = TmpResPath + info.path;
			if(!TutFileUtil.FileExist(new_path))
				return;	
			if(TutFileUtil.FileExist(info.path))
				return;
			CreateFolder (TutFileUtil.GetFilePath(info.path));
			AssetDatabase.MoveAsset (new_path,info.path);
		}
	}
}

