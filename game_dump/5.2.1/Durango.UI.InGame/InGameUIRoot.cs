using UnityEngine;

namespace Durango.UI.InGame;

public class InGameUIRoot : MonoBehaviour
{
	private void Awake()
	{
		GameObject[] array = Resources.LoadAll<GameObject>("InGame");
		for (int i = 0; i < array.Length; i++)
		{
			Object.Instantiate(array[i], base.transform);
		}
	}

	public T MakeTempPrefabObject<T>()
	{
		GameObject[] array = Resources.LoadAll<GameObject>("InGame");
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<T>() != null)
			{
				GameObject obj = Object.Instantiate(gameObject, base.transform);
				obj.hideFlags = HideFlags.DontSave;
				return obj.GetComponent<T>();
			}
		}
		return default(T);
	}
}
