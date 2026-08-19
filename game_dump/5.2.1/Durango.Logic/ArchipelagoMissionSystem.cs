using System;
using System.Collections.Generic;
using Durango.Logic.Explore;
using Durango.Network;
using JetBrains.Annotations;
using Messages;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class ArchipelagoMissionSystem : GameSystem<ArchipelagoMissionSystem>
{
	private readonly ArchipelagoToDoCollection _collection = new ArchipelagoToDoCollection();

	private const string ToDoCollectionKey = "ArchipelagoToDo.Collection";

	private const string SeasonKey = "Season.ArchipelagoMission";

	public event Action<ArchipelagoMission> MissionStarted;

	public event Action<ArchipelagoToDoCollection> MissionEnded;

	private void Awake()
	{
		_collection.Key = "ArchipelagoToDo.Collection";
		_collection.Season = "Season.ArchipelagoMission";
		Connections.Frontend.On<CurrentArchipelagoTodos>(OnCurrentArchipelagoTodos);
		Connections.Frontend.On<NotifyArchipelagoTodoProceed>(OnNotifyArchipelagoTodoProceed);
	}

	public void OnCurrentArchipelagoTodos(CurrentArchipelagoTodos packet, PacketHeader header)
	{
		ArchipelagoMission currentMission = GetCurrentMission();
		if (currentMission == null || (packet.IsClearedRegion && GetNextRegion() == null))
		{
			return;
		}
		_collection.ToDoList.Clear();
		_collection.CurrentPoint.Changed = null;
		_collection.CurrentPoint.Value = 0;
		_collection.Reward = null;
		if (packet.CurrentTodos.HasValue)
		{
			ArchipelagoTodos value = packet.CurrentTodos.Value;
			_collection.CurrentPoint.Value = value.CurrentPoint;
			_collection.Reward = value.Reward;
			Messages.ArchipelagoToDo[] todos = value.Todos;
			for (int i = 0; i < todos.Length; i++)
			{
				Messages.ArchipelagoToDo archipelagoToDo = todos[i];
				ToDoContents toDoContents = currentMission.ToDos.Get(archipelagoToDo.Id);
				ArchipelagoToDo item = new ArchipelagoToDo
				{
					Key = archipelagoToDo.Id,
					CurrentProgress = archipelagoToDo.Progress,
					TargetProgress = archipelagoToDo.GoalCount,
					LocalText = ((toDoContents != null) ? toDoContents.Subject.ToString() : string.Empty),
					Point = (toDoContents?.Point ?? 0)
				};
				_collection.ToDoList.Add(item);
			}
			if (!value.IsNotified && this.MissionStarted != null)
			{
				this.MissionStarted(currentMission);
			}
		}
		_collection.Title = currentMission.Title;
		_collection.Intro = currentMission.Intro;
		if (currentMission.Intro != null && KUtility.GetSize(currentMission.Intro.Talks) > 0)
		{
			_collection.Icon = IconMap.Get(currentMission.Intro.Talks[0].Messenger);
		}
		if (string.IsNullOrEmpty(_collection.Icon))
		{
			_collection.Icon = "todo_icon_walkie";
		}
		_collection.Outro = currentMission.Outro;
		_collection.Description = currentMission.Description;
		_collection.ClearPoint = currentMission.ClearPoint;
		_collection.CurrentState = (packet.IsClearedRegion ? ArchipelagoToDoCollection.State.Done : (_collection.HasEnoughPoint ? ArchipelagoToDoCollection.State.Reportable : ((!(packet.ActiveRegionId == GameManager.Region.Id)) ? ArchipelagoToDoCollection.State.CanDo : ArchipelagoToDoCollection.State.Doing)));
		_collection.ShowUI = !packet.IsEpic;
		Action<Durango.Logic.Explore.Region> onRegion = delegate(Durango.Logic.Explore.Region region)
		{
			_collection.ActiveRegion = region;
			if (GameSystem<ToDoListSystem>.Instance().FindCollection("ArchipelagoToDo.Collection") == null)
			{
				GameSystem<ToDoListSystem>.Instance().Add(_collection);
			}
			GameSystem<ToDoListSystem>.Instance().SetUpdated(_collection);
		};
		if (_collection.CurrentState == ArchipelagoToDoCollection.State.CanDo)
		{
			GameSystem<MapSystem>.Instance().GetRegion(packet.ActiveRegionId, delegate(Messages.Region region)
			{
				onRegion(new Durango.Logic.Explore.Region(region));
			});
		}
		else
		{
			onRegion(GameManager.Region);
		}
	}

	private void OnNotifyArchipelagoTodoProceed(NotifyArchipelagoTodoProceed packet, PacketHeader header)
	{
		_collection.Update(packet);
		if (_collection.HasEnoughPoint)
		{
			_collection.CurrentState = ArchipelagoToDoCollection.State.Reportable;
		}
		GameSystem<ToDoListSystem>.Instance().SetUpdated(_collection);
	}

	public void EndMission()
	{
		_collection.CurrentState = ArchipelagoToDoCollection.State.Done;
		_collection.ToDoList.Clear();
		GameSystem<ToDoListSystem>.Instance().SetUpdated(_collection);
		if (this.MissionEnded != null)
		{
			this.MissionEnded(_collection);
		}
	}

	public void RequestRegionClear()
	{
		Connections.Frontend.Send(default(RequestArchipelagoRegionClear));
	}

	public static void RequestReissueArchipelagoTodos()
	{
		Connections.Frontend.Send(default(ReissueArchipelagoTodos));
	}

	public static void RequestWarpCost([NotNull] Action<long> callback)
	{
		Connections.Frontend.Send(default(GetWarpCostToNextRegion)).On(delegate(WarpCosts msg, PacketHeader header)
		{
			if (KUtility.GetSize(msg.Costs) > 0)
			{
				callback(msg.Costs[0].Cost);
			}
		});
	}

	[CanBeNull]
	private static ArchipelagoMission GetCurrentMission()
	{
		if (!GameManager.Archipelago.HasValue)
		{
			return null;
		}
		return SingletonDict<string, Dictionary<string, ArchipelagoMission>>.Instance.Get(GameManager.Archipelago.Value.TemplateId)?.Get(GameManager.Region.TemplateId);
	}

	[CanBeNull]
	public string GetNextRegion()
	{
		if (!GameManager.Archipelago.HasValue)
		{
			return null;
		}
		ArchipelagoRegionInfo[] includedRegions = GameManager.Archipelago.Value.IncludedRegions;
		for (int i = 0; i < KUtility.GetSize(includedRegions); i++)
		{
			if (includedRegions[i].Id == GameManager.Region.Id)
			{
				if (i == includedRegions.Length - 1)
				{
					return null;
				}
				return includedRegions[i + 1].Id;
			}
		}
		return null;
	}
}
