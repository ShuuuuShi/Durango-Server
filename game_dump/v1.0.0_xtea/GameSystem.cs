using JetBrains.Annotations;
using UnityEngine;

public abstract class GameSystem<T> : MonoBehaviour where T : MonoBehaviour
{
	private static bool _hasInstance;

	private static T _instance;

	public static T Instance()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (!_hasInstance)
		{
			GameObject val = new GameObject(typeof(T).Name);
			Object.DontDestroyOnLoad((Object)(object)val);
			val.transform.parent = GameSystemUtil.Transform;
			T instance = val.AddComponent<T>();
			_instance = instance;
			_hasInstance = (Object)(object)_instance != (Object)null;
		}
		return _instance;
	}

	public static bool HasInstance()
	{
		return _hasInstance;
	}

	[UsedImplicitly]
	private void Destroy()
	{
		_instance = (T)(object)null;
		_hasInstance = false;
	}
}
