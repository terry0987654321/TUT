using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using TUT;
namespace TUT.RSystem
{
    [Serializable]
    public class RSColDep
    {
        public List<RSColDepData> Dep_Assets;
        public List<string> Req_Assets;

        public string[] ReqAssets
        {
            get
            {
                if(Req_Assets != null && Req_Assets.Count != 0)
                    return Req_Assets.ToArray();
                return null;
            }
        }

        private Dictionary<string,UnityEngine.Object> Req_Objs = null;
        
        public override string ToString()
        {
            string result = "";
            if (Dep_Assets == null)
                return result;
            result = "Dep comps : " + Dep_Assets.Count.ToString() + " Need Assets: " + Req_Assets.Count.ToString() + "\n";
            foreach (RSColDepData data in Dep_Assets)
            {
                result += data.ToString() + "\n------------------\n";
            }
            if (Req_Assets != null)
            {
                foreach (string path in Req_Assets)
                {
                    result += path + "\n";
                }
            }
            return result;
        }

        public void Dismantle()
        {
            if (Dep_Assets == null || Dep_Assets.Count == 0)
                return;
            foreach (RSColDepData data in Dep_Assets)
            {
                data.Dismantle();
                if (data.req_datas != null && data.req_datas.Count != 0)
                {
                    if (Req_Assets == null)
                        Req_Assets = new List<string>();
                    for (int i = 0; i<data.req_datas.Count; i++)
                    {
                        if (!Req_Assets.Contains(data.req_datas [i].req_path))
                        {
                            Req_Assets.Add(data.req_datas [i].req_path);
                        }
                    }
                }
            }
        }

        public IEnumerator Assemble()
        {
            if (Dep_Assets == null || Dep_Assets.Count == 0)
                yield break;
            if (Req_Assets == null || Req_Assets.Count == 0)
                yield break;
            Req_Objs = new Dictionary<string, UnityEngine.Object>();
            TUT.OhResResult result = null;
            result = TUT.TutResources.Instance.Load(Req_Assets.ToArray());
            yield return result.Waiting;

            foreach (TUT.TutResResultData data in result)
            {
                Req_Objs.Add(data.path,data.result);
            }

            foreach (RSColDepData data in Dep_Assets)
            {
                try 
                {
                data.InitAssemble(Req_Objs);
                data.Assemble();
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
    }

    [Serializable]
    public class RSReqData
    {
        public string req_path;
        public int index;

        public RSReqData()
        {

        }

        public RSReqData(string path, int i)
        {
            req_path = path;
            index = i;
        }
    }

    [Serializable]
    public class RSColDepData
    {
        public Component component = null;
        public List<string> dep_assets = null;
        public List<string> keys = null;
        public List<int> indexs = null;
        public List<RSReqData> req_datas = null;
        public string attribute = "";
        public bool is_property = false;
        private List<object> assemble_objs = null;

        public RSColDepData()
        {
            
        }
        
        public RSColDepData(string attribute_name, bool property, Component comp)
        {
            attribute = attribute_name;
            is_property = property;
            component = comp;
        }
        
        public bool isVaild()
        {
            if (dep_assets != null && dep_assets.Count > 0)
                return true;
            return false;
        }

        public bool InitAssemble(Dictionary<string, UnityEngine.Object> asset_map)
        {
            if (asset_map == null)
                return false;
            if (req_datas == null || req_datas.Count == 0)
                return false;
            UnityEngine.Object asset = null;
            {
                assemble_objs = new List<object>();
                for (int i = 0; i<req_datas.Count; i++)
                {
                    asset_map.TryGetValue(req_datas [i].req_path, out asset);
                    if (asset == null)
                    {
                        Debug.LogWarning(TutNorm.LogWarFormat( "Asset Reftex","[ Assemble Failed ]: miss " + req_datas [i].req_path));
                    }
                    assemble_objs.Add(asset);
                }
            }
            return true;
        }
        
        public void AddDepAssets(string path, string parent)
        {
            if (string.IsNullOrEmpty(path) || path == parent)
                return;
            if (dep_assets == null)
            {
                dep_assets = new List<string>();
            }
            dep_assets.Add(path);
        }
        
        public void AddDepAssets(string key, string path, string parent)
        {
            if (string.IsNullOrEmpty(path) || path == parent)
                return;
            if (keys == null)
            {
                keys = new List<string>();
            }
            keys.Add(key);
            
            if (dep_assets == null)
            {
                dep_assets = new List<string>();
            }
            dep_assets.Add(path);
        }

        public void AddDepAssets(int index, string path, string parent)
        {
            if (string.IsNullOrEmpty(path) || path == parent)
                return;
            if (indexs == null)
            {
                indexs = new List<int>();
            }
            indexs.Add(index);
            
            if (dep_assets == null)
            {
                dep_assets = new List<string>();
            }
            dep_assets.Add(path);
        }

        public delegate bool NeedDismantleCallback(string path);

        public void Dismantle()
        {
            if (is_property)
            {
                DismantleInfo(component.GetType().GetProperty(attribute), component);
            } else
            {
                DismantleInfo(component.GetType().GetField(attribute), component);
            }
        }

        Array DismantleArrayObj(Array array)
        {
            int index = -1;
            for (int i = 0; i<indexs.Count; i++)
            {
                index = indexs[i];
                if(index >=0 && index < array.Length)
                {
                    array.SetValue( null,index);
                    if (req_datas == null)
                        req_datas = new List<RSReqData>();
                    req_datas.Add(new RSReqData(dep_assets [i], i));
                }
            }
            return array;
        }

        IList DismantleListObj(IList list)
        {

            int index = -1;
            for (int i = 0; i<indexs.Count; i++)
            {
                index = indexs[i];
                if(index >=0 && index < list.Count)
                {
                    list [index] = null;
                    if (req_datas == null)
                        req_datas = new List<RSReqData>();
                    req_datas.Add(new RSReqData(dep_assets [i], i));
                }
            }
            return list;
        }

        object DismantleObj(object obj)
        {

            obj = null;
            if (req_datas == null)
                req_datas = new List<RSReqData>();
            req_datas.Add(new RSReqData(dep_assets [0], 0));
            return obj;
        }

        void DismantleInfo(PropertyInfo info, Component comp)
        {
            if (info == null)
                return;
            object obj = info.GetValue(comp, null);
            {
                if (info.PropertyType.IsArray)
                {
                    Array array = (Array) obj;
                    {
                        info.SetValue(comp, DismantleArrayObj(array), null);
                    }
                } else
                if (info.PropertyType.IsGenericType)
                {
                    IList list = (IList) obj;
                    {
                        info.SetValue(comp, DismantleListObj(list), null);
                    }
                } else
                {
                    info.SetValue(comp, DismantleObj(obj), null);
                }
            }
        }

        void DismantleInfo(FieldInfo info, Component comp)
        {
            if (info == null)
                return;
            object obj = info.GetValue(comp);
            {
                if (info.FieldType.IsArray)
                {
                    Array array = (Array) obj;
                    {
                        info.SetValue(comp, DismantleArrayObj(array));
                    }
                } else
                if (info.FieldType.IsGenericType)
                {
                    IList list = (IList) obj;
                    {
                        info.SetValue(comp, DismantleListObj(list));
                    }
                } else
                {
                    info.SetValue(comp, DismantleObj(obj));
                }
            }
        }

        public void Assemble()
        {
            if (is_property)
            {
                AssembleInfo(component.GetType().GetProperty(attribute), component);
            } else
            {
                AssembleInfo(component.GetType().GetField(attribute), component);
            }
        }

        Array AssembleArrayObj(Array array, System.Type type)
        {
            if (req_datas == null || req_datas.Count == 0)
                return array;

            int index = -1;
            for (int i = 0; i<indexs.Count; i++)
            {
                index = indexs[i];
                if(index >=0 && index < array.Length)
                {
                    array.SetValue(AssetToObj(type, assemble_objs [i]),index);
                }
            }
            return array;
        }

        IList AssembleListObj(IList list, System.Type type)
        {
            if (req_datas == null || req_datas.Count == 0)
                return list;
            int index = -1;
            for (int i = 0; i<indexs.Count; i++)
            {
                index = indexs [i];
                if (index >= 0 && index < list.Count)
                {
                    list [index] = AssetToObj(type, assemble_objs [i]);
                }
            }
            return list;
        }

        object AssembleObj(object obj, System.Type type)
        {
            if (req_datas == null || req_datas.Count == 0)
                return obj;
            obj = AssetToObj(type, assemble_objs [0]);
            return obj;
        }

        private void AssembleInfo(PropertyInfo info, Component comp)
        {
            if (info == null)
                return;
            object obj = info.GetValue(comp, null);
            {
                if (info.PropertyType.IsArray)
                {
                    Array array = (Array) obj;
                    {
                        info.SetValue(comp, AssembleArrayObj(array, info.PropertyType), null);
                    }
                } else
                    if (info.PropertyType.IsGenericType)
                {
                    IList list = (IList) obj;
                    {
                        info.SetValue(comp, AssembleListObj(list, info.PropertyType), null);
                    }
                } else
                {
                    info.SetValue(comp, AssembleObj(obj, info.PropertyType), null);
                }
            }
        }

        private void AssembleInfo(FieldInfo info, Component comp)
        {
            if (info == null)
                return;
            object obj = info.GetValue(comp);
            {
                if (info.FieldType.IsArray)
                {
                    Array array = (Array) obj;
                    {
                        info.SetValue(comp, AssembleArrayObj(array, info.FieldType));
                    }
                } else
                    if (info.FieldType.IsGenericType)
                {
                    IList list = (IList) obj;
                    {
                        info.SetValue(comp, AssembleListObj(list, info.FieldType));
                    }
                } else
                {
                    info.SetValue(comp, AssembleObj(obj, info.FieldType));
                }
            }
        }

        private object AssetToObj(System.Type type, object asset)
        {
            if (type.BaseType == typeof(UnityEngine.MonoBehaviour))
            {
                GameObject gobj = asset as GameObject;
                if (gobj != null)
                {
                    return gobj.GetComponent(type);
                }
            }
            return asset;
        }

        public override string ToString()
        {
            string paths = "";
            foreach (string path in dep_assets)
            {
                paths += path + ",";
            }
            return string.Format("comp: {0} ({1}) \n " +
                "asset: {2} \n" +
                "attribute: {3}\n", component.name, component.GetType().ToString(), paths, attribute);
        }
    }

    public class RSAssetReflex : MonoBehaviour
    {
        public RSColDep ColDep = null;
    }
}
