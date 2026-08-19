using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Newtonsoft.Json.Linq;
using Shared.Faction;

namespace Durango.Logic.PlayGuide;

public class GuideEvent
{
	public const string Blank = "blank";

	public string Name;

	public string[] Messages;

	public string[] Chapter;

	public ToDoCollection ToDoCollection;

	public bool IsSystem;

	public string SpawnFlow;

	public float Duration;

	public ShowPortrait ShowPortrait;

	public bool IsBlur;

	public bool Remote;

	public string NameTag;

	public NPCType NPCType;

	public string Image;

	public bool HidePortrait;

	public HelperTarget[] HelperTargets;

	public SpotlightTarget SpotlightTarget;

	public int SurvivalMemo;

	public string TouchToDo;

	public FactionType ActivateFaction = FactionType.Invalid;

	public string CustomCommand;

	public string CardNews;

	public string NXAds;

	public QuizData[] QuizArray;

	[CanBeNull]
	public FlowStack FlowStack;

	public QuizData GetQuiz(int index)
	{
		if (KUtility.GetSize(QuizArray) > index)
		{
			return QuizArray[index];
		}
		return null;
	}

	public static GuideEvent Create(string eventName, GuideEventJson json, NPCType prevNPCType = NPCType.TheFirm)
	{
		GuideEvent guideEvent = new GuideEvent();
		guideEvent.Name = eventName;
		guideEvent.Messages = json.messages;
		guideEvent.Chapter = json.chapter;
		guideEvent.IsSystem = json.is_system;
		guideEvent.Duration = json.duration;
		guideEvent.ShowPortrait = ((!string.IsNullOrEmpty(json.portrait)) ? json.portrait.ToEnum(ShowPortrait.K) : ShowPortrait.None);
		LoadNPCType(json, guideEvent, prevNPCType);
		guideEvent.Image = json.image;
		guideEvent.HidePortrait = json.hide_portrait;
		guideEvent.IsBlur = json.is_blur;
		guideEvent.Remote = json.remote;
		guideEvent.NameTag = T._(json.name_tag);
		guideEvent.SpawnFlow = json.spawn_flow;
		if (guideEvent.IsSystem && guideEvent.Duration == 0f)
		{
			guideEvent.Duration = 5f;
		}
		guideEvent.HelperTargets = json.helper;
		guideEvent.SpotlightTarget = json.spotlight;
		guideEvent.SurvivalMemo = json.survival_memo;
		guideEvent.TouchToDo = json.touch_todo;
		guideEvent.ActivateFaction = ((!string.IsNullOrEmpty(json.activate_faction)) ? json.activate_faction.ToEnum(FactionType.ChlorophylForum) : FactionType.Invalid);
		guideEvent.CustomCommand = json.custom_cmd;
		guideEvent.CardNews = json.card_news;
		guideEvent.NXAds = json.nx_ads;
		LoadToDoCollection(json, guideEvent);
		guideEvent.QuizArray = LoadQuizArray(json.quiz);
		return guideEvent;
	}

	private static void LoadNPCType(GuideEventJson json, GuideEvent guideEvent, NPCType defaultType)
	{
		if (string.IsNullOrEmpty(json.npc_type))
		{
			ShowPortrait showPortrait = guideEvent.ShowPortrait;
			if (showPortrait == ShowPortrait.K || showPortrait == ShowPortrait.K_Indoor)
			{
				guideEvent.NPCType = NPCType.TheFirm;
			}
			else
			{
				guideEvent.NPCType = defaultType;
			}
		}
		else
		{
			guideEvent.NPCType = json.npc_type.ToEnum(defaultType);
		}
	}

	private static void LoadToDoCollection(GuideEventJson json, GuideEvent guideEvent)
	{
		if (json.todos == null || json.todos.Length == 0)
		{
			guideEvent.ToDoCollection = null;
			return;
		}
		guideEvent.ToDoCollection = new ToDoCollection();
		guideEvent.ToDoCollection.Title = T._(json.todo_title);
		guideEvent.ToDoCollection.Icon = guideEvent.NPCType.ToDoIcon();
		guideEvent.ToDoCollection.GuideEvent = guideEvent;
		guideEvent.ToDoCollection.Key = guideEvent.Name;
		for (int i = 0; i < json.todos.Length; i++)
		{
			ToDoBase toDoBase = ToDoFactory.CreateToDo(guideEvent.Name, json.todos[i]);
			if (toDoBase == null)
			{
				continue;
			}
			toDoBase.Completed += delegate
			{
				if (CheckAllToDoCompleted(guideEvent.ToDoCollection))
				{
					GameSystem<PlayGuideSystem>.Instance().CompleteEvent(guideEvent);
				}
			};
			guideEvent.ToDoCollection.ToDoList.Add(toDoBase);
		}
	}

	public static bool CheckAllToDoCompleted(ToDoCollection collection)
	{
		if (collection == null || collection.ToDoList == null)
		{
			return true;
		}
		int count = collection.ToDoList.Count;
		bool result = true;
		for (int i = 0; i < count; i++)
		{
			if (!collection.ToDoList[i].IsCompleted)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private static QuizData[] LoadQuizArray(JArray quizArray)
	{
		if (quizArray == null || quizArray.Count == 0)
		{
			return null;
		}
		int count = quizArray.Count;
		QuizData[] array = new QuizData[count];
		for (int i = 0; i < count; i++)
		{
			if (quizArray[i] is JObject obj)
			{
				array[i] = QuizData.Parse(obj);
			}
		}
		return array;
	}
}
