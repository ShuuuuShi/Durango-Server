using Durango.Logic.Faction;
using Durango.Utils;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class MissionAlertTargetController : MonoBehaviour
{
	private enum Type
	{
		RadioStation,
		Delivery
	}

	[SerializeField]
	[EnumList(typeof(Type), false, 0, -1)]
	private MissionAlertTargetWidget[] _widgets;

	private void Start()
	{
		GameSystem<FactionSystem>.Instance().MissionStateUpdated += Faction_MissionStateUpdated;
		GameSystem<FactionSystem>.Instance().FactionsUpdated += Faction_FactionUpdated;
		Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		Singleton<ArtifactManager>.Instance().Added += Artifact_Added;
		GameSystem<MapSystem>.Instance().IndicatorsInitialized += MapSystemIndicatorsInitialized;
	}

	private void Set(Type type, FactionSystem.MissionState state)
	{
		GetWidget(type).Set(state);
	}

	private MissionAlertTargetWidget GetWidget(Type type)
	{
		return _widgets[(int)type];
	}

	private void Artifact_Added(Artifact artifact)
	{
		for (int i = 0; i < _widgets.Length; i++)
		{
			MissionAlertTargetWidget missionAlertTargetWidget = _widgets[i];
			if (!missionAlertTargetWidget.IsInitedArtifact() && artifact.Blueprint.HasComponent(missionAlertTargetWidget.Component))
			{
				missionAlertTargetWidget.InitArtifact(artifact);
				missionAlertTargetWidget.Refresh();
			}
		}
	}

	private void GameManager_PreReconnect()
	{
		for (int i = 0; i < _widgets.Length; i++)
		{
			_widgets[i].Release();
		}
	}

	private void MapSystemIndicatorsInitialized()
	{
		for (int i = 0; i < _widgets.Length; i++)
		{
			_widgets[i].UpdateIndicator();
		}
	}

	private void Faction_MissionStateUpdated(FactionSystem.MissionState missionState)
	{
		Set(Type.RadioStation, missionState);
	}

	private void Faction_FactionUpdated()
	{
		int num = 0;
		foreach (Faction faction in GameSystem<FactionSystem>.Instance().GetFactions())
		{
			if (faction.Mission.HasValue)
			{
				num++;
			}
		}
		if (num == 0)
		{
			Set(Type.Delivery, FactionSystem.MissionState.Disabled);
			return;
		}
		FactionSystem.MissionState state = FactionSystem.MissionState.Idle;
		FactionType[] array = Enums<FactionType>.All();
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			MissionToDoCollection missionToDoCollection = GameSystem<FactionSystem>.Instance().FindFactionToDoCollection(array[i]);
			if (missionToDoCollection != null)
			{
				MissionToDo currentToDo = missionToDoCollection.GetCurrentToDo();
				if (currentToDo != null && currentToDo.MissionType == MissionTodoType.Delivery)
				{
					state = FactionSystem.MissionState.Ready;
					break;
				}
			}
		}
		Set(Type.Delivery, state);
	}
}
