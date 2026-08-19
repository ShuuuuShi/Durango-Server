using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

[Uri("Currency")]
public class CurrencyGroup : UIBase, IUIInitializable
{
	[SerializeField]
	private CurrencyWidgetList _currencyWidgetList;

	public float Height
	{
		get
		{
			if (_currencyWidgetList == null)
			{
				return 0f;
			}
			UIWidget component = _currencyWidgetList.GetComponent<UIWidget>();
			if (component != null)
			{
				return component.height;
			}
			return 0f;
		}
	}

	void IUIInitializable.Init()
	{
		_currencyWidgetList.gameObject.SetActive(value: false);
		_currencyWidgetList.Opened += delegate(bool isOpen)
		{
			if (isOpen)
			{
				Open();
			}
			else
			{
				Close();
			}
		};
		UIBase[] componentsInChildren = Singleton<UIManager>.Instance().UIRoot.GetComponentsInChildren<UIBase>(includeInactive: true);
		foreach (UIBase comp in componentsInChildren)
		{
			comp.OnOpenSucceed += delegate
			{
				if (comp.CurrencyList != null)
				{
					_currencyWidgetList.Add(comp.CurrencyList);
				}
			};
			comp.OnCloseSucceed += delegate
			{
				if (comp.CurrencyList != null)
				{
					_currencyWidgetList.Remove(comp.CurrencyList);
				}
			};
			comp.CurrencyChanged += delegate(IEnumerable<CurrencyData> from, IEnumerable<CurrencyData> to)
			{
				if (comp.IsOpened)
				{
					_currencyWidgetList.Remove(from);
					_currencyWidgetList.Add(to);
				}
			};
		}
	}
}
