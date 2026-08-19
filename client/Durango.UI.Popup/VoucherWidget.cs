using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class VoucherWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UILabel _expiry;

	[SerializeField]
	private SelectableButton _button;

	public void Set(string iconName, Color iconColor, string title, string description, string expiry, string count, Action clicked)
	{
		_icon.spriteName = iconName;
		_icon.color = iconColor;
		_title.text = title;
		_description.text = description;
		_button.Text = count;
		_button.Clicked = clicked;
		_button.GetComponent<BoxCollider>().enabled = clicked != null;
		if (string.IsNullOrEmpty(expiry))
		{
			_expiry.gameObject.SetActive(value: false);
			UIEventListener.Get(base.gameObject).onClick = null;
		}
		else
		{
			_expiry.gameObject.SetActive(value: true);
			_expiry.text = expiry;
			UIEventListener.Get(base.gameObject).onClick = Button_Clicked;
		}
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		bool activeSelf = _expiry.gameObject.activeSelf;
		int num = _title.height + 10 + _description.height + 50;
		num += (activeSelf ? (10 + _expiry.height) : 0);
		GetComponent<UIWidget>().height = Math.Max(num, 115);
		if (activeSelf)
		{
			_description.SetPosition(new Vector3(_description.localCenter.x, 0f), 0.5f, 0.5f);
			return;
		}
		Vector3 vector = new Vector3(0f, (float)(10 + _expiry.height) * 0.5f);
		_description.SetPosition(_description.localCenter - vector, 0.5f, 0.5f);
	}

	private void Button_Clicked(GameObject go)
	{
		string text = T._("이용권은 기한이 지나면 사라집니다.");
		UIWidget uIWidget = _title;
		if (uIWidget.transform.childCount > 0)
		{
			UIWidget component = uIWidget.transform.GetChild(0).GetComponent<UIWidget>();
			if (component != null)
			{
				uIWidget = component;
			}
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, text);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(uIWidget, Vector2.zero, 4f);
	}
}
