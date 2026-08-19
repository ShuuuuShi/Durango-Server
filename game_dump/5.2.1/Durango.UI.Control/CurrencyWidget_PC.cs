using System;
using UnityEngine;

namespace Durango.UI.Control;

[ExecuteInEditMode]
public class CurrencyWidget_PC : CurrencyWidgetBase
{
	public Action LayoutUpdated;

	public int ReferenceCount { get; set; }

	protected override bool MakeComponent()
	{
		if (_presetPrefab == null)
		{
			return false;
		}
		if (_component == null)
		{
			_component = UnityEngine.Object.Instantiate(_presetPrefab.gameObject, base.transform).GetComponent<PresetCurrencyWidget>();
			_component.Init();
			PresetCurrencyWidget component = _component;
			component.LayoutUpdated = (Action)Delegate.Combine(component.LayoutUpdated, (Action)delegate
			{
				UIWidget component2 = GetComponent<UIWidget>();
				component2.SetDimensions(_component.width, _component.height);
				_component.SetDimensions(component2.width, component2.height);
				_component.UpdateAnchors();
				if (LayoutUpdated != null)
				{
					LayoutUpdated();
				}
				_component.SetDimensions(component2.width, component2.height);
				_component.UpdateAnchors();
			});
			_component.HideExtraButton(_hideExtraButton);
		}
		Refresh();
		if (LayoutUpdated != null)
		{
			LayoutUpdated();
		}
		return true;
	}
}
