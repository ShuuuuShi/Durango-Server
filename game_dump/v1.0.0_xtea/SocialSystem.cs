using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ChatData;
using K1Network;
using Messages;
using Newtonsoft.Json;
using Shared.Chat;
using UnityEngine;

public class SocialSystem : GameSystem<SocialSystem>
{
	internal class RadiotowerConnectionHelper
	{
		private enum ConnectState
		{
			None,
			Connecting,
			Connected
		}

		private ConnectState _connectState;

		public event Action OnAuthSuccess;

		public event Action<Messages.Conversation[]> OnConversationList;

		public RadiotowerConnectionHelper()
		{
			Connections.Radiotower.ConnetionClosed += ServerConnectionClosed;
		}

		public void TryConnect()
		{
			if (!Connections.Radiotower.Connected() && !string.IsNullOrEmpty(GameSystem<SocialSystem>.Instance().Host))
			{
				StartCoroutine(CoConnect());
			}
		}

		private IEnumerator CoConnect()
		{
			SocialSystem chatSys = GameSystem<SocialSystem>.Instance();
			_connectState = ConnectState.Connecting;
			Connections.Radiotower.ConnectAsync(chatSys.Host, chatSys.Port);
			while (!Connections.Radiotower.Connected() && _connectState == ConnectState.Connecting)
			{
				yield return null;
			}
			if (!Connections.Radiotower.Connected())
			{
				_connectState = ConnectState.None;
				yield break;
			}
			_connectState = ConnectState.Connected;
			Connections.Radiotower.StartReceive();
			RequestAuth();
		}

		private IEnumerator TryReconnect()
		{
			_connectState = ConnectState.None;
			if (Connections.Radiotower.Connected() || string.IsNullOrEmpty(GameSystem<SocialSystem>.Instance().Host))
			{
				yield break;
			}
			int tryCount = 4;
			while (true)
			{
				int num;
				tryCount = (num = tryCount - 1);
				if (num < 0)
				{
					break;
				}
				yield return StartCoroutine(CoConnect());
				if (Connections.Radiotower.Connected())
				{
					yield break;
				}
				yield return (object)new WaitForSeconds(1f);
			}
			_connectState = ConnectState.None;
		}

		private void ServerConnectionClosed()
		{
			if (_connectState == ConnectState.Connected && Connections.Frontend.Connected())
			{
				StartCoroutine(TryReconnect());
			}
			else
			{
				_connectState = ConnectState.None;
			}
		}

		private Coroutine StartCoroutine(IEnumerator routine)
		{
			return ((MonoBehaviour)GameSystem<SocialSystem>.Instance()).StartCoroutine(routine);
		}

		public void RequestAuth()
		{
			Connections.Radiotower.Send(new Tune
			{
				EntityId = GameManager.PlayerId,
				SessionToken = KSingleton<GameManager>.Instance().SessionToken,
				SyncedAt = 0.0
			}).On<Conversations>(OnConversations);
		}

		private void OnConversations(Conversations msg, PacketHeader header)
		{
			if (this.OnConversationList != null)
			{
				this.OnConversationList(msg._Conversations);
			}
			_connectState = ConnectState.Connected;
			if (this.OnAuthSuccess != null)
			{
				this.OnAuthSuccess();
			}
		}
	}

	private const int MaxChatLogCount = 200;

	public readonly NewCheckerContainer ConversationsNewCount = new NewCheckerContainer();

	private Dictionary<Emotion, EmotionJson> _chatEmotionData;

	private readonly List<ChatStruct> _chattingList = new List<ChatStruct>();

	private readonly Dictionary<ulong, ChatData.Conversation> _conversations = new Dictionary<ulong, ChatData.Conversation>();

	private readonly Dictionary<ChannelType, uint> _subscriptionCount = new Dictionary<ChannelType, uint>(default(ChannelTypeComparer));

	private readonly List<ulong> _followingList = new List<ulong>();

	private readonly List<ulong> _blockList = new List<ulong>();

	private RadiotowerConnectionHelper _connectionHelper;

	private string _conversationsReadAtCachePath;

	public string Host { get; set; }

	public int Port { get; set; }

	public List<ChatStruct> ChattingList => _chattingList;

	public List<ulong> FollowingList => _followingList;

	public List<ulong> BlockList => _blockList;

	public Dictionary<ulong, ChatData.Conversation> Conversations => _conversations;

	private string ConversationsReadAtCachePath
	{
		get
		{
			if (_conversationsReadAtCachePath == null)
			{
				_conversationsReadAtCachePath = $"Players/{GameManager.PlayerId}/Conversations.txt";
			}
			return _conversationsReadAtCachePath;
		}
	}

	public ChannelType SayChannel { get; private set; }

	public ulong SayConversation { get; private set; }

	public event Action<ChatStruct> ChatAdded;

	public event Action<ChatableBase> ChatHided;

	public event Action<ulong, bool> FollowerStatusChanged;

	public event Action<ulong, bool> FollowingStatusChanged;

	public event Action FollowingListUpdated;

	public event Action BlockListUpdated;

	public event Action ChatListChanged;

	public event Action SubscriptionCountChanged;

	public event Action ConversationsUpdated;

	public event Action<ulong, ulong[]> RecipientsJoined;

	public event Action<ulong, ulong> RecipientExited;

	private void Awake()
	{
		Dictionary<Emotion, EmotionJson> dictionary = KUtility.ParseJsonFile<Dictionary<Emotion, EmotionJson>>("chat_emotion");
		_chatEmotionData = new Dictionary<Emotion, EmotionJson>(dictionary, default(EmotionComparer));
		Connections.Radiotower.On<SayInExclusiveChannel>(OnSay);
		Connections.Radiotower.On<SayInConversation>(OnSayConversation);
		Connections.Radiotower.On<SubscriptionCount>(OnSubscriptionCount);
		Connections.Radiotower.On<FollowerStatus>(OnFollowerStatus);
		Connections.Radiotower.On<FollowingStatus>(OnFollowingStatus);
		Connections.Radiotower.On<Messages.Conversation>(OnConversation);
		Connections.Radiotower.On<JoinRecipients>(OnJoinRecipients);
		Connections.Radiotower.On<ExitRecipient>(OnExitRecipient);
		Connections.Frontend.On<FollowTutorialColleagues>(OnTutorialColleagues);
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (KSingleton<GameManager>.Instance().IsEmigrated)
			{
				AddSystemChat(LocalizeSystem.Format("#chatsystem_emigrated_to_other_island_text", KSingleton<GameManager>.Instance().Region.Name), string.Empty);
			}
			if (!GameManager.IsPrologueMode)
			{
				if (_connectionHelper == null)
				{
					_connectionHelper = new RadiotowerConnectionHelper();
					_connectionHelper.OnAuthSuccess += OnAuthSuccess;
					_connectionHelper.OnConversationList += OnConversations;
				}
				_connectionHelper.TryConnect();
			}
		};
		KSingleton<GameManager>.Instance().PreReconnect += delegate
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
		};
		KSingleton<GameManager>.Instance().PostReconnect += delegate
		{
			_connectionHelper.TryConnect();
		};
		SayChannel = ChannelType.Region;
		GameSystem<ClanSystem>.Instance().ClanChanged += delegate
		{
			ClanChanged();
		};
	}

	private void OnDisable()
	{
		SaveConversations();
	}

	private void LoadConversations()
	{
		FileStream fileStream = KFileUtil.GetFileStream(ConversationsReadAtCachePath);
		if (fileStream == null)
		{
			return;
		}
		string text;
		using (TextReader textReader = new StreamReader(fileStream))
		{
			text = textReader.ReadToEnd();
		}
		fileStream.Close();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Dictionary<ulong, ConversationJson> dictionary = KUtility.ParseJson<Dictionary<ulong, ConversationJson>>(text);
		foreach (KeyValuePair<ulong, ConversationJson> item in dictionary)
		{
			ChatData.Conversation conversation = GetConversation(item.Key);
			if (conversation != null)
			{
				conversation.ReadAt = item.Value.ReadAt;
				conversation.CustomName = item.Value.CustomName;
				conversation.PushEnabled = item.Value.PushEnabled;
			}
		}
	}

	public void SaveConversations()
	{
		FileStream fileStream = KFileUtil.GetFileStream(ConversationsReadAtCachePath, FileMode.Create);
		if (fileStream == null)
		{
			return;
		}
		Dictionary<ulong, ConversationJson> dictionary = new Dictionary<ulong, ConversationJson>();
		foreach (KeyValuePair<ulong, ChatData.Conversation> conversation in _conversations)
		{
			dictionary.Add(conversation.Key, new ConversationJson
			{
				ReadAt = conversation.Value.ReadAt,
				CustomName = conversation.Value.CustomName,
				PushEnabled = conversation.Value.PushEnabled
			});
		}
		string value = JsonConvert.SerializeObject(dictionary);
		using (TextWriter textWriter = new StreamWriter(fileStream))
		{
			textWriter.Write(value);
		}
		fileStream.Close();
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
			Emotion emotion2 = (Emotion)(int)values.GetValue(i);
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

	public static string GetTextInMessage(Message_ message)
	{
		if (message.Body is RadioTalk)
		{
			return ((RadioTalk)message.Body).Text;
		}
		if (message.Body is RadioDictation)
		{
			return ((RadioDictation)message.Body).Text;
		}
		if (message.Body is RadioText)
		{
			return ((RadioText)message.Body).Text;
		}
		if (message.Body is RadioNotice)
		{
			return ((RadioNotice)message.Body).Text;
		}
		return null;
	}

	private void OnAuthSuccess()
	{
		GetLatestChatLog();
		Connections.Radiotower.Send(default(GetFollowing)).On(delegate(Following msg, PacketHeader header)
		{
			_followingList.Clear();
			_followingList.AddRange(msg.FollowingEntityIds);
			if (this.FollowingListUpdated != null)
			{
				this.FollowingListUpdated();
			}
		});
		Connections.Radiotower.Send(default(GetBlocklist)).On(delegate(Blocklist msg, PacketHeader header)
		{
			_blockList.Clear();
			_blockList.AddRange(msg.EntityIds);
			if (this.BlockListUpdated != null)
			{
				this.BlockListUpdated();
			}
		});
	}

	private void GetLatestChatLog()
	{
		bool isReceiveRegion = false;
		bool isReceiveClan = false;
		Connections.Radiotower.Send(new GetLatestChatLog
		{
			ChannelType = ChannelType.Region
		}).On(delegate(ChatLogs msg, PacketHeader header)
		{
			ReceiveChatLog(msg, ChannelType.Region);
			isReceiveRegion = true;
			if (isReceiveRegion && isReceiveClan)
			{
				ReceivedAllChatLog();
			}
		}).On<Error>(delegate
		{
			isReceiveRegion = true;
			if (isReceiveRegion && isReceiveClan)
			{
				ReceivedAllChatLog();
			}
		});
		Connections.Radiotower.Send(new GetLatestChatLog
		{
			ChannelType = ChannelType.Clan
		}).On(delegate(ChatLogs msg, PacketHeader header)
		{
			ReceiveChatLog(msg, ChannelType.Clan);
			isReceiveClan = true;
			if (isReceiveRegion && isReceiveClan)
			{
				ReceivedAllChatLog();
			}
		}).On<Error>(delegate
		{
			isReceiveClan = true;
			if (isReceiveRegion && isReceiveClan)
			{
				ReceivedAllChatLog();
			}
		});
	}

	private void ReceiveChatLog(ChatLogs msg, ChannelType type)
	{
		for (int i = 0; i < msg.Logs.Length; i++)
		{
			Message_ message_ = msg.Logs[i];
			ChatStruct chatStruct = default(ChatStruct);
			chatStruct.EntityId = message_.EntityId;
			chatStruct.Body = message_.Body;
			chatStruct.Time = message_.Time;
			chatStruct.Type = type;
			ChatStruct item = chatStruct;
			_chattingList.Add(item);
		}
	}

	private void ReceivedAllChatLog()
	{
		_chattingList.Sort((ChatStruct c1, ChatStruct c2) => (!(c1.Time < c2.Time)) ? 1 : (-1));
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
			this.SubscriptionCountChanged();
		}
	}

	private void OnSay(SayInExclusiveChannel msg, PacketHeader header)
	{
		if (msg.Message.Body is RadioNotice && KSingleton<UIManager>.HasInstance())
		{
			UIManager.SystemMsg(((RadioNotice)msg.Message.Body).Text, 3f);
		}
		ChatStruct chatStruct = default(ChatStruct);
		chatStruct.EntityId = msg.Message.EntityId;
		chatStruct.Body = msg.Message.Body;
		chatStruct.Time = msg.Message.Time;
		chatStruct.Type = msg.ChannelType;
		ChatStruct chat = chatStruct;
		AddChat(chat);
	}

	private void OnConversation(Messages.Conversation msg, PacketHeader header)
	{
		ChatData.Conversation conversation = new ChatData.Conversation(msg);
		_conversations[conversation.Id] = conversation;
		OnConversationUpdate();
	}

	private void OnJoinRecipients(JoinRecipients msg, PacketHeader header)
	{
		ChatData.Conversation conversation = GetConversation(msg.ConversationId);
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
		ChatData.Conversation conversation = GetConversation(msg.ConversationId);
		if (conversation != null)
		{
			conversation.RemoveEntityId(msg.EntityId);
			if (this.RecipientExited != null)
			{
				this.RecipientExited(conversation.Id, msg.EntityId);
			}
		}
	}

	private void OnSayConversation(SayInConversation msg, PacketHeader header)
	{
		ChatData.Conversation conversation = GetConversation(msg.ConversationId);
		if (conversation == null)
		{
			conversation = new ChatData.Conversation(msg.ConversationId, msg.Message.EntityId);
			_conversations[msg.ConversationId] = conversation;
			OnConversationUpdate();
		}
		ChatStruct chatStruct = default(ChatStruct);
		chatStruct.EntityId = msg.Message.EntityId;
		chatStruct.Body = msg.Message.Body;
		chatStruct.Time = msg.Message.Time;
		chatStruct.Type = ChannelType.Conversation;
		ChatStruct chat = chatStruct;
		conversation.AddMessage(chat);
	}

	private void OnConversations(Messages.Conversation[] conversations)
	{
		int i = 0;
		for (int num = conversations.Length; i < num; i++)
		{
			_conversations[conversations[i].Id] = new ChatData.Conversation(conversations[i]);
		}
		LoadConversations();
		UpdateConversationNewBadge();
		UpdateConversationPushSetting();
		OnConversationUpdate();
	}

	private void UpdateConversationNewBadge()
	{
		Dictionary<ulong, ChatData.Conversation>.Enumerator enumerator = _conversations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.UpdateNewCount();
		}
	}

	private void UpdateConversationPushSetting()
	{
		Dictionary<ulong, ChatData.Conversation>.Enumerator enumerator = _conversations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.ReadAt == 0.0)
			{
				AllowConversationPush(enumerator.Current.Key, enumerator.Current.Value.PushEnabled);
			}
		}
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

	private void OnTutorialColleagues(FollowTutorialColleagues msg, PacketHeader header)
	{
		if (msg.Colleagues.Length > 0)
		{
			for (int i = 0; i < msg.Colleagues.Length; i++)
			{
				Follow(msg.Colleagues[i], null, toggle: false);
			}
			UIManager.MessageBox.Show(LocalizeSystem.Format("#follow_tutorial_colleagues", msg.Colleagues.Length.ToString()), (Action)delegate
			{
			});
		}
	}

	public void AddChat(ChatStruct chat)
	{
		if (chat.Body == null)
		{
			return;
		}
		chat.Time = ((!(chat.Time > 0.0)) ? Connections.Frontend.GetPredictedServerTime() : chat.Time);
		if (!chat.IsVolatile)
		{
			_chattingList.Add(chat);
			if (_chattingList.Count > 200)
			{
				_chattingList.RemoveAt(0);
			}
		}
		if (this.ChatAdded != null)
		{
			this.ChatAdded(chat);
		}
	}

	public void AddSystemChat(string chatText, string speakerName = "", bool remainColor = false)
	{
		if (remainColor)
		{
			chatText = "[c]" + chatText + "[/c]";
		}
		AddChat(new ChatStruct
		{
			EntityId = 1000uL,
			Name = speakerName,
			Body = new RadioTalk
			{
				Text = chatText
			},
			Type = ChannelType.System
		});
	}

	public void HideChat(ChatableBase chatter)
	{
		if (this.ChatHided != null)
		{
			this.ChatHided(chatter);
		}
	}

	public void RemoveChat(ulong id, ChannelType type)
	{
		for (int num = _chattingList.Count - 1; num >= 0; num--)
		{
			if (_chattingList[num].EntityId == id && _chattingList[num].Type == type)
			{
				_chattingList.RemoveAt(num);
			}
		}
		if (this.ChatListChanged != null)
		{
			this.ChatListChanged();
		}
	}

	public static bool IsVisibleFilter(ChatStruct chat, ChatFilterType filter, ulong filterId = 0)
	{
		bool flag = IsVisibleType(chat.Type, filter);
		if (flag && filterId != 0L)
		{
			return chat.EntityId == filterId;
		}
		return flag;
	}

	public static bool IsVisibleType(ChannelType type, ChatFilterType filter)
	{
		bool result = false;
		switch (filter)
		{
		case ChatFilterType.All:
			result = true;
			break;
		case ChatFilterType.Public:
			result = type == ChannelType.Region;
			break;
		case ChatFilterType.Clan:
			result = type == ChannelType.Clan;
			break;
		case ChatFilterType.System:
			result = type == ChannelType.System;
			break;
		}
		return result;
	}

	public void SetBaseSayChannel(ChannelType channelType)
	{
		SayChannel = channelType;
		SayConversation = 0uL;
	}

	public void SetBaseSayChannel(ulong conversationId)
	{
		SayChannel = ChannelType.Conversation;
		SayConversation = conversationId;
	}

	public void Say(string message, bool isDictation = false)
	{
		if (SayChannel == ChannelType.Conversation)
		{
			Say(SayConversation, message, isDictation);
		}
		else
		{
			Say(SayChannel, message, isDictation);
		}
	}

	public void Say(ChannelType channelType, string message, bool isDictation = false)
	{
		if (!string.IsNullOrEmpty(message))
		{
			PlayChatEmotionAnimation(message);
			MaybeTryReconnect();
			SayInExclusiveChannel sayInExclusiveChannel = default(SayInExclusiveChannel);
			sayInExclusiveChannel.Message = new Message_
			{
				EntityId = GameManager.PlayerId,
				Time = Connections.Frontend.GetPredictedServerTime()
			};
			sayInExclusiveChannel.ChannelType = channelType;
			SayInExclusiveChannel msg = sayInExclusiveChannel;
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
			Connections.Radiotower.Send(msg).On<OK>(OnTalkMsgSuccess);
		}
	}

	public void Say(ulong conversationId, string message, bool isDictation = false)
	{
		if (!string.IsNullOrEmpty(message))
		{
			SayInConversation sayInConversation = default(SayInConversation);
			sayInConversation.Message = new Message_
			{
				EntityId = GameManager.PlayerId,
				Time = Connections.Frontend.GetPredictedServerTime()
			};
			sayInConversation.ConversationId = conversationId;
			SayInConversation msg = sayInConversation;
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
			Connections.Radiotower.Send(msg).On<OK>(OnTalkMsgSuccess);
		}
	}

	public void Ping(ChannelType channelType, ulong regionId, Point2 tile)
	{
		MaybeTryReconnect();
		SayInExclusiveChannel sayInExclusiveChannel = default(SayInExclusiveChannel);
		sayInExclusiveChannel.Message = new Message_
		{
			EntityId = GameManager.PlayerId,
			Time = Connections.Frontend.GetPredictedServerTime()
		};
		sayInExclusiveChannel.ChannelType = channelType;
		SayInExclusiveChannel msg = sayInExclusiveChannel;
		msg.Message.Body = new RadioPin
		{
			RegionId = regionId,
			Tile = tile
		};
		Connections.Radiotower.Send(msg).On<OK>(OnTalkMsgSuccess);
	}

	public void Follow(ulong entityId, Action success = null, bool toggle = true)
	{
		SendSocialListUpdateMsg(entityId, _followingList, delegate
		{
			Follow result2 = default(Follow);
			result2.EntityId = entityId;
			return result2;
		}, delegate
		{
			Unfollow result = default(Unfollow);
			result.EntityId = entityId;
			return result;
		}, this.FollowingListUpdated, success, toggle);
	}

	public void Block(ulong entityId, Action success = null, bool toggle = true)
	{
		SendSocialListUpdateMsg(entityId, _blockList, delegate
		{
			Block result2 = default(Block);
			result2.EntityId = entityId;
			return result2;
		}, delegate
		{
			Unblock result = default(Unblock);
			result.EntityId = entityId;
			return result;
		}, this.BlockListUpdated, success, toggle);
	}

	private static void SendSocialListUpdateMsg<T, TU>(ulong entityId, List<ulong> socialList, Func<T> msgAdd, Func<TU> msgRemove, Action actionUpdated, Action actionSuccess, bool toggle)
	{
		if (!socialList.Contains(entityId))
		{
			Connections.Radiotower.Send(msgAdd()).On<OK>(delegate
			{
				socialList.Add(entityId);
				if (actionUpdated != null)
				{
					actionUpdated();
				}
				if (actionSuccess != null)
				{
					actionSuccess();
				}
			});
		}
		else
		{
			if (!toggle)
			{
				return;
			}
			Connections.Radiotower.Send(msgRemove()).On<OK>(delegate
			{
				socialList.Remove(entityId);
				if (actionUpdated != null)
				{
					actionUpdated();
				}
				if (actionSuccess != null)
				{
					actionSuccess();
				}
			});
		}
	}

	public ChatData.Conversation GetConversation(ulong id)
	{
		_conversations.TryGetValue(id, out var value);
		return value;
	}

	public void InviteToConversation(ulong conversationId, ulong entityId)
	{
		Connections.Radiotower.Send(new InviteToConversation
		{
			ConversationId = conversationId,
			RecipientEntityIds = new ulong[1] { entityId }
		});
	}

	public void ExitConversation(ulong conversationId)
	{
		Connections.Radiotower.Send(new ExitConversation
		{
			ConversationId = conversationId
		});
		_conversations.Remove(conversationId);
		OnConversationUpdate();
		SaveConversations();
	}

	public void AllowConversationPush(ulong conversationId, bool allowPush)
	{
		Connections.Radiotower.Send(new AllowConversationPushMessage
		{
			ConversationId = conversationId,
			Allow = allowPush
		});
	}

	public void RequestConversation(ulong[] entityIds, Action<ChatData.Conversation> callback)
	{
		if (entityIds == null || entityIds.Length == 0)
		{
			return;
		}
		ChatData.Conversation conversation = null;
		if (entityIds.Length == 1)
		{
			Dictionary<ulong, ChatData.Conversation>.Enumerator enumerator = _conversations.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.RepresentId == entityIds[0])
				{
					conversation = enumerator.Current.Value;
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
		ConversationsNewCount.ClearChild();
		Dictionary<ulong, ChatData.Conversation>.Enumerator enumerator = _conversations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ConversationsNewCount.AddChild(enumerator.Current.Value);
		}
		if (this.ConversationsUpdated != null)
		{
			this.ConversationsUpdated();
		}
	}

	private void PlayChatEmotionAnimation(string text)
	{
		if (IsTextEmotion(text, Emotion.Smile))
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Laugh");
		}
		else if (IsTextEmotion(text, Emotion.Sad))
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Crying");
		}
		else if (IsTextEmotion(text, Emotion.Yes))
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Head_nod", 2f);
		}
		else if (IsTextEmotion(text, Emotion.No))
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Head_shake", 2f);
		}
		else if (IsTextEmotion(text, Emotion.Question))
		{
			KSingleton<PlayerController>.Instance().Motion("Emotion_Wonder");
		}
		else if (Random.value > 0.8f)
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Speak");
		}
	}

	private void OnTalkMsgSuccess(OK msg, PacketHeader header)
	{
	}

	public void ClanChanged()
	{
		((MonoBehaviour)this).StartCoroutine(SendUpdateClan());
	}

	private IEnumerator SendUpdateClan()
	{
		yield return (object)new WaitForSeconds(10f);
		Connections.Radiotower.Send(default(ResubscribeClanChannel));
	}

	public static bool HasCharacter(string text)
	{
		return UISpriteLabel.HasCharacter(text, KSingleton<UIManager>.Instance().Atlases);
	}

	private void MaybeTryReconnect()
	{
		if (!Connections.Radiotower.Connected() && !Connections.Radiotower.IsAttemptingToConnect() && _connectionHelper != null)
		{
			_connectionHelper.TryConnect();
		}
	}
}
