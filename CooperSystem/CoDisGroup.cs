using System.Collections;
using System.Collections.Generic;

namespace TUT.Cooper
{
    public class CoDisGroup
    {
        public enum DisPriority
        {
            DP_NORMAL,
            DP_URGENCY,
        }

        public class DisMsg
        {
            public CoMsgBase msg;
            public object finish = null;

            public DisMsg(CoMsgBase _msg, object _finish)
            {
                msg = _msg;
                finish = _finish;
            }
        }

        public delegate void DisFinishCallback<T>(T msg) where T : CoMsgBase;

        public delegate void BlockMsgCallback();

        private BlockMsgCallback mFinish = null;

        private List<System.WeakReference> mDisposes = new List<System.WeakReference>();

        private List<DisMsg> mMsgPools = new List<DisMsg>();

        private bool mUpingMsg = false;

        private bool mBlockMsg = false;

        private System.Reflection.MethodInfo mDisMethod = null; 
        
        public void Enter<T>(ICoDispose<T> disposer,bool force_sort = false) where T : CoMsgBase
        {
            if (mDisposes.FindIndex(i => i.Target == disposer) >= 0)
            {
                if(force_sort) mDisposes.Sort(DisSort<T>);
                return;
            }
            mDisposes.Add(new System.WeakReference(disposer));
            mDisposes.Sort(DisSort<T>);
        }

        public static int DisSort<T>(System.WeakReference w1,System.WeakReference w2) where T : CoMsgBase
        {
            if (w1 == null || w2 == null)
                    throw new System.Exception(" Cooper Dispose Sort Exception: disposer is null. ");

            ICoDispose<T> d1 = w1.Target as ICoDispose<T>;
            ICoDispose<T> d2 = w2.Target as ICoDispose<T>;

            if (d1 == null || d2 == null)
                throw new System.Exception(" Cooper Dispose Sort Exception: disposer is null. ");

            int p1 = d1.Priority<T>();
            int p2 = d2.Priority<T>();

            if(p1 == p2)
                return 0;

            if (p1 > p2)
            {
                return -1;
            } else
                return 1;
        }

        public void Exit<T>(ICoDispose<T> disposer) where T:CoMsgBase
        {
            int index = mDisposes.FindIndex(i => i.Target == disposer);
            if (index < 0)
                return;
            mDisposes.RemoveAt(index);
        }

        public void ExitAll()
        {
            BlockDisMsg(() => {
                mDisposes.Clear();
            });
        }

        public void SendMsg<T>(T msg,DisPriority priority = DisPriority.DP_NORMAL,DisFinishCallback<T> finish = null) where T : CoMsgBase
        {
            DisMsg d_msg = new DisMsg(msg,finish != null? new DisFinishCallback<T>(finish):null);
            switch (priority)
            {
                case DisPriority.DP_NORMAL:
                    mMsgPools.Add(d_msg);
                    break;
                case DisPriority.DP_URGENCY:
                    mMsgPools.Insert(0,d_msg);
                    break;
            }
            NeedUpMsg();
        }

        private void NeedUpMsg()
        {
            if (!mUpingMsg)
            {
                TUT.TutCoroutine.Instance.Oh_StartCoroutine(UpDisMsg());
            }
        }

        public void BlockDisMsg(BlockMsgCallback finish = null)
        {
            if (!mUpingMsg)
                return;
            mBlockMsg = true;
            mFinish = finish;
        }

        public void _FroceClearMsgPool()
        {
            mUpingMsg = false;
            mBlockMsg = false;
            mMsgPools.Clear();
        }

        private IEnumerator UpDisMsg()
        {
            mUpingMsg = true;
            if(mDisMethod == null)
                mDisMethod = typeof(CoDisGroup).GetMethod("DisposeMsg",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public);
            if(mDisMethod == null)
                throw new System.Exception(" Cooper Dispose Msg Exception: cannt found DisposeMsg func ");
            System.Reflection.MethodInfo dis = null;
            for (int i = 0; i<mMsgPools.Count;)
            {
                if(mBlockMsg)
                    break;
                dis = mDisMethod.MakeGenericMethod(mMsgPools[i].msg.GetType());
                if(dis == null)
                    throw new System.Exception(" Cooper Dispose Msg Exception: failed to convert the type ");
                yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( (IEnumerator)dis.Invoke(this,new object[]{mMsgPools[i].msg,mMsgPools[i].finish}) ).Waiting;
                mMsgPools.RemoveAt(i);
            }
            mUpingMsg = false;
            if (mBlockMsg)
            {
                mBlockMsg = false;
                mMsgPools.Clear();
                BlockMsgCallback cb = mFinish;
                if(cb != null)
                {
                    cb();
                }
            }
        }

        public IEnumerator DisposeMsg<T>(T msg , object finish = null) where T:CoMsgBase
        {
            if (mDisposes.Count == 0)
                yield break;
            ICoDispose<T> disposer = null;
            for (int i = 0; i<mDisposes.Count;)
            {
                if(mBlockMsg)
                    yield break;
                if( mDisposes[i].IsAlive )
                {
                    disposer = mDisposes[i].Target as ICoDispose<T>;
                    if(disposer != null)
                    {
                        yield return TUT.TutCoroutine.Instance.Oh_StartCoroutine( disposer.Dispose(msg,mBlockMsg)).Waiting;
                        if(msg.is_block)
                            break;
                    }
                    i++;
                }
                else
                {
                    mDisposes.RemoveAt(i);
                }
            }

            if(finish!=null)
            {
                DisFinishCallback<T> cb = finish as DisFinishCallback<T>;
                if (cb != null)
                    cb(msg);
            }
        }
    }
}
