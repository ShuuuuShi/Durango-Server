using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class LinkText : UIWidget, ITextLinkWithValue, ITextLink
{
	private static readonly char[] Separator = new char[1] { ',' };

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	protected RectLayout _layout;

	private string _link;

	private void Set(string text)
	{
		_textLabel.text = text;
	}

	protected void SetFontSize(int size)
	{
		_textLabel.fontSize = size;
		_textLabel.ProcessText();
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (!string.IsNullOrEmpty(_link))
		{
			UIUtility.OpenUri(_textLabel.text, _link);
		}
	}

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		string[] array = text.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 2)
		{
			Set(text);
			_link = null;
		}
		else
		{
			Set(array[1].Trim());
			_link = array[0].Trim();
		}
	}

	public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)
	{
		SetFontSize(size);
		int num = size + 12;
		_layout.UpdateLayout(0f, num);
		UIUtility.UpdateAnchors(base.transform);
		LinkLayoutOption result = default(LinkLayoutOption);
		result.Offset = -6f;
		return result;
	}
}
