using UnityEngine;
using System.Collections;

namespace TUT
{
    public abstract class TutSingleton<T> where T : TutSingleton<T>
    {
        protected static T m_Instance = null;

        public static T Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    m_Instance = System.Activator.CreateInstance<T>();
                    m_Instance.Initialize();
                }
                return m_Instance;
            }
        }

        protected virtual void Initialize()
        {

        }
    }
}
