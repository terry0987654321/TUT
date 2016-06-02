using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using TUT;

namespace TUT.RSystem
{
    public class RSManifest
    {
        public List<RSFileInfo> files = null;

        private Dictionary<int,RSGroup> m_Groups = new Dictionary<int, RSGroup>();
        private Dictionary<string,RSFileInfo> mFileMap = new Dictionary<string,RSFileInfo>();
        private bool mInited = false;

        private void Init()
        {
            if(mInited)
                return;
            if(files != null)
            {
                for(int i = 0;i<files.Count;i++)
                {
                    PackFileInfo(files[i]);
                }
            }
            mInited = true;
        }

        public void ResetManifest()
        {
            m_Groups.Clear();
            mFileMap.Clear();
            mInited = false;
        }

        public void PackFileInfo(RSFileInfo info)
        {
            RSGroup group = null;
            if(m_Groups.TryGetValue(info.group,out group))
            {
                if(!group.Contains(info.path))
                {
                    group.PushFileInfo(info);
                }
            }
            else
            {
                group = new RSGroup();
                group.group = info.group;
                group.PushFileInfo(info);
                m_Groups.Add(info.group,group);
            }

            if(!mFileMap.ContainsKey(info.path))
            {
                mFileMap.Add(info.path,info);
            }
            else
            {
                mFileMap[info.path] = info;
            }
        }

        public bool ContainInGroup(RSFileInfo info)
        {
            Init();
            RSGroup group = null;
            if(!m_Groups.TryGetValue(info.group,out group))
            {
                return false;
            }
            return group.Contains(info.path);
        }

        public bool ContainInFileMap(string path)
        {
            Init();
            return mFileMap.ContainsKey(path);
        }

        public void CombineManifest(RSManifest manifest)
        {
            if(manifest == this)
                return;
            Init();
            if(manifest.files != null)
            {
                for(int i = 0;i<manifest.files.Count;i++)
                {
                    PackFileInfo(manifest.files[i]);
                }
            }
        }

        public RSFileInfo GetInfo(string path)
        {
            Init();
            RSFileInfo info = null;
            mFileMap.TryGetValue(path,out info);
            return info;
        }
    }

}