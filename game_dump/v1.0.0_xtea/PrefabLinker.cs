using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PrefabLinker : MonoBehaviour
{
	[SerializeField]
	private bool _loadOnEnable;

	[SerializeField]
	private List<GameObject> _prefabs;

	private readonly Dictionary<Type, Component> _cachedScript = new Dictionary<Type, Component>();

	private void OnEnable()
	{
		if (_loadOnEnable)
		{
			Load();
		}
	}

	public void Load(Action<GameObject> init = null, Func<GameObject, bool> condition = null)
	{
		List<string> list = new List<string>();
		Transform transform = ((Component)this).transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			list.Add(((Object)child).name);
			init?.Invoke(((Component)child).gameObject);
		}
		int j = 0;
		for (int size = KUtility.GetSize(_prefabs); j < size; j++)
		{
			GameObject val = _prefabs[j];
			if (!((Object)(object)val == (Object)null) && (condition == null || condition(val)) && list.IndexOf(((Object)val).name) == -1)
			{
				GameObject obj = AddChild(val, transform);
				init?.Invoke(obj);
			}
		}
	}

	private static GameObject AddChild([NotNull] GameObject o, Transform parent)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (GameObject)Object.Instantiate((Object)(object)o, parent);
		val.SetActive(true);
		val.transform.localPosition = o.transform.localPosition;
		val.transform.localScale = o.transform.localScale;
		((Object)val).name = ((Object)o).name;
		return val;
	}

	public T FindScript<T>() where T : Component
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Type typeFromHandle = typeof(T);
		if (_cachedScript.TryGetValue(typeFromHandle, out var value))
		{
			return (T)(object)((value is T) ? value : null);
		}
		Transform transform = ((Component)this).transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			T component = ((Component)transform.GetChild(i)).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				_cachedScript.Add(typeFromHandle, (Component)(object)component);
				return component;
			}
		}
		int j = 0;
		for (int size = KUtility.GetSize(_prefabs); j < size; j++)
		{
			GameObject val = _prefabs[j];
			if (!((Object)(object)val == (Object)null) && !((Object)(object)val.GetComponent<T>() == (Object)null))
			{
				GameObject val2 = (GameObject)Object.Instantiate((Object)(object)val, ((Component)this).transform);
				val2.SetActive(true);
				val2.transform.localPosition = val.transform.localPosition;
				val2.transform.localScale = val.transform.localScale;
				((Object)val2).name = ((Object)val).name;
				T component2 = val2.GetComponent<T>();
				_cachedScript.Add(typeFromHandle, (Component)(object)component2);
				return component2;
			}
		}
		return (T)(object)null;
	}

	[ExposedInEditor(null)]
	private void Sort()
	{
		int size = KUtility.GetSize(_prefabs);
		for (int num = size - 1; num >= 0; num--)
		{
			if ((Object)(object)_prefabs[num] == (Object)null)
			{
				_prefabs.RemoveAt(num);
			}
		}
		_prefabs.Sort(PrefabComparison);
	}

	private static int PrefabComparison(GameObject o1, GameObject o2)
	{
		return string.CompareOrdinal(((Object)o1).name, ((Object)o2).name);
	}
}
