using UnityEngine;

namespace Durango.UI.Control;

[ExecuteInEditMode]
public class CurrencyWidget : CurrencyWidgetBase
{
	protected override bool MakeComponent()
	{
		if (_presetPrefab == null)
		{
			return false;
		}
		if (_component == null)
		{
			_component = Object.Instantiate(_presetPrefab.gameObject, base.transform).GetComponent<PresetCurrencyWidget>();
			_component.Init();
			_component.HideExtraButton(_hideExtraButton);
		}
		Refresh();
		return true;
	}
}
