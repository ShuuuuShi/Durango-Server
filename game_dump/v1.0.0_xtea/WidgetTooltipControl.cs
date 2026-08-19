using UnityEngine;

public class WidgetTooltipControl : TooltipBase
{
	[SerializeField]
	private UIWidget _widgetArea;

	[SerializeField]
	private UIWidget _textArea;

	[SerializeField]
	private KeyValueLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _textLabel;

	[SerializeField]
	private UIWidget _bg;

	private UIWidget _customWidget;

	private SyncString _title;

	private SyncString _subtitle;

	private string _text;

	private int _maxWidth;

	private int _minWidth;

	private Vector2 _textPadding;

	protected override void OnAwake()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		base.OnAwake();
		_textPadding = Vector2.op_Implicit(((Component)((Component)_titleLabel).transform.FindChild("Key")).GetComponent<UIWidget>().GetPosition(0f, 1f) - _titleLabel.Widget.localCorners[1]);
		_textPadding.y = 0f - _textPadding.y;
	}

	public void Set(string title, string text, int maxWidth = 0, int minWidth = 0)
	{
		Set(null, title, string.Empty, text, maxWidth, minWidth);
	}

	public void Set(SyncString title, SyncString subtitle, string text, int maxWidth = 0, int minWidth = 0)
	{
		Set(null, title, subtitle, text, maxWidth, minWidth);
	}

	public void Set(UIWidget widget, SyncString title, SyncString subtitle, string text, int maxWidth = 0, int minWidth = 0)
	{
		_customWidget = widget;
		_title = title;
		_subtitle = subtitle;
		_text = text;
		_minWidth = minWidth;
		_maxWidth = maxWidth;
	}

	private bool HasTitle()
	{
		return _title.HasText() || _subtitle.HasText();
	}

	protected override void FillData()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_customWidget == (Object)null)
		{
			((Component)_widgetArea).gameObject.SetActive(false);
		}
		else
		{
			for (int num = ((Component)_widgetArea).transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)((Component)_widgetArea).transform.GetChild(num)).gameObject);
			}
			((Component)_widgetArea).gameObject.SetActive(true);
			UIWidget uIWidget = Object.Instantiate<UIWidget>(_customWidget);
			_widgetArea.pivot = _customWidget.pivot;
			_widgetArea.width = _customWidget.width;
			_widgetArea.height = _customWidget.height;
			((Component)uIWidget).transform.parent = ((Component)_widgetArea).transform;
			((Component)uIWidget).transform.localScale = ((Component)_customWidget).transform.localScale;
			((Component)uIWidget).transform.localPosition = Vector3.zero;
			uIWidget.SetAnchor(((Component)_widgetArea).gameObject, 0, 0, 0, 0);
			uIWidget.UpdateAnchors();
			((Component)uIWidget).gameObject.SetActive(true);
			uIWidget.ParentHasChanged();
		}
		if (HasTitle())
		{
			((Component)_titleLabel).gameObject.SetActive(true);
			_titleLabel.Set(_title, _subtitle);
		}
		else
		{
			((Component)_titleLabel).gameObject.SetActive(false);
		}
		if (string.IsNullOrEmpty(_text))
		{
			((Component)_textLabel).gameObject.SetActive(false);
			return;
		}
		((Component)_textLabel).gameObject.SetActive(true);
		_textLabel.text = _text;
	}

	protected override void UpdateLayout()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		if (((Component)_widgetArea).gameObject.activeSelf)
		{
			num = _widgetArea.width;
			num2 = _widgetArea.height;
		}
		Vector2 val = Vector2.zero;
		Vector2 zero = Vector2.zero;
		if (((Component)_textLabel).gameObject.activeSelf)
		{
			_textLabel.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
			zero.x = _textLabel.Label.printedSize.x + _textPadding.x * 2f;
			zero.y = _textLabel.Label.printedSize.y - (float)_textLabel.Label.spacingY + _textPadding.y * 2f;
		}
		if (((Component)_titleLabel).gameObject.activeSelf)
		{
			val = _titleLabel.GetPredictSize();
			zero.y -= _textPadding.y;
		}
		float num3 = num + Mathf.Max(val.x, zero.x);
		if (num3 < (float)_minWidth)
		{
			if (((Component)_titleLabel).gameObject.activeSelf)
			{
				_titleLabel.UpdateLayout(_minWidth);
				val = _titleLabel.Widget.localSize;
			}
		}
		else if (_maxWidth > 0 && num3 > (float)_maxWidth)
		{
			num3 = _maxWidth;
			float num4 = num3 - num;
			if (((Component)_textLabel).gameObject.activeSelf)
			{
				_textLabel.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
				_textLabel.Label.width = (int)(num4 - _textPadding.x * 2f);
				zero.x = _textLabel.Label.printedSize.x + _textPadding.x * 2f;
				zero.y = _textLabel.Label.printedSize.y - (float)_textLabel.Label.spacingY + _textPadding.y * 2f;
			}
			if (((Component)_titleLabel).gameObject.activeSelf)
			{
				_titleLabel.UpdateLayout((!(zero.x > 0f)) ? ((int)num4) : ((int)Mathf.Max((float)_minWidth, Mathf.Min(zero.x, num4))));
				val = _titleLabel.Widget.localSize;
				zero.y -= _textPadding.y;
			}
		}
		else if (((Component)_titleLabel).gameObject.activeSelf)
		{
			_titleLabel.UpdateLayout((int)Mathf.Max(zero.x, val.x));
			val = _titleLabel.Widget.localSize;
		}
		_textArea.width = (int)Mathf.Max(val.x, zero.x);
		num2 = Mathf.Max(num2, val.y + zero.y);
		base.Widget.width = (int)num + _textArea.width;
		base.Widget.height = (int)num2;
		_bg.UpdateAnchors();
		Reposition();
	}

	private void Reposition()
	{
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		if (((Component)_widgetArea).gameObject.activeSelf)
		{
			int width = _widgetArea.width;
			_widgetArea.leftAnchor.target = transform;
			_widgetArea.leftAnchor.relative = 0f;
			_widgetArea.leftAnchor.absolute = 0;
			_widgetArea.rightAnchor.target = transform;
			_widgetArea.rightAnchor.relative = 0f;
			_widgetArea.rightAnchor.absolute = width;
			_widgetArea.topAnchor.target = transform;
			_widgetArea.topAnchor.relative = 1f;
			_widgetArea.topAnchor.absolute = 0;
			_widgetArea.bottomAnchor.target = transform;
			_widgetArea.bottomAnchor.relative = 0f;
			_widgetArea.bottomAnchor.absolute = 0;
			_widgetArea.UpdateAnchors();
		}
		int absolute = 0;
		if (((Component)_widgetArea).gameObject.activeSelf)
		{
			absolute = _widgetArea.width;
		}
		_textArea.leftAnchor.target = transform;
		_textArea.leftAnchor.relative = 0f;
		_textArea.leftAnchor.absolute = absolute;
		_textArea.rightAnchor.target = transform;
		_textArea.rightAnchor.relative = 1f;
		_textArea.rightAnchor.absolute = 0;
		_textArea.topAnchor.target = transform;
		_textArea.topAnchor.relative = 1f;
		_textArea.topAnchor.absolute = 0;
		_textArea.bottomAnchor.target = transform;
		_textArea.bottomAnchor.relative = 0f;
		_textArea.bottomAnchor.absolute = 0;
		_textArea.UpdateAnchors();
		if (((Component)_titleLabel).gameObject.activeSelf)
		{
			Vector3 position = _titleLabel.Widget.GetPosition(0f, 0f);
			((Component)_textLabel).transform.localPosition = position + Vector3.right * _textPadding.x;
		}
		else
		{
			Vector3 val = _textArea.localCorners[1];
			((Component)_textLabel).transform.localPosition = val + new Vector3(_textPadding.x, 0f - _textPadding.y);
		}
	}
}
