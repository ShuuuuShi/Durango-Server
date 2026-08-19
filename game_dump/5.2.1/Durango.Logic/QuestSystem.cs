using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.PlayGuide;
using Durango.Logic.Quest;
using Durango.Network;
using JetBrains.Annotations;
using Messages;
using Shared.Quest;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class QuestSystem : GameSystem<QuestSystem>
{
	private class Progress
	{
		public int Current;

		public int Goal;
	}

	private class QuestProgressPerCategory
	{
		public string Category;

		public string Target;

		public readonly Dictionary<string, Progress> Progresses = new Dictionary<string, Progress>();
	}

	private readonly Dictionary<string, Category> _questCategories = new Dictionary<string, Category>();

	private readonly List<QuestProgressPerCategory> _displayedQuestCategories = new List<QuestProgressPerCategory>();

	public IEnumerable<Category> VisibleCategories => _questCategories.Where(delegate(KeyValuePair<string, Category> pair)
	{
		KeyValuePair<string, Category> keyValuePair2 = pair;
		return keyValuePair2.Key != EpicCategory;
	}).Select(delegate(KeyValuePair<string, Category> pair)
	{
		KeyValuePair<string, Category> keyValuePair = pair;
		return keyValuePair.Value;
	});

	public string EpicCategory { get; private set; }

	public event Action<QuestRewardResults> Rewarded;

	public event Action<NotifyQuestProceed> QuestProceeded;

	public event Action<bool> QuestNotificationUpdated;

	public event Action<QuestScoreInfos> QuestScoreInfosUpdated;

	public event Action<string> ChapterStarted;

	public event Action<string> QuestCategoryChanged;

	public event Action<string> QuestStarted;

	public event Action<string> QuestFinished;

	private void Start()
	{
		Connections.Frontend.On<NotifyQuestProceed>(OnNotifyQuestProceed);
		Connections.Frontend.On<QuestStarted>(OnQuestStarted);
		Connections.Frontend.On<QuestCategories>(OnQuestCategories);
		Connections.Frontend.On<QuestRewardResults>(OnQuestRewardResults);
		GameSystem<StatisticsSystem>.Instance().LevelChanged += delegate(int prev, int cur)
		{
			GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.Story, cur >= 20);
		};
	}

	private void OnQuestCategories(QuestCategories msg, PacketHeader header)
	{
		int i = 0;
		for (int size = KUtility.GetSize(msg.Categories); i < size; i++)
		{
			QuestCategory msg2 = msg.Categories[i];
			if (msg2.Category != null)
			{
				Category category = _questCategories.Get(msg2.Category);
				if (category == null)
				{
					category = new Category();
					category.Changed += OnQuestCategoryChanged;
					_questCategories[msg2.Category] = category;
				}
				category.Set(msg2);
			}
		}
		QuestCategory? epic = msg.Epic;
		if (epic.HasValue)
		{
			EpicCategory = msg.Epic.Value.Category;
			Category category2 = _questCategories.Get(EpicCategory);
			if (category2 == null)
			{
				category2 = new Category();
				category2.Changed += OnQuestCategoryChanged;
				_questCategories[EpicCategory] = category2;
			}
			category2.Set(msg.Epic.Value);
		}
		else
		{
			EpicCategory = null;
		}
		Dictionary<string, Category>.Enumerator enumerator = _questCategories.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			bool flag = true;
			int j = 0;
			for (int size2 = KUtility.GetSize(msg.Categories); j < size2; j++)
			{
				if (msg.Categories[j].Category == key)
				{
					flag = false;
					break;
				}
			}
			if (EpicCategory == key)
			{
				flag = false;
			}
			if (flag)
			{
				_questCategories.Remove(key);
				enumerator.Dispose();
				enumerator = _questCategories.GetEnumerator();
			}
		}
		UpdateNotification();
	}

	private void OnQuestRewardResults(QuestRewardResults msg, PacketHeader header)
	{
		_questCategories.Get(msg.Category)?.SetQuestRewardResults(msg);
		OnQuestScoreInfos(msg.QuestScoreInfos);
		bool flag = false;
		if (msg.Category == EpicCategory)
		{
			Chapter chatper = GetChatper(msg.QuestId);
			if (chatper != null && chatper.Quests != null && chatper.Quests.LastOrDefault() == msg.QuestId)
			{
				Chapter chapter = chatper;
				while (true)
				{
					chapter = GetNextChatper(chapter);
					if (chapter == null || GetChapterProgress(chapter) < 1f)
					{
						break;
					}
					if (chapter.GetKind() != Chapter.Kind.Movie)
					{
						continue;
					}
					flag = true;
					chapter.PlayMovie(delegate
					{
						Connections.Frontend.Send(default(RequestEpicWarp));
						if (this.Rewarded != null)
						{
							this.Rewarded(msg);
						}
						if (this.QuestFinished != null)
						{
							this.QuestFinished(msg.QuestId);
						}
					});
					break;
				}
			}
		}
		if (!flag)
		{
			if (this.Rewarded != null)
			{
				this.Rewarded(msg);
			}
			if (this.QuestFinished != null)
			{
				this.QuestFinished(msg.QuestId);
			}
		}
	}

	public Category GetCategory(string category)
	{
		return _questCategories.Get(category);
	}

	public void GetQuests(string category)
	{
		Connections.Frontend.Send(new GetQuests
		{
			Category = category
		}).On(delegate(Quests msg, PacketHeader header)
		{
			OnUpdateQuests(category, msg);
		}).Rest(delegate
		{
			OnUpdateQuests(category, null);
		});
	}

	public void GetQuestState(string questId, Action<Shared.Quest.QuestState> result)
	{
		Connections.Frontend.Send(new GetQuestState
		{
			QuestIds = new string[1] { questId }
		}).On(delegate(Messages.QuestState msg, PacketHeader _)
		{
			Shared.Quest.QuestState obj = msg.States.Get(questId, Shared.Quest.QuestState.Invalid);
			result(obj);
		});
	}

	private void OnQuestScoreInfos(QuestScoreInfos msg)
	{
		OnUpdateQuestScoreReward(msg.Category, msg.QuestScoreRewards);
		if (this.QuestScoreInfosUpdated != null)
		{
			this.QuestScoreInfosUpdated(msg);
		}
	}

	private void OnNotifyQuestProceed(NotifyQuestProceed msg, PacketHeader header)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(msg.QuestId);
		if (questYml == null)
		{
			return;
		}
		Category category = _questCategories.Get(questYml.Category);
		if (category != null)
		{
			category.UpdateQuestProceed(msg);
			UpdateQuestTodoProceed(msg);
			if (this.QuestProceeded != null)
			{
				this.QuestProceeded(msg);
			}
			UpdateNotification();
		}
	}

	private void OnQuestStarted(QuestStarted msg, PacketHeader header)
	{
		Category category = _questCategories.Get(msg.Category);
		if (category == null)
		{
			return;
		}
		category.UpdateQuests(msg.Quests);
		int i = 0;
		for (int size = KUtility.GetSize(msg.Quests); i < size; i++)
		{
			QuestToDo quest = msg.Quests[i];
			UpdateQuestTodoProceed(quest);
			if (this.QuestStarted != null)
			{
				this.QuestStarted(quest.Id);
			}
			QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(quest.Id);
			if (questYml != null && !string.IsNullOrEmpty(questYml.ChapterSubject) && this.ChapterStarted != null)
			{
				this.ChapterStarted(quest.Id);
			}
		}
		UpdateNotification();
	}

	private void OnQuestCategoryChanged(Category category)
	{
		if (this.QuestCategoryChanged != null)
		{
			this.QuestCategoryChanged(category.Key);
		}
	}

	public void GetQuestScoreInfos(string category)
	{
		Connections.Frontend.Send(new GetQuestScoreInfos
		{
			Category = category
		}).On(delegate(QuestScoreInfos msg, PacketHeader header)
		{
			OnQuestScoreInfos(msg);
		});
	}

	public void RequestQuestReward(string questId)
	{
		Connections.Frontend.Send(new RequestQuestReward
		{
			QuestId = questId
		});
	}

	public void RequestQuestScoreReward(string category, int score)
	{
		Connections.Frontend.Send(new RequestQuestScoreReward
		{
			Category = category,
			Score = score
		}).On(delegate(QuestScoreInfos msg, PacketHeader header)
		{
			OnQuestScoreInfos(msg);
			if (score >= 100 && score % 100 == 0)
			{
				KUtility.DelayedCall(this, StoreReview.Request, 1f);
			}
		});
	}

	private void OnUpdateQuests(string category, Quests? msg)
	{
		Category category2 = _questCategories.Get(category);
		if (category2 == null)
		{
			return;
		}
		category2.SetQuests(msg);
		if (msg.HasValue && msg.Value.Todos != null)
		{
			QuestToDo[] todos = msg.Value.Todos;
			foreach (QuestToDo quest in todos)
			{
				UpdateQuestTodoProceed(quest);
			}
		}
		UpdateNotification();
	}

	private void UpdateQuestTodoProceed(QuestToDo quest)
	{
		UpdateQuestTodoProceed(quest.Id, quest.Progress, quest.GoalCount, quest.Finished);
	}

	private void UpdateQuestTodoProceed(NotifyQuestProceed msg)
	{
		UpdateQuestTodoProceed(msg.QuestId, msg.Progress, msg.GoalCount, msg.Finished);
	}

	private void UpdateQuestTodoProceed(string id, int current, int goal, bool finished)
	{
		QuestYml info = SingletonDict<string, QuestYml>.Instance.Get(id);
		if (info == null || !info.DisplayOnHud)
		{
			return;
		}
		QuestProgressPerCategory questProgressPerCategory = _displayedQuestCategories.FirstOrDefault((QuestProgressPerCategory x) => x.Category == info.Category);
		if (questProgressPerCategory == null)
		{
			questProgressPerCategory = new QuestProgressPerCategory();
			questProgressPerCategory.Category = info.Category;
			_displayedQuestCategories.Add(questProgressPerCategory);
		}
		bool flag = false;
		Progress progress = questProgressPerCategory.Progresses.Get(id);
		if (progress == null)
		{
			progress = new Progress();
			questProgressPerCategory.Progresses.Add(id, progress);
			flag = true;
		}
		progress.Current = current;
		progress.Goal = goal;
		if (questProgressPerCategory.Target == id)
		{
			UpdateToDo(questProgressPerCategory);
		}
		if (current >= goal || finished)
		{
			questProgressPerCategory.Progresses.Remove(id);
		}
		string target = questProgressPerCategory.Target;
		if (!string.IsNullOrEmpty(target) && questProgressPerCategory.Progresses.ContainsKey(target) && !flag)
		{
			return;
		}
		questProgressPerCategory.Target = null;
		float num = float.MinValue;
		foreach (KeyValuePair<string, Progress> item in questProgressPerCategory.Progresses.OrderBy((KeyValuePair<string, Progress> x) => GetOrder(info.Category, x.Key)))
		{
			float num2 = (float)item.Value.Current / (float)item.Value.Goal;
			if (num2 > num)
			{
				num = num2;
				questProgressPerCategory.Target = item.Key;
			}
		}
		if (target != questProgressPerCategory.Target)
		{
			ToDoCollection collection = GameSystem<ToDoListSystem>.Instance().FindCollection(target);
			GameSystem<ToDoListSystem>.Instance().Remove(collection);
			UpdateToDo(questProgressPerCategory);
		}
	}

	private static int GetOrder(string cat, string key)
	{
		Chapters chapters = SingletonDict<string, Chapters>.Instance.Get(cat);
		if (chapters != null)
		{
			Chapter[] chapterList = chapters.ChapterList;
			foreach (Chapter chapter in chapterList)
			{
				for (int j = 0; j < KUtility.GetSize(chapter.Quests); j++)
				{
					if (chapter.Quests[j] == key)
					{
						return j;
					}
				}
			}
		}
		return 0;
	}

	private void UpdateToDo([NotNull] QuestProgressPerCategory category)
	{
		string target = category.Target;
		Progress progress = category.Progresses.Get(target);
		if (progress == null)
		{
			return;
		}
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(target);
		if (questYml == null)
		{
			return;
		}
		ToDoCollection toDoCollection = GameSystem<ToDoListSystem>.Instance().FindCollection(target);
		if (toDoCollection == null)
		{
			toDoCollection = new ToDoCollection();
			toDoCollection.Key = target;
			toDoCollection.Title = questYml.Subject;
			Category category2 = _questCategories.Get(questYml.Category);
			toDoCollection.Icon = questYml.Icon;
			bool flag = questYml.Category == "sunset";
			toDoCollection.IconSize = ((!flag) ? 40 : 0);
			toDoCollection.Season = category2?.Season;
			if (flag)
			{
				toDoCollection.SubIcon = "emblem_story_small";
			}
			QuestTodo item = new QuestTodo(target, progress.Current, progress.Goal);
			toDoCollection.ToDoList.Add(item);
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
			return;
		}
		ToDoBase toDoBase = toDoCollection.FindToDo(target);
		if (toDoBase == null)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(toDoCollection);
			return;
		}
		toDoBase.CurrentProgress = progress.Current;
		toDoBase.TargetProgress = progress.Goal;
		if (progress.Current >= progress.Goal)
		{
			toDoBase.CallComplete();
		}
		else
		{
			GameSystem<ToDoListSystem>.Instance().SetUpdated(toDoBase);
		}
	}

	private void OnUpdateQuestScoreReward(string category, QuestScoreReward[] rewards)
	{
		Category category2 = _questCategories.Get(category);
		if (category2 == null)
		{
			return;
		}
		if (!category2.HasQuestScore.HasValue)
		{
			category2.HasQuestScore = KUtility.GetSize(rewards) > 0;
		}
		bool hasScoreReward = false;
		int i = 0;
		for (int size = KUtility.GetSize(rewards); i < size; i++)
		{
			if (rewards[i].State == QuestScoreRewardState.Available)
			{
				hasScoreReward = true;
				break;
			}
		}
		category2.HasScoreReward = hasScoreReward;
		UpdateNotification();
	}

	private void UpdateNotification()
	{
		bool obj = VisibleCategories.Any((Category cat) => cat.HasNotification());
		if (this.QuestNotificationUpdated != null)
		{
			this.QuestNotificationUpdated(obj);
		}
	}

	public float GetChapterProgress(Chapter chapter)
	{
		if (KUtility.GetSize(chapter.Quests) == 0)
		{
			return 1f;
		}
		Category category = _questCategories.Get(EpicCategory);
		if (category == null)
		{
			return 0f;
		}
		List<QuestToDo> quests = category.GetCachedQuestList();
		return (float)chapter.Quests.Count((string questId) => quests.Any((QuestToDo quest) => quest.Id == questId && quest.Finished)) / (float)chapter.Quests.Length;
	}

	public Chapter GetChatper(string questId)
	{
		Chapters chapters = SingletonDict<string, Chapters>.Instance.Get(EpicCategory);
		if (chapters == null)
		{
			return null;
		}
		for (int i = 0; i < KUtility.GetSize(chapters.ChapterList); i++)
		{
			Chapter chapter = chapters.ChapterList[i];
			if (chapter.Quests != null && chapter.Quests.Contains(questId))
			{
				return chapter;
			}
		}
		return null;
	}

	public Chapter GetNextChatper(Chapter cur)
	{
		Chapters chapters = SingletonDict<string, Chapters>.Instance.Get(EpicCategory);
		if (chapters == null)
		{
			return null;
		}
		int size = KUtility.GetSize(chapters.ChapterList);
		for (int i = 0; i < size; i++)
		{
			if (chapters.ChapterList[i] == cur && i + 1 < size)
			{
				return chapters.ChapterList[i + 1];
			}
		}
		return null;
	}

	public QuestToDo GetEpicQuest(string questId)
	{
		return _questCategories.Get(EpicCategory)?.GetCachedQuestList().Find((QuestToDo x) => x.Id == questId) ?? default(QuestToDo);
	}
}
