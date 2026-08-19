using System;
using UnityEngine;

namespace Durango.UI.Popup;

public class GenericSelectorItem : UIWidget
{
	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private RectLayout _layout;

	public event Action<GenericSelectorItem> Clicked;

	public void Set(string text)
	{
		_textLabel.text = text;
		base.height = Mathf.Max(120, _textLabel.height + 40);
		_layout.UpdateLayout(base.width, base.height);
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Select(bool select)
	{
		_checkSprite.gameObject.SetActive(select);
	}

	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(this);
		}
	}
}
