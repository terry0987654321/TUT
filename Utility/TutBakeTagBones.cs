using UnityEngine;
using System.Collections;

public class TutBakeTagBones : MonoBehaviour 
{
	public Transform[] TagBones = null;

	private Transform mSelfTrf = null;

	public Transform _SelfTrf
	{
		get
		{
			if(mSelfTrf == null)
			{
				mSelfTrf = this.transform;
			}
			return mSelfTrf;
		}
	}

	public Vector3 GetBonesPos(int index)
	{
		if(TagBones ==null || index >=TagBones.Length || index < 0 )
		{
			return Vector3.zero;
		}
		Transform trf = TagBones[index];
		if(trf == null)
			return Vector3.zero;
		return  _SelfTrf.InverseTransformPoint(trf.position);
	}

	public Quaternion GetBonesRotate(int index)
	{
		if(TagBones ==null || index >=TagBones.Length || index < 0)
		{
			return Quaternion.identity;
		}
		Transform trf = TagBones[index];
		if(trf == null)
			return Quaternion.identity;
		return trf.rotation*Quaternion.Inverse( _SelfTrf.localRotation);

	}
}
