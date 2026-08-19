using System;
using System.Collections.Generic;
using Durango.Logic.Notification;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

[Uri("Social")]
public class SocialGroup : UIBase, INotificationable
{
	private enum MenuType
	{
		List,
		Add,
		Manage
	}

	private Action<Social> _updated;

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private NestedPrefabLinker _tabList;

	[EnumList(typeof(MenuType), false, 0, -1)]
	[SerializeField]
	private GameObject[] _menuPages;

	private MenuType _selectedMenu;

	private IconTabList _tabs;

	private readonly List<KeyValuePair<string, Action>> _closeStack = new List<KeyValuePair<string, Action>>();

	private readonly Countable _notification = new Countable(Durango.Logic.Notification.Type.Important, ViewType.Count);

	private AsyncStackableAlarm<string, Durango.Player.PlayerInfo> _friendRequestedAlarms;

	private AsyncStackableAlarm<string, Durango.Player.PlayerInfo> _friendAcceptAlarms;

	private readonly HashSet<string> _friendRequestedIds = new HashSet<string>();

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		InitializeTab();
		_friendRequestedAlarms = new AsyncStackableAlarm<string, Durango.Player.PlayerInfo>("FriendRequested", delegate(string id, Action<string, Durango.Player.PlayerInfo, bool> onResponse)
		{
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(id, delegate(Durango.Player.PlayerInfo info)
			{
				onResponse(id, info, info.Valid);
			});
		}, (Durango.Player.PlayerInfo info, int count) => (count > 1) ? T._("{0} 님 외 {1}명 에게 친구요청을 받았습니다", info.GetNameFreq(21, string.Empty), count - 1) : T._("{0} 님의 친구요청을 받았습니다", info.GetNameFreq(21, string.Empty)), (Durango.Player.PlayerInfo info) => info.GetPortraitArgument(), majorAlarm: true, 1.8f, delegate
		{
			Open(MenuType.Add);
		});
		_friendAcceptAlarms = new AsyncStackableAlarm<string, Durango.Player.PlayerInfo>("FriendAccept", delegate(string id, Action<string, Durango.Player.PlayerInfo, bool> onResponse)
		{
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(id, delegate(Durango.Player.PlayerInfo info)
			{
				onResponse(id, info, info.Valid);
			});
		}, (Durango.Player.PlayerInfo info, int count) => (count > 1) ? T._("<em>{0}</em> 님 외 {1}명과 친구가 되었습니다.", info.GetNameFreq(21, string.Empty), count - 1) : T._("<em>{0}</em> 님과 친구가 되었습니다.", info.GetNameFreq(21, string.Empty)), (Durango.Player.PlayerInfo info) => info.GetPortraitArgument(), majorAlarm: true, 1.8f, null);
		_titleWidget.Object.SetTitle(T._("친구 목록"));
		GameSystem<SocialSystem>.Instance().FriendRequested += OnFriendRequested;
		GameSystem<SocialSystem>.Instance().FriendRequestAccepted += OnFriendRequestAccepted;
		GameSystem<SocialSystem>.Instance().SocialUpdated += OnSocial;
		SetChildrenActive(activated: false);
	}

	private void InitializeTab()
	{
		_tabs = _tabList.Object.GetComponent<IconTabList>();
		_tabs.Clicked += OnMenuSelected;
		_tabs.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(_menuPages); i < size; i++)
		{
			MenuType menuType = (MenuType)i;
			string icon = null;
			string text = null;
			switch (menuType)
			{
			case MenuType.List:
				icon = "friends_list";
				text = T._("내 친구");
				break;
			case MenuType.Add:
				icon = "friends_add";
				text = T._("친구 추가");
				break;
			case MenuType.Manage:
				icon = "friends_info";
				text = T._("친구 관리");
				break;
			}
			_tabs.Add(icon, text);
		}
		_tabs.EndLoad();
	}

	private void Open(MenuType type)
	{
		_selectedMenu = type;
		Open();
	}

	protected override bool TryOpen()
	{
		_mainWidget.alpha = 0f;
		GameSystem<SocialSystem>.Instance().GetSocial();
		SelectMenuTab(_selectedMenu);
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		if (_closeStack.Count > 0)
		{
			int index = _closeStack.Count - 1;
			KeyValuePair<string, Action> keyValuePair = _closeStack[index];
			_closeStack.RemoveAt(index);
			keyValuePair.Value();
			return false;
		}
		return base.TryClose();
	}

	public void AddCloseStack([NotNull] string key, [NotNull] Action action)
	{
		int num = -1;
		for (int i = 0; i < _closeStack.Count; i++)
		{
			if (_closeStack[i].Key == key)
			{
				num = i;
				break;
			}
		}
		KeyValuePair<string, Action> keyValuePair = new KeyValuePair<string, Action>(key, action);
		if (num == -1)
		{
			_closeStack.Add(keyValuePair);
		}
		else
		{
			_closeStack[num] = keyValuePair;
		}
	}

	public void RemoveCloseStack([NotNull] string key)
	{
		int num = -1;
		for (int i = 0; i < _closeStack.Count; i++)
		{
			if (_closeStack[i].Key == key)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			_closeStack.RemoveAt(num);
		}
	}

	public void AddOnUpdated(Action<Social> func)
	{
		_updated = (Action<Social>)Delegate.Combine(_updated, func);
		func(GameSystem<SocialSystem>.Instance().Social);
	}

	public void AcceptAllFriend()
	{
		string[] receivedFriendRequests = GameSystem<SocialSystem>.Instance().Social.ReceivedFriendRequests;
		string[] array = receivedFriendRequests;
		foreach (string entityId in array)
		{
			AcceptFriend(entityId);
		}
	}

	public void AcceptFriend(string entityId)
	{
		GameSystem<SocialSystem>.Instance().AcceptFriendRequest(entityId, delegate
		{
			_friendAcceptAlarms.Add(entityId);
		});
	}

	public void RejectAllFriend()
	{
		string[] receivedFriendRequests = GameSystem<SocialSystem>.Instance().Social.ReceivedFriendRequests;
		string[] array = receivedFriendRequests;
		foreach (string entityId in array)
		{
			RejectFriend(entityId);
		}
	}

	public void RejectFriend(string entityId)
	{
		GameSystem<SocialSystem>.Instance().RefuseFriendRequest(entityId, delegate
		{
			Durango.Player.PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
			if (cachedPlayerInfoOrEmpty.Valid)
			{
				UIManager.SystemMsg(T._("<em>{0}</em> 님의 친구요청을 거절했습니다.", cachedPlayerInfoOrEmpty.GetNameFreq(21, string.Empty)));
			}
		});
	}

	public void RequestFriend(string entityId)
	{
		GameSystem<SocialSystem>.Instance().RequestFriend(entityId, enable: true, delegate
		{
			Durango.Player.PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
			if (cachedPlayerInfoOrEmpty.Valid)
			{
				UIManager.SystemMsg(T._("<em>{0}</em> 님에게 친구요청을 보냈습니다.", cachedPlayerInfoOrEmpty.GetNameFreq(21, string.Empty)));
			}
		});
	}

	public void CancelFollow(string entityId)
	{
		GameSystem<SocialSystem>.Instance().Follow(entityId, enable: false, delegate
		{
			Durango.Player.PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
			if (cachedPlayerInfoOrEmpty.Valid)
			{
				UIManager.SystemMsg(T._("<em>{0}</em> 님을 즐겨찾기에서 제거했습니다.", cachedPlayerInfoOrEmpty.GetNameFreq(21, string.Empty)));
			}
		});
	}

	public void CancelBlock(string entityId)
	{
		Durango.Player.PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
		if (cachedPlayerInfoOrEmpty.Valid)
		{
			BlockPopup blockPopup = UIManager.Popup.Tooltip<BlockPopup>();
			blockPopup.Set(cachedPlayerInfoOrEmpty, null);
			blockPopup.Show();
		}
	}

	public void CancelRequest(string entityId)
	{
		Durango.Player.PlayerInfo info = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(entityId);
		if (!info.Valid)
		{
			return;
		}
		string mainText = T._("<em>{0}</em> 님이 수락 대기중인 친구 요청을 취소하시겠습니까?", info.GetNameFreq(24, string.Empty));
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				string successText = T._("<em>{0}</em> 님이 수락 대기중인 친구 요청을 취소했습니다.", info.GetNameFreq(21, string.Empty));
				string failureText = T._("친구 요청을 취소할 수 없습니다.");
				GameSystem<SocialSystem>.Instance().CancelFriendRequest(entityId, delegate
				{
					UIManager.SystemMsg(successText);
				}, delegate
				{
					UIManager.SystemMsg(failureText);
				});
			}
		}, T._("네"), T._("아니오"));
	}

	private void OnSocial()
	{
		Social social = GameSystem<SocialSystem>.Instance().Social;
		_notification.Count = KUtility.GetSize(social.ReceivedFriendRequests);
		_tabs.SetNotification(1, _notification.Count > 0, _notification.Type);
		if (base.IsOpened)
		{
			_mainWidget.alpha = 1f;
			ShowMenuPage(_selectedMenu);
			if (_updated != null)
			{
				_updated(social);
			}
		}
	}

	private void OnMenuSelected(int index)
	{
		if (index != -1)
		{
			SelectMenuTab((MenuType)index);
			ShowMenuPage((MenuType)index);
		}
	}

	private void SelectMenuTab(MenuType type)
	{
		_selectedMenu = type;
		_tabs.Select((int)type);
	}

	private void ShowMenuPage(MenuType type)
	{
		for (int i = 0; i < _menuPages.Length; i++)
		{
			_menuPages[i].gameObject.SetActive(i == (int)type);
		}
	}

	private void OnFriendRequested(string entityId)
	{
		if (!GameSystem<SocialSystem>.Instance().IgnoreFriendReqestedAlarm && _friendRequestedIds.Add(entityId))
		{
			_friendRequestedAlarms.Add(entityId);
		}
	}

	private void OnFriendRequestAccepted(string entityId)
	{
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				UIManager.Alarm.ShowNotify(T._("{0} 님이 친구요청을 수락했습니다", info.Name), info.GetPortraitArgument(), major: true, 1.8f, delegate
				{
					_selectedMenu = MenuType.List;
					Open();
				});
			}
		});
		if (base.IsOpened)
		{
			GameSystem<SocialSystem>.Instance().GetSocial();
		}
	}
}
