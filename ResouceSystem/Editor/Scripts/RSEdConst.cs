
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{

    public static class RSEdConst
    {
        public static readonly string s_RSEdDataPath = "Assets/_oohhoo_data/";

        public static readonly string s_RSEdTmpPath = "Assets/_oohhoo_tmp/";

        public static readonly string s_RSEdDepotFolder ="_manifest_depot"; 

        public static readonly string s_RSEdWorkingName = "working.manifest";

        public static readonly string s_RSEdManifestSuffix = ".manifest";

		public static readonly string s_RSLimitedSuffixName = "_limited_suffix";

//        public static readonly string s_RSEdManifestPath = Application.dataPath + "/../ProjectSettings/";
//
//        public static readonly string s_RSEdManifestFile = "resouces.manifest";

        private static Texture m_nil_icon = null;

        public static Texture nil_icon
        {
            get
            {
                if (m_nil_icon != null)
                    return m_nil_icon;
                m_nil_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/nil_icon.png", typeof(Texture));
                if (m_nil_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/nil_icon.png cant found!"));
                }
                return m_nil_icon;
            }
        }

        private static Texture m_bld_icon = null;

        public static Texture bld_icon
        {
            get
            {
                if (m_bld_icon != null)
                    return m_bld_icon;
                m_bld_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/bld_icon.png", typeof(Texture));
                if (m_bld_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/bld_icon.png cant found!"));
                }
                return m_bld_icon;
            }
        }

        private static Texture m_res_icon = null;

        public static Texture res_icon
        {
            get
            {
                if (m_res_icon != null)
                    return m_res_icon;
                m_res_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/res_icon.png", typeof(Texture));
                if (m_res_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/res_icon.png cant found!"));
                }
                return m_res_icon;
            }
        }

        private static Texture m_stm_icon = null;


        public static Texture stm_icon
        {
            get
            {
                if (m_stm_icon != null)
                    return m_stm_icon;
                m_stm_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/stm_icon.png", typeof(Texture));
                if (m_stm_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/stm_icon.png cant found!"));
                }
                return m_stm_icon;
            }
        }


        private static GUISkin mSkin = null;

        public static GUISkin Skin
        {
            get
            {
                if (mSkin != null)
                    return mSkin;
                mSkin = (GUISkin)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/RSGUI.guiskin", typeof(GUISkin));
                if (mSkin == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/RSGUI.guiskin cant found!"));
                }
                return mSkin;
            }
        }

        public static readonly string[] RSTypeNames = new string[]
        {
            "Undefined Asset",
            "Local Asset",
            "Extra Local Asset",
            "External Asset"
        };

        public static readonly int[] RSTypeIndexs = new int[]{0,1,2,3,-1,-2};

        private static Texture merr_icon = null;

        public static Texture err_icon
        {
            get
            {
                if (merr_icon != null)
                    return merr_icon;
                merr_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/error.png", typeof(Texture));
                if (merr_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/error.png cant found!"));
                }
                return merr_icon;
            }
        }

        private static Texture msuccess_icon = null;

        public static Texture success_icon
        {
            get
            {
                if (msuccess_icon != null)
                    return msuccess_icon;
                msuccess_icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/TUT/ResouceSystem/" +
                    "Editor/Gizmos/success.png", typeof(Texture));
                if (msuccess_icon == null)
                {
                    Debug.LogError(TutNorm.LogErrFormat("RS Editor Const" ,"Assets/TUT/ResouceSystem/" +
                        "Editor/Gizmos/success.png cant found!"));
                }
                return msuccess_icon;
            }
        }

        private static List<int> mRSGroupIndexs = null;

        public static readonly int MaxGroupNum = 32;

        public static int[] RSGroupIndexs
        {
            get
            {
                if (mRSGroupIndexs == null)
                {
                    mRSGroupIndexs = new List<int>();
                    for (int i = 0; i<(MaxGroupNum + 1); i++)
                    {
                        mRSGroupIndexs.Add(i);
                    }
                }
                return mRSGroupIndexs.ToArray();
            }
        }

        private static List<string> mRSGroupNames = null;

        public static string[] RSGroupNames
        {
            get
            {
                if (mRSGroupNames == null)
                {
                    mRSGroupNames = new List<string>();
                    for (int i = 0; i<MaxGroupNum; i++)
                    {
                        mRSGroupNames.Add("AssetGroup" + (i).ToString("D2"));
                    }
                }
                return mRSGroupNames.ToArray();
            }
        }

        public static bool isResTypeFromPath(string path)
        {
            string t = path.Replace("\\", "/");
            if (t.EndsWith("Resources"))
                return true;
            return t.Contains("/Resources/");
        }
    }
}
