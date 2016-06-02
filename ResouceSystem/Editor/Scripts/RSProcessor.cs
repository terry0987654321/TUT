using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using TUT;

namespace TUT.RSystem
{
    public class  RSAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
		public static bool IgnorePostprocess = false;

        public static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
			if(IgnorePostprocess)
				return;
            SpecialProUpdate.Reset();
            SpecialProUpdate.LoadRecord();
            RemoveRS(deletedAssets);
            RemoveRS(movedFromAssetPaths);
            ImportRS(importedAssets);
            RemoveRS(movedAssets);
            ImportRS(movedAssets);
            SpecialProUpdate.Reset();
        }

        private static void RemoveRS(string[] assets)
        {
            foreach (string name in assets)
            {
                RSEdManifest.RemoveInfo(name);
            }
        }

        private static void ImportRS(string[] assets)
        {
            List<string> files = new List<string>();
            foreach (string path in assets)
            {
                if(TutFileUtil.IsFolder(path))
                {
                    ProImportRS(path);
                }
                else
                {
                    files.Add(path);
                }
            }
            foreach(string path in files)
            {
                ProImportRS(path);  
            }

            SpecialProUpdate.StartUpdate(files.ToArray());
        }

        private static void ProImportRS(string path)
        {
            RSFolderInfo info = RSEdManifest.GetInfo(TutFileUtil.GetPathParent(path)) as RSFolderInfo;
            if (info != null && info.processors != null && info.processors.Count != 0)
            {
                foreach (RSProcessorInfo pinfo in info.processors)
                {
                    if ((RSProcessorType)pinfo.processor_type == RSProcessorType.PT_AFTER_IMPORT)
                    {
                        RSProlib.Execute(RSProcessorType.PT_AFTER_IMPORT, pinfo.processor_func, new object[]{path,info});
                    }
                }
            }
        }

        public static Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
			if(IgnorePostprocess)
				return material;
            return material;
        }

        public static void OnPostprocessAudio(AudioClip clip)
        {
			if(IgnorePostprocess)
				return;
        }

        public static void OnPostprocessModel(GameObject obj)
        {
			if(IgnorePostprocess)
				return;
        }

        public static void OnPostprocessTexture(Texture2D texture)
        {
			if(IgnorePostprocess)
				return;
        }

//        public static void OnPreprocessAudio()
//        {
//
//        }
//
//        public static void OnPreprocessModel()
//        {
//
//        }
//
//        public static void OnPreprocessTexture()
//        {
//
//        }
    }

    [InitializeOnLoad]
    public class SpecialProUpdate
    {
        public class UpdateRecord
        {
            public List<string> NeedUpdates = new List<string>();
            public double LastTime = 0;
        }
        private static UpdateRecord  mRecord = new UpdateRecord();

        public static string LoadPath
        {
            get
            {
                return RSEdConst.s_RSEdTmpPath+"_tmp_special.tmp";
            }
        }

        static SpecialProUpdate()
        {
            RegisterUpdate();
        }

        static void UpdateSpecialPros()
        {
            if (mRecord.NeedUpdates.Count == 0)
                return;
            if (!EditorApplication.isCompiling)
            {
                ExeSpecialPro();
            }
        }
        public static void StartUpdate(string[] files)
        {
            foreach(string path in files)
            {
                if(path.EndsWith(".cs") || path.EndsWith(".js"))
                {
                    mRecord.NeedUpdates.Add(path);
                }
            }
            if (mRecord.NeedUpdates.Count != 0)
            {
                if (!EditorApplication.isCompiling)
                {
                    ExeSpecialPro();
                } else
                {
                    mRecord.LastTime = RSProlib.LastModifyTime;
                    TutFileUtil.WriteJsonFile(mRecord,LoadPath);                  
                }
            }
        }

        public static void LoadRecord()
        {
            if (TutFileUtil.FileExist(LoadPath))
            {
                mRecord.NeedUpdates.Clear();
                mRecord = TutFileUtil.ReadJsonFile<UpdateRecord>(LoadPath);
            }
        }

        private static void RegisterUpdate()
        {
            if (TutFileUtil.FileExist(LoadPath))
            {
                mRecord.NeedUpdates.Clear();
                mRecord = TutFileUtil.ReadJsonFile<UpdateRecord>(LoadPath);
                if (mRecord == null)
                {
                    mRecord = new UpdateRecord();
                }
                if (mRecord.NeedUpdates.Count == 0 )
                    return;
                if(mRecord.LastTime != RSProlib.LastModifyTime)
                    TutFileUtil.DeleteFile(LoadPath);
                else
                    return;
                bool need_register = true;
                if(EditorApplication.update != null)
                {
                    foreach (System.Delegate i in EditorApplication.update.GetInvocationList())
                    {
                        if (i.Method.Name == "UpdateSpecialPros")
                        {
                            need_register = false;
                            break;
                        }
                    }
                }
                if (need_register)
                {
                    EditorApplication.update += UpdateSpecialPros;
                }
            }
        }

        private static void ExeSpecialPro()
        {
            for(int i = 0;i<mRecord.NeedUpdates.Count;i++)
            {
                ProSpecialImport(mRecord.NeedUpdates[i]);
            }
            Reset();
            EditorApplication.update -= UpdateSpecialPros;
        }

        public static void Reset()
        {
            mRecord.NeedUpdates.Clear();
            mRecord.LastTime = 0;
        }

        private static void ProSpecialImport(string path)
        {
            string type_name = TutFileUtil.GetFileName(path);
            RSProlib.ExecuteSpecialPro(type_name);
        }
    }

//    public class RSModificationProcessor : UnityEditor.AssetModificationProcessor
//    {
//        public static string OnWillCreateAsset(string path)
//        {
//            string new_path = path;
////            if(!path.Contains(".meta"))
////            {
////               
////                int las = new_path.LastIndexOf("/");
////                string file_name = new_path.Substring(las+1,new_path.LastIndexOf(".")-las);
////                string new_name = file_name.ToLower();
////                new_path = new_path.Replace(file_name,new_name);
////               
////            }
////            Debug.Log(" OnWillCreateAsset "+new_path);
//            return new_path;
//        }
//
//        public static AssetDeleteResult OnWillDeleteAsset(int path, RemoveAssetOptions option)
//        {
//            Debug.Log(" OnWillDeleteAsset " + path + "   RemoveAssetOptions " + option.ToString());
//            return AssetDeleteResult.DidNotDelete;
//        }
//
//        public static AssetMoveResult OnWillMoveAsset(string source_path, string target_path)
//        {
//            Debug.Log(" OnWillMoveAsset " + source_path + "  to " + target_path);
//            return AssetMoveResult.DidNotMove;
//        }
//
//        public static void OnWillSaveAssets(string[] paths)
//        {
//        }
//    }

    public enum RSProcessorType
    {
        PT_AFTER_IMPORT,
        PT_ASSIGN_MAT_MOD,
        PT_AFTER_PRO_AUDIO,
        PT_AFTER_PRO_MOD,
        PT_AFTER_PRO_TEX,
    }


    public class RSProcessorInfo
    {

        public int priority;

        public int processor_type;

        public string processor_func;
        [System.NonSerialized]
        public int editor_index = 0;

        public RSProcessorType ProType
        {
            get
            {
                return (RSProcessorType)processor_type;
            }
        }

        public RSProcessorInfo Clone()
        {
            RSProcessorInfo info = new RSProcessorInfo();
            info.priority = priority;
            info.editor_index = editor_index;
            info.processor_func = processor_func;
            info.processor_type = processor_type;
            return info;
        }

        public static bool operator ==(RSProcessorInfo info1, RSProcessorInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return true;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return false;
            if (info1.priority == info2.priority &&
                info1.processor_type == info2.processor_type &&
                info1.processor_func == info2.processor_func)
                return true;
            return false;
        }

        public static bool operator !=(RSProcessorInfo info1, RSProcessorInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return false;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return true;
            if (info1.priority != info2.priority ||
                info1.processor_type != info2.processor_type ||
                info1.processor_func != info2.processor_func)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            try
            {
                return (this == (RSProcessorInfo)obj);
            } catch
            {
                return false;
            }
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ResProcess : Attribute
    {
        public RSProcessorType ProType;
        public string ProName;
        public int Priority;

        public ResProcess(RSProcessorType type, string pro_name)
        {
            this.ProType = type;
            this.ProName = pro_name;
            Priority = 0;
        }

        public ResProcess(RSProcessorType type, string pro_name, int priority)
        {
            this.ProType = type;
            this.ProName = pro_name;
            Priority = priority;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SpecialProcess : Attribute
    {
        public System.Type SpecifiedType;
       
        public SpecialProcess(System.Type type)
        {
            SpecifiedType = type;
        }
    }



    public class RSProlib
    {
        private static Dictionary<string,MethodInfo> mSpecialProMethods = null;
        private static List<string> mSpecialNamespaces = null;

        private static Dictionary<RSProcessorType,Dictionary<string,MethodInfo>> mProMethods = null;
        private static List<ResProcess> mResProcess = null;

        private static string[] mProNames = null;

        public static string[] AllProNames
        {
            get
            {
                if (mProNames == null)
                {
                    FullLib();
                }
                return mProNames;
            }
        }

        private static int mProInvaildIndex = -1;

        public static int ProInvaild
        {
            get
            {
                if (mProInvaildIndex < 0)
                {
                    FullLib();
                }
                return mProInvaildIndex;
            }
        }

        private static int[] mProIndexs = null;

        public static int[] AllProIndexs
        {
            get
            {
                if (mProIndexs == null)
                {
                    FullLib();
                }
                return mProIndexs;
            }
        }

        public static int GetProIndex(string pro_name)
        {
            if(mProNames == null)
            {
                FullLib();
            }
            for(int i = 0;i<mProNames.Length;i++)
            {
                if(pro_name == mProNames[i])
                    return i;
            }
            return -1;
        }

        private static int[] mProTypeIndexs = null;

        public static int[] AllProTypeIndexs
        {
            get
            {
                if (mProTypeIndexs == null)
                {
                    FullLib();
                }
                return mProTypeIndexs;
            }
        }

        private static Assembly mRuntimeAssembly = null;

        public static Assembly RuntimeAssembly
        {
            get
            {
                if(mRuntimeAssembly == null)
                {
                    string epath = Application.dataPath;
                    epath = epath.Replace("Assets", "Library/ScriptAssemblies/Assembly-CSharp.dll");
                    if (! File.Exists(epath))
                        return null;
                    mRuntimeAssembly = Assembly.LoadFile(epath);
                }
                return mRuntimeAssembly;
            }
        }

        public static double LastModifyTime
        {
            get
            {
                if(RuntimeAssembly == null)
                    return 0;
                string epath = Application.dataPath;
                epath = epath.Replace("Assets", "Library/ScriptAssemblies");
                return TutFileUtil.GetFileLastTime(epath);
            }
        }

        private static void FullLib()
        {
            if (mProMethods != null)
            {
                return;
            }

            string epath = Application.dataPath;
            epath = epath.Replace("Assets", "Library/ScriptAssemblies/Assembly-CSharp-Editor.dll");
            if (! File.Exists(epath))
                return;

            mProMethods = new Dictionary<RSProcessorType, Dictionary<string, MethodInfo>>();
            mSpecialProMethods = new Dictionary<string, MethodInfo>();
            mSpecialNamespaces = new List<string>();
            mSpecialNamespaces.Add(string.Empty);
            Assembly assembly = Assembly.LoadFile(epath);
            ResProcess rp = null;
            SpecialProcess sp = null;
            string specialProName = string.Empty;
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo mInfo in type.GetMethods(BindingFlags.Public|BindingFlags.Static))
                {
                    foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                    {
                        if (attr.GetType() == typeof(ResProcess))
                        {
                            rp = attr as ResProcess;
                            if (mProMethods.ContainsKey(rp.ProType))
                            {
                                if (mProMethods [rp.ProType].ContainsKey(rp.ProName))
                                {
                                    Debug.LogError(TutNorm.LogErrFormat("Resource Process" ,type.ToString() + " => resource process: " + rp.ProName + "  Has been defined" +
                                        " in  " + mProMethods [rp.ProType] [rp.ProName].GetType().ToString() + "    !!!!!"));
                                    continue;
                                } else
                                {
                                    mProMethods [rp.ProType].Add(rp.ProName, mInfo);
                                    if (mResProcess == null)
                                        mResProcess = new List<ResProcess>();
                                    mResProcess.Add(rp);
                                }
                            } else
                            {
                                Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
                                methods.Add(rp.ProName, mInfo);
                                mProMethods.Add(rp.ProType, methods);
                                if (mResProcess == null)
                                    mResProcess = new List<ResProcess>();
                                mResProcess.Add(rp);
                            }
                        }
                        else
                         if (attr.GetType() == typeof(SpecialProcess))
                        {
                            sp = attr as SpecialProcess;
                            if(sp.SpecifiedType == null)
                                continue;
                            specialProName = sp.SpecifiedType.Namespace+"."+sp.SpecifiedType.Name;
                            if(mSpecialProMethods.ContainsKey(specialProName))
                            {
                                Debug.LogError(TutNorm.LogErrFormat("Special Process" ,type.ToString() + " => special process: " + sp.SpecifiedType.FullName + "  Has been defined     !!!!!"));
                                continue;
                            }
                            else
                            {
                                mSpecialProMethods.Add(specialProName,mInfo);
                                mSpecialNamespaces.Add(sp.SpecifiedType.Namespace);
                            }
                        }
                    }
                }
            }

            if (mResProcess != null)
            {
                List<string> names = new List<string>();
                List<int> indexs = new List<int>();
                List<int> types = new List<int>();
                for (int i = 0; i<mResProcess.Count; i++)
                {
                    names.Add(mResProcess [i].ProName);
                    indexs.Add(i);
                    types.Add((int)mResProcess [i].ProType);
                }
                mProInvaildIndex = indexs.Count;
                indexs.Add(indexs.Count);
                mProNames = names.ToArray();
                mProIndexs = indexs.ToArray();
                mProTypeIndexs = types.ToArray();
            } else
            {
                mProInvaildIndex = 0;
                mProNames = new string[]{};
                mProIndexs = new int[]{};
                mProTypeIndexs = new int[]{};
            }
        }



        public static object ExecuteSpecialPro(string type_name)
        {
            FullLib();

            if (mSpecialProMethods == null || mSpecialProMethods.Count == 0 || RuntimeAssembly == null)
                return null;


            string full_name = string.Empty;
            System.Type type = null;
            for (int i = 0; i<mSpecialNamespaces.Count; i++)
            {
                full_name = string.IsNullOrEmpty(mSpecialNamespaces[i])? type_name:mSpecialNamespaces[i]+"."+type_name;

                type = RuntimeAssembly.GetType(full_name);

                if(type != null)
                {
                    if(mSpecialProMethods.ContainsKey(full_name))
                    {
                        return mSpecialProMethods [full_name].Invoke(null, new object[]{type});
                    }
                    else
                    {

                        if( type.BaseType != null)
                        {
                            full_name = type.BaseType.Namespace+"."+type.BaseType.Name;
                            if(mSpecialProMethods.ContainsKey(full_name))
                            {
                                return mSpecialProMethods [full_name].Invoke(null, new object[]{type});
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static object Execute(RSProcessorType type, string method_name, object[] param)
        {
            FullLib();
            if (mProMethods == null)
                return null;

            if (!mProMethods.ContainsKey(type))
            {
                Debug.LogError(TutNorm.LogErrFormat("Resource Process" ,type.ToString() + " undefined !!!!!! "));
                return null;
            }

            if (!mProMethods [type].ContainsKey(method_name))
            {
                Debug.LogError(TutNorm.LogErrFormat("Resource Process" ,type.ToString() + " process : " + method_name + " undefined !!!!!! "));
                return null;
            }
            return mProMethods [type] [method_name].Invoke(null, param);
        }
    }
}
