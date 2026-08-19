using JetBrains.Annotations;
using UnityEngine;

public abstract class GameSystem<T> : MonoBehaviour where T : MonoBehaviour
{
	private static bool _hasInstance;

	private static T _instance;

	public static T Instance()
	{
		if (!_hasInstance)
		{
			GameObject gameObject = new GameObject(typeof(T).Name);
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad(gameObject);
			}
			gameObject.transform.parent = GameSystemUtil.Transform;
			_instance = gameObject.AddComponent<T>();
			_hasInstance = _instance != null;
		}
		return _instance;
	}

	public static bool HasInstance()
	{
		return _hasInstance;
	}

	[UsedImplicitly]
	private static void Destroy()
	{
		_instance = null;
		_hasInstance = false;
	}
}
