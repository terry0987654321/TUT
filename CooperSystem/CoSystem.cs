using System.Collections;
using System.Collections.Generic;

namespace TUT.Cooper
{
    public class CoSendOperation<T> where T : CoMsgBase
    {
        private bool mIsDone = false;

        public bool IsDone
        {
            get
            {
                return mIsDone;
            }
        }

        private T mResult = null;

        public T Result
        {
            get
            {
                return mResult;
            }
        }

        public CoSendOperation()
        {
            mResult = null;
            mIsDone = false;
        }

        public void SendOperationFinish(T result)
        {
            mResult = result;
            mIsDone = true;
        }

        public void Destroy()
        {
            mIsDone = false;
        }
    }

    /// <summary>
    /// Cooper Sys 主要解决模块之间的协作问题
    ///  它包含了两部分: Msg 和 Dispose
    /// Cooper Msg： 定义了协作操作中的数据结构
    /// Cooper Dispose ： 定义了对协作信息的解读
    /// Cooper Sys 则是他们对外的总接口
    /// </summary>
    public class CoSystem : TUT.TutSingletonBehaviour<CoSystem>
    {
        /// <summary>
        /// 无需显示的调用Init函数
        /// </summary>
        /// <returns><c>true</c> if this instance is auto init; otherwise, <c>false</c>.</returns>
        public override bool IsAutoInit()
        {
            return true;
        }

        /// <summary>
        /// 获取当前单例行为全局的，
        ///  全局的单列行为 在change scene时是不会被销毁的
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        public override bool IsGlobal()
        {
            return true;
        }

        private Dictionary<System.Type,CoDisGroup> mGroups = new Dictionary<System.Type,CoDisGroup>();

        private List<CoMsgBase> mMsgPools = new List<CoMsgBase>();

        public CoDisGroup GetOrCreateGroup<T>() where T:CoMsgBase
        {
            CoDisGroup group = null;
            if (!mGroups.TryGetValue(typeof(T), out group))
            {
                group = new CoDisGroup();
                mGroups.Add(typeof(T), group);
            }
            return group;
        }
    
        /// <summary>
        /// 注册你的协作信息处理者
        /// </summary>
        /// <param name="disposer">Disposer.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Enter<T>(ICoDispose<T> disposer) where T : CoMsgBase
        {
            CoDisGroup group = null;
            if (!mGroups.TryGetValue(typeof(T), out group))
            {
                group = new CoDisGroup();
                mGroups.Add(typeof(T),group);
            } 
            group.Enter<T>(disposer);
        }

        /// <summary>
        /// 取消一个你的协作信息处理者
        /// </summary>
        /// <param name="disposer">Disposer.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Exit<T>(ICoDispose<T> disposer) where T : CoMsgBase
        {
            CoDisGroup group = null;
            if (mGroups.TryGetValue(typeof(T), out group))
            {
                group.Exit<T>(disposer);
            }
        }

        /// <summary>
        /// 取消一个协作信息的全部处理者
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Exit<T>() where T:CoMsgBase
        {
            CoDisGroup group = null;
            if (mGroups.TryGetValue(typeof(T), out group))
            {
                group.ExitAll();
            }
        }

        /// <summary>
        /// 中断一个协作信息的消息处理
        /// </summary>
        /// <param name="finish">Finish.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public bool Block<T>(CoDisGroup.BlockMsgCallback finish = null) where T:CoMsgBase
        {
            CoDisGroup group = null;
            if (mGroups.TryGetValue(typeof(T), out group))
            {
                group.BlockDisMsg(finish);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 中断全部的协作信息处理
        /// </summary>
        public void BlockAll()
        {
            StopAllCoroutines();
            foreach (CoDisGroup group in mGroups.Values)
            {
                group._FroceClearMsgPool();
            }
        }

        /// <summary>
        /// 发送指定的协作信息
        /// </summary>
        /// <param name="msg">Message.</param>
        /// <param name="finish">Finish.</param>
        /// <param name="priority">Priority.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SendMsg<T>(T msg,CoDisGroup.DisFinishCallback<T> finish = null,CoDisGroup.DisPriority priority = CoDisGroup.DisPriority.DP_NORMAL) where T : CoMsgBase
        {
            CoDisGroup group = GetOrCreateGroup<T>();
            group.SendMsg<T>(msg, priority, finish);
        }

        public CoSendOperation<T> SendMsgEx<T>(T msg,CoDisGroup.DisPriority priority = CoDisGroup.DisPriority.DP_NORMAL) where T : CoMsgBase
        {
            CoDisGroup group = GetOrCreateGroup<T>();
            CoSendOperation<T> operation = new CoSendOperation<T>();
            group.SendMsg<T>(msg, priority, operation.SendOperationFinish);
            return operation;
        }
    }
}

