using System;
using Durango.Logic.Notification;
using Durango.Logic.Social;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class MotionWidget : UIWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private SelectableWidget _selectableWidget;

	[CanBeNull]
	[SerializeField]
	private NotificationControl _notification;

	[CanBeNull]
	[SerializeField]
	private UISprite _byEquipment;

	public void Set([CanBeNull] Durango.Logic.Social.Motion data, [CanBeNull] Action clicked)
	{
		INotificationable notification = null;
		bool flag = false;
		Color color = Color.white;
		float num = 1f;
		if (data == null)
		{
			_textLabel.text = string.Empty;
			num = 0f;
			_selectableWidget.Clicked = null;
		}
		else
		{
			flag = data.IsEquipmentsMotion();
			_textLabel.text = data.Name;
			if (data.IsRare || flag)
			{
				color = PresetColor.UIYellow;
			}
			if (!data.Available)
			{
				color.a = 0.3f;
				num = 0.3f;
			}
			_selectableWidget.Clicked = clicked;
			if (flag)
			{
				num = 0.3f;
			}
			else
			{
				notification = data;
			}
		}
		_textLabel.color = color;
		_background.alpha = num;
		if (_byEquipment != null)
		{
			if (flag)
			{
				_byEquipment.gameObject.SetActive(value: true);
				_byEquipment.color = color;
			}
			else
			{
				_byEquipment.gameObject.SetActive(value: false);
			}
		}
		if (_notification != null)
		{
			_notification.SetNotification(notification);
		}
	}
}
