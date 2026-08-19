using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.Logic.Social;
using Durango.Network;
using Durango.System.Config;
using Durango.UI.InGame;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Chat;
using Shared.Player;
using Shared.Region;
using Shared.Social;
using UnityEngine;

public class SocialSystem : GameSystem<SocialSystem>
{
	private class RadiotowerConnectionHelper
	{
		public enum ConnectState
		{
			None,
			Connecting,
			Authenticating,
			Ready
		}

		private int _connectAttempted;

		private readonly List<KeyValuePair<string, int>> _endpoints = new List<KeyValuePair<string, int>>();

		private int _endpointIndex;

		public ConnectState State { get; private set; }

		public event Action<Messages.Conversation[]> Ready;

		public void SetEndpoints([CanBeNull] IList<KeyValuePair<string, int>> endpoints)
		{
			_endpoints.Clear();
			if (endpoints != null)
			{
				_endpoints.AddRange(endpoints);
			}
			_ = _endpoints.Count;
			_endpointIndex = UnityEngine.Random.Range(0, _endpoints.Count);
		}

		public void TryConnect()
		{
			_connectAttempted = 0;
			State = ConnectState.None;
		}

		public void Process()
		{
			if (GameManager.IsReady)
			{
				if (GameManager.ClusterMode != 0)
				{
					State = ConnectState.Ready;
				}
				else if (State == ConnectState.None)
				{
					TryReconnect();
				}
				else
				{
					UpdateState();
				}
			}
		}

		private void UpdateState()
		{
			if (State == ConnectState.Connecting && Connections.Radiotower.IsAttemptingToConnect())
			{
				return;
			}
			if (Connections.Radiotower.Connected())
			{
				if (State == ConnectState.Connecting)
				{
					RequestAuth();
					State = ConnectState.Authenticating;
				}
			}
			else
			{
				State = ConnectState.None;
			}
		}

		private void TryReconnect()
		{
			if (_connectAttempted <= 5)
			{
				_connectAttempted++;
				Connect();
			}
		}

		private void Connect()
		{
			if (_endpoints.Count != 0)
			{
				_endpointIndex %= _endpoints.Count;
				KeyValuePair<string, int> keyValuePair = _endpoints[_endpointIndex];
				string key = keyValuePair.Key;
				int value = keyValuePair.Value;
				_endpointIndex++;
				State = ConnectState.Connecting;
				Connections.Radiotower.ConnectAsync(key, value);
			}
		}

		private void RequestAuth()
		{
			Connections.Radiotower.Send(new Tune
			{
				EntityId = GameManager.PlayerId,
				SessionToken = GameManager.SessionToken,
				SyncedAt = 0.0
			}).On(delegate(Conversations msg, PacketHeader header)
			{
				_connectAttempted = 0;
				State = ConnectState.Ready;
				if (this.Ready != null)
				{
					this.Ready(msg._Conversations);
				}
			});
		}
	}

	public class Channel
	{
		public ChannelType Type;

		public string Id;
	}

	private const int MaxChatLogCount = 200;

	private const int CheckCurrentChatCount = 5;

	private const double CheckCurrentChatSeconds = 5.0;

	public readonly Durango.Logic.Notification.Container ConversationsNewCount = new Durango.Logic.Notification.Container();

	private readonly ChannelType[] _channels = new ChannelType[6]
	{
		ChannelType.Region,
		ChannelType.PersonalRegions,
		ChannelType.Clan,
		ChannelType.ClanWar,
		ChannelType.Party,
		ChannelType.Conversation
	};

	private Dictionary<Emotion, EmotionJson> _chatEmotionData;

	private readonly List<ChatStruct> _chattingList = new List<ChatStruct>();

	private readonly Dictionary<string, Durango.Logic.Social.Conversation> _conversations = new Dictionary<string, Durango.Logic.Social.Conversation>();

	private readonly Dictionary<ChannelType, uint> _subscriptionCount = new Dictionary<ChannelType, uint>(default(ChannelTypeComparer));

	private readonly RadiotowerConnectionHelper _connectionHelper = new RadiotowerConnectionHelper();

	private Dictionary<ChannelType, bool> _clanChannelPushEnabled = new Dictionary<ChannelType, bool>();

	private readonly Queue<double> _currentChatTimestamps = new Queue<double>();

	private float _quickChatEnableAt;

	private readonly Channel _currentChannel = new Channel
	{
		Type = ChannelType.Region,
		Id = string.Empty
	};

	private SocialOptions _socialOptions;

	public static bool AutoTranslation { get; set; }

	public List<ChatStruct> ChattingList => _chattingList;

	public Social Social { get; private set; }

	public Dictionary<string, Durango.Logic.Social.Conversation> Conversations => _conversations;

	public Emotional Emotional { get; private set; }

	[NotNull]
	public Channel CurrentChannel => _currentChannel;

	public ChatChannelInfo ChannelInfo { get; private set; }

	public bool IgnoreFriendReqestedAlarm
	{
		get
		{
			return Preferences.GetBool("ignore_friend_reqested_alarm");
		}
		set
		{
			Preferences.SetBool("ignore_friend_reqested_alarm", value);
		}
	}

	public event Action<ChatStruct> ChatAdded;

	public event Action<ChatableBase> ChatHided;

	public event Action ChatListChanged;

	public event Action<ChannelType> SubscriptionCountChanged;

	public event Action ConversationsUpdated;

	public event Action<Durango.Logic.Social.Conversation> NewConversation;

	public event Action<string, string[]> RecipientsJoined;

	public event Action<string, string> RecipientExited;

	public event Action<string, bool> FollowerStatusChanged;

	public event Action<string, bool> FollowingStatusChanged;

	public event Action<string> FriendRequested;

	public event Action<string> FriendRequestAccepted;

	public event Action SocialUpdated;

	public event Action<string> EmoticonPlayed;

	private void Awake()
	{
		_chatEmotionData = new Dictionary<Emotion, EmotionJson>(default(EmotionComparer));
		Dictionary<Emotion, EmotionJson> dictionary = Json.ReadFromFile<Dictionary<Emotion, EmotionJson>>("chat_emotion");
		if (dictionary != null)
		{
			_chatEmotionData.AddRange(dictionary);
		}
		Emotional = new Emotional();
		ChannelInfo = new ChatChannelInfo();
		_connectionHelper.Ready += ConnectionHelper_Ready;
		Singleton<GameManager>.Instance().WelcomeReceived += OnWelcome;
		Connections.Radiotower.On<SayInExclusiveChannel>(OnSay);
		Connections.Radiotower.On<SayInConversation>(OnSayConversation);
		Connections.Radiotower.On<SubscriptionCount>(OnSubscriptionCount);
		Connections.Radiotower.On<Messages.Conversation>(OnConversation);
		Connections.Radiotower.On<JoinRecipients>(OnJoinRecipients);
		Connections.Radiotower.On<ExitRecipient>(OnExitRecipient);
		Connections.Radiotower.On<FollowerStatus>(OnFollowerStatus);
		Connections.Radiotower.On<FollowingStatus>(OnFollowingStatus);
		Connections.Frontend.On<SayInExclusiveChannel>(OnSay);
		Connections.Frontend.On<FollowTutorialColleagues>(OnTutorialColleagues);
		Connections.Frontend.On<FriendRequested>(OnFriendRequested);
		Connections.Frontend.On<FriendRequestAccepted>(OnFriendRequestAccepted);
		Connections.Frontend.On<AvailableEmotions>(OnAvailableEmotions);
		Connections.Frontend.On<PlayEmoticon>(OnPlayEmoticon);
		Connections.Frontend.On<Social>(OnSocial);
		Connections.Radiotower.ConnectionSucceed += delegate
		{
			ClearChats();
			AddSystemChat(T._("채팅 서버와 연결되었습니다."), string.Empty);
		};
		Connections.Radiotower.ConnectionClosed += delegate
		{
			if (!GameManager.IsSceneClosing)
			{
				AddSystemChat(T._("채팅 서버와 연결이 끊어졌습니다."), string.Empty);
			}
		};
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode && GameManager.Emigrated != 0)
			{
				AddSystemChat(T._("<{0}> 섬으로 이동했습니다.", GameManager.Region.Name), string.Empty);
			}
		};
		Singleton<GameManager>.Instance().AddOnReady(OnReady);
		Singleton<GameManager>.Instance().PostReconnect += delegate
		{
			_connectionHelper.TryConnect();
		};
		GameSystem<ClanSystem>.Instance().ClanChanged += ClanChanged;
		GameSystem<PartySystem>.Instance().MembersUpdated += delegate
		{
			if (!IsAllowedChannel(ChannelType.Party))
			{
				RemoveChat(ChannelType.Party);
			}
		};
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += delegate
		{
			foreach (Durango.Logic.Social.Motion motion in Emotional.Motions)
			{
				motion.MarkAsChanged();
			}
		};
	}

	private void LateUpdate()
	{
		_connectionHelper.Process();
	}

	private void OnReady()
	{
		GetAvailableEmotions();
		GetSocial();
	}

	private void OnWelcome(Welcome welcome)
	{
		OnSocialOptions(welcome.SocialOptions);
		Dictionary<string, byte[]> data = welcome.Storage.Data;
		Emotional.LoadFavorites(data);
		ChannelInfo.LoadStorage(data);
	}

	public void SaveChannelInfo()
	{
		ChannelInfo.SaveStorage(_conversations);
	}

	private void OnSocialOptions(SocialOptions socialOptions)
	{
		_socialOptions = socialOptions;
		if (_socialOptions.Options != null)
		{
			ConfigInstance.ChangeValue("permit_conversation", _socialOptions.Options.Get(SocialOptionType.AllowOutlanderConversation, defaultValue: false), save: false);
		}
	}

	public uint GetSubscriptionCount(ChannelType type)
	{
		return _subscriptionCount.Get(type, 0u);
	}

	public Emotion GetTextEmotion(string text)
	{
		Emotion emotion = Emotion.None;
		Array values = Enum.GetValues(typeof(Emotion));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			Emotion emotion2 = (Emotion)values.GetValue(i);
			if (IsTextEmotion(text, emotion2))
			{
				emotion |= emotion2;
			}
		}
		return emotion;
	}

	public bool IsTextEmotion(string text, Emotion emo)
	{
		if (_chatEmotionData.TryGetValue(emo, out var value))
		{
			int i = 0;
			for (int size = KUtility.GetSize(value.Word); i < size; i++)
			{
				if (text.Contains(value.Word[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ConnectionHelper_Ready(Messages.Conversation[] conversations)
	{
		int i = 0;
		for (int num = conversations.Length; i < num; i++)
		{
			_conversations[conversations[i].Id] = new Durango.Logic.Social.Conversation(conversations[i]);
		}
		OnConversationUpdate();
		GetLatestChatLog();
		Connections.Radiotower.Send(default(GetClanNotificationEnabled)).On(delegate(ToggleClanNotification msg, PacketHeader header)
		{
			_clanChannelPushEnabled = msg.ChannelNotificationsEnabled;
		});
	}

	private void GetLatestChatLog()
	{
		ChannelType[] array = new ChannelType[5]
		{
			ChannelType.Region,
			ChannelType.Clan,
			ChannelType.ClanWar,
			ChannelType.Party,
			ChannelType.PersonalRegions
		};
		foreach (ChannelType channelType in array)
		{
			Connections.Radiotower.Send(new GetLatestChatLog
			{
				ChannelType = channelType
			}).On(delegate(ChatLogs msg, PacketHeader header)
			{
				ReceiveChatLog(msg, channelType);
			});
		}
	}

	private void ReceiveChatLog(ChatLogs msg, ChannelType type)
	{
		for (int i = 0; i < msg.Logs.Length; i++)
		{
			Message_ message_ = msg.Logs[i];
			ChatStruct chatStruct = new ChatStruct
			{
				EntityId = message_.EntityId,
				Body = message_.Body,
				Time = message_.Time,
				Type = type,
				TranslationOn = AutoTranslation
			};
			if (message_.Speaker.HasValue)
			{
				chatStruct.Name = message_.Speaker.Value.Name;
			}
			_chattingList.Add(chatStruct);
		}
		_chattingList.Sort(SortChattingList);
		if (this.ChatListChanged != null)
		{
			this.ChatListChanged();
		}
	}

	private void OnSubscriptionCount(SubscriptionCount msg, PacketHeader header)
	{
		_subscriptionCount[msg.ChannelType] = msg.Count;
		if (this.SubscriptionCountChanged != null)
		{
			this.SubscriptionCountChanged(msg.ChannelType);
		}
	}

	private void OnSay(SayInExclusiveChannel msg, PacketHeader header)
	{
		ChatStruct chatStruct = new ChatStruct
		{
			EntityId = msg.Message.EntityId,
			Body = msg.Message.Body,
			Time = msg.Message.Time,
			Type = msg.ChannelType,
			TranslationOn = AutoTranslation
		};
		if (msg.Message.Speaker.HasValue)
		{
			chatStruct.Name = msg.Message.Speaker.Value.Name;
		}
		if (msg.Message.Body is RadioNotice)
		{
			if (GameManager.Region.IsPvpIsland())
			{
				chatStruct.NoBubble = true;
			}
			else if (Singleton<UIManager>.HasInstance())
			{
				UIManager.SystemMsg(((RadioNotice)msg.Message.Body).Text);
			}
		}
		AddChat(chatStruct);
	}

	private void OnConversation(Messages.Conversation msg, PacketHeader header)
	{
		Durango.Logic.Social.Conversation conversation = new Durango.Logic.Social.Conversation(msg);
		_conversations[conversation.Id] = conversation;
		if (this.NewConversation != null)
		{
			this.NewConversation(conversation);
		}
		OnConversationUpdate();
	}

	private void OnJoinRecipients(JoinRecipients msg, PacketHeader header)
	{
		Durango.Logic.Social.Conversation conversation = GetConversation(msg.ConversationId);
		if (conversation != null)
		{
			conversation.AddEntityIds(msg.EntityIds);
			if (this.RecipientsJoined != null)
			{
				this.RecipientsJoined(conversation.Id, msg.EntityIds);
			}
		}
	}

	private void OnExitRecipient(ExitRecipient msg, PacketHeader header)
	{
		Durango.Logic.Social.Conversation conversation = GetConversation(msg.ConversationId);
		if (conversation != null)
		{
			if (conversation.IsIndividual && msg.EntityId == PlayerBehavior.LocalPlayer.EntityId)
			{
				_conversations.Remove(msg.ConversationId);
			}
			else
			{
				conversation.RemoveEntityId(msg.EntityId);
			}
			OnConversationUpdate();
			if (this.RecipientExited != null)
			{
				this.RecipientExited(conversation.Id, msg.EntityId);
			}
		}
	}

	private void OnSayConversation(SayInConversation msg, PacketHeader header)
	{
		Durango.Logic.Social.Conversation conversation = GetConversation(msg.ConversationId);
		if (conversation == null)
		{
			conversation = new Durango.Logic.Social.Conversation(msg.ConversationId, msg.Message.EntityId);
			_conversations[msg.ConversationId] = conversation;
			OnConversationUpdate();
		}
		ChatStruct chatStruct = new ChatStruct
		{
			EntityId = msg.Message.EntityId,
			Body = msg.Message.Body,
			Time = msg.Message.Time,
			Type = ChannelType.Conversation
		};
		if (msg.Message.Speaker.HasValue)
		{
			chatStruct.Name = msg.Message.Speaker.Value.Name;
		}
		conversation.AddMessage(chatStruct);
	}

	private void OnFollowerStatus(FollowerStatus msg, PacketHeader header)
	{
		if (this.FollowerStatusChanged != null)
		{
			this.FollowerStatusChanged(msg.EntityId, msg.Followed);
		}
	}

	private void OnFollowingStatus(FollowingStatus msg, PacketHeader header)
	{
		if (this.FollowingStatusChanged != null)
		{
			this.FollowingStatusChanged(msg.EntityId, msg.Online);
		}
	}

	private void OnFriendRequested(FriendRequested msg, PacketHeader header)
	{
		if (this.FriendRequested != null)
		{
			this.FriendRequested(msg.EntityId);
		}
		GetSocial();
	}

	private void OnFriendRequestAccepted(FriendRequestAccepted msg, PacketHeader header)
	{
		if (this.FriendRequestAccepted != null)
		{
			this.FriendRequestAccepted(msg.EntityId);
		}
	}

	private void OnTutorialColleagues(FollowTutorialColleagues msg, PacketHeader header)
	{
		if (msg.Colleagues.Length != 0)
		{
			for (int i = 0; i < msg.Colleagues.Length; i++)
			{
				Follow(msg.Colleagues[i], enable: true, null);
			}
			UIManager.MessageBox.Show(T._("같이 뗏목을 만들었던 {0}명의 플레이어들을 친구로 추가했습니다.", msg.Colleagues.Length.ToString()), (Action)delegate
			{
			}, (string)null);
		}
	}

	public void AddChat(ChatStruct chat)
	{
		if (chat.Body == null)
		{
			return;
		}
		chat.Time = ((!(chat.Time > 0.0)) ? Connections.Frontend.GetPredictedServerTime() : chat.Time);
		bool flag = false;
		if (chat.HasTranslatedText)
		{
			int num = _chattingList.FindIndex((ChatStruct x) => x.Time == chat.Time && x.EntityId == chat.EntityId);
			if (num != -1)
			{
				_chattingList.RemoveAt(num);
				flag = true;
			}
		}
		if (!chat.IsVolatile)
		{
			_chattingList.Add(chat);
			if (_chattingList.Count > 200)
			{
				_chattingList.RemoveAt(0);
			}
		}
		_chattingList.Sort(SortChattingList);
		if (this.ChatAdded != null)
		{
			this.ChatAdded(chat);
		}
		if (flag && this.ChatListChanged != null)
		{
			this.ChatListChanged();
		}
	}

	public static int SortChattingList(ChatStruct c1, ChatStruct c2)
	{
		if (c1.Time < c2.Time)
		{
			return -1;
		}
		return 1;
	}

	public void AddSystemChat(string chatText, string speakerName = "", bool remainColor = false, ChannelType channelType = ChannelType.System)
	{
		if (remainColor)
		{
			chatText = "[c]" + chatText + "[/c]";
		}
		AddChat(new ChatStruct
		{
			EntityId = "1000",
			Name = speakerName,
			Body = new RadioTalk
			{
				Text = chatText
			},
			Type = channelType
		});
	}

	public void HideChat(ChatableBase chatter)
	{
		if (this.ChatHided != null)
		{
			this.ChatHided(chatter);
		}
	}

	public void RemoveChat(ChannelType type)
	{
		bool flag = false;
		for (int num = _chattingList.Count - 1; num >= 0; num--)
		{
			if (_chattingList[num].Type == type)
			{
				_chattingList.RemoveAt(num);
				flag = true;
			}
		}
		if (flag && this.ChatListChanged != null)
		{
			this.ChatListChanged();
		}
	}

	public static bool IsVisibleChat(ChatStruct chat, ChatFilterType filter, string filterId = null)
	{
		bool flag = IsVisibleType(chat.Type, filter);
		if (flag && !string.IsNullOrEmpty(filterId))
		{
			return chat.EntityId == filterId;
		}
		return flag;
	}

	public static bool IsVisibleType(ChannelType type, ChatFilterType filter)
	{
		if (filter == ChatFilterType.All)
		{
			return true;
		}
		return ConvertToChannelType(filter) == type;
	}

	public static bool IsAllowedChannel(ChannelType type)
	{
		switch (type)
		{
		default:
			return type != ChannelType.Invalid;
		case ChannelType.Clan:
		case ChannelType.ClanWar:
			return GameSystem<ClanSystem>.Instance().PlayerClan != null;
		case ChannelType.Party:
			return GameSystem<PartySystem>.Instance().IsAcceptedInParty;
		case ChannelType.PersonalRegions:
			return GameManager.Region.Role() == Role.Personal;
		}
	}

	public static int ConversationComparison(Durango.Logic.Social.Conversation c1, Durango.Logic.Social.Conversation c2)
	{
		double lastestUpdateTime = c1.GetLastestUpdateTime();
		double lastestUpdateTime2 = c2.GetLastestUpdateTime();
		if (lastestUpdateTime > lastestUpdateTime2)
		{
			return -1;
		}
		if (lastestUpdateTime < lastestUpdateTime2)
		{
			return 1;
		}
		return 0;
	}

	private bool CheckQuickChatEnabled(bool wantCheckOnly = false)
	{
		if (Time.time < _quickChatEnableAt)
		{
			return false;
		}
		if (!wantCheckOnly)
		{
			_quickChatEnableAt = Time.time + 1f;
		}
		return true;
	}

	public void QuickSay(string message, bool isDictation = false)
	{
		if (!string.IsNullOrEmpty(message))
		{
			if (_currentChannel.Type == ChannelType.Conversation)
			{
				Say(_currentChannel.Id, message, isDictation);
			}
			else if (CheckQuickChatEnabled())
			{
				Say(message, isDictation);
			}
		}
	}

	private string StripSymbols(string text)
	{
		return ResourceSingleton<UILabelStyleTable>.Instance().StripStyle(NGUIText.StripSymbols(text));
	}

	public void Say(string message, bool isDictation = false)
	{
		ChannelType channelType = _currentChannel.Type;
		if (!IsAllowedChannel(channelType))
		{
			channelType = ChannelType.Region;
		}
		if (string.IsNullOrEmpty(message))
		{
			return;
		}
		if (IsBlockedContinuousChat(channelType))
		{
			AddContinuousChatTime(channelType);
			AddSystemChat(T._("너무 많은 메시지를 보내 잠시 입력이 제한됩니다."), T._("시스템"), remainColor: true, channelType);
			return;
		}
		PlayChatEmotionAnimation(message);
		if (!CanSayOrReconnect())
		{
			return;
		}
		SayInExclusiveChannel sayInExclusiveChannel = default(SayInExclusiveChannel);
		sayInExclusiveChannel.Message = new Message_
		{
			EntityId = GameManager.PlayerId,
			Time = Connections.Frontend.GetPredictedServerTime()
		};
		sayInExclusiveChannel.ChannelType = channelType;
		SayInExclusiveChannel msg = sayInExclusiveChannel;
		message = StripSymbols(message);
		if (!string.IsNullOrEmpty(message))
		{
			if (isDictation)
			{
				msg.Message.Body = new RadioDictation
				{
					Text = message
				};
			}
			else
			{
				msg.Message.Body = new RadioTalk
				{
					Text = message
				};
			}
			((GameManager.ClusterMode != 0) ? Connections.Frontend : Connections.Radiotower).Send(msg);
			AddContinuousChatTime(channelType);
		}
	}

	public void Say(string conversationId, string message, bool isDictation = false)
	{
		if (string.IsNullOrEmpty(message) || !CanSayOrReconnect())
		{
			return;
		}
		SayInConversation sayInConversation = default(SayInConversation);
		sayInConversation.Message = new Message_
		{
			EntityId = GameManager.PlayerId,
			Time = Connections.Frontend.GetPredictedServerTime()
		};
		sayInConversation.ConversationId = conversationId;
		SayInConversation msg = sayInConversation;
		message = StripSymbols(message);
		if (!string.IsNullOrEmpty(message))
		{
			if (isDictation)
			{
				msg.Message.Body = new RadioDictation
				{
					Text = message
				};
			}
			else
			{
				msg.Message.Body = new RadioTalk
				{
					Text = message
				};
			}
			Connections.Radiotower.Send(msg);
		}
	}

	public void SystemSay(object body, ChannelType? channelType = null, string conversationId = null)
	{
		if (!CanSayOrReconnect())
		{
			return;
		}
		if (!channelType.HasValue)
		{
			channelType = _currentChannel.Type;
		}
		if (IsAllowedChannel(channelType.Value))
		{
			Message_ message_ = default(Message_);
			message_.EntityId = GameManager.PlayerId;
			message_.Time = Connections.Frontend.GetPredictedServerTime();
			Message_ message = message_;
			if (channelType.GetValueOrDefault() == ChannelType.Conversation)
			{
				SayInConversation sayInConversation = default(SayInConversation);
				sayInConversation.Message = message;
				sayInConversation.ConversationId = ((!string.IsNullOrEmpty(conversationId)) ? conversationId : _currentChannel.Id);
				SayInConversation msg = sayInConversation;
				msg.Message.Body = body;
				Connections.Radiotower.Send(msg);
			}
			else
			{
				SayInExclusiveChannel sayInExclusiveChannel = default(SayInExclusiveChannel);
				sayInExclusiveChannel.Message = message;
				sayInExclusiveChannel.ChannelType = channelType.Value;
				SayInExclusiveChannel msg2 = sayInExclusiveChannel;
				msg2.Message.Body = body;
				Connections.Radiotower.Send(msg2);
			}
		}
	}

	private void GetAvailableEmotions()
	{
		Connections.Frontend.Send(default(GetAvailableEmotions));
	}

	private void OnAvailableEmotions(AvailableEmotions msg, PacketHeader header)
	{
		Emotional.Set(msg);
	}

	public void GetSocial(Action<Social> onSocial = null)
	{
		Connections.Frontend.Send(default(GetSocial)).On(delegate(Social msg, PacketHeader header)
		{
			SetSocial(msg, null);
			if (onSocial != null)
			{
				onSocial(msg);
			}
		});
	}

	private void SetSocial(Social social, Action onSuccess)
	{
		Social = social;
		onSuccess?.Invoke();
		if (this.SocialUpdated != null)
		{
			this.SocialUpdated();
		}
	}

	public bool IsFollowing(string entityId)
	{
		if (Social.FollowingEntityIds != null)
		{
			return Social.FollowingEntityIds.IndexOf(entityId) != -1;
		}
		return false;
	}

	public bool IsFriend(string entityId)
	{
		return GetFriendly(entityId) != Shared.Player.FriendType.Invalid;
	}

	public Shared.Player.FriendType GetFriendly(string entityId)
	{
		if (Social.FriendEntities != null)
		{
			return Social.FriendEntities.Get(entityId, Shared.Player.FriendType.Invalid);
		}
		return Shared.Player.FriendType.Invalid;
	}

	public bool IsFriendRequested(string entityId)
	{
		if (Social.ReceivedFriendRequests == null)
		{
			return false;
		}
		return Social.ReceivedFriendRequests.IndexOf(entityId) != -1;
	}

	public bool IsSentFriendRequested(string entityId)
	{
		if (Social.SentFriendRequests == null)
		{
			return false;
		}
		return Social.SentFriendRequests.IndexOf(entityId) != -1;
	}

	public bool IsBlocked(string entityId)
	{
		if (Social.BlockedEntityIds == null)
		{
			return false;
		}
		return Social.BlockedEntityIds.IndexOf(entityId) != -1;
	}

	public void ChangeFriendType(string entityId, Shared.Player.FriendType friendType)
	{
		Connections.Frontend.Send(new SetFriendType
		{
			EntityId = entityId,
			Type = friendType
		}).On(delegate(Social social, PacketHeader header)
		{
			SetSocial(social, null);
		});
	}

	public void AcceptFriendRequest(string entityId, Action onSuccess)
	{
		Connections.Frontend.Send(new AcceptFriendRequest
		{
			EntityId = entityId
		}).On(delegate(Social msg, PacketHeader header)
		{
			SetSocial(msg, onSuccess);
		}).Rest(delegate
		{
			GetSocial();
		});
	}

	public void CancelFriendRequest(string entityId, Action onSucceeded, Action onFailed)
	{
		Connections.Frontend.Send(new CancelFriendRequest
		{
			EntityId = entityId
		}).On(delegate(Social msg, PacketHeader header)
		{
			bool flag = msg.FriendEntities.ContainsKey(entityId);
			SetSocial(msg, (!flag) ? onSucceeded : onFailed);
		});
	}

	public void RefuseFriendRequest(string entityId, Action onSuccess)
	{
		Connections.Frontend.Send(new RefuseFriendRequest
		{
			EntityId = entityId
		}).On(delegate(Social msg, PacketHeader header)
		{
			SetSocial(msg, onSuccess);
		});
	}

	public void RequestFriend(string entityId, bool enable, Action onSuccess)
	{
		if (enable)
		{
			Connections.Frontend.Send(new RequestFriend
			{
				EntityId = entityId
			}).On(delegate(Social msg, PacketHeader header)
			{
				SetSocial(msg, onSuccess);
			});
		}
		else
		{
			Connections.Frontend.Send(new RemoveFriend
			{
				EntityId = entityId
			}).On(delegate(Social msg, PacketHeader header)
			{
				SetSocial(msg, onSuccess);
			});
		}
	}

	public static void GetMyFriendType(string entityId, Action<Messages.FriendType> onResult)
	{
		Connections.Frontend.Send(new GetMyFriendType
		{
			EntityId = entityId
		}).On(delegate(Messages.FriendType msg, PacketHeader packetHeader)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		});
	}

	public void Follow(string entityId, bool enable, Action onSuccess)
	{
		((!enable) ? Connections.Radiotower.Send(new Unfollow
		{
			EntityId = entityId
		}) : Connections.Radiotower.Send(new Follow
		{
			EntityId = entityId
		})).All(delegate(Packet packet)
		{
			if (Packet.IsSuccess(packet))
			{
				GetSocial();
			}
		});
	}

	public void Block(string entityId, bool block, Action onSuccess)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar;
		if (block)
		{
			Connections.Frontend.Send(new KickVisitor
			{
				EntityId = entityId,
				Silent = true
			});
			replyMessageHandlerRegistrar = Connections.Radiotower.Send(new Block
			{
				EntityId = entityId
			});
		}
		else
		{
			replyMessageHandlerRegistrar = Connections.Radiotower.Send(new Unblock
			{
				EntityId = entityId
			});
		}
		replyMessageHandlerRegistrar.All(delegate(Packet packet)
		{
			if (Packet.IsSuccess(packet))
			{
				GetSocial();
			}
		});
	}

	public void AddFavoriteRegionOwners(IEnumerable<string> entityIds)
	{
		Connections.Frontend.Send(new AddFavoriteRegionOwners
		{
			EntityIds = entityIds.ToArray()
		}).On(delegate(Social msg, PacketHeader header)
		{
			SetSocial(msg, null);
		});
	}

	public void RemoveFavoriteRegionOwners(string entityId)
	{
		Connections.Frontend.Send(new RemoveFavoriteRegionOwners
		{
			EntityIds = new string[1] { entityId }
		}).On(delegate(Social msg, PacketHeader header)
		{
			SetSocial(msg, null);
		});
	}

	private void OnSocial(Social msg, PacketHeader header)
	{
		SetSocial(msg, null);
	}

	private void OnPlayEmoticon(PlayEmoticon msg, PacketHeader header)
	{
		Emoticon emoticon = Emotional.GetEmoticon(msg.EmoticonId);
		if (emoticon != null)
		{
			Singleton<EmoticonEffectControl>.Instance().Show(msg.EntityId, emoticon);
		}
	}

	public void PlayEmoticon(Emoticon emoticon)
	{
		if (emoticon == null || !emoticon.Available)
		{
			return;
		}
		Singleton<EmoticonEffectControl>.Instance().Show(GameManager.PlayerId, emoticon, findLocalPlayer: true);
		if (CheckQuickChatEnabled(wantCheckOnly: true))
		{
			PlayEmoticon msg = default(PlayEmoticon);
			msg.EntityId = GameManager.PlayerId;
			msg.EmoticonId = emoticon.Key;
			Connections.Frontend.Send(msg);
			QuickSay("[icon=" + emoticon.Icon + "]");
			if (this.EmoticonPlayed != null)
			{
				this.EmoticonPlayed(emoticon.Key);
			}
		}
	}

	public bool PlayMotion(Durango.Logic.Social.Motion motion)
	{
		if ((bool)PlayerBehavior.LocalPlayer.IsMoving)
		{
			return false;
		}
		if (motion == null || !motion.Available)
		{
			UIManager.SystemMsg(T._("사용할 수 없는 모션입니다."));
			return false;
		}
		int size = KUtility.GetSize(motion.MotionNames);
		if (size == 0)
		{
			return false;
		}
		int num = UnityEngine.Random.Range(0, size);
		PlayerController.MotionUpdater.Motion(motion.MotionNames[num]);
		return true;
	}

	public Durango.Logic.Social.Conversation GetConversation(string id)
	{
		_conversations.TryGetValue(id, out var value);
		return value;
	}

	public void InviteToConversation(string conversationId, IList<string> players)
	{
		Connections.Radiotower.Send(new InviteToConversation
		{
			ConversationId = conversationId,
			RecipientEntityIds = players.ToArray()
		});
	}

	public void ExitConversation(string conversationId)
	{
		Connections.Radiotower.Send(new ExitConversation
		{
			ConversationId = conversationId
		});
		_conversations.Remove(conversationId);
		OnConversationUpdate();
	}

	public static ChannelType ConvertToChannelType(ChatFilterType chatFilterType, ChannelType defaultValue = ChannelType.Invalid)
	{
		return chatFilterType switch
		{
			ChatFilterType.Region => ChannelType.Region, 
			ChatFilterType.PersonalRegions => ChannelType.PersonalRegions, 
			ChatFilterType.Clan => ChannelType.Clan, 
			ChatFilterType.System => ChannelType.System, 
			ChatFilterType.ClanWar => ChannelType.ClanWar, 
			ChatFilterType.Party => ChannelType.Party, 
			_ => defaultValue, 
		};
	}

	public bool ToggleClanPush(ChatFilterType chatFilterType)
	{
		ChannelType key = ConvertToChannelType(chatFilterType);
		_clanChannelPushEnabled[key] = !_clanChannelPushEnabled.Get(key, defaultValue: false);
		Connections.Radiotower.Send(new ToggleClanNotification
		{
			ChannelNotificationsEnabled = _clanChannelPushEnabled
		});
		return _clanChannelPushEnabled.Get(key, defaultValue: false);
	}

	public static bool IsKindOfClanChannelFilter(ChatFilterType filterType)
	{
		if (filterType != ChatFilterType.Clan)
		{
			return filterType == ChatFilterType.ClanWar;
		}
		return true;
	}

	public bool IsClanPushEnabled(ChatFilterType chatFilterType)
	{
		ChannelType key = ConvertToChannelType(chatFilterType);
		return _clanChannelPushEnabled.Get(key, defaultValue: false);
	}

	public void AllowConversationPush(string conversationId, bool allowPush)
	{
		if (_conversations.TryGetValue(conversationId, out var value))
		{
			value.PushEnabled = allowPush;
		}
		Connections.Radiotower.Send(new ToggleConversationNotification
		{
			ConversationId = conversationId,
			Enabled = allowPush
		});
	}

	public void RequestConversation(string[] entityIds, Action<Durango.Logic.Social.Conversation> callback)
	{
		if (KUtility.GetSize(entityIds) == 0)
		{
			return;
		}
		Durango.Logic.Social.Conversation conversation = null;
		if (entityIds.Length == 1)
		{
			foreach (KeyValuePair<string, Durango.Logic.Social.Conversation> conversation2 in _conversations)
			{
				if (conversation2.Value.RepresentId == entityIds[0])
				{
					conversation = conversation2.Value;
					break;
				}
			}
		}
		if (conversation != null)
		{
			callback(conversation);
		}
		Connections.Radiotower.Send(new InviteToConversation
		{
			ConversationId = conversation?.Id,
			RecipientEntityIds = entityIds
		}).On<OK>(delegate
		{
			OnConversationUpdate();
		});
	}

	private void OnConversationUpdate()
	{
		ConversationsNewCount.BeginSetting();
		ConversationsNewCount.ClearChild();
		foreach (KeyValuePair<string, Durango.Logic.Social.Conversation> conversation in _conversations)
		{
			ConversationsNewCount.AddChild(conversation.Value);
		}
		ConversationsNewCount.EndSetting();
		if (this.ConversationsUpdated != null)
		{
			this.ConversationsUpdated();
		}
	}

	private void PlayChatEmotionAnimation(string text)
	{
		if (IsTextEmotion(text, Emotion.Smile))
		{
			PlayerController.MotionUpdater.Motion("Avatar_Laugh");
		}
		else if (IsTextEmotion(text, Emotion.Sad))
		{
			PlayerController.MotionUpdater.Motion("Avatar_Crying");
		}
		else if (IsTextEmotion(text, Emotion.Yes))
		{
			PlayerController.MotionUpdater.Motion("Avatar_Head_nod", 2f);
		}
		else if (IsTextEmotion(text, Emotion.No))
		{
			PlayerController.MotionUpdater.Motion("Avatar_Head_shake", 2f);
		}
		else if (IsTextEmotion(text, Emotion.Question))
		{
			PlayerController.MotionUpdater.Motion("Emotion_Wonder");
		}
		else if (UnityEngine.Random.value > 0.8f)
		{
			PlayerController.MotionUpdater.Motion("Avatar_Speak");
		}
	}

	public void ClanChanged()
	{
		string clanId = PlayerBehavior.LocalPlayer.ClanId;
		KUtility.DelayedCall(this, delegate
		{
			if (!(clanId != PlayerBehavior.LocalPlayer.ClanId))
			{
				Connections.Radiotower.Send(default(ResubscribeClanChannel));
			}
		}, 10f);
	}

	private bool IsBlockedContinuousChat(ChannelType channelType)
	{
		if (!IsBlockTargetChannelType(channelType))
		{
			return false;
		}
		if (_currentChatTimestamps.Count < 5)
		{
			return false;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		return _currentChatTimestamps.Peek() + 5.0 >= predictedServerTime;
	}

	private void AddContinuousChatTime(ChannelType channelType)
	{
		if (IsBlockTargetChannelType(channelType))
		{
			_currentChatTimestamps.Enqueue(Connections.Frontend.GetPredictedServerTime());
			if (_currentChatTimestamps.Count > 5)
			{
				_currentChatTimestamps.Dequeue();
			}
		}
	}

	private static bool IsBlockTargetChannelType(ChannelType channelType)
	{
		return channelType == ChannelType.Region;
	}

	public bool CanSay()
	{
		return _connectionHelper.State == RadiotowerConnectionHelper.ConnectState.Ready;
	}

	private bool CanSayOrReconnect()
	{
		if (CanSay())
		{
			return true;
		}
		MaybeTryReconnect();
		return false;
	}

	private void MaybeTryReconnect()
	{
		if (_connectionHelper.State == RadiotowerConnectionHelper.ConnectState.None)
		{
			_connectionHelper.TryConnect();
		}
	}

	private void ClearChats()
	{
		for (int num = _chattingList.Count - 1; num >= 0; num--)
		{
			if (_chattingList[num].Type != ChannelType.System)
			{
				_chattingList.RemoveAt(num);
			}
		}
		_conversations.Clear();
		_subscriptionCount.Clear();
	}

	public void SwitchToChannel(ChannelType channelType)
	{
		_currentChannel.Type = channelType;
		_currentChannel.Id = string.Empty;
	}

	public void SwitchToConversationChannel(string conversationId)
	{
		_currentChannel.Type = ChannelType.Conversation;
		_currentChannel.Id = conversationId;
	}

	public void SwitchChannel(int amount)
	{
		int num = amount;
		int num2 = Math.Sign(num);
		do
		{
			switch (_currentChannel.Type)
			{
			case ChannelType.Region:
			case ChannelType.Clan:
			case ChannelType.System:
			case ChannelType.ClanWar:
			case ChannelType.Party:
			case ChannelType.PersonalRegions:
				SwitchToChannel(ChangeChannel(num2));
				if (_currentChannel.Type != ChannelType.Conversation)
				{
					break;
				}
				if (_conversations.Keys.Count == 0)
				{
					SwitchToChannel(ChangeChannel((num2 == 0) ? 1 : num2));
					break;
				}
				goto case ChannelType.Conversation;
			case ChannelType.Conversation:
			{
				List<string> list = _conversations.Keys.ToList();
				int num3 = list.IndexOf((string elem) => elem == _currentChannel.Id);
				if (num3 == -1 && list.Count > 0)
				{
					string id = ((num2 <= 0) ? list.LastOrDefault() : list.FirstOrDefault());
					_currentChannel.Id = id;
					break;
				}
				int num4 = num3 + num2;
				if (num4 < 0 || num4 > list.Count - 1)
				{
					SwitchToChannel(ChangeChannel((num2 == 0) ? 1 : num2));
				}
				else
				{
					_currentChannel.Id = list[num4];
				}
				break;
			}
			default:
				num = 0;
				_currentChannel.Type = ChannelType.Region;
				break;
			}
			num -= num2;
		}
		while (num != 0);
	}

	private ChannelType ChangeChannel(int amount)
	{
		int num = AllowedChannelCount();
		int index = (AllowChannelIndexOf(_currentChannel.Type) + num + amount) % num;
		return AllowedChannel(index);
	}

	private int AllowedChannelCount()
	{
		int num = 0;
		ChannelType[] channels = _channels;
		for (int i = 0; i < channels.Length; i++)
		{
			if (IsAllowedChannel(channels[i]))
			{
				num++;
			}
		}
		return num;
	}

	private int AllowChannelIndexOf(ChannelType type)
	{
		int num = -1;
		ChannelType[] channels = _channels;
		foreach (ChannelType channelType in channels)
		{
			if (IsAllowedChannel(channelType))
			{
				num++;
				if (type == channelType)
				{
					return num;
				}
			}
		}
		return -1;
	}

	private ChannelType AllowedChannel(int index)
	{
		int num = -1;
		ChannelType[] channels = _channels;
		foreach (ChannelType channelType in channels)
		{
			if (IsAllowedChannel(channelType))
			{
				num++;
				if (num == index)
				{
					return channelType;
				}
			}
		}
		return ChannelType.Region;
	}

	public void SetSocialOption(SocialOptionType type, bool value)
	{
		if (_socialOptions.Options == null || _socialOptions.Options.Get(type, defaultValue: false) != value)
		{
			Dictionary<SocialOptionType, bool> dictionary = _socialOptions.Options;
			if (dictionary == null)
			{
				dictionary = new Dictionary<SocialOptionType, bool>();
			}
			dictionary[type] = value;
			Connections.Frontend.Send(new SetSocialOptions
			{
				Options = dictionary
			});
		}
	}

	public void SetEndpoints(List<KeyValuePair<string, int>> endpoints)
	{
		_connectionHelper.SetEndpoints(endpoints);
	}
}
