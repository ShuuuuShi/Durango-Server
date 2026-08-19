using UnityEngine;

public class IndicatorLabel : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _background;

	public MapIndicator Indicator { get; private set; }

	public void Set(MapIndicator indicator, SpriteData spriteData, string text)
	{
		Indicator = indicator;
		spriteData.Set(_icon);
		_text.text = text;
		_background.UpdateAnchors();
	}
}
