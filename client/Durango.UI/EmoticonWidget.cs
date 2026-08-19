using System;
using Durango.Logic.Social;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EmoticonWidget : UIWidget
{
	[SerializeField]
	private SelectableWidget _selectableWidget;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private NotificationControl _notification;

	public string Key { get; private set; }

	public virtual void Set(Emoticon emoticon, Action clickButton)
	{
		Key = ((emoticon == null) ? string.Empty : emoticon.Key);
		if (emoticon == null)
		{
			_selectableWidget.Clicked = null;
			if (_iconSprite != null)
			{
				_iconSprite.spriteName = "bg_cooltime_circle";
			}
		}
		else
		{
			_selectableWidget.Clicked = clickButton;
			if (_iconSprite != null)
			{
				_iconSprite.spriteName = emoticon.UIIcon;
			}
		}
		if (_notification != null)
		{
			_notification.SetNotification(emoticon);
		}
	}
}
