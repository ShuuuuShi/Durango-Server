using System;
using System.Collections.Generic;
using UnityEngine;

public class UIEffectController : KSingleton<UIEffectController>
{
	[SerializeField]
	private GameObject _effectContainer;

	private readonly List<KeyValuePair<Type, UIEffect>> _effectsBase = new List<KeyValuePair<Type, UIEffect>>();

	private readonly List<KeyValuePair<Type, Stack<UIEffect>>> _effectsPool = new List<KeyValuePair<Type, Stack<UIEffect>>>();

	protected override void OnAwake()
	{
		Transform transform = _effectContainer.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			((Component)transform.GetChild(i)).gameObject.SetActive(false);
		}
	}

	public T Play<T>(Transform parent, Vector3 offset) where T : UIEffect
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		UIEffect uIEffect = Get(typeof(T));
		if ((Object)(object)uIEffect == (Object)null)
		{
			return (T)null;
		}
		uIEffect.SetParent(parent, offset);
		uIEffect.Play();
		return uIEffect as T;
	}

	public T Play<T>(Vector3 position) where T : UIEffect
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		UIEffect uIEffect = Get(typeof(T));
		if ((Object)(object)uIEffect == (Object)null)
		{
			return (T)null;
		}
		uIEffect.SetPosition(position);
		uIEffect.Play();
		return uIEffect as T;
	}

	private UIEffect Get(Type type)
	{
		int num = -1;
		int i = 0;
		for (int count = _effectsPool.Count; i < count; i++)
		{
			if ((object)_effectsPool[i].Key == type)
			{
				if (_effectsPool[i].Value.Count > 0)
				{
					num = i;
				}
				break;
			}
		}
		UIEffect uIEffect = null;
		if (num != -1)
		{
			uIEffect = _effectsPool[num].Value.Pop();
		}
		else
		{
			int j = 0;
			for (int count2 = _effectsBase.Count; j < count2; j++)
			{
				if ((object)_effectsBase[j].Key == type)
				{
					num = j;
					break;
				}
			}
			if (num == -1)
			{
				Transform transform = _effectContainer.transform;
				UIEffect uIEffect2 = null;
				int k = 0;
				for (int childCount = transform.childCount; k < childCount; k++)
				{
					Component component = ((Component)transform.GetChild(k)).GetComponent(type);
					if ((Object)(object)component != (Object)null)
					{
						uIEffect2 = component as UIEffect;
						break;
					}
				}
				if ((Object)(object)uIEffect2 == (Object)null)
				{
					return null;
				}
				num = _effectsBase.Count;
				_effectsBase.Add(new KeyValuePair<Type, UIEffect>(type, uIEffect2));
				_effectsPool.Add(new KeyValuePair<Type, Stack<UIEffect>>(type, new Stack<UIEffect>()));
			}
			UIEffect value = _effectsBase[num].Value;
			uIEffect = ((Component)this).gameObject.AddChild(((Component)value).gameObject).GetComponent<UIEffect>();
			uIEffect.Disabled = UIEffect_Disabled;
		}
		return uIEffect;
	}

	private void UIEffect_Disabled(UIEffect uiEffect)
	{
		Type type = ((object)uiEffect).GetType();
		int i = 0;
		for (int count = _effectsPool.Count; i < count; i++)
		{
			if ((object)_effectsPool[i].Key == type)
			{
				_effectsPool[i].Value.Push(uiEffect);
				break;
			}
		}
	}
}
