using System.Collections.Generic;
using L10N;
using MenuData;
using Shared.Faction;

namespace PlayGuide;

public class GuideEvent
{
	public string Name;

	public string[] Messages;

	public ToDoCollection ToDoCollection;

	public bool IsSystem;

	public bool Autorun;

	public string SpawnFlow;

	public float MsgDuration;

	public ShowPortrait ShowPortrait;

	public bool IsBlur;

	public bool PlayAudio;

	public bool IsInstant;

	public string OverrideColorRGB;

	public FactionType Faction;

	public string NameTag;

	public NPCType NPCType;

	public MenuType[] LockMenus;

	public MenuType[] UnlockMenus;

	public string[] HighLightTargets;

	public string[] MarkingNew;

	public string CustomCommand;

	public HelperTarget[] HelperTargets;

	public SpotlightTarget SpotlightTarget;

	public int SurvivalMemo;

	public FlowStack FlowStack;

	public static GuideEvent Create(string eventName, GuideEventJson json, NPCType prevNPCType = NPCType.TheFirm)
	{
		GuideEvent guideEvent = new GuideEvent();
		guideEvent.Name = eventName;
		guideEvent.Messages = json.messages;
		guideEvent.IsSystem = json.is_system;
		guideEvent.Autorun = json.autorun;
		guideEvent.MsgDuration = json.msg_duration;
		guideEvent.IsInstant = false;
		guideEvent.OverrideColorRGB = null;
		guideEvent.Faction = ((!string.IsNullOrEmpty(json.faction)) ? json.faction.ToEnum(FactionType.ChlorophylForum) : FactionType.Invalid);
		guideEvent.ShowPortrait = ((!string.IsNullOrEmpty(json.portrait)) ? json.portrait.ToEnum(ShowPortrait.K) : ShowPortrait.None);
		LoadNPCType(json, guideEvent, prevNPCType);
		guideEvent.IsBlur = json.is_blur;
		guideEvent.PlayAudio = json.play_audio;
		guideEvent.NameTag = T._(json.name_tag);
		guideEvent.SpawnFlow = json.spawn_flow;
		if (guideEvent.IsSystem && guideEvent.MsgDuration == 0f)
		{
			guideEvent.MsgDuration = 5f;
		}
		guideEvent.LockMenus = ParseMenuTypes(json.lock_menus);
		guideEvent.UnlockMenus = ParseMenuTypes(json.unlock_menus);
		guideEvent.HighLightTargets = json.highlight;
		guideEvent.MarkingNew = json.marking_new;
		guideEvent.HelperTargets = json.helper;
		guideEvent.SpotlightTarget = json.spotlight;
		guideEvent.CustomCommand = json.custom_cmd;
		guideEvent.SurvivalMemo = json.survival_memo;
		LoadToDoCollection(json, guideEvent);
		return guideEvent;
	}

	public static NPCType FactioTypeToNPCType(FactionType type)
	{
		return type switch
		{
			FactionType.ChlorophylForum => NPCType.ChlorophylForum, 
			FactionType.ChamberOfPioneer => NPCType.ChamberOfPioneer, 
			FactionType.TheFirm => NPCType.TheFirm, 
			FactionType.TheCommittee => NPCType.TheCommittee, 
			FactionType.Lama => NPCType.Lama, 
			_ => NPCType.TheFirm, 
		};
	}

	private static void LoadNPCType(GuideEventJson json, GuideEvent guideEvent, NPCType defaultType)
	{
		if (string.IsNullOrEmpty(json.npc_type))
		{
			switch (guideEvent.ShowPortrait)
			{
			case ShowPortrait.K:
				guideEvent.NPCType = NPCType.TheFirm;
				break;
			case ShowPortrait.Optimistic:
				guideEvent.NPCType = NPCType.Optimistic;
				break;
			case ShowPortrait.Faction:
				guideEvent.NPCType = FactioTypeToNPCType(guideEvent.Faction);
				break;
			default:
				guideEvent.NPCType = defaultType;
				break;
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
		guideEvent.ToDoCollection.NPCType = guideEvent.NPCType;
		guideEvent.ToDoCollection.ToDoList = new List<ToDoBase>();
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

	private static MenuType[] ParseMenuTypes(string[] types)
	{
		int size = KUtility.GetSize(types);
		if (size == 0)
		{
			return null;
		}
		List<MenuType> list = new List<MenuType>();
		for (int i = 0; i < size; i++)
		{
			string source = types[i];
			if (source.TryEnum<MenuType>(out var value))
			{
				list.Add(value);
			}
		}
		return list.ToArray();
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
}
