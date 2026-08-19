using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MenuWidget : SelectableWidget
{
	[SerializeField]
	private UISprite _menuIcon;

	[SerializeField]
	private UILabel _menuLabel;

	[SerializeField]
	private GameObject _parentIcon;

	[SerializeField]
	private GameObject _selection;

	[SerializeField]
	private UISprite _toggleNotification;

	[SerializeField]
	private CountableNotificationLabel _countableNotification;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	private Notification _notification;

	public MenuType Type { get; private set; }

	public bool NotificationOn { get; private set; }

	public Type NotificationType { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	protected override void OnRefresh(State state)
	{
		if (_selection != null)
		{
			_selection.SetActive(state == State.Selected);
		}
		RefreshParentIcon(state);
		base.OnRefresh(state);
	}

	public void Set(MenuType type)
	{
		Type = type;
		base.Disabled = false;
		SetMenuText(type.GetName());
		SetMenuIcon(IconMap.Get(type));
		RefreshParentIcon(State.Normal);
		SetNotification(MenuHelper.GetNotificationable(type)?.Notification);
	}

	public void Set(string text)
	{
		base.Disabled = false;
		SetMenuText(text);
	}

	public void PlayTweener(float delay)
	{
		if (_tweenerPlayer != null)
		{
			_tweenerPlayer.Play(delay);
		}
	}

	private void SetNotification(Notification notification)
	{
		if (_notification != notification)
		{
			if (_notification != null)
			{
				_notification.Changed -= UpdateNotification;
			}
			_notification = notification;
			if (_notification != null)
			{
				_notification.Changed += UpdateNotification;
			}
		}
		UpdateNotification();
	}

	private void UpdateNotification()
	{
		if (IsRecentlyUnlocked(Type))
		{
			if (_toggleNotification != null)
			{
				_toggleNotification.gameObject.SetActive(value: true);
			}
			if (_countableNotification != null)
			{
				int count = ((!(_toggleNotification != null)) ? 1 : 0);
				_countableNotification.Set(count);
			}
			NotificationOn = true;
			NotificationType = Durango.Logic.Notification.Type.Important;
			SetNotificationColor(NotificationType);
			return;
		}
		if (_notification == null)
		{
			if (_toggleNotification != null)
			{
				_toggleNotification.gameObject.SetActive(value: false);
			}
			if (_countableNotification != null)
			{
				_countableNotification.Set(0);
			}
			NotificationOn = false;
			NotificationType = Durango.Logic.Notification.Type.Normal;
			return;
		}
		switch (_notification.ViewType)
		{
		case ViewType.Toggle:
			if (_toggleNotification != null)
			{
				_toggleNotification.gameObject.SetActive(_notification.Count > 0);
				if (_countableNotification != null)
				{
					_countableNotification.Set(0);
				}
			}
			else if (_countableNotification != null)
			{
				_countableNotification.Set(_notification.Count);
			}
			break;
		case ViewType.Count:
			if (_countableNotification != null)
			{
				_countableNotification.Set(_notification.Count);
				if (_toggleNotification != null)
				{
					_toggleNotification.gameObject.SetActive(value: false);
				}
			}
			else if (_toggleNotification != null)
			{
				_toggleNotification.gameObject.SetActive(_notification.Count > 0);
			}
			break;
		}
		SetNotificationColor(_notification.Type);
		NotificationOn = _notification.On;
		NotificationType = _notification.Type;
	}

	private static bool IsRecentlyUnlocked(MenuType type)
	{
		if (MenuContainer.HasChildren(type))
		{
			return MenuContainer.GetChildren(type).Any((MenuType c) => GameSystem<MenuSystem>.Instance().IsRecentlyUnlocked(c));
		}
		return GameSystem<MenuSystem>.Instance().IsRecentlyUnlocked(type);
	}

	private void SetNotificationColor(Type type)
	{
		Color typeColor = Notification.GetTypeColor(type);
		if (_toggleNotification != null)
		{
			_toggleNotification.color = typeColor;
		}
		if (_countableNotification != null)
		{
			_countableNotification.SetColor(typeColor);
		}
	}

	public int GetPreferredSize()
	{
		if (_menuLabel == null)
		{
			return base.Widget.width;
		}
		return (int)_menuLabel.printedSize.x + 120;
	}

	private void SetMenuIcon(string icon)
	{
		if (!(_menuIcon == null))
		{
			_menuIcon.spriteName = icon;
		}
	}

	private void SetMenuText(string text)
	{
		if (!(_menuLabel == null))
		{
			if (string.IsNullOrEmpty(text))
			{
				_menuLabel.gameObject.SetActive(value: false);
				return;
			}
			_menuLabel.gameObject.SetActive(value: true);
			_menuLabel.text = text;
		}
	}

	private void RefreshParentIcon(State state)
	{
		if (_parentIcon != null)
		{
			_parentIcon.SetActive(MenuContainer.HasChildren(Type) && state != State.Selected);
		}
	}
}
