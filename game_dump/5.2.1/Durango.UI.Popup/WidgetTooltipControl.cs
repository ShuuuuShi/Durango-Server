using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class WidgetTooltipControl : TooltipBase
{
	[SerializeField]
	private KeyValueLabel _titleLabel;

	[SerializeField]
	private UIWidget _spaceWidget;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private RectLayout _layout;

	private SyncString _title;

	private SyncString _subtitle;

	private SyncString _text;

	private int _maxWidth;

	private int _minWidth;

	private const int CommentPadding = 40;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		SoundType = UISound.GroupType.NoSound;
	}

	public void Set(string title, string text, int maxWidth = 0, int minWidth = 0)
	{
		Set(title, string.Empty, text, maxWidth, minWidth);
	}

	public void Set(SyncString title, SyncString subtitle, SyncString text, int maxWidth = 0, int minWidth = 0)
	{
		_title = title;
		_subtitle = subtitle;
		_text = text;
		_minWidth = minWidth;
		_maxWidth = ((maxWidth <= 0) ? UIManager.SafeWidth : maxWidth);
	}

	private bool HasTitle()
	{
		if (!_title.HasText())
		{
			return _subtitle.HasText();
		}
		return true;
	}

	private bool HasText()
	{
		return _text.HasText();
	}

	protected override void FillData()
	{
		if (HasTitle())
		{
			_titleLabel.gameObject.SetActive(value: true);
			_titleLabel.Set(_title, _subtitle);
		}
		else
		{
			_titleLabel.gameObject.SetActive(value: false);
		}
		if (HasText())
		{
			_textLabel.overflowWidth = ((_maxWidth > 40) ? (_maxWidth - 40) : 0);
			_textLabel.SetText(_text);
			_textLabel.gameObject.SetActive(value: true);
		}
		else
		{
			_textLabel.gameObject.SetActive(value: false);
		}
	}

	protected override void UpdateLayout()
	{
		bool flag = HasTitle();
		bool flag2 = HasText();
		float num = 0f;
		float num2 = 0f;
		if (flag2)
		{
			num2 = _textLabel.printedSize.x + 40f;
		}
		if (flag)
		{
			num = _titleLabel.GetPreferredSize().x;
		}
		float num3 = Mathf.Max(num, num2);
		if (num3 < (float)_minWidth)
		{
			num3 = _minWidth;
			if (flag)
			{
				_titleLabel.UpdateLayout(_minWidth);
			}
		}
		else if (_maxWidth > 0 && num3 > (float)_maxWidth)
		{
			if (flag)
			{
				num = _titleLabel.GetPreferredSize(_maxWidth).x;
			}
			num3 = Mathf.Min(num3, Mathf.Max(num2, num));
			if (flag)
			{
				_titleLabel.UpdateLayout((int)num3);
			}
		}
		else if (flag)
		{
			_titleLabel.UpdateLayout((int)num3);
		}
		_spaceWidget.gameObject.SetActive(flag && flag2);
		_layout.UpdateLayout(num3, null);
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnClickWidget()
	{
		Hide();
	}
}
