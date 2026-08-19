using UnityEngine;

namespace Durango.UI.Control;

public class UnorderedListText : UIWidget, ITextLinkWithValue, ITextLink
{
	[SerializeField]
	private UIWidget _prefixWidget;

	[SerializeField]
	private UISprite _circle;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private RectLayout _layout;

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		_textLabel.text = text;
	}

	public override void Invalidate(bool includeChildren)
	{
		base.Invalidate(includeChildren);
		Color color = new Color(this.color.r, this.color.g, this.color.b);
		_circle.color = color;
		_textLabel.color = color;
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		_textLabel.fontSize = size;
		_prefixWidget.height = size;
		_textLabel.overflowWidth = ((builder.Width < 1000000) ? Mathf.Max(0, builder.Width - _prefixWidget.width) : 0);
		_textLabel.ProcessText();
		_layout.UpdateLayout();
		LinkLayoutOption result = default(LinkLayoutOption);
		result.IsSingle = true;
		return result;
	}
}
