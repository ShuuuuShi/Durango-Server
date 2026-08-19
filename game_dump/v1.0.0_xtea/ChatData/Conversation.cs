using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using Shared.Chat;

namespace ChatData;

public class Conversation : INewCheckerable
{
	private readonly NewCheckerCountableNode _newChecker = new NewCheckerCountableNode();

	private readonly HashSet<ulong> _entitySet = new HashSet<ulong>();

	private ulong _representId;

	private readonly double _makeAt;

	public bool PushEnabled { get; set; }

	public ulong Id { get; private set; }

	public List<ChatStruct> Messages { get; private set; }

	public double ReadAt { get; set; }

	public bool IsIndividual => EntityCount == 2;

	public bool IsGroup => EntityCount > 2;

	public bool IsEmpty => EntityCount == 1;

	public int EntityCount => _entitySet.Count;

	public ulong RepresentId
	{
		get
		{
			if (IsGroup)
			{
				return 0uL;
			}
			if (_representId != 0L)
			{
				return _representId;
			}
			ulong playerId = GameManager.PlayerId;
			HashSet<ulong>.Enumerator enumerator = _entitySet.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current != playerId)
				{
					_representId = enumerator.Current;
					break;
				}
			}
			return _representId;
		}
	}

	public string CustomName { get; set; }

	public NewChecker NewChecker => _newChecker;

	public static event Action<Conversation> MessagesUpdated;

	public Conversation(ulong id, ulong entityId)
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
		PushEnabled = true;
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
			Messages.Add(new ChatStruct
			{
				EntityId = message_.EntityId,
				Body = message_.Body,
				Time = message_.Time,
				Type = ChannelType.Conversation
			});
		}
		_makeAt = Connections.Frontend.GetPredictedServerTime();
	}

	public void AddMessage(ChatStruct chat)
	{
		Messages.Add(chat);
		if (chat.EntityId != GameManager.PlayerId)
		{
			NewChecker.Count++;
		}
		if (Conversation.MessagesUpdated != null)
		{
			Conversation.MessagesUpdated(this);
		}
	}

	public void AddEntityIds(ulong[] ids)
	{
		_entitySet.UnionWith(ids);
	}

	public void RemoveEntityId(ulong id)
	{
		_entitySet.Remove(id);
	}

	public ulong[] GetEntityIds()
	{
		return _entitySet.ToArray();
	}

	public void FillEntityIds(HashSet<ulong> target)
	{
		target?.UnionWith(_entitySet);
	}

	public bool Contains(ulong entityId)
	{
		return _entitySet.Contains(entityId);
	}

	public double GetLastestUpdateTime()
	{
		return (Messages.Count != 0) ? Messages[Messages.Count - 1].Time : _makeAt;
	}

	public void UpdateNewCount()
	{
		int num = 0;
		ulong playerId = GameManager.PlayerId;
		int i = 0;
		for (int count = Messages.Count; i < count; i++)
		{
			ChatStruct chatStruct = Messages[i];
			if (chatStruct.EntityId != playerId && chatStruct.Time > ReadAt)
			{
				num++;
			}
		}
		NewChecker.Count = num;
	}
}
