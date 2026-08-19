using UnityEngine;

namespace Durango.Prologue;

public class ChangeHierarchy : MonoBehaviour
{
	public GameObject _rootObject;

	public string _mainDummyName = "MainDummy";

	private void Awake()
	{
		GameObject gameObject = KUtility.FindObjectByName(_rootObject, _mainDummyName, includeInactive: true);
		if (!gameObject)
		{
			Debug.LogError("Cannot find MainDummy");
			return;
		}
		int childCount = base.transform.childCount;
		Transform[] array = new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = base.transform.GetChild(i);
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform transform = array[j];
			if ((bool)transform)
			{
				transform.parent = gameObject.transform;
			}
		}
	}
}
