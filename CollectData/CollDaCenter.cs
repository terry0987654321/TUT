using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TUT.CollDa
{

	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CollDaParameter : Attribute
	{
		public Type ParamType;
		
		public CollDaParameter(Type type)
		{
			this.ParamType = type;
		}
	}

	public interface ICollDaDispose<T>
	{
		IEnumerator Dispose (T data);
	}

	public class CollDaCenter : TUT.TutSingletonBehaviour<CollDaCenter>
	{

		public override bool IsAutoInit ()
		{
			return true;
		}

		public override bool IsGlobal ()
		{
			return false;
		}

		public class CollDispose
		{
			private object mDisposer = null;

			public object Disposer
			{
				get
				{
					return mDisposer;
				}
			}

			private Dictionary<System.Type,System.Reflection.MethodInfo> mDisMethods = new Dictionary<System.Type, System.Reflection.MethodInfo> ();

			public CollDispose(object disposer)
			{
				mDisposer = disposer;
				System.Type[] inter_types = mDisposer.GetType ().GetInterfaces ();
				System.Reflection.MethodInfo info = null;
				System.Reflection.ParameterInfo[] pinfos = null;
				for(int i =0;i<inter_types.Length;i++)
				{
					info = inter_types[i].GetMethod("Dispose");
					if(info != null)
					{
						pinfos = info.GetParameters();
						mDisMethods.Add(pinfos[0].ParameterType,info);
					}
				}
			}

			public IEnumerator DisposeData(object data)
			{
				System.Reflection.MethodInfo minfo = null;
				TUT.TutRoutine routine = null;
				System.Type type = data.GetType ();
				CollDaParameter p = null;
				foreach (Attribute attr in type.GetCustomAttributes(false))
				{
					if (attr.GetType() == typeof(CollDaParameter))
					{
						p = attr as CollDaParameter;
						if(p.ParamType != null)
						{
							break;
						}
					}
				}

				if(p != null)
					type = p.ParamType;

				if(mDisMethods.TryGetValue(type,out minfo))
				{
					routine = TUT.TutCoroutine.Instance.Oh_StartCoroutine((IEnumerator) minfo.Invoke (mDisposer, new object[]{data}));
					yield return routine;
					yield return routine.Waiting;
				}
				else
					yield break;
			}

			public void DisposeDataEx(object data)
			{
				System.Reflection.MethodInfo minfo = null;
				System.Type type = data.GetType ();
				CollDaParameter p = null;
				foreach (Attribute attr in type.GetCustomAttributes(false))
				{
					if (attr.GetType() == typeof(CollDaParameter))
					{
						p = attr as CollDaParameter;
						if(p.ParamType != null)
						{
							break;
						}
					}
				}
				
				if(p != null)
					type = p.ParamType;
				
				if(mDisMethods.TryGetValue(type,out minfo))
				{
					TUT.TutCoroutine.Instance.Oh_StartCoroutine((IEnumerator) minfo.Invoke (mDisposer, new object[]{data}));
				}
			}
		}


		private Dictionary<object,CollDispose> mDisposers = new Dictionary<object,CollDispose>();

		public void RegisterDisposer(object disposer)
		{
			if(disposer == null || mDisposers.ContainsKey (disposer))
				return;
			CollDispose cd = new CollDispose (disposer);
			mDisposers.Add (disposer,cd);
		}

		public bool UnregisterDisposer(object disposer)
		{
			return mDisposers.Remove (disposer);
		}

		public IEnumerator Collect<T>( T data)
		{
			TUT.TutRoutine routine = null;

            List< CollDispose > lst = new List<CollDispose>( mDisposers.Values );

            foreach(CollDispose dispose in lst )
			{
				routine = TUT.TutCoroutine.Instance.Oh_StartCoroutine(dispose.DisposeData(data));
				yield return routine;
				yield return routine.Waiting;
			}
		}

		public void CollectEx<T>(T data)
		{
			List< CollDispose > lst = new List<CollDispose>( mDisposers.Values );
			
			foreach(CollDispose dispose in lst )
			{
				dispose.DisposeDataEx(data);
			}
		}
	}
}

