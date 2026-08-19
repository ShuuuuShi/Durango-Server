using UnityEngine;

namespace Durango.UI.InGame;

public class InGameUIRoot : MonoBehaviour
{
	private void Awake()
	{
		GameObject[] array = Resources.LoadAll<GameObject>("InGame");
		GameObject[] array2 = array;
		foreach (GameObject original in array2)
		{
			Object.Instantiate(original, base.transform);
		}
	}

	public T MakeTempPrefabObject<T>()
	{
		GameObject[] array = Resources.LoadAll<GameObject>("InGame");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			T component = gameObject.GetComponent<T>();
			if (component != null)
			{
				GameObject gameObject2 = Object.Instantiate(gameObject, base.transform);
				gameObject2.hideFlags = HideFlags.DontSave;
				return gameObject2.GetComponent<T>();
			}
		}
		return default(T);
	}
}
