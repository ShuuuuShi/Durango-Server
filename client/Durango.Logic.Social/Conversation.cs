using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Notification;
using Durango.Network;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Chat;

namespace Durango.Logic.Social;

public class Conversation : INotificationable
{
	private Countable _notification;

	private readonly HashSet<string> _entitySet = new HashSet<string>();

	private string _representId;

	private readonly double _makeAt;

	public bool PushEnabled { get; set; }

	public string Id { get; private set; }

	public List<ChatStruct> Messages { get; private set; }

	public bool IsIndividual => EntityCount == 2;

	public bool IsGroup => EntityCount > 2;

	public bool IsEmpty => EntityCount == 1;

	public int EntityCount => _entitySet.Count;

	public string RepresentId
	{
		get
		{
			if (IsGroup)
			{
				return string.Empty;
			}
			if (!string.IsNullOrEmpty(_representId))
			{
				return _representId;
			}
			string playerId = GameManager.PlayerId;
			foreach (string item in _entitySet)
			{
				if (item != playerId)
				{
					_representId = item;
					break;
				}
			}
			return _representId;
		}
	}

	[CanBeNull]
	public string CustomName
	{
		get
		{
			return GameSystem<SocialSystem>.Instance().ChannelInfo.GetCustomName(Id);
		}
		set
		{
			GameSystem<SocialSystem>.Instance().ChannelInfo.SetCustomName(Id, value);
		}
	}

	private double ReadAt
	{
		get
		{
			return GameSystem<SocialSystem>.Instance().ChannelInfo.GetReadAt(Id);
		}
		set
		{
			GameSystem<SocialSystem>.Instance().ChannelInfo.SetReadAt(Id, value);
		}
	}

	public Durango.Logic.Notification.Notification Notification
	{
		get
		{
			if (_notification == null)
			{
				_notification = new Countable(Durango.Logic.Notification.Type.Important, ViewType.Count);
			}
			return _notification;
		}
	}

	public static event Action<Conversation> MessagesUpdated;

	public Conversation(string id, string entityId)
	{
		PushEnabled = true;
		Id = id;
		_entitySet.Add(GameManager.PlayerId);
		_entitySet.Add(entityId);
		_representId = entityId;
		Messages = new List<ChatStruct>();
		_makeAt = Connections.Frontend.GetPredictedServerTime();
	}

	public Conversation(global::Messages.Conversation msg)
	{
		PushEnabled = msg.Notification;
		Id = msg.Id;
		for (int i = 0; i < msg.EntityIds.Length; i++)
		{
			_entitySet.Add(msg.EntityIds[i]);
		}
		Messages = new List<ChatStruct>(msg.Messages.Length);
		int j = 0;
		for (int num = msg.Messages.Length; j < num; j++)
		{
			Message_ message_ = msg.Messages[j];
			string name = null;
			if (message_.Speaker.HasValue)
			{
				name = message_.Speaker.Value.Name;
			}
			Messages.Add(new ChatStruct
			{
				EntityId = message_.EntityId,
				Body = message_.Body,
				Time = message_.Time,
				Type = ChannelType.Conversation,
				Name = name
			});
		}
		_makeAt = Connections.Frontend.GetPredictedServerTime();
		UpdateNewCount();
	}

	public bool GetTitle([NotNull] Action<string> onResult)
	{
		if (!string.IsNullOrEmpty(CustomName))
		{
			onResult(CustomName);
			return true;
		}
		if (IsEmpty)
		{
			onResult(T._("빈 그룹"));
			return true;
		}
		if (IsIndividual)
		{
			bool flag = false;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(RepresentId, delegate(Durango.Player.PlayerInfo info)
			{
				flag = true;
				onResult(info.Valid ? $"{info.Name} {info.Freq:0000} kHz" : T._("알수없음"));
			});
			return flag;
		}
		onResult(T._("그룹 채팅 {0}명", EntityCount));
		return true;
	}

	public void AddMessage(ChatStruct chat)
	{
		Messages.Add(chat);
		if (chat.EntityId != GameManager.PlayerId)
		{
			Notification.Count++;
		}
		if (Conversation.MessagesUpdated != null)
		{
			Conversation.MessagesUpdated(this);
		}
	}

	public void AddEntityIds(string[] ids)
	{
		_entitySet.UnionWith(ids);
	}

	public void RemoveEntityId(string id)
	{
		_entitySet.Remove(id);
	}

	public string[] GetEntityIds()
	{
		return _entitySet.ToArray();
	}

	public void FillEntityIds(HashSet<string> target)
	{
		target?.UnionWith(_entitySet);
	}

	public bool Contains(string entityId)
	{
		return _entitySet.Contains(entityId);
	}

	public double GetLastestUpdateTime()
	{
		return (Messages.Count != 0) ? Messages[Messages.Count - 1].Time : _makeAt;
	}

	private void UpdateNewCount()
	{
		int num = 0;
		int i = 0;
		for (int count = Messages.Count; i < count; i++)
		{
			ChatStruct chatStruct = Messages[i];
			if (!(chatStruct.EntityId == GameManager.PlayerId) && chatStruct.Time > ReadAt)
			{
				num++;
			}
		}
		Notification.Count = num;
	}

	public void MarkAsRead()
	{
		Notification.Count = 0;
		ReadAt = GetLastestUpdateTime();
	}
}
