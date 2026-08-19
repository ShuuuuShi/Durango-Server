using System;
using System.Collections.Generic;
using L10N;
using Messages;
using PlayGuide;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class FactionTodoUpdater
{
	public class FactionTodo : ToDoBase
	{
		public double? ExpiresAt;

		public string DisplayText;

		private float _lastUpdatedAt;

		public override void Process()
		{
			if (!ExpiresAt.HasValue)
			{
				base.LocalText = DisplayText;
				return;
			}
			float time = Time.time;
			if (!(time - _lastUpdatedAt < 1f))
			{
				_lastUpdatedAt = time;
				double totalSeconds = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
				if (totalSeconds > ExpiresAt.Value)
				{
					ExpiresAt = null;
					return;
				}
				base.LocalText = T._("{0}\n남은 시간 {1}", DisplayText, TimeToString(ExpiresAt.Value - totalSeconds));
				GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
			}
		}

		private string TimeToString(double time)
		{
			int num = (int)time;
			int num2 = num / 3600;
			int num3 = num / 60 % 60;
			int num4 = num % 60;
			if (num2 > 0)
			{
				return T._("{0:D2}:{1:D2}:{2:D2}", num2, num3, num4);
			}
			return T._("{0:D2}:{1:D2}", num3, num4);
		}
	}

	private readonly Dictionary<FactionType, string> _todoIconMap = new Dictionary<FactionType, string>
	{
		{
			FactionType.ChlorophylForum,
			"icon_map_chlorophy"
		},
		{
			FactionType.ChamberOfPioneer,
			"icon_map_pioneer"
		},
		{
			FactionType.TheFirm,
			"icon_map_company"
		},
		{
			FactionType.TheCommittee,
			"icon_map_committee"
		},
		{
			FactionType.Lama,
			"icon_map_professor"
		}
	};

	private List<ToDoCollection> _factionTodos = new List<ToDoCollection>();

	private HashSet<FactionType> _playingFactions = new HashSet<FactionType>();

	public void LoadTodos()
	{
		Connections.Frontend.Send(default(GetFactionEvents));
	}

	public bool IsFactionPlaying(FactionType faction)
	{
		return _playingFactions.Contains(faction);
	}

	private string GetFactionName(FactionType factionType)
	{
		if (SingletonDict<FactionType, Yaml.Faction>.Instance.TryGetValue(factionType, out var value))
		{
			return value.name;
		}
		return factionType.ToString();
	}

	public void FactionTodoUpdated(FactionEvents msg)
	{
		_playingFactions.Clear();
		List<ToDoCollection> factionTodos = _factionTodos;
		List<ToDoCollection> list = new List<ToDoCollection>();
		int num = msg.Events.Length;
		for (int i = 0; i < num; i++)
		{
			FactionEvent factionEvent = msg.Events[i];
			int num2 = factionEvent.Todos.Length;
			_playingFactions.Add(factionEvent.Faction);
			for (int j = 0; j < num2; j++)
			{
				FactionToDo factionToDo = factionEvent.Todos[j];
				string text = $"Faction.{(int)factionEvent.Faction}.{j}";
				ToDoCollection toDoCollection = GameSystem<ToDoListSystem>.Instance().FindCollection(text);
				bool flag = toDoCollection == null;
				FactionTodo factionTodo = ((!flag) ? ((FactionTodo)toDoCollection.ToDoList[0]) : new FactionTodo());
				factionTodo.Key = text;
				factionTodo.ExpiresAt = factionEvent.ExpiresAt;
				bool flag2 = false;
				if (!flag)
				{
					flag2 = factionTodo.TargetProgress != factionToDo.GoalCount || factionTodo.CurrentProgress != factionToDo.Progress || factionTodo.LocalText != factionToDo.Label;
				}
				factionTodo.LocalText = factionToDo.Label;
				factionTodo.DisplayText = factionToDo.Label;
				factionTodo.TargetProgress = factionToDo.GoalCount;
				factionTodo.CurrentProgress = factionToDo.Progress;
				factionTodo.IsCompleted = factionTodo.TargetProgress == factionTodo.CurrentProgress;
				factionTodo.Tooltip = factionToDo.Tooltip;
				if (flag)
				{
					ToDoCollection toDoCollection2 = new ToDoCollection();
					toDoCollection2.Title = GetFactionName(factionEvent.Faction);
					toDoCollection2.NPCType = GuideEvent.FactioTypeToNPCType(factionEvent.Faction);
					toDoCollection2.ToDoList = new ToDoBase[1] { factionTodo };
					toDoCollection = toDoCollection2;
				}
				list.Add(toDoCollection);
				if (flag)
				{
					GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
				}
				else if (flag2)
				{
					GameSystem<ToDoListSystem>.Instance().SetUpdated(factionTodo);
				}
				int count = factionTodos.Count;
				for (int k = 0; k < count; k++)
				{
					if (factionTodos[k].Key == text)
					{
						factionTodos.RemoveAt(k);
						break;
					}
				}
			}
		}
		int count2 = factionTodos.Count;
		for (int l = 0; l < count2; l++)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(factionTodos[l]);
		}
		_factionTodos = list;
	}
}
