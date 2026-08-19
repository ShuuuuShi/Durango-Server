using System;

namespace Yaml.Util;

public class Singleton<T> : ISingletonable where T : class
{
	public static T Instance { get; private set; }

	private static event Action Initalized;

	public void Initialize(object inst)
	{
		Instance = inst as T;
		OnInitalized();
		if (Singleton<T>.Initalized != null)
		{
			Singleton<T>.Initalized();
		}
		Singleton<T>.Initalized = null;
	}

	protected virtual void OnInitalized()
	{
	}
}
