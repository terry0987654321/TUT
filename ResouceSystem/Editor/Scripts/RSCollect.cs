using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using TUT;

namespace TUT.RSystem
{
    public class RSCollectDependency
    {
        private string self_path = "";

        public RSColDep ExpColDepDatas(GameObject obj)
        {
            Component[] v_comps = ExpPrefabAllComp(obj);
            if (v_comps == null)
                return null;
            self_path = AssetDatabase.GetAssetPath(obj);
            RSColDep deps = new RSColDep();
            deps.Dep_Assets = new List<RSColDepData>();
            RSColDepData data = null;
            for (int i = 0; i<v_comps.Length; i++)
            {
                PropertyInfo[] infos = v_comps [i].GetType().GetProperties();//BindingFlags.Public|BindingFlags.Instance
                foreach (PropertyInfo info in infos)
                {
                    if (CheckDefineType(info))
                    {
                        data = MakeDepData(info.GetValue(v_comps [i], null), info.Name, true, v_comps [i]);
                        if (data != null && data.isVaild())
                        {
                            deps.Dep_Assets.Add(data);
                        }
                    }
                }
                FieldInfo[] finfos = v_comps [i].GetType().GetFields();
                foreach (FieldInfo info in finfos)
                {
                    if (CheckDefineType(info))
                    {
                        data = MakeDepData(info.GetValue(v_comps [i]), info.Name, false, v_comps [i]);
                        if (data != null && data.isVaild())
                        {
                            deps.Dep_Assets.Add(data);
                        }
                    }
                }
            }
            return deps;
        }
        
        private RSColDepData MakeDepData(object obj, string property, bool is_property, Component comp)
        {
            if (obj == null)
                return null;
            RSColDepData data = new RSColDepData(property, is_property, comp);
            string path = string.Empty;
            if (obj is Array)
            {
                int index = 0;
                foreach (object elem in (Array) obj)
                {
                    path = ObjToAssetPath(elem);
                    if(RSEdManifest.GetInfo(path) != null)
                    {
                        data.AddDepAssets(index,path, self_path);
                    }

                    index ++;
                }
                return data;
            }
            
            if (obj is IList)
            {
                int index = 0;
                foreach (object elem in (IList) obj)
                {
                    path = ObjToAssetPath(elem);
                    if(RSEdManifest.GetInfo(path) != null)
                    {
                        data.AddDepAssets(index,path, self_path);
                    }
                    
                    index ++;
                }
                return data;
            }
            
            if (obj is IDictionary)
            {
                foreach (DictionaryEntry entry in (IDictionary) obj)
                {

                    path = ObjToAssetPath(entry.Value);
                    if(RSEdManifest.GetInfo(path) != null)
                    {
                        string key = "";
                        if (entry.Key.GetType() != typeof(string))
                            key = entry.Key.ToString();
                        else
                            key = (string)entry.Key;
                        data.AddDepAssets(key,path, self_path);
                    }
                }
                return data;
            }

            path = ObjToAssetPath(obj);
            if (RSEdManifest.GetInfo(path) != null)
            {
                data.AddDepAssets(path, self_path);
            }
            return data;
        }
        
        private bool CheckDefineType(FieldInfo info)
        {
            System.Type type = info.FieldType;
            
            if (info.FieldType.IsArray)
            {
                type = info.FieldType.GetElementType();
            } else
                if (info.FieldType.IsGenericType)
            {
                type = info.FieldType.GetGenericArguments() [0];
            }
            {
                if (type == typeof(Material) || 
                    type == typeof(TextAsset) ||
                    type == typeof(Transform) ||
                    type == typeof(AudioClip) ||
                    type == typeof(Texture) ||
                    type == typeof(GameObject))
                {
                    if (info.Name == "materials" || info.Name == "material")
                        return false;
                    return true;
                }
            }


            if (type.BaseType == typeof(Behaviour))
            {
                return true;
            }
            return false;
        }
        
        private bool CheckDefineType(PropertyInfo info)
        {
            if (!info.CanRead || !info.CanWrite)
                return false;
            
            System.Type type = info.PropertyType;
            
            if (info.PropertyType.IsArray)
            {
                type = info.PropertyType.GetElementType();
            } else
                if (info.PropertyType.IsGenericType)
            {
                type = info.PropertyType.GetGenericArguments() [0];
            }
            {
                if (type == typeof(Material) || 
                    type == typeof(TextAsset) ||
                    type == typeof(Transform) ||
                    type == typeof(AudioClip) ||
                    type == typeof(Texture) ||
                    type == typeof(GameObject))
                {
                    if (info.Name == "materials" || info.Name == "material")
                        return false;
                    return true;
                }
            }
            if (type.BaseType == typeof(Behaviour) || type.BaseType == typeof(MonoBehaviour))
            {
                return true;
            }
            return false;
        }
        
        private string ObjToAssetPath(object obj)
        {
            UnityEngine.Object uni_obj = obj as UnityEngine.Object;
            if (uni_obj == null)
            {
                return string.Empty;
            }
            return AssetDatabase.GetAssetPath(uni_obj);
        }
        
        public Component[] ExpPrefabAllComp(GameObject obj)
        {
            if (obj == null)
                return null;
            
            List<Component> vaild_components = new List<Component>();
            Component[] comps = obj.GetComponents<Component>();
            foreach (Component comp in comps)
            {
                if (comp.GetType() != typeof(Transform))
                {
                    vaild_components.Add(comp);
                }
            }
            for (int i = 0; i<obj.transform.childCount; i++)
            {
                vaild_components.AddRange(ExpPrefabAllComp(obj.transform.GetChild(i).gameObject));
            }
            return vaild_components.ToArray();
        }
    }
}
