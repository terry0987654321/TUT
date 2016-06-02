using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;
using TUT;

public class TutEdProfiler
{
    static MethodInfo m_getStorageMemorySize;

    public class PerfabMemroyDetail
    {
        public int sub = 0;
        public int size;
        public string path;
        public Dictionary<string, int> detail;
        public List<PerfabMemroyDetail> sub_detail;

        public PerfabMemroyDetail(string _path)
        {
            path = _path;
            size = 0;
            detail = new Dictionary<string, int>();
            sub_detail = new List<PerfabMemroyDetail>();
        }

        public override string ToString()
        {
            string space = "";
            for (int i = 0; i < sub; i++)
            {
                space += "\t";
            }
            string result = space + "Perfab Usage Memory:  " + path + " [" + EditorUtility.FormatBytes(size) + " ] \n";

            if (detail.Count != 0)
            {
                result += space + "===========Asset===========\n";


                foreach (string name in detail.Keys)
                {
                    string tag = "";
                    if (detail[name] > 1024)
                    {
                        tag = " \t [ K ] >>";
                    }
                    if (detail[name] > 1048576)
                    {
                        tag = " \t [ M ] >>>>";
                    }
                    result += space + " > " + tag + name + " [ " + EditorUtility.FormatBytes(detail[name]) + " ] \n";
                }
            }
            if (sub_detail.Count != 0)
            {
                result += space + "===========Sub Perfabs ===========\n";
                foreach (PerfabMemroyDetail s_detail in sub_detail)
                {
                    s_detail.sub = sub + 1;
                    result += s_detail.ToString();
                    result += space + "-------------------------------------------------------\n ";
                }
            }
            return result;
        }

    }

    //    [MenuItem("Assets/Get Usage Memory")]
    //    static void GetUsageMemory()
    //    {
    //        if(Selection.activeObject != null)
    //        {
    //            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //			PerfabMemroyDetail detail = GetPerfabSize(path);
    //
    //			Debug.Log(detail.ToString());
    //        }
    //    }

    static public PerfabMemroyDetail GetPerfabSize(string path)
    {
        List<string> caled = new List<string>();
        return GetPerfabSize(path, ref caled);
    }
//    public static int cont = 0;
//    public static List<string> m_List = new List<string>();
    static PerfabMemroyDetail GetPerfabSize(string path, ref List<string> caled)
    {
        PerfabMemroyDetail detail = new PerfabMemroyDetail(path);
        //caled.Add(path);
        string[] dep_paths = AssetDatabase.GetDependencies(new string[] { path });
        int total = 0;
        int size = 0;
        PerfabMemroyDetail s_detail = null;
        foreach (string dep in dep_paths)
        {
            if (dep == path || caled.Contains(dep))
                continue;

            if (dep.Contains(".cs"))
                continue;

            if (dep.Contains(".prefab"))
            {
                s_detail = GetPerfabSize(dep, ref caled);
                detail.sub_detail.Add(s_detail);
                Debug.Log("obj:" + dep + " size="+ s_detail.size);
                total += s_detail.size;
            }
            else
                if (dep.Contains(".fbx"))
                {
                    if (dep.Contains("@"))
                    {
                        GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dep, typeof(GameObject));
                        Animation[] anims = obj.GetComponents<Animation>();
                        if (anims != null)
                        {
                            foreach (Animation anim in anims)
                            {
                                size = Profiler.GetRuntimeMemorySize(anim);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". animation  " + anim.name + ")", size);
                            }
                        }
                        anims = obj.GetComponentsInChildren<Animation>();
                        if (anims != null)
                        {
                            foreach (Animation anim in anims)
                            {
                                size = Profiler.GetRuntimeMemorySize(anim);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". animation  " + anim.name + ")", size);
                            }
                        }
                    }
                    else
                    {
                        GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dep, typeof(GameObject));
                        MeshFilter[] meshs = obj.GetComponents<MeshFilter>();
                        if (meshs != null)
                        {
                            foreach (MeshFilter filter in meshs)
                            {
                                size = Profiler.GetRuntimeMemorySize(filter.sharedMesh);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". mesh  " + filter.name + ")", size);
                            }
                        }

                        meshs = obj.GetComponentsInChildren<MeshFilter>(true);
                        if (meshs != null)
                        {
                            foreach (MeshFilter filter in meshs)
                            {
                                size = Profiler.GetRuntimeMemorySize(filter.sharedMesh);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". mesh  " + filter.name + ")", size);
                            }
                        }

                        SkinnedMeshRenderer[] skin_meshs = obj.GetComponents<SkinnedMeshRenderer>();
                        if (skin_meshs != null)
                        {
                            foreach (SkinnedMeshRenderer skin in skin_meshs)
                            {
                                size = Profiler.GetRuntimeMemorySize(skin.sharedMesh);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". skin_mesh  " + skin.name + ")", size);
                            }
                        }

                        skin_meshs = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        if (skin_meshs != null)
                        {
                            foreach (SkinnedMeshRenderer skin in skin_meshs)
                            {
                                size = Profiler.GetRuntimeMemorySize(skin.sharedMesh);
                                total += size;
                                detail.detail.Add(dep + " (" + detail.detail.Count.ToString() + ". skin_mesh  " + skin.name + ")", size);
                            }
                        }
                    }
                }
                else
                {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(dep, typeof(UnityEngine.Object));

                    Shader shader = obj as Shader;
                    if (shader != null)
                    {
                        continue;
                    }

                    Material mat = (obj as Material);
                    if (mat != null)
                    {
                        caled.Add(dep);
                    }

                    Texture tex = (obj as Texture);
                    if (tex != null)
                    {
                        caled.Add(dep);
                    }

                if ( obj is Texture || obj is Texture2D ){
                    size = GetStorageSize( obj );
                }else{
                    size = Profiler.GetRuntimeMemorySize( obj);
                }
                //Debug.Log("obj:" + obj.name + " size="+ size);

                    total += size;
                    detail.detail.Add(dep, size);
                }
        }
        detail.size = total;

//        foreach (var item in detail.detail)
//        {
//            m_List.Add((cont++) + "Sbu :::" + item.Key + ":::" + TutProfiler.toMemoryString(item.Value));
//        }

        //m_List.Add((cont++) + "Total :::" + path + ":::" + TutProfiler.toMemoryString(detail.size));
        return detail;
    }

    static MethodInfo GetStorageMemorySize {
        get {
            if (m_getStorageMemorySize == null) {
                Type type = Types.GetType ("UnityEditor.TextureUtil", "UnityEditor.dll");
                m_getStorageMemorySize = type.GetMethod ("GetStorageMemorySize", BindingFlags.Public | BindingFlags.Static, null, new Type[]{typeof(Texture)}, null);
            }
            return m_getStorageMemorySize;
        }
    }

    static int GetStorageSize (UnityEngine.Object @object)
    {
        return (int) GetStorageMemorySize.Invoke(null, new object[]{@object});
    }
}
