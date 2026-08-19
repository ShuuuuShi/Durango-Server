using System.Text;
using Durango.Logic.Explore;
using Durango.Logic.PlayGuide;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using Yaml;

namespace Durango.Logic;

public class ArchipelagoToDoCollection : ToDoCollection
{
	public enum State
	{
		Doing,
		Reportable,
		Done,
		CanDo
	}

	public readonly Observable<int> CurrentPoint = new Observable<int>();

	public int ClearPoint;

	public Durango.Logic.Explore.Region ActiveRegion;

	public Dialogue Intro;

	public Dialogue Outro;

	public string Description;

	public RewardInfo? Reward;

	public State CurrentState;

	public bool ShowUI;

	public bool HasEnoughPoint => (int)CurrentPoint >= ClearPoint;

	public ArchipelagoToDoCollection()
	{
		SetHelpClicked(delegate
		{
			MissionGroup missionGroup = UIManager.FindScript<MissionGroup>();
			if (!(missionGroup == null))
			{
				MissionInfoPopup.Data data = default(MissionInfoPopup.Data);
				data.ClientName = string.Format("[icon=icon_mission]  {0}", T._("개척 임무"));
				data.Subject = Title;
				data.Reward = Reward;
				MissionInfoPopup.Data mission = data;
				using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
				{
					StringBuilder value = reusable.Value;
					foreach (ToDoBase toDo in ToDoList)
					{
						if (toDo is ArchipelagoToDo archipelagoToDo)
						{
							value.Append(T._("<em>{0}</em> ({1:pt:})\n", archipelagoToDo.LocalText, archipelagoToDo.Point));
						}
					}
					value.Append("\n");
					value.Append(Description);
					mission.Description = value.ToString();
				}
				missionGroup.Open(mission, isAcceptable: false);
			}
		});
	}

	public override string GetSubIcon()
	{
		return "mission_unstable_factor";
	}

	public override Detail? GetDetail()
	{
		if (!ShowUI)
		{
			return null;
		}
		switch (CurrentState)
		{
		case State.Doing:
		{
			Detail value3 = default(Detail);
			value3.IsHeaderVisible = true;
			value3.IsTodoListVisible = true;
			value3.Progress = new Pair<int, int>(CurrentPoint, ClearPoint);
			return value3;
		}
		case State.Reportable:
		{
			string text3 = string.Format("<em>[icon=icon_mission_todo] {0}</em>", T._("개척 임무 단계 완료"));
			string text4 = T._("개척 임무를 완료했습니다.\n진행 상황을 보고하세요.");
			Detail value2 = default(Detail);
			value2.CommonText = "\n[size=24]" + text3 + "[/size]<br>10</br>" + text4;
			value2.CommonTextAlignment = NGUIText.Alignment.Center;
			value2.ButtonText = T._("임무 보고");
			value2.ButtonClicked = ReportArchipelagoMission;
			value2.ButtonEffect = PresetButton.Effect.Emphasis;
			value2.ButtonStyle = PresetButton.Style.Solid;
			return value2;
		}
		case State.Done:
		{
			string text5 = T._("<em>다음 단계 진행</em>");
			string text6 = T._("개척 임무를 완료했습니다.\n다음 섬으로 이동하세요.");
			Detail value4 = default(Detail);
			value4.CommonText = "\n[size=24]" + text5 + "[/size]<br>10</br>" + text6;
			value4.CommonTextAlignment = NGUIText.Alignment.Center;
			value4.ButtonText = string.Format("[icon=icon_map_warphole] {0}", T._("다음 섬으로"));
			value4.ButtonClicked = WarpToNextArchipelagoRegion;
			value4.ButtonStyle = PresetButton.Style.Border;
			return value4;
		}
		case State.CanDo:
		{
			string text = T._("<em>다른 군도의 개척 임무</em>");
			string text2 = T._("<em>{0}</em> 섬에서 진행중인 개척 임무가 있습니다.", ActiveRegion.Name);
			Detail value = default(Detail);
			value.CommonText = "\n[size=24]" + text + "[/size]<br>38</br>" + text2;
			value.CommonTextAlignment = NGUIText.Alignment.Center;
			value.ButtonText = T._("개척 임무 새로 받기");
			value.ButtonClicked = RequestNewArchipelagoMission;
			value.ButtonStyle = PresetButton.Style.Border;
			return value;
		}
		default:
			return null;
		}
	}

	public void Update(NotifyArchipelagoTodoProceed todoProgress)
	{
		CurrentPoint.Value = todoProgress.CurrentPoint;
		Messages.ArchipelagoToDo todo = todoProgress.Todo;
		foreach (ToDoBase toDo in ToDoList)
		{
			if (toDo is ArchipelagoToDo archipelagoToDo && !(archipelagoToDo.Key != todo.Id))
			{
				archipelagoToDo.CurrentProgress = todo.Progress;
			}
		}
	}

	private static void ReportArchipelagoMission()
	{
		GameSystem<ArchipelagoMissionSystem>.Instance().RequestRegionClear();
	}

	private static void WarpToNextArchipelagoRegion()
	{
		string nextRegion = GameSystem<ArchipelagoMissionSystem>.Instance().GetNextRegion();
		if (string.IsNullOrEmpty(nextRegion))
		{
			return;
		}
		GameSystem<MapSystem>.Instance().GetRegion(nextRegion, delegate(Messages.Region region)
		{
			ArchipelagoMissionSystem.RequestWarpCost(delegate(long cost)
			{
				string comment = T._("<em>{0}</em> 섬으로 이동하시겠습니까?", region.Name);
				UIManager.MessageBox.ShowPayConfirm(cost, Currency.TStone, comment, null, delegate(bool ok)
				{
					if (ok)
					{
						ExploreSystem.WarpToNextArchipelagoRegion(region);
					}
				});
			});
		});
	}

	private static void RequestNewArchipelagoMission()
	{
		UIManager.MessageBox.Show(T._("개척 임무를 새로 받으면\n진행중인 개척 임무는 <em>초기화</em>됩니다.\n\n이 군도의 개척 임무를 새로 받으시겠습니까?"), null, delegate(bool ok)
		{
			if (ok)
			{
				ArchipelagoMissionSystem.RequestReissueArchipelagoTodos();
			}
		});
	}
}
