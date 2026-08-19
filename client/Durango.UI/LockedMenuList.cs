using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class LockedMenuList : MenuListWidgetBase, IMenuList
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private SelectableWidget _lockButton;

	[SerializeField]
	private LockedMenuScrollNotification _scrollBottomNotification;

	private LockedMenuScrollNotification _scrollTopNotification;

	private float _scrollOffset;

	private int _beginNode;

	private int _endNode;

	public event Action LockClicked;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		SelectableWidget lockButton = _lockButton;
		lockButton.Clicked = (Action)Delegate.Combine(lockButton.Clicked, (Action)delegate
		{
			if (this.LockClicked != null)
			{
				this.LockClicked();
			}
		});
		GameObject gameObject = _scrollBottomNotification.transform.parent.gameObject;
		_scrollTopNotification = gameObject.AddChild(_scrollBottomNotification.gameObject).GetComponent<LockedMenuScrollNotification>();
		_scrollTopNotification.transform.localEulerAngles = Vector3.forward * 180f;
		_scrollBottomNotification.SetAnchor(_scrollView.gameObject, 0f, 0, 0f, 0, 1f, 0, 0f, _scrollBottomNotification.height);
		_scrollTopNotification.SetAnchor(_scrollView.gameObject, 0f, 0, 1f, -_scrollTopNotification.height, 1f, 0, 1f, 0);
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			Init();
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (Application.isPlaying)
		{
			float currentOffset = _scrollView.CurrentOffset;
			if (currentOffset != _scrollOffset)
			{
				_scrollOffset = currentOffset;
				UpdateScrollNotification();
			}
		}
	}

	private void UpdateScrollNotification()
	{
		int num = _baseNode.Widget.height;
		int num2 = Mathf.FloorToInt(_scrollOffset / (float)num);
		int num3 = Mathf.CeilToInt((_scrollOffset + _scrollView.ViewLength) / (float)num);
		if (_beginNode == num2 && _endNode == num3)
		{
			return;
		}
		_beginNode = num2;
		_endNode = num3;
		int count = _menuList.Count;
		bool on = false;
		Durango.Logic.Notification.Type type = Durango.Logic.Notification.Type.Normal;
		int i = 0;
		for (int num4 = Mathf.Min(_beginNode, count); i < num4; i++)
		{
			MenuWidget menuWidget = _menuList[i];
			if (menuWidget.NotificationOn)
			{
				on = true;
				if (menuWidget.NotificationType > type)
				{
					type = menuWidget.NotificationType;
				}
			}
		}
		_scrollTopNotification.Set(on, type);
		on = false;
		type = Durango.Logic.Notification.Type.Normal;
		int j = Mathf.Max(0, _endNode);
		for (int num5 = count; j < num5; j++)
		{
			MenuWidget menuWidget2 = _menuList[j];
			if (menuWidget2.NotificationOn)
			{
				on = true;
				if (menuWidget2.NotificationType > type)
				{
					type = menuWidget2.NotificationType;
				}
			}
		}
		_scrollBottomNotification.Set(on, type);
	}

	public void Refresh()
	{
		Init();
		_menuList.BeginLoad();
		AddMenus(MenuContainer.Menus);
		AddMenus(MenuContainer.FixedMenus);
		_menuList.EndLoad();
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int i = 0; i < _menuList.Count; i++)
		{
			widgets.Add(_menuList[i].Widget);
		}
		_scrollView.ResetPosition();
		_scrollOffset = 0f;
		_beginNode = 0;
		_endNode = 0;
		UpdateScrollNotification();
	}

	private void AddMenus(IEnumerable<MenuType> types)
	{
		foreach (MenuType item in types.Where((MenuType t) => GameSystem<MenuSystem>.Instance().IsEnabled(t)))
		{
			MenuWidget next = _menuList.GetNext();
			next.Set(item);
		}
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
