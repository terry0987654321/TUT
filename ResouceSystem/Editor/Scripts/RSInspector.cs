
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Rotorz.ReorderableList;
using TUT;

namespace TUT.RSystem
{
	public class RSLimitedSuffixs
	{
		public List<string> NoSupportLocalAssetSuffixs = new List<string> ();
		
		public List<string> OnlyExtralLocalAssetSuffixs = new List<string> ();
		
		public List<string> OnlyExternalAssetSuffixs = new List<string> ();
		
		public List<string> DontNeedBundleAssetSuffixs = new List<string> ();

		public bool isNoSupportLocalAsset(string path)
		{
			if(NoSupportLocalAssetSuffixs == null)
				return false;
			return NoSupportLocalAssetSuffixs.Contains (TutFileUtil.GetFileSuffix (path));
		}

		public bool isOnlyExtralLocalAsset(string path)
		{
			if(OnlyExtralLocalAssetSuffixs == null)
				return false;
			return OnlyExtralLocalAssetSuffixs.Contains (TutFileUtil.GetFileSuffix (path));	
		}

		public bool isOnlyExternalAsset(string path)
		{
			if(OnlyExternalAssetSuffixs == null)
				return false;
			return OnlyExternalAssetSuffixs.Contains (TutFileUtil.GetFileSuffix (path));	
		}

		public bool isDontNeedBundleAsset(string path)
		{
			if(DontNeedBundleAssetSuffixs == null)
				return false;
			return DontNeedBundleAssetSuffixs.Contains (TutFileUtil.GetFileSuffix (path));	
		}
	}


    [InitializeOnLoad]
    public class RSInspector : EditorWindow
    {
		[MenuItem ("Assets/TUT Asset Inspector",false,1111)]
		[MenuItem ("Window/TUT Asset Inspector &a",false,1111)]
        static void InitWindow()
        {
            RSInspector inspector = (RSInspector)EditorWindow.GetWindow 
                (typeof(RSInspector), false, "AssetInspector");
            inspector.Show();
        }

        private Guid mGuid;

        private string mPath = "";

        private string mName = "";

        private List<string> mSelect_paths = new List<string>();

        private Texture2D mThumbnail = null;

        private RSInfo mInfo = null;

        private RSInfo mSourceInfo = null;

        private bool mFolder_active_self = false;

        private bool mFolder_active_childs = false;

        private DrawType mType = DrawType.DT_NIL;

        private TutEdProfiler.PerfabMemroyDetail mFileMemroyDetail = null;

        public static readonly string s_read_tmp_prefab_file = "tmp_prefab_memory.tmp";

        private bool mNeedChangeManifestName = false;

        private int mCurSelectedManifestIndex = -1;

        private string mNewManifestName = string.Empty;

		private static RSLimitedSuffixs mLimitedSuffixs = null;

		public static RSLimitedSuffixs LimitedSuffixs
		{
			get
			{
				if(mLimitedSuffixs == null)
				{
					mLimitedSuffixs = TutFileUtil.ReadJsonFile<RSLimitedSuffixs>(RSEdConst.s_RSEdDataPath + RSEdConst.s_RSEdDepotFolder + "/" + RSEdConst.s_RSLimitedSuffixName);
					if(mLimitedSuffixs == null)
					{
						mLimitedSuffixs = new RSLimitedSuffixs();
						SaveLimitedSuffixs();
					}
				}
				return mLimitedSuffixs;
			}
		}

		private static void SaveLimitedSuffixs()
		{
			for(int i = 0;i<mLimitedSuffixs.DontNeedBundleAssetSuffixs.Count;)
			{
				if(string.IsNullOrEmpty(mLimitedSuffixs.DontNeedBundleAssetSuffixs[i]))
				{
					mLimitedSuffixs.DontNeedBundleAssetSuffixs.RemoveAt(i);
				}
				else
					i++;
			}
			for(int i = 0;i<mLimitedSuffixs.NoSupportLocalAssetSuffixs.Count;)
			{
				if(string.IsNullOrEmpty(mLimitedSuffixs.NoSupportLocalAssetSuffixs[i]))
				{
					mLimitedSuffixs.NoSupportLocalAssetSuffixs.RemoveAt(i);
				}
				else
					i++;
			}
			for(int i = 0;i<mLimitedSuffixs.OnlyExternalAssetSuffixs.Count;)
			{
				if(string.IsNullOrEmpty(mLimitedSuffixs.OnlyExternalAssetSuffixs[i]))
				{
					mLimitedSuffixs.OnlyExternalAssetSuffixs.RemoveAt(i);
				}
				else
					i++;
			}
			for(int i = 0;i<mLimitedSuffixs.OnlyExtralLocalAssetSuffixs.Count;)
			{
				if(string.IsNullOrEmpty(mLimitedSuffixs.OnlyExtralLocalAssetSuffixs[i]))
				{
					mLimitedSuffixs.OnlyExtralLocalAssetSuffixs.RemoveAt(i);
				}
				else
					i++;
			}

			TutFileUtil.WriteJsonFile(mLimitedSuffixs,RSEdConst.s_RSEdDataPath + RSEdConst.s_RSEdDepotFolder + "/" + RSEdConst.s_RSLimitedSuffixName,true);
		}
       
        private enum DrawType
        {
            DT_NIL,
            DT_INFO,
            DT_FILE,
            DT_FOLDER,
        }

        private RSInfo info
        {
            get
            {
                if (mInfo == null)
                {
                    RefrushCurObj();
                }
                return mInfo;
            }
        }

        void SelectObj(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                Guid guid = TutGuidUtil.TextToGUID(path);
                if (string.IsNullOrEmpty(path))
                {
                    Reset();
                    return;
                }

                if (!mGuid.Equals(guid) || mInfo == null)
                {

                    if(path.Contains(".prefab"))
                        mFileMemroyDetail = TutEdProfiler.GetPerfabSize(path);
                    else
                        mFileMemroyDetail = null;
                    
                    mFolder_active_childs = false;
                    mFolder_active_self = false;
                    mGuid = guid;
                    mName = obj.name;
                    mThumbnail = AssetPreview.GetMiniThumbnail(obj);
                    mPath = path;
                    mType = TutFileUtil.IsFolder(mPath) ? DrawType.DT_FOLDER : DrawType.DT_FILE;

                    mSourceInfo = RSEdManifest.GetInfo(mPath);
                    mInfo = RSEdManifest.GetInfo(mPath);

                    if (mInfo == null)
                    {
                        switch (mType)
                        {
                            case DrawType.DT_FILE:
                                mInfo = new RSFileInfo();
                                break;
                            case DrawType.DT_FOLDER:
                                mInfo = new RSFolderInfo();
                                break;
                        }
                        mInfo.path = mPath;
                        if (RSInfo.isResTypeFromPath(mPath))
                            mInfo.type = (int)RSType.RT_RESOURCES;
                        else
                            mInfo.type = (int)RSType.RT_NIL;
                    }
                }
                
                if (mSourceInfo == null)
                {
                    switch (mType)
                    {
                        case DrawType.DT_FILE:
                            mSourceInfo = new RSFileInfo();
                            break;
                        case DrawType.DT_FOLDER:
                            mSourceInfo = new RSFolderInfo();
                            break;
                    }
                    mSourceInfo.path = mPath;
                    if (RSInfo.isResTypeFromPath(mPath))
                        mSourceInfo.type = (int)RSType.RT_RESOURCES;
                    else
                        mSourceInfo.type = (int)RSType.RT_NIL;
                }
            } else
            {
                Reset();
            }
        }

        void Reset()
        {
            mGuid = TutGuidUtil.TextToGUID("");
            mFileMemroyDetail = null;
            mInfo = null;
            mType = DrawType.DT_NIL;
            mFolder_active_childs = false;
            mFolder_active_self = false;
        }

        void SelectObjs(UnityEngine.Object[] objs)
        {
            mSelect_paths.Clear();

            string path = "";
            string tpath = "";
            bool had_res_asset = false;
            foreach (UnityEngine.Object obj in objs)
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    if (!mSelect_paths.Contains(path))
                    {
                        mSelect_paths.Add(path);
                        tpath += path;
                        if (RSInfo.isResTypeFromPath(path))
                        {
                            had_res_asset = true;
                        }
                    }
                }
            }

            if (mSelect_paths.Count == 0)
            {
                Reset();
                return;
            }

            Guid guid = TutGuidUtil.TextToGUID(tpath);
            if (!mGuid.Equals(guid) || info == null)
            {
                mFolder_active_childs = false;
                mFolder_active_self = false;
                mGuid = guid;
                mPath = "Multi-object not supported";
                mName = mSelect_paths.Count.ToString() + " Assets";
                mThumbnail = null;
                mType = DrawType.DT_INFO;

                bool same = true;
                RSInfo per_info = null;
                RSInfo cur_info = null;
                foreach (string i in mSelect_paths)
                {
                    cur_info = RSEdManifest.GetInfo(i);
                    if (per_info == null)
                        per_info = cur_info;
                    else
                    {
                        if ((cur_info == null) || (per_info.group != cur_info.group || per_info.type != cur_info.type))
                        {
                            same = false;
                            break;
                        }
                        per_info = cur_info;
                    }
                }

                if (same)
                {
                    mSourceInfo = RSEdManifest.GetInfo(mSelect_paths [0]);
                    mInfo = RSEdManifest.GetInfo(mSelect_paths [0]);
                    
                    if (mInfo == null)
                    {
                        mInfo = new TUT.RSystem.RSInfo();
                        mInfo.path = string.Empty;
                        mInfo.type = had_res_asset ? (int)RSType.RT_RESOURCES : (int)RSType.RT_NIL;
                    }
                    if (mSourceInfo == null)
                    {
                        mSourceInfo = new TUT.RSystem.RSInfo();
                        mSourceInfo.path = string.Empty;
                        mSourceInfo.type = had_res_asset ? (int)RSType.RT_RESOURCES : (int)RSType.RT_NIL;

                    }
                } else
                {
                    mInfo = new TUT.RSystem.RSInfo();
                    mInfo.path = string.Empty;
                    mInfo.type = -1;
                    mInfo.group = RSEdConst.MaxGroupNum + 1;

                    mSourceInfo = new TUT.RSystem.RSInfo();
                    mSourceInfo.path = string.Empty;
                    mSourceInfo.type = -1;
                    mSourceInfo.group = RSEdConst.MaxGroupNum + 1;
                }

                if (had_res_asset)
                {
                    mInfo.path = "/Resources/"; 
                    mSourceInfo.path = "/Resources/"; 
                    if (same && (mInfo.type == (int)RSType.RT_RESOURCES))
                    {
                        mInfo.type = -2;
                        mSourceInfo.type = -2;
                    }
                }
            }
        }

        void RefrushCurObj()
        {
            if (Selection.objects != null)
            {
                if (Selection.objects.Length == 1)
                {
                    SelectObj(Selection.activeObject);
                } else
                {
                    SelectObjs(Selection.objects);
                }
            } else
            {
                Reset();
            }
        }

        void OnInspectorUpdate()
        {
            RefrushCurObj();
            Repaint();
        }

        void DrawInfo()
        {
            int type = info.type;
            string path = mPath;

            GUI.skin = RSEdConst.Skin;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
            GUILayout.Label(mThumbnail, GUILayout.Width(64), GUILayout.Height(64));
            SetLabel(mName, RSEdConst.Skin.customStyles [1]);

            EditorGUILayout.BeginVertical();
            GUILayout.Space(20);
            if (info.group < (RSEdConst.MaxGroupNum + 1))
                SetLabel("Asset Group " + info.group.ToString("D2"), null, 100);
            else
                SetLabel("Asset Group --", null, 100);
            GUILayout.Space(-20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(100);
            GUI.enabled = !RSInfo.isResTypeFromPath(info.path);
            int g = EditorGUILayout.IntPopup(info.group, RSEdConst.RSGroupNames, RSEdConst.RSGroupIndexs, GUILayout.Width(11));
            GUI.enabled = true;
            if (g != info.group)
            {
                if (!string.IsNullOrEmpty(info.path))
                {
                    if (RSInfo.isResTypeFromPath(info.path))
                    {
                        g = info.group;
                        this.ShowNotification(new GUIContent("Invalid Operation"));
                    }
                }
            }
            info.group = g;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(-30);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(80);
            SetLabel(path,null,0,true);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));
            SetLabel("Request Type:", RSEdConst.Skin.customStyles [2], 120);
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            Texture icon = RSEdConst.nil_icon;
            if (type < 0)
            {
                if (type == -1)
                    GUILayout.Box("(multifold request type)", GUILayout.Height(30), GUILayout.Width(200));
                else
                    GUILayout.Box("(contain res files )", GUILayout.Height(30), GUILayout.Width(200));
            } else
            {
                switch ((RSType)type)
                {
                    case RSType.RT_BUNDLE:
                        icon = RSEdConst.bld_icon;
                        break;
                    case RSType.RT_NIL:
                        icon = RSEdConst.nil_icon;
                        break;
                    case RSType.RT_RESOURCES:
                        icon = RSEdConst.res_icon;
                        break;
                    case RSType.RT_STREAM:
                        icon = RSEdConst.stm_icon;
                        break;
                }
                GUILayout.Box(new GUIContent(RSEdConst.RSTypeNames [type], icon), GUILayout.Height(35), GUILayout.Width(200));
            }
          
            GUILayout.Space(-30);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.enabled = !RSInfo.isResTypeFromPath(info.path);
            int t = EditorGUILayout.IntPopup(type, RSEdConst.RSTypeNames, RSEdConst.RSTypeIndexs, GUILayout.Width(10)); 
            GUI.enabled = true;
            if (t != type)
            {
                if (!string.IsNullOrEmpty(info.path))
                {
                    if (RSInfo.isResTypeFromPath(info.path))
                    {
                        t = info.type;
                        this.ShowNotification(new GUIContent("Invalid Operation"));
                    }
					else
					if(LimitedSuffixs.isNoSupportLocalAsset(info.path))
					{
						if(t == (int)RSType.RT_RESOURCES)
						{
							t = info.type;
							this.ShowNotification(new GUIContent("Invalid Operation: No Support Local Asset"));
						}
					}
					else
					if(LimitedSuffixs.isOnlyExternalAsset(info.path))
					{
						if(t != (int)RSType.RT_BUNDLE)
						{
							t = info.type;
							this.ShowNotification(new GUIContent("Invalid Operation: Only Support External Asset"));
						}
					}
					else
					if(LimitedSuffixs.isOnlyExtralLocalAsset(info.path))
					{
						if(t != (int)RSType.RT_STREAM)
						{
							t = info.type;
							this.ShowNotification(new GUIContent("Invalid Operation: Only Support Extral Local Asset"));
						}
					}
                }

                info.type = t;
            }

            GUI.skin = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            GUI.skin = null;
        }

        void DrawFileInfo()
        {
            if (info == null)
            {
                Reset();
                return;
            }
            DrawInfo();

            RSFileInfo finfo = (RSFileInfo)info;

            GUI.skin = RSEdConst.Skin;
            EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.Height(210));
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Width(350), GUILayout.Height(205));


            GUILayout.Space(-345);
            EditorGUILayout.BeginVertical();
            // file info
            SetLabel("ASSET DETAIL", RSEdConst.Skin.customStyles [2]);
            string value = "--";

            if (string.IsNullOrEmpty(finfo.guid))
            {
                value = "(unknown)";
            } else
                value = finfo.guid;
            
            DrawFileDetail("asset guid:", value);

            if (string.IsNullOrEmpty(finfo.buid))
            {
                value = "(unknown)";
            } else
                value = finfo.buid;
            
            DrawFileDetail("build guid:", value);

            if (finfo.size < 0)
            {
                value = "(unknown)";
            } else
                value = finfo.size.ToString();

            DrawFileDetail("build size:", value);

           if(mFileMemroyDetail != null) 
                DrawFileDetail("usage memroy:", TutProfiler.toMemoryString(mFileMemroyDetail.size),"detail",-30,()=>{
                    if(mFileMemroyDetail != null)
                    {
                        if( TutFileUtil.WriteTxtFile(mFileMemroyDetail.ToString(),RSEdConst.s_RSEdTmpPath
                                                    +s_read_tmp_prefab_file))
                        {
                            TextAsset txt = (TextAsset)AssetDatabase.LoadAssetAtPath(RSEdConst.s_RSEdTmpPath
                                                                                     +s_read_tmp_prefab_file,typeof(TextAsset));
                            AssetDatabase.OpenAsset(txt);
                        }
                    }
                });

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUI.skin = null;

            DrawOperation(DrawType.DT_FILE, 0);
        }

        void DrawOperation(DrawType type, int off_vertical)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(off_vertical);
            bool changed = isChanged();

            if (type == DrawType.DT_FOLDER)
            {
                GUI.skin = RSEdConst.Skin;
                EditorGUILayout.BeginHorizontal(GUILayout.Width(450));
                GUILayout.Space(110);
                //GUI.enabled = changed;

                SetLabel("Active Self", null, 80);
                mFolder_active_self= EditorGUILayout.Toggle(mFolder_active_self);
                GUILayout.Space(-80);
                SetLabel("Active Childs", null, 80);
                mFolder_active_childs = EditorGUILayout.Toggle(mFolder_active_childs);

                if (mFolder_active_childs)
                {
                    mFolder_active_self = true;
                }

                //GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                GUI.skin = null;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(110);
            GUI.enabled = changed || mFolder_active_self || mFolder_active_childs;

            if (GUILayout.Button("Revert", GUILayout.Width(80)))
            {
                mFolder_active_childs = false;
                mFolder_active_self = false;
                mInfo = mSourceInfo.Clone();
            }
            if (GUILayout.Button("Apply", GUILayout.Width(80)))
            {
                ApplyOperation();
                mFolder_active_childs = false;
                mFolder_active_self = false;
            }

            if(mType != DrawType.DT_FOLDER || mFolder_active_self || mFolder_active_childs)
            {
                GUI.enabled = (!changed) && (mInfo.type != (int)RSType.RT_NIL  && mInfo.type >= 0);//&& mInfo.type != (int)RSType.RT_RESOURCES

                if (GUILayout.Button("Build ...", GUILayout.Width(80)))
                {
                    BuildOperation();
                    mFolder_active_childs = false;
                    mFolder_active_self = false;
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void BuildOperation()
        {
            string exprot_path = EditorUtility.OpenFolderPanel("Exprot Asset Bundle Path",Application.dataPath+"/../","");
            if(string.IsNullOrEmpty(exprot_path))
                return;
            switch (mType)
            {
                case DrawType.DT_INFO:
                    BuildMutiled(exprot_path);
                    break;
                case DrawType.DT_FILE:
                    BuildFile(exprot_path);
                    break;
                case DrawType.DT_FOLDER:
                    BuildFolder(exprot_path);
                    break;
                case DrawType.DT_NIL:
                    BuildAll(exprot_path);
                    break;
            }
        }

        void BuildMutiled(string exprot_path)
        {
            RSFileInfo _info = null;
            List<RSFileInfo> infos= new List<RSFileInfo>();

            foreach(string path in mSelect_paths)
            {
                if(TutFileUtil.IsFolder(path))
                    continue;
                _info =(RSFileInfo) RSEdManifest.GetInfo(path);
                if(_info == null)
                {
                    continue;
                }
                infos.Add(_info);
            }

            if(infos.Count == 0)
                return;

            RSManifest manifest = RSBuildPipelineUtil.BuildPipeline(infos.ToArray(),exprot_path);
            if(manifest != null)
            {
                for(int i = 0; i< infos.Count;i++)
                {
                    infos[i] = (RSFileInfo)(manifest.GetInfo(infos[i].path).Clone());
                    RSEdManifest.RefrushInfo(infos[i],RSEdManifest.GetInfoState(infos[i]));
                }
            }
//            SaveManifest();
        }

        void BuildFile(string exprot_path)
        {
            RSManifest manifest = RSBuildPipelineUtil.BuildPipeline(new RSFileInfo[]{(RSFileInfo)mInfo},exprot_path);
            if (manifest != null)
            {
                mInfo = manifest.GetInfo(mInfo.path).Clone();
                RSEdManifest.RefrushInfo(mInfo, RSEdManifest.GetInfoState(mInfo));
                SyncSourceInfo();
            }

//            SaveManifest();
        }

        void BuildFolder(string exprot_path)
        {
            if(!TutFileUtil.IsFolder( mInfo.path ))
                return;

            RSFileInfo[] infos = RSEdManifest.GetFileInfoFromFolder((RSFolderInfo)mInfo,mFolder_active_childs);

            if(infos == null)
                return;

            RSManifest manifest = RSBuildPipelineUtil.BuildPipeline(infos,exprot_path);

            try
            {
            for(int i = 0;i<infos.Length;i++)
            {
                infos[i] = (RSFileInfo)(manifest.GetInfo(infos[i].path).Clone());
                RSEdManifest.RefrushInfo(infos[i],RSEdManifest.GetInfoState(infos[i]));
            }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
//            SaveManifest();
        }

        public void BuildAll(string exprot_path,string tag ="")
        {
            RSFileInfo[] infos = RSEdManifest.GetFileInfos();
            RSManifest manifest = RSBuildPipelineUtil.BuildPipeline(infos,exprot_path,tag);
            RSFileInfo info = null;
            for(int i = 0;i<infos.Length;i++)
            {
                info = manifest.GetInfo(infos[i].path);
                if(info != null)
                {
                    infos[i] = (RSFileInfo)(info.Clone());
                    RSEdManifest.RefrushInfo(infos[i],RSEdManifest.GetInfoState(infos[i]));
                }
            }
            RSEdManifest.WriteEdManifest();
        }

        void ApplyOperation()
        {
            switch (mType)
            {
                case DrawType.DT_INFO:
                    ApplyMutiled();
                    break;
                case DrawType.DT_FILE:
                    ApplyFile();
                    break;
                case DrawType.DT_FOLDER:
                    ApplyFolder();
                    break;
            }
            EditorApplication.RepaintProjectWindow();
        }

        public delegate void FDtlBtnClickHandler();

        void DrawFileDetail(string detail, string value,string btn_msg =  "",float offset = 0,FDtlBtnClickHandler cb = null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            SetLabel(detail);

            if(btn_msg != "")
            {
                GUISkin pre = GUI.skin;
                GUI.skin = null;
                GUILayout.Space(offset);
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(btn_msg,GUILayout.Width(50)))
                {
                    if(cb != null)
                        cb();
                }
                EditorGUILayout.EndHorizontal();
                GUI.skin = pre;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(60);
            SetLabel(value);
            EditorGUILayout.EndHorizontal();
        }

        Vector2 folder_offset = Vector2.zero;

        void DrawFolderInfo()
        {
            if (info == null)
            {
                Reset();
                return;
            }
            DrawInfo();

            EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.Height(210));
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUI.skin = RSEdConst.Skin;
            GUILayout.Box("", GUILayout.Width(350), GUILayout.Height(165));
           
            GUILayout.Space(-345);

            EditorGUILayout.BeginVertical(GUILayout.Width(340), GUILayout.Height(165));
            SetLabel("SETUP PROCESS...", RSEdConst.Skin.customStyles [2]);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-10);
            folder_offset = EditorGUILayout.BeginScrollView(folder_offset);
            RSFolderInfoAdaptor adaptor = new RSFolderInfoAdaptor((RSFolderInfo)info, this);
            ReorderableListControl.DrawControlFromState(adaptor, null, 0);
//            mInfo = (RSInfo)adaptor.info;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUI.skin = null;

            DrawOperation(DrawType.DT_FOLDER, -30);
        }

        void DrawMultiObject()
        {
            if (info == null)
            {
                Reset();
                return;
            }
            DrawInfo();
            DrawOperation(DrawType.DT_INFO, 10);
        }

        bool isChanged()
        {
            if (mSourceInfo == null || mInfo == null)
                return false;
            switch (mType)
            {
                case DrawType.DT_INFO:
                    return mSourceInfo != mInfo;
                case DrawType.DT_FILE:
                    return ((RSFileInfo)mSourceInfo) != ((RSFileInfo)mInfo);
                case DrawType.DT_FOLDER:
                    return ((RSFolderInfo)mSourceInfo) != ((RSFolderInfo)mInfo);
            }
            return false;
        }

        void SyncSourceInfo()
        {
            switch (mType)
            {
                case DrawType.DT_INFO:
                    mSourceInfo = mInfo.Clone();
                    break;
                case DrawType.DT_FILE:
                    mSourceInfo = ((RSFileInfo)mInfo).Clone();
                    break;
                case DrawType.DT_FOLDER:
                    mSourceInfo = ((RSFolderInfo)mInfo).Clone();
                    break;
            }
        }

        void SaveManifest()
        {
            RSEdManifest.SaveEdManifest();
        }

		Vector3 limit_pos = Vector3.zero;
		int build_type = 0;
        void DrawError()
        {
            GUI.skin = RSEdConst.Skin;
            GUILayout.BeginVertical(GUILayout.Width(400));
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(RSEdConst.err_icon),GUILayout.Width(80),GUILayout.Height(80));
            SetLabel("\nPLEASE SELECT A FILE\n\t\t OR A FOLDER....", RSEdConst.Skin.customStyles [2],50);
            GUILayout.EndHorizontal();
//            GUILayout.BeginHorizontal();
            SetLabel("Resource definition layout",RSEdConst.Skin.customStyles [3],700);
            SetLabel("current using: "+RSEdManifest.GetWorkingManifestName() + " manifest",RSEdConst.Skin.customStyles [2]);
            GUILayout.BeginVertical(GUILayout.Width(400));
//            GUILayout.EndHorizontal();
            GUI.skin = null;
            if(GUILayout.Button(" Create New Manifest "))
            {
                mCurSelectedManifestIndex = -2;
                SaveManifestToDepot(mCurSelectedManifestIndex);
            }
            if(!mNeedChangeManifestName && GUILayout.Button("Save Cur Manifest to Depot") )
            {
                mNewManifestName = RSEdManifest.GetWorkingManifestName();
                SaveManifestToDepot(RSEdManifest.CurDepotIndex());
            }
            if (!SelectManifest())
            {
                GUI.skin = null;
                if(mNeedChangeManifestName)
                {
                    SaveManifestToDepot(mCurSelectedManifestIndex);
                }
            }
            GUILayout.EndVertical();
			limit_pos = EditorGUILayout.BeginScrollView (limit_pos);
			EditorGUILayout.BeginVertical ();

//			EditorGUILayout.BeginHorizontal();
//			SetLabel ("Build Mainfest Type:", RSEdConst.Skin.customStyles [2], 300);
//			EditorGUILayout.BeginVertical(GUILayout.Width(150));
//
//			Texture icon = RSEdConst.nil_icon;
//			{
//				switch ((RSType)build_type)
//				{
//				case RSType.RT_BUNDLE:
//					icon = RSEdConst.bld_icon;
//					break;
//				case RSType.RT_NIL:
//					icon = RSEdConst.nil_icon;
//					break;
//				case RSType.RT_RESOURCES:
//					icon = RSEdConst.res_icon;
//					break;
//				case RSType.RT_STREAM:
//					icon = RSEdConst.stm_icon;
//					break;
//				}
//				GUILayout.Box(new GUIContent(RSEdConst.RSTypeNames [build_type], icon), GUILayout.Height(35), GUILayout.Width(200));
//			}
//			
//			GUILayout.Space(-30);
//			EditorGUILayout.BeginHorizontal();
//			GUILayout.Space(10);
//			build_type = EditorGUILayout.IntPopup(build_type, RSEdConst.RSTypeNames, RSEdConst.RSTypeIndexs, GUILayout.Width(10)); 
//			GUI.skin = null;
//			EditorGUILayout.EndHorizontal();
//			EditorGUILayout.EndHorizontal ();
			if(GUILayout.Button("Build ....",GUILayout.MaxWidth(200)))
			{
                RSAssetsTransfer.TransferToBuildState();//(RSType)build_type);
//                EditorGUILayout.EndVertical ();
                BuildOperation();
			}
            if(GUILayout.Button("Recover ....",GUILayout.MaxWidth(200)))
			{
				RSAssetsTransfer.TransferToDepState();
			}
//			EditorGUILayout.EndVertical ();

			SetLabel("Dont Need Bundle Asset Suffixs",RSEdConst.Skin.customStyles [2]);
			ReorderableListGUI.ListField<string> (LimitedSuffixs.DontNeedBundleAssetSuffixs, ItemDrawer);
			if(GUILayout.Button(" Apply "))
			{
				SaveLimitedSuffixs();
			}
			SetLabel("No Support Local Asset Suffixs",RSEdConst.Skin.customStyles [2]);
			ReorderableListGUI.ListField<string> (LimitedSuffixs.NoSupportLocalAssetSuffixs, ItemDrawer);
			if(GUILayout.Button(" Apply "))
			{
				SaveLimitedSuffixs();
			}
			SetLabel("Only External Asset Suffixs",RSEdConst.Skin.customStyles [2]);
			ReorderableListGUI.ListField<string> (LimitedSuffixs.OnlyExternalAssetSuffixs, ItemDrawer);
			if(GUILayout.Button(" Apply "))
			{
				SaveLimitedSuffixs();
			}
			SetLabel("Only Extral Local Asset Suffixs",RSEdConst.Skin.customStyles [2]);
			ReorderableListGUI.ListField<string> (LimitedSuffixs.OnlyExtralLocalAssetSuffixs, ItemDrawer);
			if(GUILayout.Button(" Apply "))
			{
				SaveLimitedSuffixs();
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndScrollView ();
            GUILayout.EndVertical();
            GUI.skin = null;
        }

		string ItemDrawer(Rect position, string item)
		{
			return EditorGUI.TextField (new Rect (position.x, position.y, position.width, 20), "suffix ", item);
		}

        bool SelectManifest()
        {
            bool need_save = false;
            if (!mNeedChangeManifestName)
            {
                mCurSelectedManifestIndex = EditorGUILayout.IntPopup("Change Manifest: ", RSEdManifest.CurDepotIndex(), RSEdManifest.DepotManifests(), RSEdManifest.DepotManifestIndexs());
                if (mCurSelectedManifestIndex != RSEdManifest.CurDepotIndex() && mCurSelectedManifestIndex >= 0)
                {
                    mNewManifestName = RSEdManifest.GetWorkingManifestName();
                    need_save = true;
                }
            }
            if (need_save)
            {
                SaveManifestToDepot(mCurSelectedManifestIndex);
                return true;
            }
            else
                return false;
        }

        void SaveManifestToDepot(int index)
        {
            string select =mCurSelectedManifestIndex==-2?"Default": RSEdManifest.GetDepotManifestName(index);
            string cur = RSEdManifest.GetWorkingManifestName();
            string path_format = RSEdConst.s_RSEdDataPath+RSEdConst.s_RSEdDepotFolder+"/{0}"+ RSEdConst.s_RSEdManifestSuffix;
            if (index >= 0)
            {
                if(TutFileUtil.FileExist(RSEdManifest.WoringFile()) )
                    mNeedChangeManifestName = !TutGuidUtil.FileToGUID(RSEdManifest.WoringFile()).Equals(TutGuidUtil.FileToGUID(string.Format(path_format, select)));
                else
                    mNeedChangeManifestName =false;
            }
            else
                mNeedChangeManifestName = true;

            if(!mNeedChangeManifestName)
            {
                if(TutFileUtil.FileExist(string.Format(path_format,cur)))   
                {
                    mNeedChangeManifestName = true;
                }
                else
                {
                    RSEdManifest.ChanagerEdManifest(string.Format(path_format,select));
                    EditorApplication.RepaintProjectWindow();
                }
            }

//            if(mNeedChangeManifestName)
//                mNewManifestName = cur;
    
            if (mNeedChangeManifestName)
            {
                SetLabel("Save Current Manifest To Depot");
                mNewManifestName = EditorGUILayout.TextField("new manifest name",mNewManifestName);
                if(!string.IsNullOrEmpty(mNewManifestName) && mNewManifestName != cur)
                {
                    if(!TutFileUtil.FileExist(string.Format(path_format,mNewManifestName))) 
                    {
                        if(GUILayout.Button("Save as to "+mNewManifestName+" manifest"))
                        {
                            RSEdManifest.SaveToDepot(mNewManifestName);
                            if(index>=0)
                                RSEdManifest.ChanagerEdManifest(string.Format(path_format,select));
                            if(index == -2)
                                RSEdManifest.ChanagerEdManifest(null,"Default");
                            RSEdManifest.RefrushDepotManifest();
                            mNeedChangeManifestName = false;
                            mCurSelectedManifestIndex = RSEdManifest.GetDepotManifestIndex(index<0?cur:select);
                            EditorApplication.RepaintProjectWindow();
                        }
                    }
                }
                if(GUILayout.Button("Cover "+cur +" manifest" ))
                {
                    RSEdManifest.SaveToDepot(mNewManifestName);
                    if(index>=0)
                        RSEdManifest.ChanagerEdManifest(string.Format(path_format,select));
                    if(!string.IsNullOrEmpty(select)&& index<0)
                        RSEdManifest.ChanagerEdManifest(null,select);
                    RSEdManifest.RefrushDepotManifest();
                    mNeedChangeManifestName = false;
                    mCurSelectedManifestIndex = RSEdManifest.GetDepotManifestIndex(index<0?cur:select);
                    EditorApplication.RepaintProjectWindow();
                }
                if(GUILayout.Button("Dont Save On Change"))
                {
                    mNeedChangeManifestName = false;
                    if(index>=0)
                        RSEdManifest.ChanagerEdManifest(string.Format(path_format,select));
                    if(!string.IsNullOrEmpty(select) && index<0)
                        RSEdManifest.ChanagerEdManifest(null,select);
                    RSEdManifest.RefrushDepotManifest();
                    mNeedChangeManifestName = false;
                    mCurSelectedManifestIndex = RSEdManifest.GetDepotManifestIndex(index<0?cur:select);
                    EditorApplication.RepaintProjectWindow();
                }
            }
        }


        void SetLabel(string str, GUIStyle style = null, int width = 0,bool text_mode = false)
        {
            if (EditorGUIUtility.isProSkin)
            {
                GUI.color = Color.white;
            } else
            {
                GUI.color = Color.black;
            }
            if (style != null && width == 0)
            {
				if(text_mode)
					EditorGUILayout.TextField(str,style);
				else
                	GUILayout.Label(str, style);
            } else
                if (style == null && width > 0)
            {
				if(text_mode)
					EditorGUILayout.TextField(str,GUILayout.Width(width));
				else
                	GUILayout.Label(str, GUILayout.Width(width)); 
            } else
             if (style != null && width > 0)
            {
				if(text_mode)
					EditorGUILayout.TextField(str, style, GUILayout.Width(width));
				else
                	GUILayout.Label(str, style, GUILayout.Width(width));
            } else
			{
				if(text_mode)
					EditorGUILayout.TextField(str);
				else
                GUILayout.Label(str);
			}
            GUI.color = Color.white;
        }

        void ApplyFile()
        {
            RSEdManifest.RefrushInfo(info,RSEdManifest.GetInfoState(info));
            SyncSourceInfo();
            SaveManifest();
        }

        void ApplyFolder()
        {
            RSEdManifest.FiltrateFolderInfo((RSFolderInfo)info);
            RSEdManifest.RefrushInfo(info,RSEdManifest.GetInfoState(info));
            if(mFolder_active_self)
                RSEdManifest.ApplyFolderInfo((RSFolderInfo)info,mFolder_active_childs);
            else
            {
                RSFolderInfo finfo = (RSFolderInfo)info;
                if(finfo.rstype == RSType.RT_NIL &&( finfo.processors == null || finfo.processors.Count == 0))
                {
                    RSEdManifest.RemoveFolderInfo(finfo.path);
                }
            }
            SyncSourceInfo();   
            SaveManifest();
        }

        void ApplyMutiled()
        {
            RSInfo _info = null;
            foreach(string path in mSelect_paths)
            {
                _info = RSEdManifest.GetInfo(path);
                if(_info == null)
                {
                    if(TUT.TutFileUtil.IsFolder(path))
                    {
                        _info = new RSFolderInfo();

                    }
                    else
                    {
                        _info = new RSFileInfo();
                    }
                    _info.path = path;
                }
                _info.group = info.group;
                _info.type = info.type;
                RSEdManifest.RefrushInfo(_info,RSEdManifest.GetInfoState(_info));
            }
            SyncSourceInfo(); 
            SaveManifest();
        }

        void OnLostFocus()
        {
            if (isChanged())
            {
                if (!EditorUtility.DisplayDialog("Unapplied Asset Settting", "unapplied asset setting for " + mPath, "Apply", "Revert"))
                {
                    mInfo = mSourceInfo.Clone();
                } else
                {
                    ApplyOperation();
                }
            }
        }

        void OnGUI()
        {
            if (info == null)
            {
                DrawError();
                return;
            }
            switch (mType)
            {
                case DrawType.DT_INFO:
                    DrawMultiObject();
                    return;
                case DrawType.DT_FILE:
                    DrawFileInfo();
                    return;
                case DrawType.DT_FOLDER:
                    DrawFolderInfo();
                    return;
            }

            DrawError();
        }
    }
}
