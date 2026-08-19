using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIToggle))]
[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Toggled Components")]
public class UIToggledComponents : MonoBehaviour
{
	public List<MonoBehaviour> activate;

	public List<MonoBehaviour> deactivate;

	[HideInInspector]
	[SerializeField]
	private MonoBehaviour target;

	[SerializeField]
	[HideInInspector]
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
		if (((Behaviour)this).enabled)
		{
			for (int i = 0; i < activate.Count; i++)
			{
				MonoBehaviour val = activate[i];
				((Behaviour)val).enabled = UIToggle.current.value;
			}
			for (int j = 0; j < deactivate.Count; j++)
			{
				MonoBehaviour val2 = deactivate[j];
				((Behaviour)val2).enabled = !UIToggle.current.value;
			}
		}
	}
}
