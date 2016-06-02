using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{
    /// <summary>
    /// 资源定义类型
    /// </summary>
    public enum RSType
    {
        /// <summary>
        /// 未定义资源类型
        /// </summary>
        RT_NIL,
        /// <summary>
        /// 定义放在Resources文件夹内的资源类型
        /// </summary>
        RT_RESOURCES,
        /// <summary>
        /// 定义放在StreamingAssets文件夹内的资源类型
        /// </summary>
        RT_STREAM,
        /// <summary>
        /// 定义放在app外部的资源类型
        /// </summary>
        RT_BUNDLE,
    }

    /// <summary>
    /// 资源基本信息数据结构
    /// </summary>
    public class RSInfo
    {
        /// <summary>
        /// 被定义资源的存放的路径（Assets/....）
        /// </summary>
        public string path;
        /// <summary>
        /// 资源定义类型的索引
        /// </summary>
        public int type;

        /// <summary>
        /// 获得资源定义类型
        /// </summary>
        /// <value>资源定义类型</value>
        public RSType rstype
        {
            get
            {
                return (RSType)type;
            }
        }

        /// <summary>
        /// 克隆一份当前资源信息
        /// </summary>
        public virtual RSInfo Clone()
        {
            RSInfo info = new RSInfo();
            info.path = path;
            info.group = group;
            info.type = type;
            return info;
        }

        /// <summary>
        /// 定义资源所在的资源组
        /// </summary>
        public int group = 0;

        public static bool operator ==(RSInfo info1, RSInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return true;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return false;

            if (info1.path == info2.path && 
                info1.type == info2.type && 
                info1.group == info2.group)
                return true;
            return false;
        }

        public static bool operator !=(RSInfo info1, RSInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return false;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return true;

            if (info1.path != info2.path || 
                info1.type != info2.type || 
                info1.group != info2.group)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            try
            {
                return (this == (RSInfo)obj);
            } catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool isResTypeFromPath(string path)
        {
            string t = path.Replace("\\", "/");
            if (t.EndsWith("/Resources"))
                return true;
            return t.Contains("/Resources/");
        }
    }

    /// <summary>
    /// 文件类资源基本信息
    /// </summary>
    public class RSFileInfo : RSInfo
    {
        /// <summary>
        /// 当该文件被进行打包后的大小
        /// </summary>
        public int size = -1;
        /// <summary>
        /// 针对文件内容的guid
        /// </summary>
        public string guid = string.Empty;
        /// <summary>
        /// 针对文件打包后内容的guid
        /// </summary>
        public string buid = string.Empty;
        /// <summary>
        /// 文件依赖的其他资源列表
        /// </summary>
        public string[] dependency_assets = null;
       
        /// <summary>
        /// 克隆一份当前资源信息
        /// </summary>
        public override RSInfo Clone()
        {
            RSFileInfo info = new RSFileInfo();
            info.path = path;
            info.group = group;
            info.type = type;
            info.size = size;
            info.guid = guid;
            info.buid = buid;
            return info;
        }

        public static string GetResourceLoadPath(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
                return file_path;
            string tmp = file_path;
            if (file_path.Contains("."))
                tmp = file_path.Substring(0, file_path.LastIndexOf("."));
			int index = tmp.IndexOf ("Resources/");
			if(index >= 0)
			{
				return tmp.Substring( index+ 10);
			}
			else
				return tmp;
           
        }

        public static bool operator ==(RSFileInfo info1, RSFileInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return true;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return false;
            if (info1.path == info2.path && 
                info1.type == info2.type && 
                info1.group == info2.group &&
                info1.size == info2.size &&
                info1.buid == info2.buid &&
                info1.guid == info2.guid)
                return true;
            return false;
        }

        public static bool operator !=(RSFileInfo info1, RSFileInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return false;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return true;
            if (info1.path != info2.path || 
                info1.type != info2.type || 
                info1.group != info2.group || 
                info1.size != info2.size || 
                info1.buid != info2.buid ||
                info1.guid != info2.guid)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            try
            {
                return (this == (RSFileInfo)obj);
            } catch
            {
                return false;
            }
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 资源组
    /// 用于以不同目的来划分资源 ，可分配按步进行资源处理
    /// </summary>
    public class RSGroup
    {
        /// <summary>
        /// 资源组的ID
        /// </summary>
        public int group;

        /// <summary>
        /// 资源组包含的文件类资源信息
        /// </summary>
        public List<RSFileInfo> assets = null;

        private Dictionary<string,int> mIndexMap =null;

        /// <summary>
        /// 判断指定路径的资源是否在资源组中
        /// </summary>
        /// <param name="path">Path.</param>
        public bool Contains(string path)
        {
            if(mIndexMap == null)
                return false;
            return mIndexMap.ContainsKey(path);
        }

        /// <summary>
        /// 向资源组中添加指定的文件类资源
        /// </summary>
        /// <param name="info">Info.</param>
        public void PushFileInfo(RSFileInfo info)
        {
            if(assets == null)
            {
                assets = new List<RSFileInfo>();
            }
            if( !assets.Contains(info))
            {
                assets.Add(info);
                if(mIndexMap == null)
                {
                    mIndexMap = new Dictionary<string,int>();
                }
                if(!mIndexMap.ContainsKey(info.path))
                {
                    mIndexMap.Add(info.path,assets.Count - 1);
                }
            }
        }
    }

}
