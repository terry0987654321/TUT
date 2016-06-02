using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Rotorz.ReorderableList;

namespace TUT
{
	public class GuideUnitListAdaptor : IReorderableListAdaptor
	{
		protected IGuideCfg mHandle;

		public string ActiveName = string.Empty;

		public int SelectedIndex = -1;

		public GuideUnitListAdaptor(IGuideCfg handle)
		{
			mHandle = handle;
		}

		public void setHandle(IGuideCfg handle)
		{
			mHandle = handle;
		}
		
		public int Count
		{ 
			get
			{
				if (mHandle == null)
					return 0;
				return mHandle.Count(); 
			}
		}
		
		public IGuideCfg Handle
		{
			get
			{
				return mHandle;
			}
		}
		
		public virtual bool CanDrag(int index)
		{
			return true;
		}
		
		public virtual bool CanRemove(int index)
		{
			return true;
		}
		
		public void Add()
		{
			if(string.IsNullOrEmpty(ActiveName))
				return;
			mHandle.AddNewPoint (ActiveName);
		}
		
		public void Insert(int index)
		{
			if(string.IsNullOrEmpty(ActiveName))
				return;
			mHandle.InsertPoint (index, ActiveName);
		}
		
		public void Duplicate(int index)
		{
			
		}
		
		public void Remove(int index)
		{
			mHandle.RemovePoint (index);
		}
		
		public void Move(int sourceIndex, int destIndex)
		{
			mHandle.MovePoint (sourceIndex, destIndex);
		}
		
		public void Clear()
		{
			mHandle.Clear ();
		}

		public virtual void DrawItem(Rect position, int index)
		{

		}
		
		public virtual float GetItemHeight(int index)
		{
			return 10;
		}
	}
}
