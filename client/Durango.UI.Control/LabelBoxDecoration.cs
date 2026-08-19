using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Control;

public class LabelBoxDecoration : UIWidget, ITextLinkWithValue, ITextLink
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private float _hPaddingRatio;

	[SerializeField]
	private float _vPaddingRatio;

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		ParamsDictionary paramsDictionary = ParamsDictionary.MakeParams(text);
		if (paramsDictionary == null)
		{
			_label.text = text;
			return;
		}
		_label.text = paramsDictionary.Get("text");
		_background.color = paramsDictionary.Get("color").ToColor(_background.color);
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		_label.fontSize = size;
		int num = (int)((float)size * _hPaddingRatio * 2f);
		int num2 = (int)((float)size * _vPaddingRatio * 2f);
		SetDimensions(_label.width + num, size + num2);
		Point2 point = new Point2(base.width, base.height);
		if (_background.type == UIBasicSprite.Type.Simple)
		{
			_background.SetDimensions(point.x, point.y);
		}
		else
		{
			Vector3 one = Vector3.one;
			UISpriteData atlasSprite = _background.GetAtlasSprite();
			if (atlasSprite != null)
			{
				Point2 point2 = new Point2(atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight, atlasSprite.height + atlasSprite.paddingBottom + atlasSprite.paddingTop);
				if (point.x < point2.x)
				{
					one.x = (float)point.x / (float)point2.x;
					point.x = point2.x;
				}
				if (point.y < point2.y)
				{
					one.y = (float)point.y / (float)point2.y;
					point.y = point2.y;
				}
				_background.SetDimensions(point.x, point.y);
			}
			_background.SetDimensions(point.x, point.y);
			_background.transform.localScale = one;
		}
		LinkLayoutOption result = default(LinkLayoutOption);
		result.Offset = (float)(-num2) * 0.5f;
		return result;
	}
}
