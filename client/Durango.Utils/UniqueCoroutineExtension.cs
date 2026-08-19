using System.Collections;
using CoroutineBinderHelper;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public static class UniqueCoroutineExtension
{
	public static Coroutine StartCoroutine(this MonoBehaviour owner, [CanBeNull] ref ICoroutineBinder binder, [NotNull] IEnumerator coroutine)
	{
		if (owner == null)
		{
			return null;
		}
		if (binder == null)
		{
			binder = new CoroutineBinder(owner);
		}
		CoroutineBinder coroutineBinder = (CoroutineBinder)binder;
		coroutineBinder.Stop();
		coroutineBinder.CachedCoroutine = coroutine;
		if (GameManager.IsSceneClosing)
		{
			return null;
		}
		return owner.StartCoroutine(coroutineBinder.CachedCoroutine);
	}

	public static void StopCoroutine(this MonoBehaviour owner, ICoroutineBinder binder)
	{
		if (binder is CoroutineBinder coroutineBinder)
		{
			coroutineBinder.Stop();
		}
	}
}
