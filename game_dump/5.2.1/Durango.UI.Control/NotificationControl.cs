using Durango.Logic.Notification;
using UnityEngine;

namespace Durango.UI.Control;

public class NotificationControl : MonoBehaviour
{
	[SerializeField]
	private UISprite _sprite;

	[SerializeField]
	private bool _applyColor = true;

	[SerializeField]
	private CountableNotificationLabel _countable;

	private Notification _notification;

	private void OnEnable()
	{
		Notification_Changed();
	}

	public void SetNotification(Notification notification)
	{
		if (_notification != null)
		{
			_notification.Changed -= Notification_Changed;
		}
		_notification = notification;
		if (_notification != null)
		{
			_notification.Changed += Notification_Changed;
		}
		Notification_Changed();
	}

	public void SetNotification(INotificationable notificationable)
	{
		SetNotification(notificationable?.Notification);
	}

	private void Notification_Changed()
	{
		if (_countable != null)
		{
			_countable.Set((_notification != null) ? _notification.Count : 0);
		}
		else if (_sprite != null)
		{
			bool flag = _notification != null && _notification.On;
			_sprite.gameObject.SetActive(flag);
			if (_applyColor && flag)
			{
				_sprite.color = Notification.GetTypeColor(_notification.Type);
			}
		}
	}
}
