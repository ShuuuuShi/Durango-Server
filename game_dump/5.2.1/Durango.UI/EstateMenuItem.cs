using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EstateMenuItem : SelectableWidget
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _textLabel;

	public EstateMenuWidget.Menu Menu { get; set; }

	public void Set(string icon, string text)
	{
		_iconSprite.spriteName = icon;
		_textLabel.text = text;
	}
}
