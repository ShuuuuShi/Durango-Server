using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

[Uri("Chat")]
public class ChattingGroup_PC : ChattingGroupBase
{
	public enum ChatVisibility
	{
		Full,
		Shrink,
		Hide
	}

	public Action<ChatVisibility> ShowStateChanged;

	public Action<bool> TabNotificationStateChaged;

	[SerializeField]
	private ChattingTabList_PC _chatTabList;

	[SerializeField]
	private ChatRoomMaker _chatRoomMaker;

	[SerializeField]
	private ChattingChannelOption _channelOption;

	[SerializeField]
	private ChatLineList_PC _chatLineList;

	[SerializeField]
	private ChattingInputControl_PC _chatInputCtrl;

	[SerializeField]
	private ChatChannelSelector _inputChannelSelector;

	[SerializeField]
	private PositionSharer _positionSharer;

	[SerializeField]
	private UIWidget _windowWidget;

	[SerializeField]
	private UISprite _windowBgExpand;

	[SerializeField]
	private UISprite _windowBgShrink;

	[SerializeField]
	private UIWidget[] _hideOnShrinks;

	[SerializeField]
	private UIWidget[] _hideOnHides;

	[SerializeField]
	[Tooltip("Shrink 상태에서 Hide로 자동으로 변하기까지 대기시간")]
	private float _waitTimeUntilHide;

	[SerializeField]
	[Tooltip("Shrink상태에서 보여줄 채팅 줄 갯수")]
	private int _chatLineCountOnShrink;

	[SerializeField]
	[Tooltip("Shrink(Hide)상태에서 비활성화시킬 충돌박스")]
	private BoxCollider[] _hidableColliders;

	private float _hiddenTime;

	public ChatVisibility Visibility { get; private set; }

	public bool HoldShowState { get; set; }

	public bool IsCurrentlyAllChannel => base.FilterType == ChatFilterType.All && base.CurrentConv == null;

	public bool IsCurrentlySystemChannel => base.FilterType == ChatFilterType.System && base.CurrentConv == null;

	protected override void Start()
	{
		base.Start();
		IsConnectionClosed = true;
		GameSystem<InputSystem>.Instance().On(InputCommand.PopChatImmediately, delegate
		{
			OnPressEnter();
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, delegate
		{
			if (base.IsOpened && Visibility == ChatVisibility.Full)
			{
				Shrink();
			}
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.ChatTabSwitch, delegate
		{
			if (base.IsOpened)
			{
				_chatTabList.OnClickArrow(isNext: true);
				if (Visibility == ChatVisibility.Hide)
				{
					Shrink();
				}
			}
		});
		base.VisibleController.Changed += delegate(bool visible)
		{
			if (!visible)
			{
				ClosePopups();
			}
		};
		UIBase.UIOpened += delegate
		{
			if (!(UIBase.CurrentUI == this) && Visibility == ChatVisibility.Full)
			{
				Shrink();
			}
		};
		_chatInputCtrl.Submitted = OnSubmit;
		ChatLineList_PC chatLineList = _chatLineList;
		chatLineList.ChatLinkClicked = (Action<ChatStruct>)Delegate.Combine(chatLineList.ChatLinkClicked, new Action<ChatStruct>(base.OnChatLinkClick));
		ChatLineList_PC chatLineList2 = _chatLineList;
		chatLineList2.PositionUpdated = (Action)Delegate.Combine(chatLineList2.PositionUpdated, new Action(OnChatLinePositionUpdate));
		_inputChannelSelector.ChannelSelected = OnInputChannelSelect;
		ChatRoomMaker chatRoomMaker = _chatRoomMaker;
		chatRoomMaker.TabClicked = (Action<int>)Delegate.Combine(chatRoomMaker.TabClicked, new Action<int>(OnMakeChatRoom));
		_chatTabList.FilterTabClicked += base.OnClickFilterTab;
		_chatTabList.ChatRoomClicked += base.OnClickChatRoom;
		_chatTabList.NotificationStateChanged += OnTabNotificationStateChanged;
		_channelOption.ChatRoomOptionBox.OnInvite += base.OnConversationInvite;
		_channelOption.ChatRoomOptionBox.OnRename += OnConversationRename;
		_channelOption.ChatRoomOptionBox.OnExit += base.OnConversationExit;
		RefreshChattingTab();
		_chatInputCtrl.SetEnabled(isEnabled: true);
		Open();
		Hide();
	}

	public void OnPressEnter()
	{
		BottomLeftMenuGroup_PC bottomLeftMenuGroup_PC = UIManager.FindScript<BottomLeftMenuGroup_PC>();
		BottomMenuWidget_PC bottomMenuWidget_PC = bottomLeftMenuGroup_PC.BottomMenuWidget as BottomMenuWidget_PC;
		if (bottomMenuWidget_PC != null)
		{
			bottomMenuWidget_PC.ExistEmotionTooltip = false;
		}
		if (base.Visible)
		{
			if (Visibility != 0)
			{
				Show();
			}
			else
			{
				Shrink();
			}
		}
	}

	private void OnSubmit(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			Shrink();
			return;
		}
		_chatLineList.ChattingScrollLock = true;
		if (IsCurrentlyAllChannel)
		{
			ChannelType selectedChannelType = _inputChannelSelector.GetSelectedChannelType();
			if (SocialSystem.IsAllowedChannel(selectedChannelType))
			{
				if (selectedChannelType == ChannelType.Conversation)
				{
					string selectedConversationId = _inputChannelSelector.GetSelectedConversationId();
					GameSystem<SocialSystem>.Instance().Say(selectedConversationId, text);
				}
				else
				{
					GameSystem<SocialSystem>.Instance().SwitchToChannel(selectedChannelType);
					GameSystem<SocialSystem>.Instance().Say(text);
				}
			}
		}
		else if (base.CurrentConv == null)
		{
			GameSystem<SocialSystem>.Instance().Say(text);
		}
		else
		{
			GameSystem<SocialSystem>.Instance().Say(base.CurrentConv.Id, text);
		}
	}

	private void OnChatLinePositionUpdate()
	{
		if (Visibility == ChatVisibility.Shrink)
		{
			if (_chatLineList.IsEmpty)
			{
				Hide();
			}
			else
			{
				UpdateWindowHeight(isShrink: true);
			}
		}
		else if (Visibility == ChatVisibility.Hide)
		{
			Shrink();
		}
	}

	private void OnInputChannelSelect(string channelName)
	{
		_positionSharer.SpecifiedChannelType = _inputChannelSelector.GetSelectedChannelType();
		_positionSharer.SpecifiedConversationId = _inputChannelSelector.GetSelectedConversationId();
		_chatInputCtrl.SetChannelName(channelName);
		_chatInputCtrl.SetFocus(isSelected: true, isClearText: false);
	}

	protected override void RadiotowerConnectUpdated(bool connected)
	{
		_chatInputCtrl.Refresh();
	}

	protected override void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (chat.IsVolatile)
		{
			return;
		}
		if (IsCurrentlyAllChannel)
		{
			if (!GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(chat.Type))
			{
				AppendFullChatLine(chat);
			}
		}
		else if (base.CurrentConv == null && SocialSystem.IsVisibleChat(chat, base.FilterType, FilterId))
		{
			AppendFullChatLine(chat);
		}
		_chatTabList.UpdateNotifications(chat, IsCurrentlyAllChannel);
	}

	protected override void SocialSystem_ChatListChanged()
	{
		RefreshChannel();
	}

	protected override void SocialSystem_NewConversation(Conversation conv)
	{
		Open(conv);
	}

	protected override void OnConversationMemberUpdated(string convId)
	{
		if (!GameSystem<SocialSystem>.Instance().Conversations.ContainsKey(convId))
		{
			LastConversation = null;
			Open();
		}
		_chatTabList.UpdateConversations();
	}

	protected override void Conversation_MessageUpdated(Conversation conv)
	{
		if (conv.Messages.Count == 0)
		{
			return;
		}
		ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
		if (IsCurrentlyAllChannel)
		{
			if (!GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(conv))
			{
				AppendFullChatLine(chat);
			}
		}
		else if (base.CurrentConv != null && conv.Id == base.CurrentConv.Id)
		{
			AppendFullChatLine(chat);
		}
		_chatTabList.UpdateNotifications(conv, IsCurrentlyAllChannel);
	}

	private void OnConversationRename()
	{
		if (base.CurrentConv != null)
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string newName)
			{
				base.CurrentConv.CustomName = newName;
				RefreshChattingTab();
				GameSystem<SocialSystem>.Instance().SaveChannelInfo();
			}, T._("그룹 이름을 변경합니다."));
		}
	}

	private void OnMakeChatRoom(int index)
	{
		NewRoomParams.SetNew();
		StartSearchChattingTarget();
		PlayerSearchGroup playerSearchGroup = UIManager.FindScript<PlayerSearchGroup>();
		playerSearchGroup.SelectTab(index);
	}

	private void OnTabNotificationStateChanged(bool hasActiveNotification)
	{
		if (TabNotificationStateChaged != null)
		{
			TabNotificationStateChaged(hasActiveNotification);
		}
	}

	public override bool Open(ChatFilterType type, string filterId = "")
	{
		base.FilterType = type;
		FilterId = filterId;
		LastConversation = null;
		base.CurrentConv = null;
		RefreshChannel();
		return OpenInternal();
	}

	public override bool Open(Conversation conv)
	{
		base.FilterType = ChatFilterType.All;
		FilterId = string.Empty;
		base.CurrentConv = conv;
		GameSystem<SocialSystem>.Instance().SwitchToConversationChannel(conv.Id);
		RefreshChannel();
		return OpenInternal();
	}

	private bool OpenInternal()
	{
		if (!base.IsOpened)
		{
			if (BaseOpen())
			{
				Show();
				return true;
			}
			return false;
		}
		Show();
		return true;
	}

	public void Show(bool isFocus = true)
	{
		if (!base.IsOpened || HoldShowState)
		{
			return;
		}
		if (Visibility == ChatVisibility.Full)
		{
			_chatInputCtrl.SetFocus(isSelected: true, isClearText: false);
			return;
		}
		UpdateWindowHeight(isShrink: false);
		UIWidget[] hideOnShrinks = _hideOnShrinks;
		foreach (UIWidget uIWidget in hideOnShrinks)
		{
			uIWidget.alpha = 1f;
		}
		UIWidget[] hideOnHides = _hideOnHides;
		foreach (UIWidget uIWidget2 in hideOnHides)
		{
			uIWidget2.alpha = 1f;
		}
		BoxCollider[] hidableColliders = _hidableColliders;
		foreach (BoxCollider boxCollider in hidableColliders)
		{
			boxCollider.enabled = true;
		}
		_chatLineList.EnableChatlineColliders(isEnable: true);
		_chatLineList.ResetScroll();
		_chatInputCtrl.SetFocus(isFocus);
		ChangeShowState(ChatVisibility.Full);
	}

	public void Shrink()
	{
		if (!base.IsOpened || HoldShowState || Visibility == ChatVisibility.Shrink)
		{
			return;
		}
		if (_chatLineList.IsEmpty)
		{
			Hide();
			return;
		}
		UpdateWindowHeight(isShrink: true);
		UIWidget[] hideOnShrinks = _hideOnShrinks;
		foreach (UIWidget uIWidget in hideOnShrinks)
		{
			uIWidget.alpha = 0f;
		}
		UIWidget[] hideOnHides = _hideOnHides;
		foreach (UIWidget uIWidget2 in hideOnHides)
		{
			uIWidget2.alpha = 1f;
		}
		BoxCollider[] hidableColliders = _hidableColliders;
		foreach (BoxCollider boxCollider in hidableColliders)
		{
			boxCollider.enabled = false;
		}
		_chatLineList.EnableChatlineColliders(isEnable: false);
		_chatLineList.ResetScroll();
		_chatInputCtrl.SetFocus(isSelected: false);
		_hiddenTime = Time.time;
		if (Visibility == ChatVisibility.Full && !IsConnectionClosed)
		{
			GameSystem<SocialSystem>.Instance().SaveChannelInfo();
		}
		ClosePopups();
		ChangeShowState(ChatVisibility.Shrink);
	}

	public void Hide()
	{
		if (base.IsOpened && !HoldShowState && Visibility != ChatVisibility.Hide)
		{
			UIWidget[] hideOnShrinks = _hideOnShrinks;
			foreach (UIWidget uIWidget in hideOnShrinks)
			{
				uIWidget.alpha = 0f;
			}
			UIWidget[] hideOnHides = _hideOnHides;
			foreach (UIWidget uIWidget2 in hideOnHides)
			{
				uIWidget2.alpha = 0f;
			}
			BoxCollider[] hidableColliders = _hidableColliders;
			foreach (BoxCollider boxCollider in hidableColliders)
			{
				boxCollider.enabled = false;
			}
			_chatLineList.EnableChatlineColliders(isEnable: false);
			_chatInputCtrl.SetFocus(isSelected: false);
			if (Visibility == ChatVisibility.Full && !IsConnectionClosed)
			{
				GameSystem<SocialSystem>.Instance().SaveChannelInfo();
			}
			ClosePopups();
			ChangeShowState(ChatVisibility.Hide);
		}
	}

	private void ClosePopups()
	{
		_chatRoomMaker.Open(isOpen: false);
		_inputChannelSelector.Open(isOpen: false);
		_channelOption.HidePopup();
	}

	private void ChangeShowState(ChatVisibility state)
	{
		Visibility = state;
		if (ShowStateChanged != null)
		{
			ShowStateChanged(Visibility);
		}
	}

	private void UpdateWindowHeight(bool isShrink)
	{
		if (isShrink)
		{
			UIWidget component = _chatLineList.GetComponent<UIWidget>();
			int num = component.bottomAnchor.absolute - component.topAnchor.absolute;
			int heightOnShrink = _chatLineList.GetHeightOnShrink(_chatLineCountOnShrink);
			heightOnShrink += num;
			_windowWidget.height = heightOnShrink;
		}
		else
		{
			_windowWidget.topAnchor.absolute = ((ScreenInfo.GetCurrentScreenSize().Height % 2 != 0) ? 1 : 0);
		}
		_windowBgExpand.gameObject.SetActive(!isShrink);
		_windowBgShrink.gameObject.SetActive(isShrink);
		UIUtility.UpdateAnchors(_windowWidget.transform);
		_windowBgShrink.UpdateAnchors();
	}

	private void RefreshChannel()
	{
		bool isCurrentlyAllChannel = IsCurrentlyAllChannel;
		bool isCurrentlySystemChannel = IsCurrentlySystemChannel;
		_inputChannelSelector.gameObject.SetActive(isCurrentlyAllChannel);
		_positionSharer.IsAllChatChannel = isCurrentlyAllChannel;
		_positionSharer.SpecifiedChannelType = _inputChannelSelector.GetSelectedChannelType();
		_positionSharer.SpecifiedConversationId = _inputChannelSelector.GetSelectedConversationId();
		_positionSharer.SetEnabled(!isCurrentlySystemChannel);
		_chatInputCtrl.IsAllChatChannel = isCurrentlyAllChannel;
		_chatInputCtrl.Widget.leftAnchor.absolute = (isCurrentlyAllChannel ? _inputChannelSelector.Widget.width : 0);
		_chatInputCtrl.SetChannelName(_inputChannelSelector.GetSelectedChannelName());
		_chatInputCtrl.SetEnabled(!isCurrentlySystemChannel);
		if (base.CurrentConv == null)
		{
			_chatTabList.Select(base.FilterType);
			_channelOption.Select(base.FilterType);
			if (isCurrentlyAllChannel)
			{
				_chatTabList.MarkUnHiddenChannelsAsRead();
				List<ChatStruct> list = new List<ChatStruct>();
				List<ChatStruct> chattingList = GameSystem<SocialSystem>.Instance().ChattingList;
				foreach (ChatStruct item in chattingList)
				{
					if (!GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(item.Type))
					{
						list.Add(item);
					}
				}
				List<Conversation> list2 = GameSystem<SocialSystem>.Instance().Conversations.Values.ToList();
				foreach (Conversation item2 in list2)
				{
					if (!GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(item2))
					{
						list.AddRange(item2.Messages);
					}
				}
				list.Sort(SocialSystem.SortChattingList);
				_chatLineList.Set(list, base.FilterType, FilterId, isAllChat: true);
			}
			else
			{
				_chatLineList.Set(GameSystem<SocialSystem>.Instance().ChattingList, base.FilterType, FilterId);
			}
		}
		else
		{
			_chatTabList.Select(base.CurrentConv.Id);
			_channelOption.Select(base.CurrentConv);
			_chatLineList.Set(base.CurrentConv.Messages, base.FilterType, FilterId);
		}
		_chatInputCtrl.Refresh();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void RefreshChattingTab()
	{
		IList<KeyValuePair<ChatFilterType, uint>> visibleFilterList = GetVisibleFilterList();
		IEnumerable<Conversation> values = GameSystem<SocialSystem>.Instance().Conversations.Values;
		_chatTabList.Set(visibleFilterList, values);
		_channelOption.Set(visibleFilterList);
		_inputChannelSelector.SetChannelList(GetChattableFilterList(), values);
		if (base.CurrentConv == null)
		{
			_chatTabList.Select(base.FilterType);
			_channelOption.Select(base.FilterType);
		}
		else
		{
			_chatTabList.Select(base.CurrentConv.Id);
			_channelOption.Select(base.CurrentConv);
		}
	}

	private void AppendFullChatLine(ChatStruct chat)
	{
		_chatLineList.Append(chat);
	}

	protected override List<KeyValuePair<ChatFilterType, uint>> GetVisibleFilterList()
	{
		List<KeyValuePair<ChatFilterType, uint>> visibleFilterList = base.GetVisibleFilterList();
		visibleFilterList.Insert(0, new KeyValuePair<ChatFilterType, uint>(ChatFilterType.All, 0u));
		return visibleFilterList;
	}

	private List<ChatFilterType> GetChattableFilterList()
	{
		List<ChatFilterType> list = new List<ChatFilterType>();
		foreach (ChatFilterType value in Enum.GetValues(typeof(ChatFilterType)))
		{
			if (IsChattableFilter(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	private bool IsChattableFilter(ChatFilterType filter)
	{
		if (filter == ChatFilterType.All || filter == ChatFilterType.System)
		{
			return false;
		}
		ChannelType type = SocialSystem.ConvertToChannelType(filter);
		return SocialSystem.IsAllowedChannel(type);
	}

	private void LateUpdate()
	{
		if (base.IsOpened && Visibility == ChatVisibility.Shrink && _hiddenTime + _waitTimeUntilHide < Time.time)
		{
			Hide();
		}
	}

	public static string GetConversationName(Conversation conv)
	{
		if (conv == null)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(conv.CustomName))
		{
			return conv.CustomName;
		}
		if (conv.IsEmpty)
		{
			return T._("빈 그룹");
		}
		if (conv.IsIndividual)
		{
			return string.Empty;
		}
		return T._("그룹 채팅");
	}

	public static void RequestPartnerName(Conversation conv, Action<PlayerInfo> response)
	{
		string text = string.Empty;
		if (conv != null)
		{
			string[] entityIds = conv.GetEntityIds();
			text = ((entityIds != null) ? Array.Find(entityIds, (string x) => x != GameManager.PlayerId) : string.Empty);
		}
		if (text != null)
		{
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(text, response);
		}
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		UpdateWindowHeight(Visibility == ChatVisibility.Shrink);
	}
}
