
using UnityEngine;
using System.Collections;

public class example_TutResources : MonoBehaviour 
{
    //"Assets/New Folder 1/New Material.mat",
    private string[] load_paths = new string[]{"Assets/New Folder 1/New Material.mat","Assets/Resources/sssss.mat"};
    TUT.OhResResult mResult = null; 
	void Start()
	{
		CallbackLoadAssets ();

		StartCoroutine (CoroutineLoadAssets ());

        mResult = TUT.TutResources.Instance.Load(load_paths);
	}

//    public void CallbackLoadAssets()
//    {
//        TUT.RSystem.RSBldReqResult result = new TUT.RSystem.RSBldReqResult(load_paths, (TUT.RSystem.RSBldReqResult.ResultData[] data) => {
//            for(int i = 0;i< data.Length;i++)
//            {
//                Debug.Log(data[i].path+"    type: "+ data[i].result.GetType().ToString());
//            }
//        });
//        TUT.TutResources.Instance.Load(result);
//    }

    TUT.OhResResult result  = null;
   public void CallbackLoadAssets()
    {
        TUT.TutResources.Instance.Load(load_paths).ResultCB = (TUT.OhResResult  _result)=>{ 
            foreach (TUT.TutResResultData data in _result)
            {
                Debug.Log("Callback => "+data.path+"    type: "+ data.result.GetType().ToString());
            }
        };
    }

//    public IEnumerator CoroutineLoadAssets()
//    {
//        TUT.RSystem.RSBldReqResult result = new TUT.RSystem.RSBldReqResult(load_paths);
//        yield return StartCoroutine(TUT.TutResources.Instance.LoadCoroutine(result));
//        foreach (TUT.RSystem.RSBldReqResult.ResultData data in result)
//        {
//            Debug.Log(data.path+"    type: "+ data.result.GetType().ToString());
//        }
//    }

    public IEnumerator CoroutineLoadAssets()
    {
        TUT.OhResResult result = TUT.TutResources.Instance.Load(load_paths);
        yield return result.Waiting;
        foreach (TUT.TutResResultData data in result)
        {
            Debug.Log("Coroutine => "+data.path+"    type: "+ data.result.GetType().ToString());
        }
    }


    void Update()
    {
        if (mResult != null && mResult.IsDone)
        {
            foreach (TUT.TutResResultData data in mResult)
            {
                Debug.Log("Update => "+data.path+"    type: "+ data.result.GetType().ToString());
            }
            mResult = null;
        }
    }
}
