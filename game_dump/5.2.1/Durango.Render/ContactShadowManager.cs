using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class ContactShadowManager : Singleton<ContactShadowManager>
{
	[SerializeField]
	private GameObject _shadowModelPrefab;

	private readonly List<ContactShadowModel> _shadows = new List<ContactShadowModel>();

	public ContactShadowModel Create(GameObject target, bool isRapidUpdateMode = false, bool destroyIfInvisible = true)
	{
		GameObject gameObject = base.gameObject.AddChild(_shadowModelPrefab);
		if (gameObject == null)
		{
			return null;
		}
		ContactShadowModel component = gameObject.GetComponent<ContactShadowModel>();
		if (component == null)
		{
			return null;
		}
		component.Target = target;
		component.IsRapidUpdateMode = isRapidUpdateMode;
		component.DestroyIfInvisible = destroyIfInvisible;
		component.OnRemove = (Action<ContactShadowModel>)Delegate.Combine(component.OnRemove, new Action<ContactShadowModel>(Remove));
		_shadows.Add(component);
		return component;
	}

	public void Remove(GameObject obj)
	{
		for (int num = _shadows.Count - 1; num >= 0; num--)
		{
			if (_shadows[num] != null && _shadows[num].Target == obj)
			{
				UnityEngine.Object.Destroy(_shadows[num].gameObject);
				_shadows.RemoveAt(num);
				break;
			}
		}
	}

	private void Remove(ContactShadowModel shadow)
	{
		if (shadow == null)
		{
			return;
		}
		for (int num = _shadows.Count - 1; num >= 0; num--)
		{
			if (_shadows[num] == shadow)
			{
				UnityEngine.Object.Destroy(shadow.gameObject);
				_shadows.RemoveAt(num);
				break;
			}
		}
	}
}
