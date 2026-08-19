using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

[Uri("Chat")]
public abstract class ChattingGroupBase : UIBase
{
	protected class MakeChatParams
	{
		public enum MakeChatMode
		{
			NewChat,
			InviteCurrentChat
		}

		public MakeChatMode Modes;

		public Durango.Logic.Social.Conversation Conversation;

		public void SetNew(Durango.Logic.Social.Conversation conv = null)
		{
			Modes = MakeChatMode.NewChat;
			Conversation = conv;
		}

		public void SetInvite(Durango.Logic.Social.Conversation conv)
		{
			Modes = MakeChatMode.InviteCurrentChat;
			Conversation = conv;
		}
	}

	[SerializeField]
	private ChatFilterType _defaultFilterType;

	protected readonly MakeChatParams NewRoomParams = new MakeChatParams();

	protected Durango.Logic.Social.Conversation LastConversation;

	protected string FilterId;

	protected bool IsConnectionClosed;

	private Durango.Logic.Social.Conversation _conversation;

	private ChatFilterType _filterType;

	protected Durango.Logic.Social.Conversation CurrentConv
	{
		get
		{
			return _conversation;
		}
		set
		{
			if (value != null)
			{
				LastConversation = value;
			}
			_conversation = value;
		}
	}

	public ChatFilterType FilterType
	{
		get
		{
			return _filterType;
		}
		protected set
		{
			_filterType = value;
			ChannelType channelType = SocialSystem.ConvertToChannelType(FilterType);
			if (channelType != ChannelType.Invalid && channelType != ChannelType.System)
			{
				GameSystem<SocialSystem>.Instance().SwitchToChannel(channelType);
			}
			else
			{
				GameSystem<SocialSystem>.Instance().SwitchChannel(0);
			}
		}
	}

	protected virtual void Start()
	{
		Connections.Radiotower.ConnectionSucceed += Radiotower_Connected;
		Connections.Radiotower.ConnectionClosed += Radiotower_Closed;
		GameSystem<SocialSystem>.Instance().ChatAdded += SocialSystem_ChatAdded;
		GameSystem<SocialSystem>.Instance().ChatListChanged += SocialSystem_ChatListChanged;
		GameSystem<SocialSystem>.Instance().SubscriptionCountChanged += delegate
		{
			RefreshChattingTab();
		};
		GameSystem<SocialSystem>.Instance().ConversationsUpdated += RefreshChattingTab;
		GameSystem<SocialSystem>.Instance().NewConversation += SocialSystem_NewConversation;
		GameSystem<SocialSystem>.Instance().RecipientsJoined += SocialSystem_RecipientsJoined;
		GameSystem<SocialSystem>.Instance().RecipientExited += SocialSystem_RecipientExited;
		base.OnCloseSucceed += delegate
		{
			CurrentConv = null;
			GameSystem<SocialSystem>.Instance().SaveChannelInfo();
		};
	}

	private void OnEnable()
	{
		Durango.Logic.Social.Conversation.MessagesUpdated += Conversation_MessageUpdated;
	}

	private void OnDisable()
	{
		Durango.Logic.Social.Conversation.MessagesUpdated -= Conversation_MessageUpdated;
	}

	protected bool BaseOpen()
	{
		return base.Open();
	}

	public override bool Open()
	{
		if (!IsVisibleFilter(FilterType))
		{
			FilterType = _defaultFilterType;
		}
		if (LastConversation == null)
		{
			return Open(FilterType, string.Empty);
		}
		return Open(LastConversation);
	}

	public void Open(string entityId)
	{
		GameSystem<SocialSystem>.Instance().RequestConversation(new string[1] { entityId }, delegate(Durango.Logic.Social.Conversation conversation)
		{
			Open(conversation);
		});
	}

	public abstract bool Open(ChatFilterType type, string filterId = "");

	public abstract bool Open(Durango.Logic.Social.Conversation conv);

	protected void OnClickFilterTab(ChatFilterType filter)
	{
		Open(filter, string.Empty);
	}

	protected void OnClickChatRoom(Durango.Logic.Social.Conversation conversation)
	{
		Open(conversation);
	}

	protected void OnConversationInvite()
	{
		if (CurrentConv.IsEmpty)
		{
			NewRoomParams.SetNew(CurrentConv);
		}
		else
		{
			NewRoomParams.SetInvite(CurrentConv);
		}
		StartSearchChattingTarget();
	}

	protected void OnConversationExit()
	{
		if (CurrentConv == null)
		{
			return;
		}
		string exitConversationId = CurrentConv.Id;
		UIManager.MessageBox.Show(T._("채팅방에서 나가시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SocialSystem>.Instance().ExitConversation(exitConversationId);
				LastConversation = null;
				Open();
			}
		});
	}

	private void OnSelectedPlayers(IList<string> players)
	{
		if (KUtility.GetSize(players) <= 0)
		{
			return;
		}
		Durango.Logic.Social.Conversation conversation = NewRoomParams.Conversation;
		if (NewRoomParams.Modes == MakeChatParams.MakeChatMode.NewChat)
		{
			HashSet<string> hashSet = new HashSet<string>();
			if (conversation != null)
			{
				conversation.FillEntityIds(hashSet);
				hashSet.Remove(GameManager.PlayerId);
			}
			foreach (string player in players)
			{
				hashSet.Add(player);
			}
			if ((hashSet.Count != 1 || !hashSet.Contains(GameManager.PlayerId)) && hashSet.Count != 0)
			{
				GameSystem<SocialSystem>.Instance().RequestConversation(hashSet.ToArray(), OnClickChatRoom);
			}
		}
		else
		{
			GameSystem<SocialSystem>.Instance().InviteToConversation((conversation == null) ? string.Empty : conversation.Id, players);
		}
	}

	private void Radiotower_Connected()
	{
		if (IsConnectionClosed)
		{
			IsConnectionClosed = false;
			RadiotowerConnectUpdated(connected: true);
		}
	}

	private void Radiotower_Closed()
	{
		if (!(this == null))
		{
			KUtility.DelayedCall(this, DelayedRadiotowerClosed, 0.1f);
		}
	}

	private void DelayedRadiotowerClosed()
	{
		if (!GameManager.IsSceneClosing && !IsConnectionClosed)
		{
			IsConnectionClosed = true;
			RadiotowerConnectUpdated(connected: false);
		}
	}

	protected abstract void RadiotowerConnectUpdated(bool connected);

	private void SocialSystem_RecipientsJoined(string convId, string[] entityIds)
	{
		OnConversationMemberUpdated(convId);
	}

	private void SocialSystem_RecipientExited(string convId, string entityId)
	{
		OnConversationMemberUpdated(convId);
	}

	protected abstract void RefreshChattingTab();

	protected abstract void OnConversationMemberUpdated(string convId);

	protected abstract void SocialSystem_ChatAdded(ChatStruct chat);

	protected abstract void SocialSystem_ChatListChanged();

	protected abstract void SocialSystem_NewConversation(Durango.Logic.Social.Conversation conv);

	protected abstract void Conversation_MessageUpdated(Durango.Logic.Social.Conversation conv);

	protected void OnChatLinkClick(ChatStruct chatStruct)
	{
		if (chatStruct.Body is RadioPin)
		{
			RadioPin radioPin = (RadioPin)chatStruct.Body;
			OnRadioPinClick(chatStruct.EntityId, radioPin.Tile, radioPin.RegionId, radioPin.RegionName);
		}
		else if (chatStruct.Body is RadioPinWithText)
		{
			RadioPinWithText radioPinWithText = (RadioPinWithText)chatStruct.Body;
			OnRadioPinClick(chatStruct.EntityId, radioPinWithText.Tile, radioPinWithText.RegionId, radioPinWithText.RegionName);
		}
		else if (chatStruct.Body is RadioLink)
		{
			RadioLink radioLink = (RadioLink)chatStruct.Body;
			ParamsDictionary paramsDictionary = ParamsDictionary.MakeParams(radioLink.Link);
			string link = ((paramsDictionary != null) ? paramsDictionary.Get("link") : radioLink.Link);
			UIUtility.OpenUri(string.Empty, link);
		}
	}

	private void OnRadioPinClick(string entityId, Point2 tile, string regionId, string regionName)
	{
		Vector2 vector = new Vector2((short)tile.x, (short)tile.y);
		if (GameManager.Region.Id == regionId)
		{
			UIManager.FindScript<WorldMapGroup>().OpenForAnnounceBalloon(AnnounceType.SharePinPoint, vector, entityId);
		}
		else
		{
			UIManager.FindScript<SharedMapGroup>().Open(regionId, regionName, entityId, vector);
		}
	}

	protected void StartSearchChattingTarget()
	{
		PlayerSearchGroup playerSearchGroup = UIManager.FindScript<PlayerSearchGroup>();
		Durango.Logic.Social.Conversation conversation = NewRoomParams.Conversation;
		playerSearchGroup.OpenForMultiple(0, T._("대화 상대선택"), conversation?.GetEntityIds(), OnSelectedPlayers, T._("초대"));
	}

	private bool IsVisibleFilter(ChatFilterType filter)
	{
		ChannelType type = SocialSystem.ConvertToChannelType(filter);
		return SocialSystem.IsAllowedChannel(type);
	}

	protected virtual List<KeyValuePair<ChatFilterType, uint>> GetVisibleFilterList()
	{
		List<KeyValuePair<ChatFilterType, uint>> list = new List<KeyValuePair<ChatFilterType, uint>>();
		Array values = Enum.GetValues(typeof(ChatFilterType));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			ChatFilterType chatFilterType = (ChatFilterType)values.GetValue(i);
			if (IsVisibleFilter(chatFilterType))
			{
				ChannelType type = SocialSystem.ConvertToChannelType(chatFilterType, ChannelType.System);
				uint subscriptionCount = GameSystem<SocialSystem>.Instance().GetSubscriptionCount(type);
				list.Add(new KeyValuePair<ChatFilterType, uint>(chatFilterType, subscriptionCount));
			}
		}
		return list;
	}

	public static void ShowToggleButtonTooltip(string text, UIWidget parent, Vector3 offset)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, text);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(parent, offset, 4f);
	}
}
