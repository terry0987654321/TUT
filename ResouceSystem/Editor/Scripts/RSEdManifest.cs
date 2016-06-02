
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{

    public class RSEdManifest
    {
        public string ManifestName = "";

        public List<RSFileInfo> files = new List<RSFileInfo>();

        public Dictionary<string,RSFileInfo> edfiles = new Dictionary<string, RSFileInfo>();

        public Dictionary<string,RSFolderInfo> folders = new Dictionary<string, RSFolderInfo>();

       
        public enum ManifestState
        {
            MS_NIL,
            MS_UNCHANGED,
            MS_CHANGED,
            MS_NONEXISTENCE,
        }

        private static RSEdManifest mInstance = null;

        public static string WoringFile()
        {
            return RSEdConst.s_RSEdDataPath + RSEdConst.s_RSEdDepotFolder + "/" + RSEdConst.s_RSEdWorkingName;
        }

        private static RSEdManifest Instance
        {
            get
            {
                if (mInstance == null)
                {
                    ChanagerEdManifest(WoringFile(),"Default");
                }
                return mInstance;
            }
        }

        private static List<string> mDepotManifests = new List<string>();

        private static int mCurDepotIndex = -1;

        public static int CurDepotIndex()
        {
            return mCurDepotIndex;
        }

        public static string[] DepotManifests()
        {
            if (mDepotManifests.Count == 0)
            {
                RefrushDepotManifest();
            }
            return mDepotManifests.ToArray();
        }

        private static List<int> mDepotManifestIndexs = new List<int>();

        public static int[] DepotManifestIndexs()
        {
            if (mDepotManifestIndexs.Count == 0)
            {
                RefrushDepotManifest();
            }
            return mDepotManifestIndexs.ToArray();
        }

        public static bool DepotExistManifest(string name)
        {
            return mDepotManifests.Contains(name);
        }

        public static int GetDepotManifestIndex(string name)
        {
            if (mDepotManifests.Count == 0)
            {
                RefrushDepotManifest();
            }
            if (!mDepotManifests.Contains(name))
                return -1;
            return mDepotManifests.FindIndex(i=>i==name);
        }

        public static string GetDepotManifestName(int index)
        {
            if (mDepotManifests.Count == 0)
            {
                RefrushDepotManifest();
            }
            if (index < 0)
                return string.Empty;
            return mDepotManifests[index];
        }

        public static void RefrushDepotManifest()
        {
            mDepotManifests.Clear();
            mDepotManifestIndexs.Clear();
            string[] manifests = TutFileUtil.GetFiles(RSEdConst.s_RSEdDataPath + RSEdConst.s_RSEdDepotFolder + "/", System.IO.SearchOption.TopDirectoryOnly, "*" + RSEdConst.s_RSEdManifestSuffix);
            for (int i = 0; i<manifests.Length; i++)
            {
                if(!manifests[i].EndsWith(RSEdConst.s_RSEdWorkingName))
                {
                    mDepotManifests.Add(TutFileUtil.GetFileName(manifests[i]));
                    mDepotManifestIndexs.Add(i);
                }
            }
        }

        public static void KeepVaildEdManifest()
        {
            for (int i =0; i<Instance.files.Count; )
            {
                if(!TUT.TutFileUtil.FileExist(Instance.files[i].path))
                {
                    Instance.files.RemoveAt(i);
                }
                else
                    i++;
            }

            List<string> path = new List<string>();
            foreach(string f in Instance.edfiles.Keys)
            {
                path.Add(f);
            }

            for (int i = 0; i<path.Count; i++)
            {
                if(!TUT.TutFileUtil.FileExist(path[i]))
                {
                    Instance.edfiles.Remove(path[i]);
                }
            }

            path.Clear();

            foreach(string f in Instance.folders.Keys)
            {
                path.Add(f);
            }
            
            for (int i = 0; i<path.Count; i++)
            {
                if(!System.IO.Directory.Exists(path[i]))
                {
                    Instance.folders.Remove(path[i]);
                }
            }
            path.Clear();
        }

        public static void SaveEdManifest()
        {
            KeepVaildEdManifest();
            WriteEdManifest();
        }

        public static void WriteEdManifest()
        {
            TutFileUtil.WriteJsonFile(Instance, WoringFile(),true);
        }

        public static void SaveToDepot(string new_name)
        {
            Instance.ManifestName = new_name;
            string path = RSEdConst.s_RSEdDataPath + RSEdConst.s_RSEdDepotFolder + "/" + new_name + RSEdConst.s_RSEdManifestSuffix;
            if (TutFileUtil.FileExist(path))
                TutFileUtil.DeleteFile(path);
            TutFileUtil.WriteJsonFile(Instance, path,true);
            //TutFileUtil.CopyFile(WoringFile(),);
        }

        public static string GetWorkingManifestName()
        {
            return Instance.ManifestName;
        }

        public static void ChanagerEdManifest(string path,string create_name = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(create_name))
                    return;
                mInstance = new RSEdManifest();
                mInstance.ManifestName = create_name;
                mCurDepotIndex = -1;

            } else
            {
                if(TutFileUtil.FileExist(path))
                {
                    mInstance = TutFileUtil.ReadJsonFile<RSEdManifest>(path);
                    if(string.IsNullOrEmpty(mInstance.ManifestName))
                    {
                        mInstance.ManifestName = TutFileUtil.GetFileName(path);
                    }
                    mCurDepotIndex = GetDepotManifestIndex(mInstance.ManifestName);
//                    SaveEdManifest();
                } else
                {
                    mInstance = new RSEdManifest();
                    mInstance.ManifestName = create_name;
                    mCurDepotIndex = -1;
                }
            }
            WriteEdManifest();
//            SaveEdManifest();
        }


        private static RSFolderInfo GetFolderInfo(string path)
        {
            if (Instance.folders == null)
                return null;
            RSFolderInfo info = null;
            Instance.folders.TryGetValue(path, out info);
            if(info != null)
            {
                return (RSFolderInfo)info.Clone();
            }
            return info;
        }

        private static void RefrushFolderInfo(RSFolderInfo info, ManifestState state)
        {
            switch (state)
            {
                case ManifestState.MS_NIL:
                case ManifestState.MS_UNCHANGED:
                    break;
                case ManifestState.MS_CHANGED:
                    Instance.folders [info.path] = info;
                    break;
                case ManifestState.MS_NONEXISTENCE:
                    if(info.processors == null)
                    {
                        info.processors = new List<RSProcessorInfo>();
                    }
                    if( info.processors.Find(rsp => rsp.processor_func == "SyncParentProcessorInfo(Create/MoveIn)") == null)
                    {
                        RSProcessorInfo rsp = new RSProcessorInfo();
                        rsp.editor_index = RSProlib.GetProIndex("SyncParentProcessorInfo(Create/MoveIn)");
                        rsp.priority = info.processors.Count;
                        rsp.processor_type = 0;
                        rsp.processor_func = "SyncParentProcessorInfo(Create/MoveIn)";
                        info.processors.Add(rsp);
                    }
                    Instance.folders.Add(info.path, info);
                    break;
            }
        }

        public static void RemoveFolderInfo(string path, bool apply = false, bool include_child = false)
        {
            if (string.IsNullOrEmpty(path))
                return;

            RSFolderInfo info = GetFolderInfo(path);
            if (info == null)
                return;

            Instance.folders.Remove(info.path);

            if (apply)
            {
                string[] files = TutFileUtil.GetFiles(info.path);
                for (int i = 0; i<files.Length; i++)
                {
                    if (files [i].Contains(".meta"))
                        continue;
                    RemoveFileInfo(ConvertToRSPath(files [i]));
                }
            }

            if (include_child)
            {
                string[] folders = TutFileUtil.GetDirectories(info.path);
                for (int i = 0; i<folders.Length; i++)
                {
                    RemoveFolderInfo(ConvertToRSPath(folders [i]), true, true);
                }
            }
        }

        public static void FiltrateFolderInfo(RSFolderInfo info)
        {
            if(info.processors != null)
            {
                int invaild = RSProlib.ProInvaild;
                for(int i =0;i<info.processors.Count;)
                {
                    if(info.processors[i].editor_index == invaild )
                    {
                        info.processors.RemoveAt(i);
                    }
                    else
                        i++;
                }
            }
        }

        [MenuItem("Edit/Sync RSProcess Cfg to Manifast..")]
        public static void FiltrateFolderInfoProcess()
        {
            foreach(RSFolderInfo info in Instance.folders.Values)
            {
                if( info.processors != null && info.processors.Count != 0)
                {
                    for(int i = 0; i<info.processors.Count;)
                    {
                        if( RSProlib.GetProIndex(info.processors[i].processor_func) < 0)
                        {
                            info.processors.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    if(info.processors.Count == 0)
                    {
                        info.processors = null;
                    }
                }
            }
            SaveEdManifest();
        }

        public static void ApplyFolderInfo(RSFolderInfo info, bool active_child =true)
        {
            if (info == null)
            {
                return;
            }
            string[] files = TutFileUtil.GetFiles(info.path);
            for (int i = 0; i<files.Length; i++)
            {
                if (files [i].Contains(".meta"))
                    continue;
                ConvertToRSPath(files [i]);
                RefrushInfoInheritFromFolder(files[i],info);
            }

            if (!active_child)
            {
                if(info.rstype == RSType.RT_NIL)
                {
                    Instance.folders.Remove(info.path);
                }
                return;
            }

            string[] folders = TutFileUtil.GetDirectories(info.path);

            for (int i = 0; i<folders.Length; i++)
            {
                RefrushInfoInheritFromFolder(ConvertToRSPath(folders [i]),info,true);
            }
            if(info.rstype == RSType.RT_NIL &&( info.processors == null || info.processors.Count == 0))
            {
                Instance.folders.Remove(info.path);
            }
        }

        private static ManifestState GetFolderState(RSFolderInfo info)
        {
            if (info == null)
            {
                return ManifestState.MS_NIL;
            }
            RSFolderInfo source = GetFolderInfo(info.path);
            if (source == null)
            {
                return ManifestState.MS_NONEXISTENCE;
            }
            if (source == info)
                return ManifestState.MS_UNCHANGED;
            return ManifestState.MS_CHANGED;
        }

        private static ManifestState GetFileState(RSFileInfo info)
        {
            if (info == null)
            {
                return ManifestState.MS_NIL;
            }
            RSFileInfo source = GetFileInfo(info.path);
            if (source == null)
            {
                return ManifestState.MS_NONEXISTENCE;
            }
            if (source == info)
                return ManifestState.MS_UNCHANGED;
            return ManifestState.MS_CHANGED;
        }

        private static RSFileInfo GetFileInfo(string path)
        {
            if (Instance.edfiles == null)
                return null;
            RSFileInfo info = null;
            Instance.edfiles.TryGetValue(path, out info);
            if(info != null)
            {
                return (RSFileInfo)info.Clone();
            }
            return info;
        }

        private static void RefrushFileInfo(RSFileInfo info, ManifestState state)
        {

			if(info.rstype == RSType.RT_RESOURCES && RSInspector.LimitedSuffixs.isNoSupportLocalAsset(info.path))
				return;
			if(info.rstype != RSType.RT_BUNDLE && RSInspector.LimitedSuffixs.isOnlyExternalAsset(info.path))
				return;
			if(info.rstype != RSType.RT_STREAM && RSInspector.LimitedSuffixs.isOnlyExtralLocalAsset(info.path))
				return;

            int index = -1;
            switch (state)
            {
                case ManifestState.MS_NIL:
                case ManifestState.MS_UNCHANGED:
                    break;
                case ManifestState.MS_CHANGED:
                    Instance.edfiles [info.path] = info;
                    index = Instance.files.FindIndex(i=>i.path == info.path);
                    if(index < 0)
                    {
                        Instance.files.Add((RSFileInfo)info.Clone());
                    }
                    else
                    {
                        Instance.files[index] =(RSFileInfo) info.Clone();
                    }
                    ApplyFileInfo(info);
                    break;
                case ManifestState.MS_NONEXISTENCE:
                    Instance.edfiles.Add(info.path, info);
                    index = Instance.files.FindIndex(i=>i.path == info.path);
                    if(index < 0)
                    {
                        Instance.files.Add((RSFileInfo)info.Clone());
                    }
                    else
                    {
                        Instance.files[index] = (RSFileInfo)info.Clone();
                    }
                    ApplyFileInfo(info);
                    break;
            }
        }

        public static void RemoveFileInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (obj != null)
            {
                List<string> groups = new List<string>();
                groups.AddRange(AssetDatabase.GetLabels(obj));
                groups = groups.FindAll(g => !g.Contains("AssetGroup"));
                if(groups!=null)
                {
                    AssetDatabase.SetLabels(obj, groups.ToArray());
                }
            }

            RSFileInfo info = GetFileInfo(path);
            if (info == null)
                return;

            Instance.edfiles.Remove(info.path);
            int index = Instance.files.FindIndex(i=>i.path == info.path);
            if(index >= 0)
            {
                Instance.files.RemoveAt(index);
            }
        }

        public static void ApplyFileInfo(RSFileInfo info)
        {
            if (info == null)
            {
                return;
            }

            Object obj = AssetDatabase.LoadAssetAtPath(info.path, typeof(Object));
            if (obj == null)
            {
                Debug.LogError(TutNorm.LogErrFormat("Editor Manifest" ," Invaild RSFileInfo path: " + info.path));
                return;
            }

            List<string> groups = new List<string>();
            groups.AddRange(AssetDatabase.GetLabels(obj));

            bool find = false;
            bool need_update = false;
            string group = "";
            string tgroup = "AssetGroup" + info.group.ToString("D2");
            for (int i = 0; i<groups.Count; i++)
            {
                group = groups [i];
                if (group.Contains("AssetGroup"))
                {
                    if (group != tgroup)
                    {
                        groups [i] = tgroup;
                        need_update = true;
                    }
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                need_update = true;
                groups.Add(tgroup);
            }

            if (need_update)
            {
                AssetDatabase.SetLabels(obj, groups.ToArray());
            }

            if(info.rstype == RSType.RT_NIL)
            {
                Instance.edfiles.Remove(info.path);
                int index = Instance.files.FindIndex(i=>i.path == info.path);
                if(index >= 0)
                {
                    Instance.files.RemoveAt(index);
                }
            }
        }

        public static string ConvertToRSPath(string path)
        {
            string tp = path;
            if (!tp .Contains("Assets/"))
            {
                return string.Empty;
            }
            
            if (!tp.StartsWith("Assets/"))
            {
                tp = tp.Substring(tp.IndexOf("Assets/"));
            }

            return tp;
        }

        public static RSInfo GetInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string tp = ConvertToRSPath(path);

            if (string.IsNullOrEmpty(tp))
                return null;

            if (TutFileUtil.IsFolder(tp))
            {
                return GetFolderInfo(tp);
            }
            return GetFileInfo(tp);
        }

        public static ManifestState GetInfoState(RSInfo info)
        {

            if (info == null)
                return ManifestState.MS_NIL;
            if (info.GetType() == typeof(RSFileInfo))
            {
                return GetFileState((RSFileInfo)info);
            }
            return GetFolderState((RSFolderInfo)info);       
        }

        public static void RefrushInfo(RSInfo info, ManifestState state)
        {
            if (info == null)
                return;
            if (info.GetType() == typeof(RSFileInfo))
            {
                RefrushFileInfo((RSFileInfo)info, state);
            } else
                RefrushFolderInfo((RSFolderInfo)info, state);
        }
      
        public static void RemoveInfo(string path)
        {
            string tp = ConvertToRSPath(path);
            
            if (string.IsNullOrEmpty(tp))
                return;
            
            if (TutFileUtil.IsFolder(tp))
            {
                RemoveFolderInfo(tp);
            }
            RemoveFileInfo(tp);
        }

        public static bool RefrushInfoInheritFromFolder(string path,RSFolderInfo finfo,bool include_child = false)
        {
            if (string.IsNullOrEmpty(path) || finfo == null)
                return false;
            RSInfo info = GetInfo(path);
            if(info == null)
            {
                if(TutFileUtil.IsFolder(path))
                {
                    RSFolderInfo _info = (RSFolderInfo)((RSFolderInfo)finfo).Clone();
                    _info.path = path;
                    ManifestState state = GetFolderState(_info);
                    
                    if (state == ManifestState.MS_CHANGED || state == ManifestState.MS_NONEXISTENCE)
                    {
                        RefrushFolderInfo(_info, state);
                        if(include_child)
                            ApplyFolderInfo(_info,true);
                    }
                }
                else
                {
                    RSFileInfo _info = new RSFileInfo();
                    _info.path = path;
                    _info.type = finfo.type;
                    _info.group = finfo.group;
                    RefrushFileInfo(_info, GetFileState(_info));
                }
            }
            else
            {
                if(TutFileUtil.IsFolder(path))
                {
                    RSFolderInfo _info = (RSFolderInfo)((RSFolderInfo)finfo).Clone();
                    _info.path = path;
                    ManifestState state = GetFolderState(_info);
                    
                    if (state == ManifestState.MS_CHANGED || state == ManifestState.MS_NONEXISTENCE)
                    {
                        RefrushFolderInfo(_info, state);
                        if(include_child)
                            ApplyFolderInfo(_info,true);
                    }
                }
                else
                {
                    if (finfo.type != info.type || finfo.group != info.group)
                    {
                        info.type = finfo.type;
                        info.group = finfo.group;
                        RefrushFileInfo((RSFileInfo)info, GetFileState((RSFileInfo)info));
                    }
                }
            }
            return true;
        }

        public static RSFileInfo[] GetFileInfoFromFolder(RSFolderInfo info,bool include_child)
        {
            if(info == null)
                return null;

            string[] files = TutFileUtil.GetFiles(info.path,System.IO.SearchOption.AllDirectories);
            List<RSFileInfo> infos = new List<RSFileInfo>();
            RSFileInfo finfo = null;
            for(int i = 0;i<files.Length;i++)
            {
                finfo = GetFileInfo(files[i]);
                if(finfo != null)
                {
                    infos.Add(finfo);
                }
            }
            return infos.ToArray();
        }

		public static RSFileInfo[] GetFileInfos()
		{
			return Instance.files.ToArray ();
		}
    }
}