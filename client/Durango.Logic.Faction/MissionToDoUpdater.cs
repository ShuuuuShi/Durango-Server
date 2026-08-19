using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Faction;

namespace Durango.Logic.Faction;

public class MissionToDoUpdater
{
	private readonly List<KeyValuePair<FactionType, MissionToDoCollection>> _collections = new List<KeyValuePair<FactionType, MissionToDoCollection>>();

	[CanBeNull]
	private Dictionary<string, GuideEvent> _helperEvents;

	[CanBeNull]
	private GuideEvent _lastHelperEvent;

	private string _lastHelperId;

	public static string GenerateKey(FactionType faction)
	{
		return $"Faction.{(int)faction}";
	}

	public void Update(IList<Mission> missions)
	{
		string text = null;
		MissionToDoCollection collection = null;
		int num = 0;
		int i = 0;
		for (int count = missions.Count; i < count; i++)
		{
			Mission mission = missions[i];
			if (!mission.StartedAt.HasValue || KUtility.GetSize(mission.Todos) == 0)
			{
				continue;
			}
			bool added = false;
			int num2 = IndexOf(mission.Faction, i);
			MissionToDoCollection missionToDoCollection;
			if (num2 == -1)
			{
				missionToDoCollection = new MissionToDoCollection(mission.Faction);
				_collections.Add(new KeyValuePair<FactionType, MissionToDoCollection>(mission.Faction, missionToDoCollection));
				num2 = _collections.Count - 1;
				GameSystem<ToDoListSystem>.Instance().Add(missionToDoCollection);
				added = true;
			}
			else
			{
				missionToDoCollection = _collections[num2].Value;
			}
			num++;
			if (i != num2)
			{
				KeyValuePair<FactionType, MissionToDoCollection> value = _collections[i];
				_collections[i] = _collections[num2];
				_collections[num2] = value;
			}
			missionToDoCollection.UpdateMission(mission);
			missionToDoCollection.UpdateTodos(added);
			if (!string.IsNullOrEmpty(text))
			{
				continue;
			}
			Messages.MissionToDo[] todos = mission.Todos;
			for (int j = 0; j < todos.Length; j++)
			{
				Messages.MissionToDo missionToDo = todos[j];
				if (missionToDo.GoalCount != missionToDo.Progress)
				{
					collection = missionToDoCollection;
					text = missionToDo.Id;
					break;
				}
			}
		}
		RemoveUnused(num);
		if (GameManager.Region.IsSafeHouse())
		{
			UpdateHelper(text, collection);
		}
	}

	private int IndexOf(FactionType factionType, int searchFrom = 0)
	{
		for (int i = searchFrom; i < _collections.Count; i++)
		{
			if (_collections[i].Key == factionType)
			{
				return i;
			}
		}
		return -1;
	}

	private void RemoveUnused(int from)
	{
		for (int i = from; i < _collections.Count; i++)
		{
			KeyValuePair<FactionType, MissionToDoCollection> keyValuePair = _collections[i];
			GameSystem<ToDoListSystem>.Instance().Remove(keyValuePair.Value, immediately: true);
		}
		_collections.RemoveRange(from, _collections.Count - from);
	}

	public void Clear()
	{
		_lastHelperId = null;
		if (_lastHelperEvent != null)
		{
			GameSystem<PlayGuideSystem>.Instance().RemoveHelperTargets(_lastHelperEvent);
			_lastHelperEvent.ToDoCollection = null;
		}
		_lastHelperEvent = null;
		_collections.Clear();
	}

	private static void LoadHelpeEvents(Dictionary<string, GuideEvent> dict)
	{
		Dictionary<string, GuideEventJson> dictionary = Json.ReadFromFile<Dictionary<string, GuideEventJson>>("PlayGuide/mission_helper_guide_event");
		if (dictionary == null)
		{
			Debug.LogError("Deserialize MissionHelperGuideEvent failed: PlayGuide/mission_helper_guide_event");
			return;
		}
		dict.Clear();
		PlayGuideSystem.LoadPlayGuideEventJson(dict, dictionary);
	}

	private void UpdateHelper(string todoId, [CanBeNull] ToDoCollection collection)
	{
		if (!(_lastHelperId == todoId))
		{
			if (_helperEvents == null)
			{
				_helperEvents = new Dictionary<string, GuideEvent>();
				LoadHelpeEvents(_helperEvents);
			}
			if (_lastHelperEvent != null)
			{
				GameSystem<PlayGuideSystem>.Instance().RemoveHelperTargets(_lastHelperEvent);
				_lastHelperEvent.ToDoCollection = null;
			}
			_lastHelperId = todoId;
			_lastHelperEvent = _helperEvents.Get(todoId);
			if (collection != null)
			{
				collection.GuideEvent = _lastHelperEvent;
			}
			if (_lastHelperEvent != null)
			{
				_lastHelperEvent.ToDoCollection = collection;
				GameSystem<PlayGuideSystem>.Instance().ApplyHelperTarget(_lastHelperEvent);
			}
		}
	}
}
