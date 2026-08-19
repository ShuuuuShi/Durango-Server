using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace CoroutineBinderHelper;

public class CoroutineBinder : ICoroutineBinder
{
	public IEnumerator CachedCoroutine;

	private readonly MonoBehaviour _coroutineOwner;

	public CoroutineBinder(MonoBehaviour owner)
	{
		_coroutineOwner = owner;
	}

	public void Stop()
	{
		if (CachedCoroutine != null)
		{
			_coroutineOwner.StopCoroutine(CachedCoroutine);
			CachedCoroutine = null;
		}
	}

	public bool TryReapply(IEnumerator newCoroutine)
	{
		if (_coroutineOwner == null)
		{
			return false;
		}
		Stop();
		CachedCoroutine = newCoroutine;
		_coroutineOwner.StartCoroutine(newCoroutine);
		return true;
	}
}
