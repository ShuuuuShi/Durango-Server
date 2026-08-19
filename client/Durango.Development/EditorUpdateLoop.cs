using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Durango.Development;

public class EditorUpdateLoop
{
	private static readonly List<EditorUpdateLoop> List = new List<EditorUpdateLoop>();

	private Action _onFinish;

	private MethodInfo _updateMethod;

	public MonoBehaviour Component { get; private set; }

	public static EditorUpdateLoop Play(MonoBehaviour comp, Action onFinish = null)
	{
		if (comp == null)
		{
			return null;
		}
		return null;
	}

	private void Update()
	{
	}

	public void Stop()
	{
	}
}
