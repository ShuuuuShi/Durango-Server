using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class CurrencyWidgetTweakerForPC : MonoBehaviour
{
	private void Awake()
	{
		UITitle componentInParent = GetComponentInParent<UITitle>();
		if (componentInParent == null)
		{
			return;
		}
		CurrencyWidgetBase[] componentsInChildren = componentInParent.GetComponentsInChildren<CurrencyWidgetBase>();
		PresetCurrencyWidget[] componentsInChildren2 = componentInParent.GetComponentsInChildren<PresetCurrencyWidget>();
		List<MonoBehaviour> list = new List<MonoBehaviour>();
		list.AddRange(componentsInChildren);
		list.AddRange(componentsInChildren2);
		foreach (MonoBehaviour item in list)
		{
			if (!(item == null))
			{
				PortraitModeAnchor componentInParent2 = item.GetComponentInParent<PortraitModeAnchor>();
				if (componentInParent2 != null)
				{
					componentInParent2.gameObject.SetActive(value: false);
				}
				else if (item != null)
				{
					item.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
