using UnityEngine;

namespace Durango.UI;

public class FatigueMomentumReason : MonoBehaviour
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _iconSprite;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void Set(string text, string iconSprite)
	{
		_textLabel.text = text;
		_iconSprite.spriteName = iconSprite;
	}
}
