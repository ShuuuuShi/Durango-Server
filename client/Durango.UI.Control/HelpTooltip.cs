using Durango.UI.Popup;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class HelpTooltip : UISprite, ITextLinkWithValue, ITextLink
{
	private string _text;

	private ParamsDictionary _params;

	protected override void Awake()
	{
		base.Awake();
		onChange = OnChange;
	}

	private void OnDestroy()
	{
		onChange = null;
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (string.IsNullOrEmpty(_text))
		{
			return;
		}
		string title = null;
		string text = null;
		int num = 400;
		TooltipBase.TooltipDirection tooltipDirection = TooltipBase.TooltipDirection.Vertical;
		float num2 = 10f;
		int num3 = 0;
		if (_params == null)
		{
			_params = ParamsDictionary.MakeParams(_text);
		}
		if (_params != null)
		{
			string text2 = _params.Get("uri");
			if (!string.IsNullOrEmpty(text2))
			{
				UIUtility.OpenUri(string.Empty, text2);
				return;
			}
			title = _params.Get("title");
			text = _params.Get("comment");
			num = _params.GetInt("width", num);
			tooltipDirection = _params.GetEnum("direction", tooltipDirection);
			num2 = _params.GetFloat("duration", num2);
			num3 = _params.GetInt("sign", num3);
		}
		else
		{
			text = _text;
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Direction = tooltipDirection;
		widgetTooltipControl.Sign = num3;
		widgetTooltipControl.Set(title, text, num);
		widgetTooltipControl.Show(this, Vector2.zero, num2);
	}

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		_text = text;
		_params = null;
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		SetDimensions(size, size);
		return default(LinkLayoutOption);
	}

	private void OnChange()
	{
		if (_text != null && _text.Contains("resize_collider"))
		{
			UIWidget uIWidget = UIUtility.FindComponentInParent<UIWidget>(base.gameObject);
			if (uIWidget != null)
			{
				BoxCollider component = GetComponent<BoxCollider>();
				component.size = uIWidget.localSize;
				component.center = uIWidget.localCenter - base.transform.localPosition;
			}
		}
	}
}
