using System;
using System.Collections;
using UnityEngine;

public class DelayedFunction
{
	private Action _func;

	private IEnumerator _yield;

	private bool _flag;

	public DelayedFunction(Action func, IEnumerator yield = null)
	{
		_func = func;
		_yield = yield;
	}

	public void Call(MonoBehaviour parent)
	{
		if (!_flag)
		{
			parent.StartCoroutine(CoRoutine());
		}
	}

	private IEnumerator CoRoutine()
	{
		_flag = true;
		yield return _yield;
		_func();
		_flag = false;
	}
}
