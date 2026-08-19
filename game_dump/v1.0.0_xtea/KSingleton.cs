using UnityEngine;

public class KSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static bool _hasInstance;

	private static T _instance;

	protected bool Used;

	protected virtual bool CheckDontDestroyOnLoad()
	{
		return false;
	}

	protected void Awake()
	{
		CheckDuplication();
		if (Used && CheckDontDestroyOnLoad())
		{
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
		OnAwake();
	}

	private void OnDestroy()
	{
		if (Used)
		{
			_hasInstance = false;
			_instance = (T)(object)null;
		}
	}

	protected virtual void OnAwake()
	{
	}

	private void CheckDuplication()
	{
		KSingleton<T> kSingleton = this;
		T[] array = Object.FindObjectsOfType<T>();
		int num = array.Length;
		if (num >= 2)
		{
			for (int i = 0; i < num; i++)
			{
				KSingleton<T> kSingleton2 = array[i] as KSingleton<T>;
				if (kSingleton2.Used)
				{
					kSingleton = kSingleton2;
					break;
				}
				if ((Object)(object)this != (Object)(object)kSingleton2)
				{
					Object.Destroy((Object)(object)((Component)kSingleton2).gameObject);
				}
			}
		}
		if ((Object)(object)kSingleton != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if ((Object)(object)_instance == (Object)null)
		{
			_instance = (T)(object)((kSingleton is T) ? kSingleton : null);
			_hasInstance = (Object)(object)_instance != (Object)null;
			kSingleton.Used = true;
		}
	}

	private static void SetInstance(bool showError)
	{
		T[] array = Object.FindObjectsOfType<T>();
		int num = array.Length;
		if (num >= 1)
		{
			_instance = array[0];
			_hasInstance = (Object)(object)_instance != (Object)null;
			KSingleton<T> kSingleton = _instance as KSingleton<T>;
			kSingleton.Used = true;
		}
		else if (showError)
		{
			Debug.LogError((object)("Cannot find singleton object - " + typeof(T)));
		}
	}

	public static bool Exist()
	{
		if (!_hasInstance)
		{
			SetInstance(showError: false);
		}
		return _hasInstance;
	}

	public static T Create(string name)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if (_hasInstance)
		{
			return _instance;
		}
		GameObject val = new GameObject(name);
		T result = val.AddComponent<T>();
		SetInstance(showError: true);
		return result;
	}

	public static T Instance()
	{
		if (!_hasInstance)
		{
			SetInstance(showError: true);
		}
		return _instance;
	}

	public static bool HasInstance()
	{
		return _hasInstance;
	}
}
