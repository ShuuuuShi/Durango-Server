using System;
using System.Collections.Generic;
using UnityEngine;

[ResourcePath("selectable_button_style")]
public class DefaultSelectableButtonStyle : ResourceSingleton<DefaultSelectableButtonStyle>
{
	[Serializable]
	public struct StyleMeta
	{
		public SpriteData SelectBorder;

		public SpriteData UnselectBorder;

		public Color SelectBorderColor;

		public Color UnselectBorderColor;

		public Color DisableBorderColor;

		public Color SelectContentsColor;

		public Color UnselectContentsColor;

		public Color DisableContentsColor;
	}

	[Serializable]
	[EnumType(typeof(DefaultSelectableButton.ButtonStyle))]
	private class StyleMetaList : EnumKeyList
	{
		[SerializeField]
		private List<StyleMeta> _values;

		public StyleMeta Get(DefaultSelectableButton.ButtonStyle style)
		{
			int num = IndexOf((int)style);
			if (num != -1)
			{
				return _values[num];
			}
			return default(StyleMeta);
		}
	}

	[SerializeField]
	private StyleMetaList _styleMetas;

	public static StyleMeta Get(DefaultSelectableButton.ButtonStyle style)
	{
		DefaultSelectableButtonStyle defaultSelectableButtonStyle = ResourceSingleton<DefaultSelectableButtonStyle>.Instance();
		if ((Object)(object)defaultSelectableButtonStyle != (Object)null)
		{
			return defaultSelectableButtonStyle._styleMetas.Get(style);
		}
		return default(StyleMeta);
	}
}
