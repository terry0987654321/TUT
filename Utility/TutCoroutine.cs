using UnityEngine;
using System.Collections;

namespace TUT
{
	public class TutRoutine : System.IDisposable
	{
		static int OCount = 0;
		public int myCount = 0;
		public TutRoutine()
		{
			myCount = OCount;
			OCount ++;
		}
		~TutRoutine()
		{
			Dispose();
		}
		
		public void Dispose()
		{
			Dispose(true);
			System.GC.SuppressFinalize(this);
		}
		
		void Dispose(bool disposing)
		{
			if (disposing)
			{
				_Destroy();
			}
		}

		private void _Destroy()
		{
			Block ();
		}
		
		private object mReturn = null;
		
		private System.Exception mException;
		
		private Coroutine mCoReturn;
		
		private bool mIsDone = false;
		
		private bool mRunning = false;
		
		private bool mIsBlock = false;
		
		public bool isBlock
		{
			get
			{
				return mIsBlock;
			}
		}

		private TutRoutine mSubRoutine = null;

		public RoutineCallback BlockCB = null;

		public object ClientBlockObj = null;

        public delegate void RoutineCallback(object result);

        private RoutineCallback mResultCB = null;

        public RoutineCallback ResultCB
        {
            get
            {
                return mResultCB;
            }
            set
            {
                mResultCB = null;
				if(value != null)
				{
					if(IsDone)
					{
						value(Result);
					}
					else
					{
						mResultCB = value;
					}
				}
            }
        }
        
        public object Result
        {
            get
            {
                if(mException != null)
                    throw mException;
                return mReturn;
            }
        }

        public bool IsDone
        {
            get
            {
                return mIsDone;
            }
        }

        public Coroutine Waiting
        {
            get
            {
                return mCoReturn;
            }
        }

        public void Block()
        {
//            if (!mRunning)
//                return;
			_Clear ();

			if(mSubRoutine != null)
			{
				mSubRoutine.Block();
			}
			mSubRoutine = null;
			RoutineCallback cb = mResultCB;
            mResultCB = null;
            if(cb != null)
            {
                cb(null);
            }
            mIsBlock = true;
			if(BlockCB != null)
				BlockCB(ClientBlockObj);
			ClientBlockObj = null;
        }
        public void _InternalInit(Coroutine coroutine)
        {
            mCoReturn = coroutine;

        }

		private void _Clear()
		{
			if(mCoReturn != null)
			{
//				TUT.TutCoroutine.Instance.StopCoroutine (mCoReturn);
				mCoReturn = null;
			}
		}

		public static int Count = 0;
		public static bool IgnoreYRNull = false;

		private int mRoutineCount = 0;
        public IEnumerator _InternalRoutine(IEnumerator coroutine)
        {
//#if UNITY_EDITOR
//			System.Diagnostics.StackTrace st1 = new System.Diagnostics.StackTrace();
//			System.Diagnostics.StackFrame[] sfs1 = st1.GetFrames();
//			System.Reflection.MethodBase mb1 = sfs1[4].GetMethod();
//			string method_name = mb1.DeclaringType.FullName;
//#endif
			if (mIsBlock)
			{
				mIsDone = true;
				_Clear();
                yield break;
			}
            mRunning = true;
            mIsDone = false;
			mRoutineCount = 0;
            while (mRunning)
            {
				mRoutineCount ++;
				if (mIsBlock)
				{
					mIsDone = true;
					if(mSubRoutine != null)
					{
						mSubRoutine.Block();
					}
					mSubRoutine = null;
					_Clear();
					yield break;
				}
//				try
				{
//#if UNITY_EDITOR
//					Profiler.BeginSample(method_name);
//#endif
					if(!coroutine.MoveNext())
                    {
                        mRunning = false;
                        mIsDone = true;
                        RoutineCallback cb = mResultCB;
                        mResultCB = null;
                        if(cb != null)
                        {
                            cb(mReturn);
                        }
//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif
						_Clear();
						yield break;
					}
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
				}
//                catch(System.Exception e)
//                {
//					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(e,true);
//					System.Diagnostics.StackFrame[] sfs = st.GetFrames();
//					string log_track = string.Empty;
//					for (int u = 0; u < sfs.Length; ++u)
//					{
//						System.Reflection.MethodBase mb = sfs[u].GetMethod();
//						log_track +=(string.Format("\n[CALL STACK][{0}]: {1} =>file: {2} =>line:{3},{4} ", u, mb.DeclaringType.FullName, 
//						                           sfs[u].GetFileName(),
//						                           sfs[u].GetFileLineNumber().ToString(),
//						                           sfs[u].GetFileColumnNumber().ToString()));            
//					 }
//                    mException = e;
//					Debug.LogError(TutNorm.LogErrFormat("[Exception]",   e.Message + log_track));
//					_Clear();
//					yield break;
//				}
				
				mReturn = coroutine.Current;
				if(mReturn != null && mReturn.GetType()==typeof(TutRoutine))
				{
					mSubRoutine = mReturn as TutRoutine;
				}
				if(IgnoreYRNull)
				{
					if(mReturn != null || mRoutineCount%1024 == 0)
					{
						Count ++;
						yield return mReturn;
					}
				}
				else
				{
					yield return mReturn;
				}

            }
        }

    }

    public class TutCoroutine : TUT.TutSingletonBehaviour<TutCoroutine>
    {
        public override bool IsAutoInit()
        {
            return true;
        }

        public TutRoutine  Oh_StartCoroutine(IEnumerator coroutine)
        {
            TutRoutine routine = new TutRoutine();
			routine._InternalInit( StartCoroutine(routine._InternalRoutine (coroutine)) );
			return routine;
        }

    }
}