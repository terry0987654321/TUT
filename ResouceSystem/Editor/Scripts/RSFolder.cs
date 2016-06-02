/// <summary>
///  Copyright (c) 2014 TUT Co., Ltd.
/// </summary>
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Rotorz.ReorderableList;
using TUT;

namespace TUT.RSystem
{
    /// <summary>
    /// RS folder info.
    /// </summary>
    public class RSFolderInfo : RSInfo
    {
        /// <summary>
        /// The processors.
        /// </summary>
        public List<RSProcessorInfo> processors = null;

        /// <summary>
        /// Clone this instance.
        /// </summary>
        public override RSInfo Clone()
        {
            RSFolderInfo info = new RSFolderInfo();
            info.path = path;
            info.group = group;
            info.type = type;
            if (processors != null)
            {
                info.processors = new List<RSProcessorInfo>();
                for(int i = 0;i<processors.Count;i++ )
                {
                    info.processors.Add(processors[i].Clone());
                }
            }
            return info;
        }

        /// <param name="info1">Info1.</param>
        /// <param name="info2">Info2.</param>
        public static bool operator ==(RSFolderInfo info1, RSFolderInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return true;
            
            if ((info1 as object) == null || (info2 as object) == null)
                return false;

            if (info1.path == info2.path && 
                info1.type == info2.type && 
                info1.group == info2.group)
            {
                if (info1.processors != null && info2.processors != null)
                {
                    if (info1.processors.Count != info1.processors.Count)
                        return false;

                    for (int i = 0; i<info1.processors.Count; i++)
                    {
                        if (info2.processors.Find(iter => iter == info1.processors [i]) == null)
                        {
                            return false;
                        }
                    }
                } else
                {
                    if (info1.processors == null && info2.processors == null)
                        return true;
                }
            }
            return false;
        }

        /// <param name="info1">Info1.</param>
        /// <param name="info2">Info2.</param>
        public static bool operator !=(RSFolderInfo info1, RSFolderInfo info2)
        {
            if ((info1 as object) == null && (info2 as object) == null)
                return false;

            if ((info1 as object) == null || (info2 as object) == null)
                return true;

            if (info1.path != info2.path || 
                info1.type != info2.type || 
                info1.group != info2.group)
                return true;
            RSProcessorInfo tpn = null;
            if (info1.processors != null && info2.processors != null)
            {

                if (info1.processors.Count != info2.processors.Count)
                    return true;
               
                for (int i = 0; i<info1.processors.Count; i++)
                {
                    tpn = info1.processors [i];
                    if (info2.processors.Find(iter => iter == tpn) == null)
                    {
                        return true;
                    }
                }
                return false;
            } else
            {

                if (info1.processors == null && info2.processors == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="TUT.RSFolderInfo"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="TUT.RSFolderInfo"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="TUT.RSFolderInfo"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            try
            {
                return (this == (RSFolderInfo)obj);
            } catch
            {
                return false;
            }
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="TUT.RSFolderInfo"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// RS folder info adaptor.
    /// </summary>
    public class RSFolderInfoAdaptor : IReorderableListAdaptor
    {
        /// <summary>
        /// The m folder info.
        /// </summary>
        private RSFolderInfo mFolderInfo;

        /// <summary>
        /// The m parent window.
        /// </summary>
        private EditorWindow mParentWin = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TUT.RSFolderInfoAdaptor"/> class.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="win">Window.</param>
        public RSFolderInfoAdaptor(RSFolderInfo info, EditorWindow win)
        {
            mFolderInfo = info;
            mParentWin = win;
        }

        #region IReorderableListAdaptor - Implementation
            
        /// <summary>
        /// Gets count of elements in list.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        { 
            get
            {
                if (mFolderInfo == null || mFolderInfo.processors == null)
                    return 0;
                return mFolderInfo.processors.Count; 
            }

        }

        public RSFolderInfo info
        {
            get
            {
                return mFolderInfo;
            }
        }

        /// <summary>
        /// Determines whether an item can be reordered by dragging mouse.
        /// </summary>
        /// <returns><c>true</c> if this instance can drag the specified index; otherwise, <c>false</c>.</returns>
        /// <param name="index">Index.</param>
        public virtual bool CanDrag(int index)
        {
            return true;
        }

        /// <summary>
        /// Determines whether an item can be removed from list.
        /// </summary>
        /// <returns><c>true</c> if this instance can remove the specified index; otherwise, <c>false</c>.</returns>
        /// <param name="index">Index.</param>
        public virtual bool CanRemove(int index)
        {
            return true;
        }

        /// <summary>
        /// Add new element at end of list.
        /// </summary>
        public void Add()
        {
            if (RSProlib.AllProNames.Length == 0)
            {
                if (mParentWin != null)
                {
                    mParentWin.ShowNotification(new GUIContent("undefined process"));
                }
                return;
            }

            if (mFolderInfo.processors != null && RSProlib.AllProNames.Length == mFolderInfo.processors.Count)
            {
                if (mParentWin != null)
                {
                    mParentWin.ShowNotification(new GUIContent("process with the number of upper limit"));
                }
                return;
            }
            if (mFolderInfo.processors == null)
                mFolderInfo.processors = new List<RSProcessorInfo>();
            mFolderInfo.processors.Add(NewResProInfo());
            RefrushPriority();
        }

        /// <summary>
        /// Insert new element at specified index.
        /// </summary>
        /// <param name="index">Zero-based index for list element.</param>
        public void Insert(int index)
        {
            if (RSProlib.AllProNames.Length == 0)
            {
                if (mParentWin != null)
                {
                    mParentWin.ShowNotification(new GUIContent("undefined process"));
                }
                return;
            }
            if (mFolderInfo.processors != null && RSProlib.AllProNames.Length == mFolderInfo.processors.Count)
            {
                if (mParentWin != null)
                {
                    mParentWin.ShowNotification(new GUIContent("process with the number of upper limit"));
                }
                return;
            }
            if (mFolderInfo.processors == null)
                mFolderInfo.processors = new List<RSProcessorInfo>();
            mFolderInfo.processors.Insert(index, NewResProInfo());
            RefrushPriority();
        }

        /// <summary>
        /// Duplicate existing element.
        /// </summary>
        /// <param name="index">Zero-based index of list element.</param>
        public void Duplicate(int index)
        {
            if (mParentWin != null)
            {
                mParentWin.ShowNotification(new GUIContent("unsupported"));
            }
        }

        /// <summary>
        /// Remove element at specified index.
        /// </summary>
        /// <param name="index">Zero-based index of list element.</param>
        public void Remove(int index)
        {
            mFolderInfo.processors.RemoveAt(index);
            if (mFolderInfo.processors.Count == 0)
                mFolderInfo.processors = null;
            RefrushPriority();
        }

        /// <summary>
        /// Move element from source index to destination index.
        /// </summary>
        /// <param name="sourceIndex">Zero-based index of source element.</param>
        /// <param name="destIndex">Zero-based index of destination element.</param>
        public void Move(int sourceIndex, int destIndex)
        {
            if (destIndex > sourceIndex)
                --destIndex;
            RSProcessorInfo info = mFolderInfo.processors [sourceIndex];
            mFolderInfo.processors.RemoveAt(sourceIndex);
            mFolderInfo.processors.Insert(destIndex, info);
            RefrushPriority();
        }

        /// <summary>
        /// Clear all elements from list.
        /// </summary>
        public void Clear()
        {
            mFolderInfo.processors.Clear();
            mFolderInfo.processors = null;
        }
        
        /// <summary>
        /// Draw interface for list element.
        /// </summary>
        /// <param name="position">Position in GUI.</param>
        /// <param name="index">Zero-based index of array element.</param>
        public virtual void DrawItem(Rect position, int index)
        {
            mFolderInfo.processors [index].editor_index = RSProlib.GetProIndex(mFolderInfo.processors [index].processor_func);
            if (mFolderInfo.processors [index].editor_index < 0)
                mFolderInfo.processors [index].editor_index = 0;

            int target = EditorGUI.IntPopup(position, mFolderInfo.processors [index].editor_index, RSProlib.AllProNames, RSProlib.AllProIndexs);

            if (target == mFolderInfo.processors [index].editor_index || target == RSProlib.ProInvaild)
            {
                return;
            }
            if (mFolderInfo.processors.Find(info => info.editor_index == target) != null)
            {
                if (mParentWin != null)
                {
                    mParentWin.ShowNotification(new GUIContent("process redefinition"));
                }
            } else
            {
                mFolderInfo.processors [index].editor_index = target;
                mFolderInfo.processors [index].processor_func = RSProlib.AllProNames [target];
                mFolderInfo.processors [index].processor_type = RSProlib.AllProTypeIndexs [target];
            }

        }
        
        /// <summary>
        /// Gets height of list item in pixels.
        /// </summary>
        /// <param name="index">Zero-based index of array element.</param>
        /// <returns>Measurement in pixels.</returns>
        public virtual float GetItemHeight(int index)
        {
            return 20;
        }

        /// <summary>
        /// Refrushs the priority.
        /// </summary>
        void RefrushPriority()
        {
            if (mFolderInfo == null)
                return;
            if (mFolderInfo.processors == null)
                return;
            for (int i = 0; i<Count; i++)
            {
                mFolderInfo.processors [i].priority = i;
            }
        }

        /// <summary>
        /// News the res pro info.
        /// </summary>
        /// <returns>The res pro info.</returns>
        RSProcessorInfo NewResProInfo()
        {
            RSProcessorInfo info = new RSProcessorInfo();
            info.editor_index = RSProlib.ProInvaild;
            return info;
        }
            #endregion
    }

    public class RSFolderProcessor
    {
        [ResProcess(RSProcessorType.PT_AFTER_IMPORT,"SyncParentProcessorInfo(CreateOrMoveIn)")]
        public static void SyncParentProcessorInfo(string path,RSFolderInfo parent)
        {
            if(parent == null) 
                return;
            RSEdManifest.RefrushInfoInheritFromFolder(path,parent);
        }
    }
}
