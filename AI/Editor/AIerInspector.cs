using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Rotorz.ReorderableList;
using System.Reflection;
using System;
using TUT.AI;

namespace TUT.AI.AIEditor
{

	public class AIStrategyInspector
	{
		public int StrategyID = -1;

		public float High = 20;

		public Type StrategyType;

		public string StrategyDisplayName;

		private GameObject mBindObj = null;

		private Type mParam = null;

		public AIStrategyInspector(GameObject bind)
		{
			mBindObj = bind;
		}

		public void Refrush(int index,Type type,string display)
		{
			StrategyID = index;
			StrategyDisplayName = display;
			if(StrategyType != type)
			{
				StrategyType = type;
//				UpdateGUIMethod ();
			}
		}

		public void UpdateGUIMethod()
		{
			if(mParam != null)
			{
				UnityEngine.Component obj = mBindObj.GetComponent(mParam);
				if(obj != null)
				{
					AIerComponentDestroyer.NeedDestroyCom.Add(obj);
				}
//					GameObject.DestroyImmediate(obj);
				obj = null;
			}
			mParam = null;
			if(StrategyType == null)
				return;

			TutAIParameter p = null;
			foreach (Attribute attr in StrategyType.GetCustomAttributes(false))
			{
				if (attr.GetType() == typeof(TutAIParameter))
				{
					p = attr as TutAIParameter;
					mParam = p.ParamType;
					if(mParam != null)
					{
						UnityEngine.Object obj = mBindObj.GetComponent(mParam);
						if(obj == null)
							mBindObj.AddComponent(mParam);
						obj = null;
					}
						
				}
			}
		}
	}

	public class AIStrategylib
	{
		private Type[] mTypes;
		private string[] mDisplay_names;
		private int[] mIndexs;
		public AIStrategylib(Type[] types,string[] displays)
		{
			mTypes = types;
			mDisplay_names = displays;
			mIndexs = new int[mTypes.Length];
			for(int i = 0;i<types.Length;i++)
			{
				mIndexs[i] = i;
			}
		}

		public int GetIntFromType(string type)
		{
			for(int i = 0;i<mTypes.Length;i++)
			{
				if(type == mTypes[i].ToString())
					return i;
			}
			return -1;
		}

		public Type[] TypeNames
		{
			get
			{
				return mTypes;
			}
		}

		public string[] DisplayNames
		{
			get
			{
				return mDisplay_names; 
			}
		}

		public int[] Indexs
		{
			get
			{
				return mIndexs;
			}
		}

	}

	public class AIerStrategyAdaptor : IReorderableListAdaptor
	{
		private AIerInspector mHandle;
		
		public AIerStrategyAdaptor(AIerInspector handle)
		{
			mHandle = handle;
		}
		
		public int Count
		{ 
			get
			{
				if (mHandle == null)
					return 0;
				return mHandle.Count; 
			}
		}
		
		public AIerInspector Handle
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
			mHandle.CreateNewStrategy ();
		}
		
		public void Insert(int index)
		{
			AIStrategyInspector ins = mHandle.CreateNewStrategy ();
			mHandle.AddStrategy (ins, index);
		}
		
		public void Duplicate(int index)
		{
			
		}
		
		public void Remove(int index)
		{
			mHandle.RemoveStrategy (index);
		}
		
		public void Move(int sourceIndex, int destIndex)
		{
			if (destIndex > sourceIndex)
				--destIndex;
			AIStrategyInspector ins = mHandle [sourceIndex];
			mHandle.RemoveAt (sourceIndex);
			mHandle.AddStrategy (ins, destIndex);
			mHandle.SaveStrategys ();
		}
		
		public void Clear()
		{
			mHandle.RemoveAll ();
		}
		
		public virtual void DrawItem(Rect position, int index)
		{
			AIStrategyInspector strategy = mHandle[index];
			float y_offset = 25;
			float y = 0;
			mHandle.OnGUI_SelectStrategy (position,y,strategy,y_offset);
		}
		
		public virtual float GetItemHeight(int index)
		{
			return mHandle[index].High;
		}
	}

	[InitializeOnLoad]
	public class AIerComponentDestroyer
	{
		public static List<Component> NeedDestroyCom = new List<Component> ();


		public AIerComponentDestroyer()
		{
			bool need_register = true;
			if(EditorApplication.update != null)
			{
				foreach (System.Delegate i in EditorApplication.update.GetInvocationList())
				{
					if (i.Method.Name == "UpdateDestroy")
					{
						need_register = false;
						break;
					}
				}
			}
			if (need_register)
			{
				EditorApplication.update += UpdateDestroy;
			}
		}
		void UpdateDestroy()
		{
			for(int i = 0;i<NeedDestroyCom.Count;i++)
			{
				if(NeedDestroyCom[i] != null)
				GameObject.DestroyImmediate(NeedDestroyCom[i]);
			}
			NeedDestroyCom.Clear ();
		}
	}

	public class AIerInspector
	{
		private AIerStrategyAdaptor mAdaptor = null;

		private AIStrategylib mStrategylib;

		private GameObject mBindObj = null;

		private TutAIer mAIer = null;

		public AIerInspector(AIStrategylib lib,GameObject bind,TutAIer AI)
		{
			mStrategylib = lib;
			mBindObj = bind;
			mAIer = AI;
			int index = -1;
			AIerComponentDestroyer destroy = new AIerComponentDestroyer ();
			for(int i = 0;i<mAIer.StrategyTypes.Count;)
			{
				index = mStrategylib.GetIntFromType(mAIer.StrategyTypes[i]);
				if(index >= 0)
				{
					if(!mStrategyMap.ContainsKey(index))
					{
						mStrategyMap.Add(index,CreateNewStrategy(index,mStrategylib.TypeNames[index],mStrategylib.DisplayNames[index]));
						i++;
					}
					else
						mAIer.StrategyTypes.RemoveAt(i);
				}
				else
				{
					mAIer.StrategyTypes.RemoveAt(i);
				}
			}
		}



		private List<AIStrategyInspector> mStrategyInspectors = new List<AIStrategyInspector> ();

		private Dictionary<int,AIStrategyInspector> mStrategyMap = new Dictionary<int, AIStrategyInspector> ();

		public int Count
		{
			get
			{
				return mStrategyInspectors.Count;
			}
		}

		public AIStrategyInspector this[int index]
		{
			get
			{
				if(index >= mStrategyInspectors.Count || index  < 0)
					return null;
				return mStrategyInspectors[index];
			}
		}

		public AIStrategyInspector CreateNewStrategy()
		{
			AIStrategyInspector strategy= new AIStrategyInspector (mBindObj);
			AddStrategy (strategy);
			return strategy;
		}

		public AIStrategyInspector CreateNewStrategy(int StrategyID,Type type,string display)
		{
			AIStrategyInspector strategy= new AIStrategyInspector (mBindObj);
			strategy.Refrush (StrategyID, type, display);
			strategy.UpdateGUIMethod ();
			AddStrategy (strategy);
			return strategy;
		}

		public void AddStrategy(AIStrategyInspector strategy,int index =-1)
		{
			if(index < 0)
			{
				mStrategyInspectors.Add (strategy);
			}
			else
				mStrategyInspectors.Insert (index,strategy);
		}

		public void RemoveStrategy(int index)
		{
			mStrategyMap.Remove (mStrategyInspectors[index].StrategyID);
			mStrategyInspectors[index].Refrush(-1,null,string.Empty);
			SaveStrategys();
			mStrategyInspectors [index].UpdateGUIMethod ();
			RemoveAt (index);
		}

		public void RemoveAt(int index)
		{
			mStrategyInspectors.RemoveAt (index);
		}

		public void RemoveAll()
		{
			for(int i = 0;i<mStrategyInspectors.Count;i++)
			{
				mStrategyInspectors[i].Refrush(-1,null,string.Empty);
				mStrategyInspectors [i].UpdateGUIMethod ();
			}

			mStrategyInspectors.Clear ();
			mStrategyMap.Clear ();
			SaveStrategys();
		}

		public void SaveStrategys()
		{
			mAIer.StrategyTypes.Clear ();
			for(int i = 0;i<mStrategyInspectors.Count;i++)
			{
				if(mStrategyInspectors[i].StrategyID >= 0)
				{
					mAIer.StrategyTypes.Add(mStrategyInspectors[i].StrategyType.ToString());
				}
			}
		}

		public void OnGUI_SelectStrategy(Rect position,float offset,AIStrategyInspector strategy,float high = 20)
		{
			if(mStrategylib == null)
				return;

			int index = EditorGUI.IntPopup (new Rect (position.x, position.y + offset, position.width, high-5), strategy.StrategyID, mStrategylib.DisplayNames, mStrategylib.Indexs);
			if(index != strategy.StrategyID)
			{
				if(!mStrategyMap.ContainsKey(index))
				{
					mStrategyMap.Remove(strategy.StrategyID);

					strategy.Refrush(index,mStrategylib.TypeNames[index],mStrategylib.DisplayNames[index]);
					SaveStrategys();
					strategy.UpdateGUIMethod();
					mStrategyMap.Add(index,strategy);

				}
			}
		}

		public void DrawAIerInspector()
		{
			if(mAdaptor == null)
			{
				mAdaptor = new AIerStrategyAdaptor(this);
			}
			ReorderableListControl.DrawControlFromState(mAdaptor, null, 0);
			mAIer.UpdateRate = EditorGUILayout.FloatField ("Update Rate", mAIer.UpdateRate);
		}
	}
}
