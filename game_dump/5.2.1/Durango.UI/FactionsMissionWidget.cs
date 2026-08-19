using System;
using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class FactionsMissionWidget : AnimationWidget, IUIInitializable
{
	[SerializeField]
	private MissionActionBar _actionBar;

	[SerializeField]
	private KGridScrollView _factionScrollView;

	[SerializeField]
	private FactionPortraits _factionPortraits;

	private MissionGroup _parent;

	private FactionType _selectedType;

	public SelectableButton StartButton => _actionBar.GetStartButton();

	void IUIInitializable.Init()
	{
		_parent = GetComponentInParent<MissionGroup>();
		List<FactionType> missionFactionTypes = GameSystem<FactionSystem>.Instance().MissionFactionTypes;
		int size = KUtility.GetSize(missionFactionTypes);
		ListObjectPool nodes = _factionScrollView.Nodes;
		nodes.Set(size);
		for (int i = 0; i < size; i++)
		{
			MissionFactionNode component = nodes[i].GetComponent<MissionFactionNode>();
			component.Init();
			PortraitMaterial portraitMaterial = _factionPortraits.Get(missionFactionTypes[i]);
			component.SetFactionType(missionFactionTypes[i], portraitMaterial.Material, portraitMaterial.Uv);
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickFactionNode));
		}
		_factionScrollView.ResetPosition();
		_actionBar.MissionStartClicked += delegate(Mission mission)
		{
			_parent.ShowMissionInfo(new MissionInfoPopup.Data(mission), isAcceptable: true);
		};
		_actionBar.MissionCancelClicked += delegate(Mission mission)
		{
			UIManager.MessageBox.Show(T._("진행 중인 임무를 중단합니다."), delegate(bool ok)
			{
				if (ok)
				{
					FactionSystem.CancelAndRecommendMission(mission.Id, _parent.EntityId, _parent.Tile);
				}
			});
		};
		_actionBar.MissionRefreshClicked += delegate(Mission mission)
		{
			if (GameSystem<FactionSystem>.Instance().ShuffleCondition.RemainCount > 0)
			{
				GameSystem<FactionSystem>.Instance().ShuffleMission(_parent.EntityId, _parent.Tile, mission.Faction);
			}
			else
			{
				FactionSystem.GetRechargeShuffleCost(mission.Faction, delegate(Costs costs)
				{
					long cost2 = costs._Costs.Get(Currency.Gem, 0L);
					UIManager.MessageBox.ShowPayConfirm(cost2, Currency.Gem, T._("다른 임무 받기 횟수를 충전합니다."), delegate(bool ok)
					{
						if (ok)
						{
							GameSystem<FactionSystem>.Instance().RechargeMissionShuffleCount(_parent.EntityId, _parent.Tile);
						}
					}, T._("예"));
				});
			}
		};
		_actionBar.MissionResetCooltimeClicked += delegate(FactionType faction)
		{
			FactionSystem.GetRecommendMissionImmediatelyCost(faction, delegate(Costs costs)
			{
				long cost = costs._Costs.Get(Currency.Gem, 0L);
				UIManager.MessageBox.ShowPayConfirm(cost, Currency.Gem, T._("다음 임무를 바로 받으시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						FactionSystem.RecommendMissionImmediately(_parent.EntityId, _parent.Tile, faction);
					}
				}, T._("예"));
			});
		};
		_actionBar.MissionDetailClicked += delegate(Mission mission)
		{
			_parent.ShowMissionInfo(new MissionInfoPopup.Data(mission), isAcceptable: false);
		};
	}

	private void OnEnable()
	{
		UpdateGridLayout();
		GameSystem<FactionSystem>.Instance().FactionsUpdated += UpdateMissionInfos;
	}

	private void OnDisable()
	{
		GameSystem<FactionSystem>.Instance().FactionsUpdated -= UpdateMissionInfos;
	}

	private void UpdateGridLayout()
	{
		Vector2 viewSize = _factionScrollView.ViewSize;
		Point2 size = ((!UIManager.IsPortraitWidget(base.gameObject)) ? new Point2(new Vector2(viewSize.x / 2f, viewSize.y / 2f)) : new Point2(new Vector2(viewSize.x, viewSize.y / 4f)));
		ForEachNodes(delegate(MissionFactionNode node)
		{
			node.Widget.SetDimensions(size.x, size.y);
			node.UpdateLayout();
		});
		_factionScrollView.ResetPosition();
	}

	public void CloseFactionNode()
	{
		ForEachNodes(delegate(MissionFactionNode node)
		{
			node.MissionWidgetOpened = false;
		});
	}

	public void UpdateMissionInfos()
	{
		_actionBar.SetShuffleCondition(GameSystem<FactionSystem>.Instance().ShuffleCondition);
		_actionBar.SetDailyMissionAvailableAt(GameSystem<FactionSystem>.Instance().DailyMissionAvailableAt);
		int num = -1;
		int num2 = -1;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		List<FactionType> missionFactionTypes = GameSystem<FactionSystem>.Instance().MissionFactionTypes;
		int i = 0;
		for (int size = KUtility.GetSize(missionFactionTypes); i < size; i++)
		{
			FactionType factionType = missionFactionTypes[i];
			MissionFactionNode missionFactionNode = _factionScrollView.Nodes.Get<MissionFactionNode>(i);
			Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(factionType);
			if (faction == null || !faction.Mission.HasValue)
			{
				double num3 = faction?.MissionAvailableAt ?? 0.0;
				string text = ((faction != null) ? faction.RecommendFailReason : string.Empty);
				if (num3 > predictedServerTime)
				{
					missionFactionNode.SetCooltime(num3);
					missionFactionNode.Disabled = false;
				}
				else if (!string.IsNullOrEmpty(text))
				{
					missionFactionNode.SetHasntMission(text);
					missionFactionNode.Disabled = true;
				}
				else if (faction != null && faction.Level > 0)
				{
					missionFactionNode.SetHasntMission(T._("전달받은 통신이 없습니다."));
					missionFactionNode.Disabled = true;
				}
				else
				{
					missionFactionNode.SetUnknown();
					missionFactionNode.Disabled = true;
				}
			}
			else
			{
				missionFactionNode.Set(faction.Mission.Value);
				missionFactionNode.Disabled = false;
			}
			if (!missionFactionNode.Disabled)
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				if (_selectedType == factionType)
				{
					num = i;
				}
			}
		}
		if (num == -1)
		{
			_selectedType = ((num2 != -1) ? missionFactionTypes[num2] : FactionType.Invalid);
		}
		SelectFaction(_selectedType);
	}

	private void OnClickFactionNode()
	{
		MissionFactionNode missionFactionNode = Selectable.Current as MissionFactionNode;
		if (!(missionFactionNode == null))
		{
			SelectFaction(missionFactionNode.Type);
		}
	}

	private void SelectFaction(FactionType type)
	{
		_selectedType = type;
		ForEachNodes(delegate(MissionFactionNode node)
		{
			node.Selected = node.Type == _selectedType;
		});
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_selectedType);
		_actionBar.SetFaction(faction);
	}

	private void ForEachNodes(Action<MissionFactionNode> action)
	{
		int i = 0;
		for (int count = _factionScrollView.Nodes.Count; i < count; i++)
		{
			MissionFactionNode obj = _factionScrollView.Nodes.Get<MissionFactionNode>(i);
			action(obj);
		}
	}
}
