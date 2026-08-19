using System;
using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Faction;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Quest;

public class Category
{
	private int _unreceivedCount;

	private bool? _hasReward;

	public bool HasScoreReward;

	private bool _isLoadingQuests;

	private Dictionary<string, QuestToDo> _quests;

	private bool _isDirtyQuestList;

	private readonly List<QuestToDo> _questList = new List<QuestToDo>();

	private Action<List<QuestToDo>> _onQuestList;

	public string Key { get; private set; }

	public string Name { get; private set; }

	public string Season { get; private set; }

	public NPCType NPCType { get; private set; }

	public bool? HasQuestScore { get; set; }

	public event Action<Category> Changed;

	public void Set(QuestCategory msg)
	{
		Key = msg.Category;
		Name = msg.Name;
		Season = msg.Season;
		_unreceivedCount = msg.UnreceivedCount;
		FactionType? faction = msg.Faction;
		NPCType = (faction.HasValue ? msg.Faction.Value.ToString().ToEnum(NPCType.Unknown) : NPCType.Unknown);
		_quests = null;
		_questList.Clear();
		_hasReward = null;
		HasQuestScore = null;
	}

	public List<QuestToDo> GetCachedQuestList()
	{
		return GetQuestList();
	}

	public void GetQuestList([NotNull] Action<List<QuestToDo>> onQuestList)
	{
		_onQuestList = (Action<List<QuestToDo>>)Delegate.Combine(_onQuestList, onQuestList);
		if (_quests == null)
		{
			if (!_isLoadingQuests)
			{
				_isLoadingQuests = true;
				GameSystem<QuestSystem>.Instance().GetQuests(Key);
			}
		}
		else
		{
			if (_onQuestList != null)
			{
				_onQuestList(GetQuestList());
			}
			_onQuestList = null;
		}
	}

	[NotNull]
	private List<QuestToDo> GetQuestList()
	{
		if (_quests == null)
		{
			_questList.Clear();
			return _questList;
		}
		if (_isDirtyQuestList)
		{
			_isDirtyQuestList = false;
			_questList.Clear();
			foreach (KeyValuePair<string, QuestToDo> quest in _quests)
			{
				_questList.Add(quest.Value);
			}
			_questList.Sort(QuestComprarison);
		}
		return _questList;
	}

	private static int QuestComprarison(QuestToDo q1, QuestToDo q2)
	{
		if (q1.Finished != q2.Finished)
		{
			return q1.Finished ? 1 : (-1);
		}
		if (!q1.Finished && q1.Progress >= q1.GoalCount != q2.Progress >= q2.GoalCount)
		{
			return (q1.Progress < q1.GoalCount) ? 1 : (-1);
		}
		QuestYml questYml = SingletonDict<string, QuestYml>.Get(q1.Id);
		QuestYml questYml2 = SingletonDict<string, QuestYml>.Get(q2.Id);
		if (questYml == null && questYml2 == null)
		{
			return string.CompareOrdinal(q1.Id, q2.Id);
		}
		if (questYml == null)
		{
			return 1;
		}
		if (questYml2 == null)
		{
			return -1;
		}
		if (questYml.Order < questYml2.Order)
		{
			return -1;
		}
		if (questYml.Order > questYml2.Order)
		{
			return 1;
		}
		float num = (float)q1.Progress / (float)q1.GoalCount;
		float num2 = (float)q2.Progress / (float)q2.GoalCount;
		if (num < num2)
		{
			return 1;
		}
		if (num2 < num)
		{
			return -1;
		}
		return string.CompareOrdinal(questYml.Subject, questYml2.Subject);
	}

	public void SetQuests(Quests? quests)
	{
		_isLoadingQuests = false;
		_isDirtyQuestList = true;
		_hasReward = null;
		if (!quests.HasValue)
		{
			_quests = null;
		}
		else
		{
			_quests = new Dictionary<string, QuestToDo>();
			QuestToDo[] todos = quests.Value.Todos;
			int i = 0;
			for (int size = KUtility.GetSize(todos); i < size; i++)
			{
				QuestToDo value = todos[i];
				_quests[value.Id] = value;
			}
		}
		if (_onQuestList != null)
		{
			_onQuestList(GetQuestList());
		}
		_onQuestList = null;
	}

	public void UpdateQuests(QuestToDo[] quests)
	{
		if (_quests != null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(quests); i < size; i++)
			{
				QuestToDo value = quests[i];
				_quests[value.Id] = value;
			}
			SetDirtyQuests();
		}
	}

	public void UpdateQuestProceed(NotifyQuestProceed msg)
	{
		if (_quests != null && _quests.TryGetValue(msg.QuestId, out var value))
		{
			value.Progress = msg.Progress;
			value.Finished = msg.Finished;
			_quests[msg.QuestId] = value;
			SetDirtyQuests();
		}
	}

	public void SetQuestRewardResults(QuestRewardResults result)
	{
		if (_quests != null && _quests.TryGetValue(result.QuestId, out var value))
		{
			_isDirtyQuestList = true;
			value.Progress = value.GoalCount;
			value.Finished = true;
			_quests[result.QuestId] = value;
			SetDirtyQuests();
		}
	}

	private void SetDirtyQuests()
	{
		_isDirtyQuestList = true;
		_hasReward = null;
		if (this.Changed != null)
		{
			this.Changed(this);
		}
	}

	private bool HasReward()
	{
		if (_quests == null)
		{
			return false;
		}
		bool? hasReward = _hasReward;
		if (!hasReward.HasValue)
		{
			_hasReward = false;
			foreach (KeyValuePair<string, QuestToDo> quest in _quests)
			{
				QuestToDo value = quest.Value;
				if (value.Progress >= value.GoalCount && !value.Finished)
				{
					_hasReward = true;
					break;
				}
			}
		}
		return _hasReward.Value;
	}

	public bool HasNotification()
	{
		if (_quests == null)
		{
			return _unreceivedCount > 0;
		}
		return HasReward() || HasScoreReward;
	}
}
