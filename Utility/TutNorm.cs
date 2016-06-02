using UnityEngine;
using System.Collections;

namespace TUT
{
    public static class TutNorm
    {
        public static string LogFormat (string tag,object message)
        {
            return string.Format("<color=green><b>[TUT]  <i>{0}</i> </b>=> {1}</color>",tag,message == null? " null ": message.ToString());
        }

        public static string LogWarFormat (string tag,object message)
        {
            return string.Format("<color=yellow><b>[TUT WARRING]  <i>{0}</i> </b>=>{1}</color>",tag,message == null? " null ": message.ToString());
        }

        public static string LogErrFormat (string tag,object message)
        {
            return string.Format("<color=red><b>[TUT ERROR]  <i>{0}</i>  </b>=>{1}</color>",tag,message == null? " null ": message.ToString());
        }
    }
}
