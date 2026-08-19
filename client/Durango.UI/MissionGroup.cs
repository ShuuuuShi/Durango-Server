using System;
using Durango.Logic.Faction;
using Durango.Network;
using Durango.UI.Control;
using InteractionData;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MissionGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private FactionsMissionWidget _factionsMission;

	[SerializeField]
	private UILabel _failedLabel;

	public string EntityId { get; private set; }

	public Point2 Tile { get; private set; }

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("임무"));
		UIManager.Popup.FindTooltip<MissionInfoPopup>().Closed += MissionInfoPopup_Closed;
		AddInteractionHandlers();
		SetChildrenActive(activated: false);
	}

	private void MissionInfoPopup_Closed()
	{
		if (!_factionsMission.gameObject.activeInHierarchy)
		{
			Close();
		}
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	public void Open(string entityId, Point2 tile)
	{
		EntityId = entityId;
		Tile = tile;
		_factionsMission.gameObject.SetActive(value: false);
		_failedLabel.gameObject.SetActive(value: false);
		base.Open();
		UIManager.Popup.LoadingRing.AttachToWidget(_factionsMission.transform.parent.gameObject);
		GameSystem<FactionSystem>.Instance().RecommendMissions(entityId, tile, OnRecommendMissions);
	}

	public void Open(MissionInfoPopup.Data mission, bool isAcceptable, bool isCancel = false)
	{
		base.Open();
		SetChildrenActive(activated: false);
		ShowMissionInfo(mission, isAcceptable, isCancel);
	}

	public void ShowMissionInfo(MissionInfoPopup.Data mission, bool isAcceptable, bool isCancel = false)
	{
		MissionInfoPopup missionInfoPopup = UIManager.Popup.FindTooltip<MissionInfoPopup>();
		missionInfoPopup.Show(mission, isAcceptable, isCancel);
	}

	public Transform GetStartButtonTransform()
	{
		SelectableButton startButton = _factionsMission.StartButton;
		return (!(startButton != null)) ? null : startButton.transform;
	}

	protected override bool TryOpen()
	{
		bool result = base.TryOpen();
		MissionInfoPopup missionInfoPopup = UIManager.Popup.FindTooltip<MissionInfoPopup>();
		if (missionInfoPopup.IsVisible)
		{
			missionInfoPopup.Hide();
		}
		return result;
	}

	protected override bool TryClose()
	{
		MissionInfoPopup missionInfoPopup = UIManager.Popup.FindTooltip<MissionInfoPopup>();
		if (missionInfoPopup.IsVisible)
		{
			missionInfoPopup.Hide();
			return false;
		}
		_factionsMission.CloseFactionNode();
		return base.TryClose();
	}

	private void OnRecommendMissions(bool success)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(_factionsMission.transform.parent.gameObject);
		if (!success)
		{
			OnError();
			return;
		}
		_factionsMission.UpdateMissionInfos();
		_factionsMission.gameObject.SetActive(value: true);
		_factionsMission.Widget.alpha = 0f;
		_factionsMission.Alpha = 1f;
	}

	private void AddInteractionHandlers()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AcceptMission, delegate(InteractionObject target)
		{
			int level = GameSystem<StatisticsSystem>.Instance().Level;
			int activationLevel = Singleton<Constants>.Instance.Faction.Mission.ActivationLevel;
			if (level < activationLevel)
			{
				UIManager.SystemMsg(T._("{0:lv:} 부터 임무를 받을 수 있습니다.", activationLevel));
			}
			else
			{
				Open(target.EntityId, new Point2(target.Tile));
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReportFactionProp, delegate(InteractionObject target)
		{
			Connections.Frontend.Send(new ReportFactionProp
			{
				EntityId = target.EntityId,
				EntityType = (ushort)target.EntityType,
				Tile = new Point2(target.Tile)
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.CancelAllMissions, delegate(InteractionObject target)
		{
			UIManager.MessageBox.Show(T._("진행 중인 모든 임무를 중단합니다."), delegate(bool ok)
			{
				if (!ok)
				{
					return;
				}
				foreach (Durango.Logic.Faction.Faction faction in GameSystem<FactionSystem>.Instance().GetFactions())
				{
					if (faction.Mission.HasValue)
					{
						Mission value = faction.Mission.Value;
						double? startedAt = value.StartedAt;
						if (startedAt.HasValue)
						{
							FactionSystem.CancelAndRecommendMission(value.Id, target.EntityId, new Point2(target.Tile));
						}
					}
				}
			});
		});
	}

	private void OnError()
	{
		_factionsMission.gameObject.SetActive(value: false);
		_failedLabel.gameObject.SetActive(value: true);
	}
}
