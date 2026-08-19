using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public static class KUtility
{
	[CompilerGenerated]
	private sealed class _003CCoDelayedCall_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action func;

		public float delay;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoDelayedCall_003Ed__7(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (func != null)
				{
					_003C_003E2__current = new WaitForSeconds(delay);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				func();
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static readonly XXHash RandomHash = new XXHash(1);

	public static int GetRandomHash(int x, int y)
	{
		return (int)RandomHash.GetHash(x, y);
	}

	public static int GetRandomHashRange(int min, int max, int key)
	{
		return RandomHash.Range(min, max, key);
	}

	[CanBeNull]
	public static GameObject FindObjectByName([NotNull] GameObject entity, string name, bool includeInactive = false)
	{
		Transform transform = FindTransformByName(entity, name, includeInactive);
		if ((bool)transform)
		{
			return transform.gameObject;
		}
		return null;
	}

	[CanBeNull]
	public static Transform FindTransformByName([NotNull] GameObject entity, string name, bool includeInactive = false)
	{
		using Reusable<List<Transform>> reusable = ReusableList<Transform>.Pop();
		List<Transform> value = reusable.Value;
		entity.GetComponentsInChildren(includeInactive, value);
		for (int i = 0; i < GetSize(value); i++)
		{
			if (value[i].name == name)
			{
				return value[i];
			}
		}
		return null;
	}

	[CanBeNull]
	public static Transform FindTransformByDist([NotNull] GameObject entity, Vector3 pos, string prefix = null, bool includeInactive = false)
	{
		using Reusable<List<Transform>> reusable = ReusableList<Transform>.Pop();
		List<Transform> value = reusable.Value;
		float num = float.MaxValue;
		Transform result = null;
		entity.GetComponentsInChildren(includeInactive, value);
		for (int i = 0; i < GetSize(value); i++)
		{
			Transform transform = value[i];
			string name = transform.name;
			if (string.IsNullOrEmpty(prefix) || name.StartsWith(prefix))
			{
				float sqrMagnitude = (transform.position - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = transform;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public static void DelayedCall(MonoBehaviour owner, Action func, float delay)
	{
		if (func != null)
		{
			if (delay < 0f)
			{
				func();
			}
			else
			{
				owner.StartCoroutine(CoDelayedCall(func, delay));
			}
		}
	}

	public static IEnumerator CoDelayedCall(Action func, float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoDelayedCall_003Ed__7(0)
		{
			func = func,
			delay = delay
		};
	}

	public static T Instantiate<T>(UnityEngine.Object asset) where T : MonoBehaviour
	{
		if (asset == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(asset) as GameObject;
		if (gameObject != null)
		{
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public static T Instantiate<T>(UnityEngine.Object asset, Transform parent) where T : MonoBehaviour
	{
		if (asset == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(asset, parent) as GameObject;
		if (gameObject != null)
		{
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public static int GetSize<T>(ICollection<T> collection)
	{
		return collection?.Count ?? 0;
	}

	public static string GetName(this Type type)
	{
		string text = string.Empty;
		Type[] array = ((!type.IsGenericType) ? Type.EmptyTypes : type.GetGenericArguments());
		int num = 0;
		string text2;
		while (true)
		{
			text2 = type.Name;
			if (type.IsGenericType)
			{
				string[] array2 = text2.Split('`');
				text2 = array2[0].Trim();
				int result = 0;
				if (array2.Length > 1)
				{
					int.TryParse(array2[1].Trim(), out result);
				}
				if (result > 0)
				{
					text2 += "<";
					for (int i = 0; i < result; i++)
					{
						if (i > 0)
						{
							text2 += ", ";
						}
						text2 += array[array.Length - num - result + i].GetName();
					}
					text2 += ">";
					num += result;
				}
			}
			if (type.ReflectedType == null)
			{
				break;
			}
			text = ((!string.IsNullOrEmpty(text)) ? (text2 + "." + text) : text2);
			type = type.ReflectedType;
		}
		string text3 = ((!string.IsNullOrEmpty(type.Namespace)) ? (type.Namespace + "." + text2) : text2);
		if (string.IsNullOrEmpty(text))
		{
			return text3;
		}
		return text3 + "." + text;
	}
}
