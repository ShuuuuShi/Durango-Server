using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class SlotSourceItem : UIWidget
{
	[SerializeField]
	private UISpriteLabel _textLabel;

	private int _padding;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_padding = base.height - _textLabel.height;
		}
	}

	public int Set(string text, int limitTextWidth)
	{
		Init();
		_textLabel.overflowWidth = limitTextWidth;
		_textLabel.text = text;
		base.height = _textLabel.height + _padding;
		return _textLabel.width + (int)(_textLabel.transform.localPosition.x + 20f);
	}
}
