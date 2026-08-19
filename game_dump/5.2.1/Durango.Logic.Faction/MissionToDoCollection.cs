using System.Collections.Generic;
using System.Linq;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Faction;

public class MissionToDoCollection : ToDoCollection
{
	private struct SkippableInfo
	{
		public string TodoId;

		public int LastProgressCount;

		public double DelayedTime;

		public bool IsNotified;

		public void Reset()
		{
			TodoId = string.Empty;
			LastProgressCount = -1;
			DelayedTime = double.MaxValue;
			IsNotified = false;
		}

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(TodoId);
		}
	}

	private readonly FactionType _factionType;

	private string _regionName;

	private Mission _mission;

	private SkippableInfo _skippableInfo;

	public bool IsSkippable => Connections.Frontend.GetPredictedServerTime() > _skippableInfo.DelayedTime;

	public string Id => _mission.Id;

	public MissionToDoCollection(FactionType factionType)
	{
		_skippableInfo.Reset();
		_factionType = factionType;
		Icon = Durango.Logic.PlayGuide.Util.FactionTypeToNPCType(factionType).ToDoIcon();
		base.Key = MissionToDoUpdater.GenerateKey(factionType);
		SetHelpClicked(delegate
		{
			MissionGroup missionGroup = UIManager.FindScript<MissionGroup>();
			if (missionGroup != null)
			{
				missionGroup.Open(new MissionInfoPopup.Data(_mission), isAcceptable: true, isCancel: true);
			}
		});
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(factionType);
		if (faction != null)
		{
			Season = faction.Season;
		}
	}

	public override Detail? GetDetail()
	{
		Detail? detail = base.GetDetail();
		if (!detail.HasValue || !IsSkippable)
		{
			return detail;
		}
		Detail value = detail.Value;
		value.ButtonText = T._("즉시 완료");
		value.ButtonStyle = PresetButton.Style.Solid;
		value.ButtonClicked = delegate
		{
			FactionSystem.RequestSkipTutorialMission(Id);
		};
		return value;
	}

	public void UpdateMission(Mission mission)
	{
		_mission = mission;
		Title = mission.Subject;
		_regionName = string.Empty;
		if (GameManager.Region.Id != mission.RegionId)
		{
			base.IsDisabled = true;
			GameSystem<MapSystem>.Instance().GetRegion(mission.RegionId, RegionReceived);
		}
	}

	public void UpdateTodos(bool added)
	{
		List<ToDoBase> toDoList = ToDoList;
		int size = KUtility.GetSize(_mission.Todos);
		for (int i = 0; i < size; i++)
		{
			Messages.MissionToDo missionToDo = _mission.Todos[i];
			MissionToDo missionToDo2;
			if (i < toDoList.Count)
			{
				missionToDo2 = (MissionToDo)toDoList[i];
			}
			else
			{
				missionToDo2 = new MissionToDo();
				toDoList.Add(missionToDo2);
			}
			missionToDo2.Key = base.Key;
			missionToDo2.ExpiresAt = ((!_mission.StartedAt.HasValue || !_mission.TimeLimit.HasValue) ? null : new double?(_mission.StartedAt.Value + (double)_mission.TimeLimit.Value));
			bool num = !added && (missionToDo2.TargetProgress != missionToDo.GoalCount || missionToDo2.CurrentProgress != missionToDo.Progress || missionToDo2.LocalText != missionToDo.Label);
			missionToDo2.Id = missionToDo.Id;
			missionToDo2.LocalText = missionToDo.Label;
			missionToDo2.DisplayText = missionToDo.Label;
			missionToDo2.TargetProgress = missionToDo.GoalCount;
			missionToDo2.CurrentProgress = missionToDo.Progress;
			missionToDo2.IsCompleted = missionToDo2.TargetProgress == missionToDo2.CurrentProgress;
			missionToDo2.Tooltip = missionToDo.Tooltip;
			missionToDo2.FactionType = _factionType;
			missionToDo2.TargetTile = ((!missionToDo.Destination.HasValue) ? Vector2.zero : missionToDo.Destination.Value.ToVector2());
			missionToDo2.MissionType = missionToDo.Type;
			missionToDo2.MissionOrder = missionToDo.Order;
			if (num)
			{
				GameSystem<ToDoListSystem>.Instance().SetUpdated(missionToDo2);
			}
		}
		UpdateNavigate();
		UpdateSkippable();
	}

	[CanBeNull]
	public MissionToDo GetCurrentToDo()
	{
		if (base.IsDisabled || ToDoList == null)
		{
			return null;
		}
		MissionToDo missionToDo = null;
		foreach (ToDoBase toDo in ToDoList)
		{
			if (toDo is MissionToDo missionToDo2)
			{
				if (missionToDo2.MissionOrder != MissionTodoOrder.StartWithPrev)
				{
					missionToDo = missionToDo2;
				}
				if (!missionToDo2.IsCompleted && missionToDo != null)
				{
					return missionToDo;
				}
			}
		}
		return null;
	}

	public override void Update()
	{
		base.Update();
		if (IsSkippable && !_skippableInfo.IsNotified)
		{
			_skippableInfo.IsNotified = true;
			GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
		}
	}

	public override SyncString GetMessage()
	{
		if (string.IsNullOrEmpty(_regionName))
		{
			return base.GetMessage();
		}
		return T._("<em>{0}</em> 섬에서 임무를 진행할 수 있습니다.", _regionName);
	}

	public override void OnAddItem()
	{
		base.OnAddItem();
		UpdateNavigate();
	}

	private void UpdateSkippable()
	{
		SkipTutorialMissions skippableMission = Singleton<Constants>.Instance.SkipTutorialMissionses.Get(_mission.Id);
		if (skippableMission != null)
		{
			MissionToDo missionToDo = ToDoList.Select((ToDoBase todo) => todo as MissionToDo).FirstOrDefault((MissionToDo todo) => todo != null && todo.Id == skippableMission.TodoId);
			if (missionToDo != null && !missionToDo.IsCompleted)
			{
				if (_skippableInfo.LastProgressCount != missionToDo.CurrentProgress)
				{
					_skippableInfo.TodoId = missionToDo.Id;
					_skippableInfo.LastProgressCount = missionToDo.CurrentProgress;
					_skippableInfo.DelayedTime = Connections.Frontend.GetPredictedServerTime() + (double)skippableMission.SkipTime;
				}
				return;
			}
		}
		if (!_skippableInfo.IsEmpty())
		{
			_skippableInfo.Reset();
			GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
		}
	}

	private void UpdateNavigate()
	{
		MissionToDo currentToDo = GetCurrentToDo();
		if (currentToDo == null || !currentToDo.HasTile())
		{
			UIManager.FindScript<NavigateGroup>().Point.ClearTarget(base.Key);
			return;
		}
		Vector3 value = Durango.Terrain.Util.TilePositionToClientPosition(currentToDo.TargetTile);
		UIManager.FindScript<NavigateGroup>().Point.SetTarget(base.Key, new PointTargetController.Arguments
		{
			Position = value,
			Icon = Icon,
			Season = Season
		});
	}

	public override void OnRemoveItem()
	{
		base.OnRemoveItem();
		UIManager.FindScript<NavigateGroup>().Point.ClearTarget(base.Key);
	}

	private void RegionReceived(Region region)
	{
		_regionName = region.Name;
		GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
	}
}
