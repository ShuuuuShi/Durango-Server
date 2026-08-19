using System;
using System.Collections.Generic;
using Durango.System;
using UnityEngine;

namespace Durango.UI.Control;

[ResourcePath("selectable_button_style")]
public class SelectableButtonStyle : ResourceSingleton<SelectableButtonStyle>
{
	[Serializable]
	[EnumType(typeof(PresetButton.Style))]
	private class StyleList : EnumKeyList
	{
		[SerializeField]
		private List<PresetButton> _values;

		public PresetButton Get(PresetButton.Style style)
		{
			int num = IndexOf((int)style);
			if (num == -1)
			{
				return null;
			}
			return _values[num];
		}
	}

	[Serializable]
	[EnumType(typeof(PresetButton.Effect))]
	private class EffectList : EnumKeyList
	{
		[SerializeField]
		private List<EffectWidget> _values;

		public EffectWidget Get(PresetButton.Effect effect)
		{
			int num = IndexOf((int)effect);
			if (num == -1)
			{
				return null;
			}
			return _values[num];
		}
	}

	[SerializeField]
	private StyleList _style;

	[SerializeField]
	private StyleList _style_PC;

	[SerializeField]
	private EffectList _effect;

	public static PresetButton GetStyle(PresetButton.Style style)
	{
		SelectableButtonStyle selectableButtonStyle = ResourceSingleton<SelectableButtonStyle>.Instance();
		if (selectableButtonStyle != null)
		{
			return selectableButtonStyle.GetStyle(Platform.Instance.UIType).Get(style);
		}
		return null;
	}

	public static EffectWidget GetEffect(PresetButton.Effect effect)
	{
		SelectableButtonStyle selectableButtonStyle = ResourceSingleton<SelectableButtonStyle>.Instance();
		if (selectableButtonStyle != null)
		{
			return selectableButtonStyle._effect.Get(effect);
		}
		return null;
	}

	private StyleList GetStyle(UIPrefabMap.Type type)
	{
		return type switch
		{
			UIPrefabMap.Type.Mobile => _style, 
			UIPrefabMap.Type.PC => _style_PC, 
			_ => null, 
		};
	}
}
