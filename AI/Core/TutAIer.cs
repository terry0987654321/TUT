using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TUT.AI
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TutAIParameter : Attribute
	{
		public Type ParamType;
		
		public TutAIParameter(Type type)
		{
			this.ParamType = type;
		}
	}

	[System.Serializable]
	public class TutAIer 
	{
		public float UpdateRate = 0.1f;

		public List<string> StrategyTypes =new List<string>();

		private bool mIsReady = false;

		public delegate void ProStrategy();

		public ProStrategy PreProStrategyCB = null;

		public ProStrategy PostProStrategyCB = null;

		private bool mIsColsedAI
        {
            get
            {
                return _miscolsedAi;
            }
            set
            {
                _miscolsedAi = value;
            }
        }

        private bool _miscolsedAi = true;

		private static Dictionary<string,System.Type> mStrategyMap = new Dictionary<string, System.Type>();

		private List<TutAIStrategy> mStrategys = new List<TutAIStrategy>();

		private Dictionary<System.Type,TutAIStrategy> mAIStrategyMap = new Dictionary<System.Type, TutAIStrategy>();

		private TUT.TutRoutine mUpdateState = null;

		private GameObject mParamObj = null;

		public GameObject ParamObj
		{
			get
			{
				return mParamObj;
			}
			set
			{
				mParamObj = value;
			}
		}

		public TutAIStrategy this[int index]
		{
			get
			{
				if(index < 0 || index >= mStrategys.Count)
					return null;
				return mStrategys[index];
			}
		}

		public int Count
		{
			get
			{
				return mStrategys.Count;
			}
		}


		public T GetAIStrategy<T>() where T:TutAIStrategy
		{
			TutAIStrategy strategy = null;
			if(!mAIStrategyMap.TryGetValue(typeof(T),out strategy))
			{
				return default(T);
			}
			return strategy as T;
		}

		private void PreProStrategyType()
		{
			if (StrategyTypes == null || StrategyTypes.Count == 0)
				return;
			mStrategys.Clear ();
			string type_name = string.Empty;
			System.Type type = null;
			TutAIStrategy strategy = null;
			for(int i = 0;i < StrategyTypes.Count;i++)
			{
				type_name = StrategyTypes[i];
				if(mStrategyMap.ContainsKey(type_name))
				{
					type = mStrategyMap[type_name];
					strategy =(TutAIStrategy) System.Activator.CreateInstance(type);
					strategy.InitStrategy(this,mParamObj);
					mStrategys.Add(strategy);
					mAIStrategyMap.Add(type,strategy);
				}
				else
				{
					type = System.Type.GetType(type_name);
					if(type != null)
					{
						mStrategyMap.Add(type_name,type);
						strategy =(TutAIStrategy)System.Activator.CreateInstance(type);
						strategy.InitStrategy(this,mParamObj);
						mStrategys.Add(strategy);
						mAIStrategyMap.Add(type,strategy);
					}
					else
					{
						Debug.LogError(TutNorm.LogErrFormat(" Add AI Strategy ","Miss Strategy Type " + type_name));
					}
				}
			}
			mIsReady = true;
		}

		private IEnumerator UpdateStrategy(float delay)
		{
			if(!mIsReady)
				PreProStrategyType();
			if (mStrategys.Count == 0)
				yield break;

			if(delay > 0)
				yield return new WaitForSeconds (delay);

			TUT.TutRoutine routine = null;
			while(true)
			{

				if(PreProStrategyCB != null)
				{
					PreProStrategyCB();
				}

				for(int i = 0;i<mStrategys.Count;i++)
				{
					routine = TUT.TutCoroutine.Instance.Oh_StartCoroutine( mStrategys[i].ExeStrategy());
					yield return routine;
					yield return routine.Waiting;
					if(routine.isBlock)
						yield break;
				}

				if(PostProStrategyCB != null)
				{
					PostProStrategyCB();
				}
				yield return new WaitForSeconds(UpdateRate);
			}
		}

		public void AIInit(GameObject param_obj)
		{
			if(!mIsReady)
			{
				mIsColsedAI =true;
				mParamObj = param_obj;
				PreProStrategyType();
			}
		}

		public void AIStart(float delay = 0)
		{
            if (mUpdateState != null)
				return;
			mIsColsedAI = false;
			mUpdateState = TUT.TutCoroutine.Instance.Oh_StartCoroutine (UpdateStrategy (delay));
			if(mIsColsedAI)
			{
				AIStop();
			}
		}

		public void AIStop()
		{

			if (mUpdateState != null)
				mUpdateState.Block ();
			mUpdateState = null;
			for(int i = 0;i<mStrategys.Count;i++)
			{
				mStrategys[i].BlockStrategy();
			}
			
			mIsColsedAI = true;
		}

		public void AIReset()
		{
			if (mUpdateState != null)
				mUpdateState.Block ();
			mUpdateState = null;
			for(int i = 0;i<mStrategys.Count;i++)
			{
				mStrategys[i].BlockStrategy();
			}
			
			if(mIsColsedAI)
			{
				AIStop();
			}
            else
            {

                mUpdateState = TUT.TutCoroutine.Instance.Oh_StartCoroutine(UpdateStrategy(0));
                mIsColsedAI = false;
            }
        }

		public void BlockAllStrategys()
		{
			for(int i = 0;i<mStrategys.Count;i++)
			{
				mStrategys[i].BlockStrategy();
			}
			
		}

		public void Destroy()
		{
			AIStop ();
			mParamObj = null;
			for(int i = 0;i<mStrategys.Count;i++)
			{
				mStrategys[i].OnDestroy();
			}
			mStrategys.Clear ();

		}
	}
}

