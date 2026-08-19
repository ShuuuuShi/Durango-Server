using Durango.Logic.Notification;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class IconTabWidget : SelectableWidget
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	protected UILabel _textLabel;

	[SerializeField]
	private UISprite _notification;

	[CanBeNull]
	[SerializeField]
	private GameObject _verticalSeparator;

	[CanBeNull]
	[SerializeField]
	private GameObject _horizontalSeparator;

	public void Set(string icon, SyncString text)
	{
		if (_textLabel != null)
		{
			_textLabel.SetText(text);
		}
		if (_iconSprite != null)
		{
			_iconSprite.spriteName = icon;
			UIUtility.ResizeToSquare(_iconSprite);
		}
	}

	public void NotifiactionOn(bool on, Type type)
	{
		if (!(_notification == null))
		{
			if (on)
			{
				_notification.gameObject.SetActive(value: true);
				_notification.color = Notification.GetTypeColor(type);
			}
			else
			{
				_notification.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetDirection(UIScrollView.Movement movement)
	{
		if (movement == UIScrollView.Movement.Horizontal)
		{
			if (_verticalSeparator != null)
			{
				_verticalSeparator.SetActive(value: false);
			}
			if (_horizontalSeparator != null)
			{
				_horizontalSeparator.SetActive(value: true);
			}
		}
		else
		{
			if (_verticalSeparator != null)
			{
				_verticalSeparator.SetActive(value: true);
			}
			if (_horizontalSeparator != null)
			{
				_horizontalSeparator.SetActive(value: false);
			}
		}
	}
}
