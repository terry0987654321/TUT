using UnityEngine;
using System.Collections;

namespace TUT
{

    /// <summary>
    ///  配置基类
    ///  主要针对系统模块的常量配置 继承该类型 会自动将其属性序列化为一个数据文件 并且存储在
    ///     Assets/_oohhoo_data/_configs/Resources/_datas/ 目录下 的 类型名文件
    /// 
    ///     该类型数据不支持动态更新
    /// 
    ///  注意  使用此类型 必须 保证以下几点
    ///     第一： 继承的类型必须包含在TUT 命名空间下
    ///     第二： 仅支持单性的继承关系 也就是 A：TutCfgBase<A> =〉 B：A =〉 B 不支持该功能
    ///     第三： 类名必须和文件名同名
    ///     第四： 该类文件不能在Editor目录下
    /// </summary>
    public abstract class TutCfgBase<T> : TutSingleton<T> where T : TutCfgBase<T>
    {

        public static string GetCfgPath()
        {
            return "Assets/_oohhoo_data/_configs/Resources/_datas/";
        }

        public static string GetCfgFilePath(System.Type type)
        {
            return GetCfgPath() + type.Name+".bytes";
        }

        public static string GetCfgLoadPath(System.Type type)
        {
            return "_datas/" + type.Name;
        }

        protected override void Initialize()
        {
            TextAsset text  = Resources.Load(GetCfgLoadPath(typeof(T))) as TextAsset;
            if (text == null)
            {
                Debug.LogError(TutNorm.LogErrFormat("Cfg Initialize"," Cant load cfg file in the "+GetCfgLoadPath(typeof(T))));
                return;
            }
            System.Reflection.MethodInfo read_json_method = typeof(TutFileUtil).GetMethod("ReadJsonString", 
                                                                                         System.Reflection.BindingFlags.Public |  System.Reflection.BindingFlags.Static);
            read_json_method = read_json_method.MakeGenericMethod(typeof(T));
            m_Instance = read_json_method.Invoke(null,new object[]{text.text}) as T;
            if(m_Instance == null)
                m_Instance = System.Activator.CreateInstance<T>();

        }
    }
}
