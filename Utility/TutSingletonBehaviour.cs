using UnityEngine;
using System.Collections;

namespace TUT
{
    ///<summary>
    /// 单例行为
    ///  注意：如果你对单例行为的初始化有特殊的要求需要手动调用起的初始化过程
    ///  或者重载IsAutoInit方法 他将会在第一次实例化的时候 调用初始化(仅支持同步的初始化方法)
    ///     
    /// </summary>
    public abstract class TutSingletonBehaviour<T> : MonoBehaviour where T : TutSingletonBehaviour<T>
    {
        public delegate void InitializeFinishHandle();

		protected static T m_Instance = null;

        protected InitializeFinishHandle mFinishHandle = null;

        protected bool mInitialized = false;

        protected static bool mValid = true;

		public static bool isValid
		{
			get
			{
				return mValid;
			}
		}

        public bool Initialized
        {
            get
            {
                return mInitialized;
            }
        }
        
		public static T Instance
		{
			get
			{
				if( m_Instance == null )
				{
					m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;
					
					// Object not found, we create a temporary one
					if( m_Instance == null )
					{
						Debug.LogWarning(TutNorm.LogWarFormat( "Singleton Behaviour","No instance of " + typeof(T).ToString() + ", a temporary one is created."));
						m_Instance = new GameObject( "Singleton of " + typeof(T).ToString(), typeof(T) ).GetComponent<T>();
						// Problem during the creation, this should not happen
						if( m_Instance == null )
						{
							Debug.LogWarning(TutNorm.LogWarFormat( "Singleton Behaviour","Problem during the creation of " + typeof(T).ToString()));
						}
						
					}
					
					if( m_Instance != null && m_Instance.IsAutoInit() )
					{
						m_Instance.Initialize();
					}
					
					if(m_Instance.IsGlobal())
					{
						GameObject.DontDestroyOnLoad(m_Instance.gameObject);
					}
				}
				return m_Instance;
			}
		}

        /// <summary>
        ///  获取当前单例行为是否为全局的，
        ///  全局的单列行为 在change scene时是不会被销毁的
        /// </summary>
        /// <returns><c>true</c> if this instance is global; otherwise, <c>false</c>.</returns>
        public virtual bool IsGlobal()
        {
            return false;
        }

        public virtual bool IsAutoInit()
        {
            return false;
        }


        /// <summary>
        /// 初始化这个单例
        /// 需注意该方法为同步执行，所以单例牵扯到的相关
        /// 异步或协同操作结果将无法及时的等到响应
        /// 如有需要请实现InitializeCoroutine接口
        /// </summary>
        public virtual void Initialize(InitializeFinishHandle finish = null,object param = null)
        {
            if (mInitialized)
                return;
            mFinishHandle = finish; 
            InitFinish();
			mValid = true;
        }

        /// <summary>
        /// 以协同的方式初始化该单例
        /// </summary>
        /// <returns>The coroutine.</returns>
        public virtual IEnumerator InitializeCoroutine(InitializeFinishHandle finish = null,object param = null)
        {
            Initialize(finish, param);
            yield break;
        }

        protected void InitFinish()
        {
            if (mInitialized)
                return;
            mInitialized = true;
            InitializeFinishHandle handle = mFinishHandle;
            mFinishHandle = null;
            if (handle != null)
            {
                handle();
            }
        }

        protected virtual void OnDestroy()
        {
			mValid = false;
            m_Instance = null;
        }

		protected virtual  void OnApplicationQuit()
        {
			mValid = false;
            m_Instance = null;
        }
    }
}