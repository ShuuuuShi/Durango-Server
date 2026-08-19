using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

[Uri("Chat")]
public class ChattingGroup : ChattingGroupBase
{
	[SerializeField]
	private GameObject _backButton;

	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private ChatRoomOption _chatRoomOption;

	[SerializeField]
	private Selectable _chatRoomOptionButton;

	[SerializeField]
	private UISprite _pushStateSprite;

	[SerializeField]
	private SpriteData _pushEnableSprite;

	[SerializeField]
	private SpriteData _pushDisableSprite;

	[SerializeField]
	private UISprite _hideStateSprite;

	[SerializeField]
	private SpriteData _chatUnhideSprite;

	[SerializeField]
	private SpriteData _chatHideSprite;

	[SerializeField]
	private ChattingTabList _tabList;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private ChatLineList _chatLineList;

	[SerializeField]
	private ChattingInputControl _chattingInputControl;

	protected override void Start()
	{
		base.Start();
		_openCloseSound = UISound.GroupType.Default;
		_chatRoomOptionButton.Clicked = delegate
		{
			_chatRoomOption.Set(base.CurrentConv, _mainWidget.height + (int)_chatRoomOption.transform.localPosition.y);
			_chatRoomOption.Show();
		};
		UIEventListener.Get(_backButton).onClick = delegate
		{
			Close();
		};
		ChatLineList chatLineList = _chatLineList;
		chatLineList.ChatLinkClicked = (Action<ChatStruct>)Delegate.Combine(chatLineList.ChatLinkClicked, new Action<ChatStruct>(base.OnChatLinkClick));
		_chattingInputControl.Submitted = OnSubmit;
		_pushStateSprite.GetComponent<Selectable>().Clicked = OnPushToggle;
		_hideStateSprite.GetComponent<Selectable>().Clicked = OnChatHideToggle;
		_chatRoomOption.OnInvite += base.OnConversationInvite;
		_chatRoomOption.OnRename += OnConversationRename;
		_chatRoomOption.OnExit += base.OnConversationExit;
		_tabList.FilterTabClicked += base.OnClickFilterTab;
		_tabList.ChatRoomClicked += base.OnClickChatRoom;
		_tabList.MakeRoomTabClicked += OnMakeChatRoom;
		SetChildrenActive(activated: false);
	}

	public override bool Open(ChatFilterType type, string filterId = "")
	{
		base.FilterType = type;
		FilterId = filterId;
		LastConversation = null;
		base.CurrentConv = null;
		if (!base.IsOpened)
		{
			return BaseOpen();
		}
		RefreshChattingList();
		return false;
	}

	public override bool Open(Conversation conv)
	{
		base.FilterType = ChatFilterType.All;
		FilterId = string.Empty;
		base.CurrentConv = conv;
		GameSystem<SocialSystem>.Instance().SwitchToConversationChannel(conv.Id);
		if (!base.IsOpened)
		{
			return BaseOpen();
		}
		RefreshChattingList();
		return false;
	}

	private void OnPushToggle()
	{
		bool flag;
		if (base.CurrentConv != null)
		{
			flag = !base.CurrentConv.PushEnabled;
			GameSystem<SocialSystem>.Instance().AllowConversationPush(base.CurrentConv.Id, flag);
		}
		else
		{
			flag = GameSystem<SocialSystem>.Instance().ToggleClanPush(base.FilterType);
		}
		((!flag) ? _pushDisableSprite : _pushEnableSprite).Set(_pushStateSprite);
		string text = ((!flag) ? T._("채널에 새로운 대화가 있어도 알림을 받지 않습니다.") : T._("채널에 새로운 대화가 있으면 알림을 받습니다."));
		ChattingGroupBase.ShowToggleButtonTooltip(text, _pushStateSprite, Vector3.zero);
		RefreshChattingTab();
	}

	private void OnChatHideToggle()
	{
		bool flag = ((base.CurrentConv == null) ? GameSystem<SocialSystem>.Instance().ChannelInfo.ToggleHide(base.FilterType) : GameSystem<SocialSystem>.Instance().ChannelInfo.ToggleHide(base.CurrentConv));
		((!flag) ? _chatUnhideSprite : _chatHideSprite).Set(_hideStateSprite);
		string text = ((!flag) ? T._("채널의 대화 내용을 한줄 채팅에 표시합니다.") : T._("채널의 대화 내용을 한줄 채팅에 표시하지 않습니다."));
		ChattingGroupBase.ShowToggleButtonTooltip(text, _hideStateSprite, Vector2.up * 8f);
		RefreshChattingTab();
	}

	private void OnConversationRename()
	{
		if (base.CurrentConv != null)
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string newName)
			{
				base.CurrentConv.CustomName = newName;
				RefreshChattingTab();
				RefreshChattingList();
				GameSystem<SocialSystem>.Instance().SaveChannelInfo();
			}, T._("그룹 이름을 변경합니다."));
		}
	}

	private void OnMakeChatRoom()
	{
		NewRoomParams.SetNew();
		StartSearchChattingTarget();
	}

	private static Vector3 GetButtonBesidePosition(GameObject gameObject, Vector3 pos)
	{
		return pos - new Vector3(gameObject.GetComponent<BoxCollider>().size.x, 0f);
	}

	private void RefreshChattingList()
	{
		Vector3 vector = _chatRoomOptionButton.transform.localPosition;
		bool flag = base.CurrentConv != null;
		if (flag)
		{
			_chatRoomOptionButton.gameObject.SetActive(value: true);
			vector = GetButtonBesidePosition(_chatRoomOptionButton.gameObject, vector);
		}
		else
		{
			_chatRoomOptionButton.gameObject.SetActive(value: false);
		}
		if (flag || SocialSystem.IsKindOfClanChannelFilter(base.FilterType))
		{
			_pushStateSprite.gameObject.SetActive(value: true);
			_pushStateSprite.transform.localPosition = vector;
			((!((!flag) ? GameSystem<SocialSystem>.Instance().IsClanPushEnabled(base.FilterType) : base.CurrentConv.PushEnabled)) ? _pushDisableSprite : _pushEnableSprite).Set(_pushStateSprite);
			vector = GetButtonBesidePosition(_pushStateSprite.gameObject, vector);
		}
		else
		{
			_pushStateSprite.gameObject.SetActive(value: false);
		}
		ChannelType channelType = SocialSystem.ConvertToChannelType(base.FilterType);
		if (flag || ChatChannelInfo.IsHideable(channelType))
		{
			_hideStateSprite.gameObject.SetActive(value: true);
			_hideStateSprite.transform.localPosition = vector;
			((!((!flag) ? GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(channelType) : GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(base.CurrentConv))) ? _chatUnhideSprite : _chatHideSprite).Set(_hideStateSprite);
		}
		else
		{
			_hideStateSprite.gameObject.SetActive(value: false);
		}
		if (!flag)
		{
			_chatLineList.Set(GameSystem<SocialSystem>.Instance().ChattingList, base.FilterType, FilterId);
			_chatLineList.SetTitle(base.FilterType.GetName());
			_tabList.Select(base.FilterType);
			_chattingInputControl.SetEnabled(base.FilterType != ChatFilterType.System);
		}
		else
		{
			_chatLineList.Set(base.CurrentConv.Messages, base.FilterType, FilterId);
			if (!base.CurrentConv.GetTitle(_chatLineList.SetTitle))
			{
				_chatLineList.SetTitle(string.Empty);
			}
			_tabList.Select(base.CurrentConv.Id);
			_chattingInputControl.SetEnabled(isEnabled: true);
		}
		_mainLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void RefreshChattingTab()
	{
		List<KeyValuePair<ChatFilterType, uint>> visibleFilterList = GetVisibleFilterList();
		_tabList.Set(visibleFilterList, GameSystem<SocialSystem>.Instance().Conversations.Values);
		if (base.CurrentConv == null)
		{
			_tabList.Select(base.FilterType);
		}
		else
		{
			_tabList.Select(base.CurrentConv.Id);
		}
	}

	private void OnSubmit(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			_chatLineList.ChattingScrollLock = true;
			if (base.CurrentConv == null)
			{
				GameSystem<SocialSystem>.Instance().Say(text);
			}
			else
			{
				GameSystem<SocialSystem>.Instance().Say(base.CurrentConv.Id, text);
			}
		}
	}

	protected override bool TryOpen()
	{
		base.CurrentConv = LastConversation;
		if (!base.TryOpen())
		{
			return false;
		}
		RefreshChattingList();
		RefreshChattingTab();
		_chattingInputControl.SetConnected(GameSystem<SocialSystem>.Instance().CanSay());
		if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			_chattingInputControl.FocusInputText(hasFocus: true);
		}
		return true;
	}

	protected override void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (!chat.IsVolatile && base.IsOpened && base.CurrentConv == null && SocialSystem.IsVisibleChat(chat, base.FilterType, FilterId))
		{
			AppendFullChatLine(chat);
		}
	}

	protected override void SocialSystem_ChatListChanged()
	{
		if (base.IsOpened)
		{
			RefreshChattingList();
		}
	}

	protected override void SocialSystem_NewConversation(Conversation conv)
	{
		if (base.IsOpened)
		{
			Open(conv);
		}
	}

	protected override void OnConversationMemberUpdated(string convId)
	{
		if (!GameSystem<SocialSystem>.Instance().Conversations.ContainsKey(convId))
		{
			LastConversation = null;
			Open();
		}
		if (base.CurrentConv != null && base.CurrentConv.Id == convId && !base.CurrentConv.GetTitle(_chatLineList.SetTitle))
		{
			_chatLineList.SetTitle(string.Empty);
		}
		_tabList.UpdateConversations();
	}

	protected override void Conversation_MessageUpdated(Conversation conv)
	{
		if (conv.Messages.Count != 0 && base.IsOpened && base.CurrentConv != null && conv.Id == base.CurrentConv.Id)
		{
			ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
			AppendFullChatLine(chat);
		}
	}

	private void AppendFullChatLine(ChatStruct chat)
	{
		_chatLineList.Append(chat);
	}

	protected override void RadiotowerConnectUpdated(bool connected)
	{
		_chattingInputControl.SetConnected(connected);
	}
}
