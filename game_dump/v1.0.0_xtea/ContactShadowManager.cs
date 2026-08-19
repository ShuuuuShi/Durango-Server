using System;
using System.Collections.Generic;
using UnityEngine;

public class ContactShadowManager : KSingleton<ContactShadowManager>
{
	[SerializeField]
	private GameObject _shadowModelPrefab;

	private GameObject _contactShadowPool;

	private readonly List<ContactShadowModel> _shadows = new List<ContactShadowModel>();

	public ContactShadowModel Create(GameObject target, bool isRapidUpdateMode = false, bool destroyIfInvisible = true)
	{
		GameObject val = ((Component)this).gameObject.AddChild(_shadowModelPrefab);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		ContactShadowModel component = val.GetComponent<ContactShadowModel>();
		if ((Object)(object)component == (Object)null)
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
			if ((Object)(object)_shadows[num] != (Object)null && (Object)(object)_shadows[num].Target == (Object)(object)obj)
			{
				Object.Destroy((Object)(object)((Component)_shadows[num]).gameObject);
				_shadows.RemoveAt(num);
				break;
			}
		}
	}

	private void Remove(ContactShadowModel shadow)
	{
		if ((Object)(object)shadow == (Object)null)
		{
			return;
		}
		for (int num = _shadows.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)_shadows[num] == (Object)(object)shadow)
			{
				Object.Destroy((Object)(object)((Component)shadow).gameObject);
				_shadows.RemoveAt(num);
				break;
			}
		}
	}
}
