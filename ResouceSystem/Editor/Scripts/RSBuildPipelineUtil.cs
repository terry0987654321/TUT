using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TUT;

namespace TUT.RSystem
{
    public class RSBuildPipelineUtil
    {

        public static readonly string ManifestFileName = "manifest.txt";

        public static readonly string TmpFolderName = "_tmp";

        private static string GetAssetGuid(string path,bool include_dep = false)
        {
            string guid = string.Empty;
            if(string.IsNullOrEmpty(path))
                return guid;
            if(TutFileUtil.IsFolder(path))
                return guid;
            guid = TutGuidUtil.GuidToString(TutGuidUtil.FileToGUID(path));
            if(include_dep)
            {
                string[] deps = AssetDatabase.GetDependencies(new string[]{path});
                for(int i = 0;i<deps.Length;i++)
                {
                    guid += GetAssetGuid(deps[i]);
                }
                guid = TutGuidUtil.GuidToString(TutGuidUtil.TextToGUID(guid));
            }
            return guid;

        }

        private static bool IsChangeAsset(RSInfo info,out string new_guid)
        {
            new_guid = string.Empty;
            if(info == null)
                return false;
            if(string.IsNullOrEmpty(info.path))
                return false;
            if(TutFileUtil.IsFolder(info.path))
                return false;
            new_guid = GetAssetGuid(info.path,true);
            if(string.Equals(new_guid,((RSFileInfo)info).guid ))
            {
                return false;
            }
            return true;
        }

        private static string[] PerfabBuildPipeline(string improt_path,string exprot_path)
        {
            string asset_file = RSEdManifest.ConvertToRSPath( improt_path );
            GameObject asset = AssetDatabase.LoadAssetAtPath(asset_file,typeof(GameObject)) as GameObject;
            {
                asset = GameObject.Instantiate(asset) as GameObject;
                asset.name = asset.name.Replace("(Clone)","");
                string[] req_assets = null;
                RSCollectDependency collect = new RSCollectDependency();
                RSColDep col_dep = null;
                {
                    RSAssetReflex reflex =  asset.GetComponent<RSAssetReflex>();
                    if(reflex != null)
                    {
                        GameObject.DestroyImmediate(reflex);
                        reflex = null;
                    }
                    col_dep = collect.ExpColDepDatas(asset);
                    col_dep.Dismantle();
                    reflex = asset.AddComponent<RSAssetReflex>();
                    reflex.ColDep = col_dep;
                    req_assets = reflex.ColDep.ReqAssets;
                }
                GameObject tmp = asset;
                TUT.TutFileUtil.CreateFolder(RSEdConst.s_RSEdTmpPath + "build_tmp/");
                asset = PrefabUtility.CreatePrefab(RSEdConst.s_RSEdTmpPath + "build_tmp/"+asset.name+".prefab",tmp);
                GameObject.DestroyImmediate(tmp);


                BuildAssetBundle(asset,exprot_path);

                AssetDatabase.Refresh();
                AssetDatabase.DeleteAsset(RSEdConst.s_RSEdTmpPath + "build_tmp"+asset.name+".prefab");

                return req_assets;
            }
        }

        private static void ObjectBuildPipeline(string improt_path,string exprot_path)
        {
            string asset_file = RSEdManifest.ConvertToRSPath( improt_path );
            string target = string.Empty;

            if(string.IsNullOrEmpty(asset_file))
            {
                target = CopyToAssets(improt_path);
                if(!string.IsNullOrEmpty(target))
                {
                    asset_file = target;
                }
                else
                {
                    //report err
                }
            }
            Object asset = AssetDatabase.LoadAssetAtPath(asset_file,typeof(Object));
            if (asset == null)
            {
                Debug.LogError(asset_file);            
            }
            BuildAssetBundle(asset,exprot_path);
            if(!string.IsNullOrEmpty(target))
            {
                AssetDatabase.DeleteAsset(target);
            }
        }

        private static string CopyToAssets(string improt_path)
        {
            if(string.IsNullOrEmpty(improt_path))
            {
                return string.Empty;
            }
            string file_name = TutFileUtil.GetFile( improt_path );
            string target_path = RSEdConst.s_RSEdTmpPath+file_name;
            if(!TutFileUtil.CopyFile(improt_path,target_path))
                return string.Empty;
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return target_path;
        }

        private static void BuildAssetBundle(Object asset,string exprot_path)
        {
            if(asset == null)
            {
                //report err
            }

            UnityEditor.BuildPipeline.BuildAssetBundle(asset,null,exprot_path,
                                           BuildAssetBundleOptions.CompleteAssets | 
                                           BuildAssetBundleOptions.CollectDependencies,
                                           EditorUserBuildSettings.activeBuildTarget);
        }

        private static RSFileInfo BuildAssetPipeline(RSFileInfo info,string exprot_path)
        {
            if(info == null)
            {
                return info;
            }

            if(info.rstype == RSType.RT_RESOURCES)
                return null;

            string new_guid = string.Empty;

            if ( ! IsChangeAsset(info,out new_guid))
            {
                return info;
            }

            string exprot_file = exprot_path.Replace("\\","/")+"/" +new_guid;
            TutFileUtil.CreateFolder(exprot_path);
            RSFileInfo new_info = (RSFileInfo)info.Clone();
            if(info.path.Contains(".prefab"))
            {
                new_info.dependency_assets = PerfabBuildPipeline(new_info.path,exprot_file);
            }
            else
            {
                ObjectBuildPipeline(new_info.path,exprot_file);
            }
            new_info.guid = new_guid;
            new_info.buid = TutGuidUtil.GuidToString(TutGuidUtil.FileToGUID(exprot_file));
            new_info.size = TutFileUtil.GetFileSize(exprot_file);
            return new_info;
        }



        private static string SaveManifest(RSManifest manifest,string path,TutZipUtil zip = null)
        {
            string vaild_path = path.Replace("\\","/");
            string exprot_path = vaild_path+"/";
            string manifest_path = vaild_path+"/"+TmpFolderName+"/"+ManifestFileName;
            TutFileUtil.WriteJsonFile(manifest,manifest_path);
            string guid = TutGuidUtil.GuidToString(TutGuidUtil.TextToGUID(exprot_path));
            exprot_path += guid;
            ObjectBuildPipeline(manifest_path,exprot_path);
            string build = TutGuidUtil.GuidToString(TutGuidUtil.FileToGUID(exprot_path));
           
            if (zip != null)
                zip.AddFile(exprot_path, guid);
            return guid + ":" + build;
        }

        public static void SaveStreamManifest(RSManifest manifest)
        {
            string path = Application.dataPath +"/OH_Build_Streamanifest/Resources/";
            string file = path +TutResources.SteamManifestName;
            TutFileUtil.WriteJsonFile(manifest,file);
//            string exprot_path = path + TutGuidUtil.GuidToString(TutGuidUtil.TextToGUID(file));
            AssetDatabase.Refresh();
//            ObjectBuildPipeline(file,exprot_path);
        }

        public static RSManifest LoadManifest(string path)
        {
            string vaild_path = path.Replace("\\","/");
            return TutFileUtil.ReadJsonFile<RSManifest>(vaild_path);
        }

        private static void PushInfoToManifest(RSFileInfo info,RSManifest manifest)
        {
            if(manifest == null)
                return;
            if( manifest.files == null)
                manifest.files = new System.Collections.Generic.List<RSFileInfo>();
            int index = manifest.files.FindIndex(i=>i.path == info.path);
            if(index < 0)
            {
                manifest.files.Add(info);
            }
            else
            {
                manifest.files[index] = (RSFileInfo)info.Clone();
            }
        }

        private static void AssetPipeline(RSFileInfo[] infos,string exprot_path,ref RSManifest manifest,TutZipUtil zip =null)
        {
            if(manifest == null)
                manifest = new RSManifest();
            RSFileInfo info = null;

            for(int i =0;i<infos.Length;i++)    
            {

                info = manifest.GetInfo( infos[i].path);
                if(info == null)
                {
                    info = new RSFileInfo();
                    info.path = infos[i].path;
                    info.type = infos[i].type;
                    info.group = infos[i].group;
                }
                if(info.guid  != infos[i].guid || string.IsNullOrEmpty(info.guid))
                    info = BuildAssetPipeline(info,exprot_path);
                if(info != null)
                    PushInfoToManifest(info,manifest);
                if(zip != null)
                    zip.AddFile(exprot_path.Replace("\\","/")+"/" +info.guid,info.guid);
            }
        }

        private static void keepSyncToEdManifest(ref RSManifest manifest,string exprot_path)
        {
            if(manifest == null || manifest.files == null)
                return;

            for(int i = 0;i<manifest.files.Count;)
            {
                if(RSEdManifest.GetInfo(manifest.files[i].path) == null)
                {
                    TutFileUtil.DeleteFile(exprot_path +manifest.files[i].guid);
                    manifest.files.RemoveAt(i);
                }
                else
                    i++;
            }
        }

        private static RSManifest BundlePipeline(RSFileInfo[] infos,string exprot_path,string tag ="")
        {
            string dstr = tag;
            if (string.IsNullOrEmpty(dstr))
            {
                dstr = System.DateTime.Now.ToString("yyyyMMddHHmm");
            }
            string zpath = exprot_path + "/"+dstr+"/" + dstr +".oh";
            string epath = exprot_path + "/bundles";
            TutFileUtil.CreateFolder(exprot_path + "/" + dstr);

            RSManifest manifest = LoadManifest(epath+"/"+TmpFolderName+"/"+ManifestFileName);
            keepSyncToEdManifest(ref manifest,epath);

            TutZipUtil zip = new TutZipUtil(zpath);
            zip.BeginAddFile();
            AssetPipeline(infos,epath,ref manifest,zip);
            if (manifest != null)
            {
                string ver = SaveManifest(manifest, epath,zip);
                TutFileUtil.WriteTxtFile(ver,exprot_path + "/"+dstr+"/up_version.ver");
            }
            zip.EndAddFile();
            zip.Close();
            return manifest;
        }

        private static RSManifest StreamPipeline(RSFileInfo[] infos)
        {
            string exprot_path = Application.dataPath+"/OH_Build_Streamanifest/Resources/";
            TutFileUtil.CreateFolder(exprot_path);
            RSManifest manifest = LoadManifest(exprot_path+TutResources.SteamManifestName);
            keepSyncToEdManifest(ref manifest,exprot_path);
            AssetPipeline(infos,Application.dataPath+"/StreamingAssets/bundles/",ref manifest);
            if(manifest != null)
                SaveStreamManifest(manifest);
            return manifest;
        }

        public static RSManifest BuildPipeline(RSFileInfo[] infos,string exprot_path,string tag ="")
        {
            List<RSFileInfo> bundles = new List<RSFileInfo>();
            List<RSFileInfo> streams = new List<RSFileInfo>();
            RSManifest bundle_manifest = null;
            RSManifest stream_manifest = null;

            RSFileInfo[] rinfos = RSAssetsTransfer.TransferToBuildState(infos);
            for(int i = 0;i<rinfos.Length;i++)
            {
                if(rinfos[i].rstype == RSType.RT_BUNDLE)
                {
                    bundles.Add(rinfos[i]);
                }
                else
                if(rinfos[i].rstype == RSType.RT_STREAM)
                {
                    streams.Add(rinfos[i]);
                }
            }

            if(bundles.Count != 0)
            {
                bundle_manifest = BundlePipeline(bundles.ToArray(),exprot_path,tag);
            }

            if(streams.Count != 0)
            {
                stream_manifest = StreamPipeline(streams.ToArray());
            }


            RSManifest manifest = new RSManifest();
            if (bundle_manifest == null && stream_manifest == null)
            {
                for(int i = 0;i<infos.Length;i++)
                    PushInfoToManifest(infos[i],manifest);
                return manifest;
            }

            if(bundle_manifest != null && bundle_manifest.files != null)
                for(int i = 0;i<bundle_manifest.files.Count;i++)
                    PushInfoToManifest(bundle_manifest.files[i],manifest);
            if(stream_manifest != null && stream_manifest.files != null)
                for(int i = 0;i<stream_manifest.files.Count;i++)
                    PushInfoToManifest(stream_manifest.files[i],manifest);
            return manifest;
        }
    }
}
