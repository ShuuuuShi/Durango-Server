using UnityEngine;

public class ChangeHierarchy : MonoBehaviour
{
	public GameObject _rootObject;

	public string _mainDummyName = "MainDummy";

	private void Awake()
	{
		GameObject val = KUtility.FindObjectByName(_rootObject, _mainDummyName, includeInactive: true);
		if (!Object.op_Implicit((Object)(object)val))
		{
			Debug.LogError((object)"Cannot find MainDummy");
			return;
		}
		int childCount = ((Component)this).transform.childCount;
		Transform[] array = (Transform[])(object)new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = ((Component)this).transform.GetChild(i);
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform val2 = array[j];
			if (Object.op_Implicit((Object)(object)val2))
			{
				val2.parent = val.transform;
			}
		}
	}
}
