using System.Collections.Generic;
using Shared.Ability;
using UnityEngine;

public class AbilityWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _labels;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void Set(IList<KeyValuePair<Basic, string>> abilities)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		int num = abilities?.Count ?? 0;
		_labels.Set(num);
		for (int i = 0; i < num; i++)
		{
			KeyValueLabel component = _labels[i].GetComponent<KeyValueLabel>();
			string text = LocalizeUtil.Get(abilities[i].Key);
			string value = abilities[i].Value;
			component.Set(text, value);
			component.UpdateLayout(Widget.width);
		}
		float num2 = _labels.Reposition(Vector3.down);
		Widget.height = (int)num2;
	}
}
