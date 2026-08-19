using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class CommodityPreviewMotionWidget : UIWidget
{
	[SerializeField]
	private SelectableWidget _selectable;

	[SerializeField]
	private UILabel _buttonText;

	[SerializeField]
	private int _margin;

	public void Set(string title, Action callback)
	{
		_buttonText.text = title;
		_selectable.Clicked = callback;
		base.width = _buttonText.width + _margin * 2;
	}
}
