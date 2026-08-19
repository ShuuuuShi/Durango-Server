using Durango.Logic.Item;
using Durango.Utils.Extensions;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Control;

public class TagDecoration : UIWidget, ITextLinkWithValue, ITextLink
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private int _hPadding;

	[SerializeField]
	private int _vPadding;

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		string[] array = text.Split(',');
		Tag tag = SingletonDict<string, Tag>.Instance.Get(array[0]);
		if (tag == null)
		{
			_label.text = text;
			return;
		}
		int num = ((array.Length > 1) ? array[1].ToInt() : 0);
		Color gradeColor = TagData.GetGradeColor(tag.Grade);
		_label.text = ((num <= 0) ? tag.Name.ToString() : $"{tag.Name} {LocalizeUtil.FormatLevel(num)}");
		_label.color = gradeColor;
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		_label.fontSize = size;
		SetDimensions(_label.width + _hPadding, size);
		_background.SetDimensions(base.width, base.height + _vPadding);
		return default(LinkLayoutOption);
	}
}
