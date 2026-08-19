using UnityEngine;

namespace Durango.UI.Control;

public class PresetButton : SelectableWidget
{
	public enum Style
	{
		Solid,
		SolidSmall,
		SolidPopup,
		Border,
		BorderSmall,
		BorderPopup,
		Alert,
		Flat,
		FlatPopup
	}

	public enum Effect
	{
		None,
		Emphasis
	}

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private int _leftPadding = 10;

	[SerializeField]
	private int _rightPadding = 10;

	[SerializeField]
	private int _bottomPadding = 10;

	[SerializeField]
	private int _topPadding = 10;

	[SerializeField]
	private UISound.ClickType _clickSoundType;

	private int _iconSize;

	private Point2 _prevSize;

	public UISound.ClickType ClickSoundType => _clickSoundType;

	protected override void OnInit()
	{
		base.OnInit();
		base.Widget.AddOnChange(OnResized);
	}

	private void OnResized()
	{
		Point2 point = new Point2(base.Widget.width, base.Widget.height);
		if (point != _prevSize)
		{
			_prevSize = point;
			UpdateContentsSize();
		}
	}

	private void UpdateContentsSize()
	{
		int num = base.Widget.width - _leftPadding - _rightPadding;
		int num2 = base.Widget.height - _bottomPadding - _topPadding;
		Vector3[] localCorners = base.Widget.localCorners;
		Vector3 pos = Vector3.Lerp(localCorners[0] + new Vector3(_leftPadding, _bottomPadding), localCorners[2] - new Vector3(_rightPadding, _topPadding), 0.5f);
		if (_textLabel != null)
		{
			_textLabel.SetDimensions(num, num2);
			_textLabel.SetPosition(pos, 0.5f, 0.5f);
		}
		if (_iconSprite != null)
		{
			int iconSize = _iconSize;
			int num3 = Mathf.Min(num, num2);
			iconSize = ((iconSize <= 0) ? num3 : Mathf.Min(iconSize, num3));
			if (iconSize > 0)
			{
				UIUtility.ResizeToSquare(_iconSprite, iconSize);
			}
			_iconSprite.SetPosition(pos, 0.5f, 0.5f);
		}
	}

	public bool SetText(string text, int fontSize = 0)
	{
		if (_textLabel == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(text))
		{
			_textLabel.gameObject.SetActive(value: true);
			if (_iconSprite != null)
			{
				_iconSprite.gameObject.SetActive(value: false);
			}
		}
		_textLabel.text = text;
		if (fontSize > 0)
		{
			_textLabel.fontSize = fontSize;
		}
		UpdateContentsSize();
		return true;
	}

	public bool SetIcon(string icon, int size)
	{
		if (_iconSprite == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(icon))
		{
			_iconSprite.gameObject.SetActive(value: true);
			if (_textLabel != null)
			{
				_textLabel.gameObject.SetActive(value: false);
			}
		}
		_iconSprite.spriteName = icon;
		_iconSize = size;
		UpdateContentsSize();
		return true;
	}

	public Point2 GetPreferredSize(int minWidth, int maxWidth, int minHeight, int maxHeight)
	{
		if (_textLabel != null && _textLabel.gameObject.activeSelf)
		{
			Vector2 localSize = _textLabel.localSize;
			UILabel.Overflow overflowMethod = _textLabel.overflowMethod;
			_textLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			Vector2 printedSize = _textLabel.printedSize;
			_textLabel.overflowMethod = overflowMethod;
			_textLabel.SetDimensions((int)localSize.x, (int)localSize.y);
			int num = (int)(printedSize.x + (float)_leftPadding + (float)_rightPadding);
			int num2 = (int)(printedSize.y + (float)_bottomPadding + (float)_topPadding);
			num = Mathf.Clamp(num, (minWidth <= 0) ? num : minWidth, (maxWidth <= 0) ? num : maxWidth);
			num2 = Mathf.Clamp(num2, (minHeight <= 0) ? num2 : minHeight, (maxHeight <= 0) ? num2 : maxHeight);
			return new Point2(num, num2);
		}
		if (_iconSprite != null && _iconSprite.gameObject.activeSelf && _iconSize > 0)
		{
			return new Point2(_iconSize + _leftPadding + _rightPadding, _iconSize + _topPadding + _bottomPadding);
		}
		return new Point2(base.Widget.width, base.Widget.height);
	}
}
