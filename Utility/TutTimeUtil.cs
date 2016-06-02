using UnityEngine;
using System.Collections;

public static class TutTimeUtil 
{
    private static float originalts = 1;

    public static void SetTimeScale( float ts )
    {
        originalts = Time.timeScale;
        Time.timeScale = ts;
    }

    public static void RecoverTimeScale()
    {
        Time.timeScale = originalts;
        originalts = 1;
    }

    public static void ResetTimeScale()
    {
        Time.timeScale = 1;
    }

    public static float GetCurrentTimeScale()
    {
        return Time.timeScale;
    }

	public static TUT.TutRoutine WaitForSeconds(float second)
	{
		return TUT.TutCoroutine.Instance.Oh_StartCoroutine (WaitForSecondsCoroutine (second));
	}

	public static IEnumerator WaitForSecondsCoroutine(float second)
	{
		float start = Time.realtimeSinceStartup;
		while(Time.realtimeSinceStartup - start < second)
		{
			yield return 0;
		}
		yield break;
	}

	public class TimeRecord
	{
		private float mTime = 0;
		private int mCount = 0;
		private int mFrame = 0;
		public TimeRecord()
		{
			mTime = Time.realtimeSinceStartup;
			mFrame = Time.frameCount;
			mCount = 0;
		}

		public string Record(string tag)
		{
			mCount ++;
			string r = mCount.ToString () + ".  " + tag + " :t= "+ (Time.realtimeSinceStartup - mTime).ToString ()
				+" :f= "+(Time.frameCount - mFrame).ToString();
			mTime = Time.realtimeSinceStartup;
			Debug.LogError (r);
			return r;
		}
	}
}
