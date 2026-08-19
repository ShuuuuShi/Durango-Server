using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

public static class GameSystemUtil
{
	private static Transform _transform;

	public static Transform Transform
	{
		get
		{
			if (_transform == null)
			{
				GameObject gameObject = new GameObject("GameSystem");
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				_transform = gameObject.transform;
			}
			return _transform;
		}
	}

	public static void Reset()
	{
		IEnumerable<Type> allDerivedGenericTypes = Reflection.GetAllDerivedGenericTypes(typeof(GameSystem<>));
		if (_transform != null)
		{
			foreach (Type item in allDerivedGenericTypes)
			{
				Reflection.Invoke(item, "Destroy");
			}
			UnityEngine.Object.Destroy(_transform.gameObject);
			_transform = null;
		}
		foreach (Type item2 in allDerivedGenericTypes)
		{
			Reflection.Invoke(item2, "Instance");
		}
	}
}
