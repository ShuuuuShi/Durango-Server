using System;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class StoryViewScrollNode : MonoBehaviour
{
	[Serializable]
	public struct Shape
	{
		public int FontSize;

		public int IconSize;

		public int BgIconSize;

		public Color BgIconColor;

		public Shape Lerp(float value, Shape other)
		{
			int fontSize = (int)Mathf.Lerp(FontSize, other.FontSize, value);
			int iconSize = (int)Mathf.Lerp(IconSize, other.IconSize, value);
			int bgIconSize = (int)Mathf.Lerp(BgIconSize, other.BgIconSize, value);
			Color bgIconColor = Color.Lerp(BgIconColor, other.BgIconColor, value);
			Shape result = default(Shape);
			result.FontSize = fontSize;
			result.IconSize = iconSize;
			result.BgIconSize = bgIconSize;
			result.BgIconColor = bgIconColor;
			return result;
		}
	}

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _icon;

	[EnumList(typeof(Chapter.Kind), false, 0, -1)]
	[SerializeField]
	private SpriteData[] _iconData;

	public void Set(Chapter.Kind kind, int num)
	{
		_label.gameObject.SetActive(kind == Chapter.Kind.Normal);
		_icon.gameObject.SetActive(kind != Chapter.Kind.Normal);
		_iconData[(int)kind].Set(_icon);
		_label.text = num.ToString();
	}

	public void SetShape(Shape shape)
	{
		_label.fontSize = shape.FontSize;
		_icon.width = shape.IconSize;
		_icon.height = shape.IconSize;
		_background.width = shape.BgIconSize;
		_background.height = shape.BgIconSize;
		_background.color = shape.BgIconColor;
	}
}
