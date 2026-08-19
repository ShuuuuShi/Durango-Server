using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class IndicatorLabel : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _background;

	public MapIndicator Indicator { get; private set; }

	public void UpdatePosition(Vector2 offset)
	{
		base.transform.localPosition = Singleton<MapContext>.Instance().TileToMapPosition(Indicator.GetTile()) + offset - new Vector2(_background.transform.localPosition.x, _background.transform.localPosition.y);
	}

	public void Set(MapIndicator indicator, SpriteData spriteData, string text)
	{
		Indicator = indicator;
		spriteData.Set(_icon);
		_text.text = text;
		_background.UpdateAnchors();
	}
}
