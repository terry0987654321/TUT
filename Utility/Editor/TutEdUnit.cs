using UnityEngine;
using System.Collections;
using UnityEditor;
using TUT.RSystem;

namespace TUT
{
    public class TutEdUnit
    {
            [SpecialProcess(typeof(TutCfgBase<>))]
            public static void CfgDispose(System.Type type)
            {
                System.Type generic_type = typeof(TutCfgBase<>).MakeGenericType(type);
                System.Reflection.MethodInfo get_path = generic_type.GetMethod("GetCfgFilePath", 
                                                                                              System.Reflection.BindingFlags.Public |  System.Reflection.BindingFlags.Static);
                object path = get_path.Invoke(null, new object[]{type});
        
                System.Reflection.MethodInfo read_json_method = typeof(TutFileUtil).GetMethod("ReadJsonFile", 
                                                                                          System.Reflection.BindingFlags.Public |  System.Reflection.BindingFlags.Static);
        
                System.Reflection.MethodInfo wirte_json_method = typeof(TutFileUtil).GetMethod("WriteJsonFile", 
                                                                                                     System.Reflection.BindingFlags.Public |  System.Reflection.BindingFlags.Static);
        
                read_json_method = read_json_method.MakeGenericMethod(type);
                object cfg = read_json_method.Invoke(null,new object[]{path});
        
                if (TutFileUtil.FileExist((string)path))
                {
                    TutFileUtil.DeleteFile((string)path);
                }
        
                if (cfg == null)
                {
                    cfg = System.Activator.CreateInstance(type);
                }
                wirte_json_method.Invoke(null,new object[]{cfg,path,true});
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
    }
}
