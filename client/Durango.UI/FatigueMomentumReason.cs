using UnityEngine;

namespace Durango.UI;

public class FatigueMomentumReason : MonoBehaviour
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _iconSprite;

	private UIWidget _widget;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public void Set(string text, string iconSprite)
	{
		_textLabel.text = text;
		_iconSprite.spriteName = iconSprite;
	}
}
