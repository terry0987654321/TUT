using System.Collections;

namespace TUT.Cooper
{
    public interface ICoDispose<T> where T : CoMsgBase
    {
        IEnumerator Dispose(T msg,bool is_block);

        int Priority<T>();
    }
    
    public class CoMsgBase
    {
        public bool is_block = false;
    }
}
