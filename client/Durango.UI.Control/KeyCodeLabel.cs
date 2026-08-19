using System;
using UnityEngine;

namespace Durango.UI.Control;

public class KeyCodeLabel : UIWidget, ITextLinkWithValue, ITextLink
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _background;

	[EnumList(typeof(UIButtonColor.State), false, 0, -1)]
	[SerializeField]
	private Color[] _labelColor;

	[EnumList(typeof(UIButtonColor.State), false, 0, -1)]
	[SerializeField]
	private Color[] _backgroundColor;

	[SerializeField]
	private int _hPadding;

	[SerializeField]
	private int _vPadding;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private int _minHeight;

	[SerializeField]
	private Vector2 _backgroundOffset;

	[SerializeField]
	private int _fontSize;

	[SerializeField]
	private KeyCode _keyCode;

	[SerializeField]
	private bool _textMode;

	[SerializeField]
	private bool _useIcon;

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		try
		{
			_keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), text);
			string text2 = InputKeyboard.KeyToCaption(_keyCode);
			if (_useIcon)
			{
				string text3 = $"key_{_keyCode.ToString().ToLower()}_icon_pc";
				if (ResourceSingleton<UISpriteManager>.Instance().TryGet(text3, out var _, out var _))
				{
					text2 = $"[icon={text3}]";
				}
			}
			_label.text = ((!_textMode) ? text2 : $"[ {text2} ]");
		}
		catch (ArgumentException)
		{
			_label.text = ((!_textMode) ? text : $"[ {text} ]");
		}
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		int num = ((_fontSize <= 0) ? size : _fontSize);
		_label.fontSize = num;
		int num2 = num;
		if (_textMode)
		{
			SetDimensions(_label.width, num);
			if (_background != null)
			{
				_background.gameObject.SetActive(value: false);
			}
			_label.color = color;
		}
		else
		{
			_label.color = _labelColor[0];
			num2 = Mathf.Max(num + _vPadding, _minHeight);
			SetDimensions(Mathf.Max(_label.width + _hPadding, _minWidth), num2);
			Point2 point = new Point2(base.width, num2);
			Vector3 one = Vector3.one;
			if (_background != null)
			{
				if (_background.type != 0)
				{
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
					}
				}
				_background.SetDimensions(point.x, point.y);
				_background.transform.localScale = one;
			}
			_label.transform.localPosition = _backgroundOffset;
		}
		LinkLayoutOption result = default(LinkLayoutOption);
		result.Offset = (float)(size - num2) * 0.5f;
		return result;
	}

	public void OnHover(bool hovered)
	{
		if (!_textMode)
		{
			_label.color = _labelColor[hovered ? 1 : 0];
			if (_background != null)
			{
				_background.color = _backgroundColor[hovered ? 1 : 0];
			}
		}
	}

	public void OnPress(bool pressed)
	{
		if (!_textMode)
		{
			_label.color = _labelColor[pressed ? 2 : 0];
			if (_background != null)
			{
				_background.color = _backgroundColor[pressed ? 2 : 0];
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!_textMode)
		{
			_label.color = _labelColor[0];
			if (_background != null)
			{
				_background.color = _backgroundColor[0];
			}
		}
	}
}
