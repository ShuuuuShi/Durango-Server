using System;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class ItemTagWidget : UIWidget
{
	[SerializeField]
	private SelectableWidget _selectable;

	[SerializeField]
	private UILabel _buttonText;

	[SerializeField]
	private UISprite _buttonSprite;

	[SerializeField]
	private int _padding;

	[SerializeField]
	private int _margin;

	public void Set(string title, Action callback)
	{
		_buttonText.text = title;
		_selectable.Clicked = callback;
		base.width = _buttonText.width + _buttonSprite.width + _margin * 2 + _padding;
		_buttonSprite.transform.localPosition = _buttonSprite.transform.localPosition.WithX(localCorners[2].x - (float)_margin - (float)_buttonSprite.width);
	}
}
