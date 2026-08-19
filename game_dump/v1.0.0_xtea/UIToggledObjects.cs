using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Toggled Objects")]
public class UIToggledObjects : MonoBehaviour
{
	public List<GameObject> activate;

	public List<GameObject> deactivate;

	[HideInInspector]
	[SerializeField]
	private GameObject target;

	[HideInInspector]
	[SerializeField]
	private bool inverse;

	private void Awake()
	{
		if ((Object)(object)target != (Object)null)
		{
			if (activate.Count == 0 && deactivate.Count == 0)
			{
				if (inverse)
				{
					deactivate.Add(target);
				}
				else
				{
					activate.Add(target);
				}
			}
			else
			{
				target = null;
			}
		}
		UIToggle component = ((Component)this).GetComponent<UIToggle>();
		EventDelegate.Add(component.onChange, Toggle);
	}

	public void Toggle()
	{
		bool value = UIToggle.current.value;
		if (((Behaviour)this).enabled)
		{
			for (int i = 0; i < activate.Count; i++)
			{
				Set(activate[i], value);
			}
			for (int j = 0; j < deactivate.Count; j++)
			{
				Set(deactivate[j], !value);
			}
		}
	}

	private void Set(GameObject go, bool state)
	{
		if ((Object)(object)go != (Object)null)
		{
			NGUITools.SetActive(go, state);
		}
	}
}
