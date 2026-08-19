using System;
using System.Collections;
using UnityEngine;

namespace Durango.Utils;

public class DelayedFunction
{
	private readonly Action _func;

	private readonly YieldInstruction _yield;

	private int _callFrame;

	public DelayedFunction(Action func, YieldInstruction yi = null)
	{
		_func = func;
		_yield = yi;
	}

	public void Call(MonoBehaviour parent)
	{
		if (parent.gameObject.activeInHierarchy)
		{
			int frameCount = Time.frameCount;
			if (frameCount != _callFrame)
			{
				_callFrame = frameCount;
				parent.StartCoroutine(CoRoutine());
			}
		}
		else
		{
			_func();
		}
	}

	private IEnumerator CoRoutine()
	{
		yield return _yield;
		_func();
	}
}
