using System;
using System.Collections.Generic;
using System.Linq;
using ChatData;
using L10N;
using MapData;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

public class ChattingGroup : UIBase
{
	internal class MakeChatParams
	{
		public enum MakeChatMode
		{
			NewChat,
			InviteCurrentChat
		}

		public MakeChatMode Modes;

		public ChatData.Conversation Conversation;

		public void SetNew(ChatData.Conversation conv = null)
		{
			Modes = MakeChatMode.NewChat;
			Conversation = conv;
		}

		public void SetInvite(ChatData.Conversation conv)
		{
			Modes = MakeChatMode.InviteCurrentChat;
			Conversation = conv;
		}
	}

	[SerializeField]
	private GameObject _backButton;

	[SerializeField]
	private ChatRoomOption _chatRoomOption;

	[SerializeField]
	private Selectable _chatRoomOptionButton;

	[SerializeField]
	private KWidgetScrollView _mainScrollView;

	[SerializeField]
	private ChattingTabList _tabList;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private ChatLineList _chatLineList;

	[SerializeField]
	private GameObject _chatTitleLabel;

	[SerializeField]
	private ChattingInputControl _chattingInputControl;

	private ChatFilterType _filterType;

	private ulong _filterId;

	private ChatData.Conversation _lastConversation;

	private ChatData.Conversation _conversation;

	private readonly MakeChatParams _makeChatParams = new MakeChatParams();

	private ChatData.Conversation CurrentConv
	{
		get
		{
			return _conversation;
		}
		set
		{
			if (_conversation != value)
			{
				if (_conversation != null)
				{
					EventDelegate.Remove(_conversation.NewChecker.OnChangeList, OnUpdateCurrentConversationNewCount);
				}
				if (value != null)
				{
					EventDelegate.Add(value.NewChecker.OnChangeList, OnUpdateCurrentConversationNewCount);
					_lastConversation = value;
				}
			}
			_conversation = value;
		}
	}

	private void Start()
	{
		ChatLineList chatLineList = _chatLineList;
		chatLineList.SharedPointButtonClicked = (Action<ChatStruct>)Delegate.Combine(chatLineList.SharedPointButtonClicked, (Action<ChatStruct>)delegate(ChatStruct chatStruct)
		{
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			if (chatStruct.Body is RadioPin)
			{
				RadioPin radioPin = (RadioPin)chatStruct.Body;
				Vector2 posPinPoint = default(Vector2);
				((Vector2)(ref posPinPoint))._002Ector((float)radioPin.Tile.x, (float)radioPin.Tile.y);
				if (KSingleton<GameManager>.Instance().Region.Id == radioPin.RegionId)
				{
					UIManager.FindScript<WorldMapGroup>().OpenForAnnounceBalloon(AnnounceType.SharePinPoint, posPinPoint, chatStruct.EntityId);
				}
				else
				{
					UIManager.FindScript<SharedMapGroup>().Open(radioPin.RegionId, radioPin.RegionName, chatStruct.EntityId, posPinPoint);
				}
			}
		});
		_chattingInputControl.Init();
		_chattingInputControl.Submitted = OnSubmit;
		UIEventListener.Get(_backButton).onClick = delegate
		{
			Close();
		};
		_chatRoomOptionButton.Clicked = delegate
		{
			if (!Selectable.Current.Disable && CurrentConv != null)
			{
				_chatRoomOption.Set(CurrentConv);
				_chatRoomOption.Show();
			}
		};
		_chatRoomOption.OnInvite += OnConversationInvite;
		_chatRoomOption.OnRename += OnConversationRename;
		_chatRoomOption.OnPushToggle += OnConversationPushToggle;
		_chatRoomOption.OnExit += OnConversationExit;
		UIEventListener.Get(_chatTitleLabel).onClick = ChatTitleLabelClicked;
		_tabList.FilterTabClicked += OnClickFilterTab;
		_tabList.ChatRoomClicked += OnClickChatRoom;
		_tabList.MakeRoomTabClicked += OnMakeChatRoom;
		base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<SocialSystem>.Instance().ChatAdded += SocialSystem_ChatAdded;
		GameSystem<SocialSystem>.Instance().FollowerStatusChanged += SocialSystem_FollowerStatusChanged;
		GameSystem<SocialSystem>.Instance().FollowingStatusChanged += SocialSystem_FollowingStatusChanged;
		GameSystem<SocialSystem>.Instance().ChatListChanged += SocialSystem_ChatListChanged;
		GameSystem<SocialSystem>.Instance().SubscriptionCountChanged += RefreshChattingTab;
		GameSystem<SocialSystem>.Instance().ConversationsUpdated += RefreshChattingTab;
		if (CurrentConv == null)
		{
			RefreshChattingTab();
		}
		ChatData.Conversation.MessagesUpdated += Conversation_MessageUpdated;
	}

	private void OnDisable()
	{
		GameSystem<SocialSystem>.Instance().ChatAdded -= SocialSystem_ChatAdded;
		GameSystem<SocialSystem>.Instance().FollowerStatusChanged -= SocialSystem_FollowerStatusChanged;
		GameSystem<SocialSystem>.Instance().FollowingStatusChanged -= SocialSystem_FollowingStatusChanged;
		GameSystem<SocialSystem>.Instance().ChatListChanged -= SocialSystem_ChatListChanged;
		GameSystem<SocialSystem>.Instance().SubscriptionCountChanged -= RefreshChattingTab;
		GameSystem<SocialSystem>.Instance().ConversationsUpdated -= RefreshChattingTab;
		ChatData.Conversation.MessagesUpdated -= Conversation_MessageUpdated;
	}

	private void OnPortraitMode(bool isPortrait)
	{
		((Behaviour)_mainScrollView.ScrollView).enabled = isPortrait;
	}

	private void UpdateLayout()
	{
		int screenHeight = UIManager.ScreenHeight;
		int width = Mathf.Min(1280 - ((Component)_tabList).GetComponent<UIWidget>().width, UIManager.ScreenWidth);
		int i = 0;
		for (int nodeCount = _mainScrollView.GetNodeCount(); i < nodeCount; i++)
		{
			UIWidget node = _mainScrollView.GetNode(i);
			node.height = screenHeight;
		}
		_mainWidget.width = width;
		UIUtility.UpdateAnchors(((Component)this).transform);
		_mainScrollView.UpdateLayout();
	}

	public override void Open()
	{
		if (_lastConversation == null)
		{
			Open(_filterType);
		}
		else
		{
			Open(_lastConversation);
		}
	}

	public void Open(ChatFilterType type, ulong filterId = 0)
	{
		_filterType = type;
		_filterId = filterId;
		_lastConversation = null;
		CurrentConv = null;
		if (base.IsOpen)
		{
			RefreshChattingList();
		}
		else
		{
			base.Open();
		}
	}

	public void Open(ulong entityId)
	{
		GameSystem<SocialSystem>.Instance().RequestConversation(new ulong[1] { entityId }, Open);
	}

	public void Open(ChatData.Conversation conv)
	{
		_filterType = ChatFilterType.All;
		_filterId = 0uL;
		CurrentConv = conv;
		if (base.IsOpen)
		{
			RefreshChattingList();
		}
		else
		{
			base.Open();
		}
	}

	private void OnUpdateCurrentConversationNewCount()
	{
		if (CurrentConv.NewChecker.Count != 0)
		{
			CurrentConv.NewChecker.Count = 0;
		}
	}

	private void OnConversationInvite()
	{
		if (CurrentConv.IsIndividual || CurrentConv.IsEmpty)
		{
			_makeChatParams.SetNew(CurrentConv);
		}
		else
		{
			_makeChatParams.SetInvite(CurrentConv);
		}
		StartSearchChattingTarget();
	}

	private void OnConversationRename()
	{
		if (CurrentConv != null)
		{
			UIManager.Popup.TextInput.Show(delegate(string newName)
			{
				CurrentConv.CustomName = newName;
				RefreshChattingTab();
				RefreshChattingList();
				GameSystem<SocialSystem>.Instance().SaveConversations();
			}, T._("그룹 이름을 변경합니다."));
		}
	}

	private void OnConversationPushToggle()
	{
		CurrentConv.PushEnabled = !CurrentConv.PushEnabled;
		GameSystem<SocialSystem>.Instance().AllowConversationPush(CurrentConv.Id, CurrentConv.PushEnabled);
		RefreshChattingTab();
	}

	private void OnConversationExit()
	{
		ulong exitConversationId = CurrentConv.Id;
		UIManager.MessageBox.Show(T._("그룹에서 나갑니다."), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SocialSystem>.Instance().ExitConversation(exitConversationId);
				Open(ChatFilterType.All);
			}
		});
	}

	private void ChatTitleLabelClicked(GameObject go)
	{
		if (CurrentConv != null && CurrentConv.IsGroup)
		{
			_chatLineList.AppendCurrentMemberLine(CurrentConv.GetEntityIds());
		}
	}

	private void OnClickFilterTab(ChatFilterType type)
	{
		switch (type)
		{
		case ChatFilterType.Public:
			GameSystem<SocialSystem>.Instance().SetBaseSayChannel(ChannelType.Region);
			break;
		case ChatFilterType.Clan:
			GameSystem<SocialSystem>.Instance().SetBaseSayChannel(ChannelType.Clan);
			break;
		}
		Open(type);
	}

	private void OnClickChatRoom(ChatData.Conversation conversation)
	{
		GameSystem<SocialSystem>.Instance().SetBaseSayChannel(conversation.Id);
		Open(conversation);
	}

	private void OnMakeChatRoom()
	{
		_makeChatParams.SetNew();
		StartSearchChattingTarget();
	}

	private void StartSearchChattingTarget()
	{
		PlayerSearchWidget playerSearch = UIManager.Popup.PlayerSearch;
		playerSearch.Show(SelectedChattingTarget, T._("대상의 이름을 입력하세요"));
	}

	private void SelectedChattingTarget(Player.PlayerInfo playerInfo)
	{
		ChatData.Conversation conversation = _makeChatParams.Conversation;
		if (_makeChatParams.Modes == MakeChatParams.MakeChatMode.NewChat)
		{
			HashSet<ulong> hashSet = new HashSet<ulong>();
			if (conversation != null)
			{
				conversation.FillEntityIds(hashSet);
				hashSet.Remove(GameManager.PlayerId);
			}
			hashSet.Add(playerInfo.EntityId);
			if ((hashSet.Count != 1 || !hashSet.Contains(GameManager.PlayerId)) && hashSet.Count != 0)
			{
				GameSystem<SocialSystem>.Instance().RequestConversation(hashSet.ToArray(), OnClickChatRoom);
			}
		}
		else if (conversation == null || !conversation.Contains(playerInfo.EntityId))
		{
			GameSystem<SocialSystem>.Instance().InviteToConversation(conversation?.Id ?? 0, playerInfo.EntityId);
		}
	}

	private void RefreshChattingList()
	{
		bool flag = CurrentConv != null;
		((Component)_chatRoomOptionButton).gameObject.SetActive(flag);
		if (!flag)
		{
			_chatLineList.Set(GameSystem<SocialSystem>.Instance().ChattingList, _filterType, _filterId);
			_chatLineList.SetTitle(LocalizeUtil.Get(_filterType));
			_tabList.Select(_filterType);
			return;
		}
		_chatLineList.Set(CurrentConv.Messages, _filterType, _filterId);
		if (CurrentConv.IsEmpty)
		{
			_chatLineList.SetTitle(T._("빈 그룹"));
		}
		else if (CurrentConv.IsIndividual)
		{
			_chatLineList.SetTitle(string.Empty);
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(CurrentConv.RepresentId, delegate(Player.PlayerInfo info)
			{
				_chatLineList.SetTitle(info.Valid ? $"{info.Name} {info.Freq:0000}KHZ" : T._("알수없음"));
			});
		}
		else
		{
			_chatLineList.SetTitle(T._("그룹 채팅 {0}명", CurrentConv.EntityCount));
		}
		if (!string.IsNullOrEmpty(CurrentConv.CustomName))
		{
			_chatLineList.SetTitle(CurrentConv.CustomName);
		}
		_tabList.Select(CurrentConv.Id);
	}

	private void RefreshChattingTab()
	{
		List<KeyValuePair<ChatFilterType, uint>> list = new List<KeyValuePair<ChatFilterType, uint>>();
		Array values = Enum.GetValues(typeof(ChatFilterType));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			ChatFilterType chatFilterType = (ChatFilterType)(int)values.GetValue(i);
			if (GameSystem<ClanSystem>.Instance().PlayerClan != null || chatFilterType != ChatFilterType.Clan)
			{
				ChannelType type = ChannelType.Invalid;
				switch (chatFilterType)
				{
				case ChatFilterType.Public:
					type = ChannelType.Region;
					break;
				case ChatFilterType.Clan:
					type = ChannelType.Clan;
					break;
				case ChatFilterType.System:
					type = ChannelType.System;
					break;
				}
				uint subscriptionCount = GameSystem<SocialSystem>.Instance().GetSubscriptionCount(type);
				list.Add(new KeyValuePair<ChatFilterType, uint>(chatFilterType, subscriptionCount));
			}
		}
		_tabList.Set(list, GameSystem<SocialSystem>.Instance().Conversations.Values.ToList());
		if (CurrentConv == null)
		{
			_tabList.Select(_filterType);
		}
		else
		{
			_tabList.Select(CurrentConv.Id);
		}
	}

	private void OnSubmit(string text)
	{
		_chatLineList.ChattingScrollLock = true;
		if (CurrentConv == null)
		{
			ChatFilterType filterType = _filterType;
			ChannelType channelType = ((filterType == ChatFilterType.Clan) ? ChannelType.Clan : ChannelType.Region);
			GameSystem<SocialSystem>.Instance().Say(channelType, text);
		}
		else
		{
			GameSystem<SocialSystem>.Instance().Say(CurrentConv.Id, text);
		}
	}

	protected override bool OnOpen()
	{
		CurrentConv = _lastConversation;
		UpdateLayout();
		base.OnOpen();
		RefreshChattingList();
		RefreshChattingTab();
		if (UIManager.IsPortraitMode)
		{
			_mainScrollView.MoveToNode(1, instant: true);
		}
		return true;
	}

	protected override bool OnClose()
	{
		base.OnClose();
		CurrentConv = null;
		GameSystem<SocialSystem>.Instance().SaveConversations();
		return true;
	}

	private void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (!chat.IsVolatile && base.IsOpen && CurrentConv == null && SocialSystem.IsVisibleFilter(chat, _filterType, _filterId))
		{
			AppendFullChatLine(chat);
		}
	}

	private void Conversation_MessageUpdated(ChatData.Conversation conv)
	{
		if (conv.Messages.Count != 0 && base.IsOpen && CurrentConv != null && conv.Id == CurrentConv.Id)
		{
			ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
			AppendFullChatLine(chat);
		}
	}

	private void SocialSystem_ChatListChanged()
	{
		if (base.IsOpen)
		{
			RefreshChattingList();
		}
	}

	private static void SocialSystem_FollowerStatusChanged(ulong entityId, bool follow)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				string text = ((!follow) ? T.N_("{0} 님이 팔로잉을 취소했습니다.") : T.N_("{0} 님이 팔로잉 합니다."));
				UIManager.Popup.Alarm.ShowAlarm(T._(text, info.Name), info.GetPortraitArgument());
			}
		});
	}

	private static void SocialSystem_FollowingStatusChanged(ulong entityId, bool online)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				string text = ((!online) ? T.N_("{0} 님이 나갔습니다.") : T.N_("{0} 님이 접속했습니다."));
				UIManager.Popup.Alarm.ShowAlarm(T._(text, info.Name), info.GetPortraitArgument());
			}
		});
	}

	public void AppendFullChatLine(ChatStruct chat)
	{
		_chatLineList.Append(chat);
	}
}
