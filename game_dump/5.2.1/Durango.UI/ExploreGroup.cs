using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Explore;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ExploreGroup : UIBase
{
	public enum RouteType
	{
		Normal,
		Shared,
		Neighbor
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private RoutesViewer _routeViewer;

	[CanBeNull]
	private Port _port;

	private InfoTooltip _lastTooltip;

	private Route? _lastRoute;

	private RouteType _routeType;

	public event Action<Route> TravelRequested;

	private void Start()
	{
		AddInteractionHandlers();
		_titleWidget.Object.SetTitle(T._("항해"));
		_routeViewer.InitializeStaticData();
		GameSystem<ExploreSystem>.Instance().RoutesUpdated += OnRoutesUpdated;
		if (GameManager.Region.Id == ExploreSystem.LastFoundRegion)
		{
			UIManager.Alarm.RewardAlarm(new AlarmRewardQueue.Args
			{
				Main = "<em>" + GameManager.Region.Name + "</em>",
				Sub = T._("섬 발견")
			}, AlarmGroup.RewardEffectType.ExplorePOI);
		}
		ExploreSystem.LastFoundRegion = string.Empty;
		base.TryClose();
	}

	private void OnEnable()
	{
		GameSystem<PartySystem>.Instance().LeaderChanged += PartySystem_LeaderChanged;
	}

	private void OnDisable()
	{
		GameSystem<PartySystem>.Instance().LeaderChanged -= PartySystem_LeaderChanged;
	}

	private void PartySystem_LeaderChanged()
	{
		if (_routeType == RouteType.Shared)
		{
			_routeViewer.ViewerReset();
			Close();
		}
	}

	protected override bool TryOpen()
	{
		if (_port == null)
		{
			return false;
		}
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		if (!_routeViewer.Back())
		{
			return false;
		}
		_port = null;
		if (_lastTooltip != null)
		{
			_lastTooltip.Hide();
			_lastTooltip = null;
			_lastRoute = null;
		}
		return base.TryClose();
	}

	public void SelectUnstableArea([CanBeNull] RegionTemplate regionTemplate)
	{
		_routeViewer.SelectUnstableRoutes(regionTemplate);
	}

	public void ShowUnknownArchipelagoInfoTooltip(ArchipelagoRoute archipelagoRoute)
	{
		_lastTooltip = UIManager.Popup.Tooltip<InfoTooltip>();
		RouteInfoTooltip.ShowUnknown(_lastTooltip, delegate
		{
			RecommendArchipelago(archipelagoRoute);
		});
	}

	private void RecommendArchipelago(ArchipelagoRoute archipelagoRoute)
	{
		if (_routeType == RouteType.Shared)
		{
			UIManager.SystemMsg(T._("공유중인 항로에서는 새로운 군도를 발견할 수 없습니다."));
			return;
		}
		if (archipelagoRoute.Level < 30)
		{
			UIManager.ShowLoadingIcon(show: true);
			GameSystem<ExploreSystem>.Instance().RecommendArchipelago(archipelagoRoute, OnFoundArchipelago);
			return;
		}
		UIManager.MessageBox.Show(T._("새로운 불안정 군도를 발견하면\n같은 해역의 모든 개척임무가 <em>초기화</em>됩니다.\n\n미지의 땅을 발견하시겠습니까?"), null, delegate(bool ok)
		{
			if (ok)
			{
				UIManager.ShowLoadingIcon(show: true);
				GameSystem<ExploreSystem>.Instance().RecommendArchipelago(archipelagoRoute, OnFoundArchipelago);
			}
		});
	}

	public void ShowUnknownUrbanInfoTooltip()
	{
		_lastTooltip = UIManager.Popup.Tooltip<InfoTooltip>();
		_lastRoute = null;
		RouteInfoTooltip.ShowUnknown(_lastTooltip, delegate
		{
			RecommendUrban();
		});
	}

	private void RecommendUrban()
	{
		if (_port != null)
		{
			UIManager.ShowLoadingIcon(show: true);
			GameSystem<ExploreSystem>.Instance().RecommendRegion(_port, Role.Urban, null, OnFoundRegion);
		}
	}

	public void ShowUnknownRouteInfoTooltip(Role role, string templateId)
	{
		_lastTooltip = UIManager.Popup.Tooltip<InfoTooltip>();
		_lastRoute = null;
		RouteInfoTooltip.ShowUnknown(_lastTooltip, delegate
		{
			RecommendRegion(role, templateId);
		});
	}

	private void RecommendRegion(Role role, string templateId)
	{
		if (_port != null)
		{
			UIManager.ShowLoadingIcon(show: true);
			GameSystem<ExploreSystem>.Instance().RecommendRegion(_port, role, templateId, OnFoundRegion);
		}
	}

	public void ShowRouteInfoTooltip(Route route, [CanBeNull] string notice = null)
	{
		if (!(route.RegionId == GameManager.Region.Id))
		{
			_lastTooltip = UIManager.Popup.Tooltip<InfoTooltip>();
			_lastRoute = route;
			RouteInfoTooltip.Show(_lastTooltip, route, TravelRegion, notice);
		}
	}

	private void TravelRegion(Route route)
	{
		if (_port == null)
		{
			return;
		}
		string text = T._("{0} 섬으로 이동하시겠습니까?", route.Region().Name);
		if (!PlayerBehavior.LocalPlayer.HasClan && route.Region().Role() == Role.Outpost)
		{
			text = text + "\n" + T._("<alert>소속 부족이 없으면 {0}에서 항상 공격 받을 수 있습니다</alert>", route.Region().Role().GetName());
		}
		Port port = _port;
		Action<bool> onOkCancel = delegate(bool ok)
		{
			if (ok)
			{
				float delay = ((this.TravelRequested == null) ? 0f : 2f);
				string partierId = ((_routeType != RouteType.Shared) ? null : GameSystem<PartySystem>.Instance().LeaderEntityId);
				bool goesNeighbor = _routeType == RouteType.Neighbor;
				GameSystem<ExploreSystem>.Instance().TravelRegion(port, route.RegionId, partierId, goesNeighbor, delay);
				if (this.TravelRequested != null)
				{
					this.TravelRequested(route);
				}
			}
		};
		Money money = ((!route.Price.HasValue) ? Money.ForFree : route.Price.Value);
		if (money.Amount > 0)
		{
			UIManager.MessageBox.ShowPayConfirm(money.Amount, money.Currency, text, onOkCancel);
		}
		else
		{
			UIManager.MessageBox.Show(text, onOkCancel);
		}
	}

	public void Open(string entityId, Point2 tile, RouteType routeType)
	{
		_port = new Port
		{
			Id = entityId,
			Tile = tile,
			Region = GameManager.Region
		};
		_routeType = routeType;
		switch (_routeType)
		{
		case RouteType.Normal:
			GameSystem<ExploreSystem>.Instance().RequestRoutes(entityId, tile);
			break;
		case RouteType.Shared:
			GameSystem<ExploreSystem>.Instance().RequestRoutesOfParty(entityId, tile);
			break;
		case RouteType.Neighbor:
			GameSystem<ExploreSystem>.Instance().RequestRouteOfArchipelago(entityId, tile);
			break;
		}
		Open();
	}

	private void AddInteractionHandlers()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingRoutes, delegate(InteractionObject target)
		{
			Open(target.EntityId, new Point2(target.Tile), RouteType.Normal);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingRoutesOfParty, delegate(InteractionObject target)
		{
			Open(target.EntityId, new Point2(target.Tile), RouteType.Shared);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingArchipelagoRegion, delegate(InteractionObject target)
		{
			Open(target.EntityId, new Point2(target.Tile), RouteType.Neighbor);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingPersonalRegion, delegate
		{
			UIManager.FindScript<PlayerSearchGroup>().OpenForPersonalSailing(delegate(IList<string> list)
			{
				string text2 = list.FirstOrDefault();
				if (!string.IsNullOrEmpty(text2))
				{
					EstateSystem.VisitEstate(OwnerType.PersonalPlayer, text2);
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingBack, delegate(InteractionObject target)
		{
			GameSystem<ExploreSystem>.Instance().GetSailingBackCost(delegate(long cost)
			{
				RegionTile? lastReturnPoint = GameSystem<MapSystem>.Instance().Points.LastReturnPoint;
				if (lastReturnPoint.HasValue)
				{
					Messages.Region region = lastReturnPoint.Value.Region;
					RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
					string comment = T._("{0} {1:lv:}\n탐험하던 섬으로 이동합니다.", region.Name, regionTemplate?.Level ?? 0);
					UIManager.MessageBox.ShowPayConfirm(cost, Currency.TStone, comment, delegate(bool ok)
					{
						if (ok)
						{
							GameSystem<ExploreSystem>.Instance().SailingBack(target.EntityId, new Point2(target.Tile));
						}
					});
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingWithdraw, delegate(InteractionObject target)
		{
			List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
			bool flag = false;
			for (int i = 0; i < playerItemList.Count; i++)
			{
				if (playerItemList[i].Unstable)
				{
					flag = true;
					break;
				}
			}
			string text = T._("출발했던 안정섬 항구로 돌아가시겠습니까?");
			if (flag)
			{
				text += string.Format("\n<alert>{0}</alert>", T._("가방/동물 가방에 불안정 아이템이 있습니다.\n화물 워프홀로 사유지에 전송하지 않을 경우 아이템은 모두 삭제됩니다.\n그래도 섬을 이동하시겠습니까?"));
			}
			UIManager.MessageBox.Show(text, delegate(bool ok)
			{
				if (ok)
				{
					ExploreSystem.Withdraw(target.EntityId, new Point2(target.Tile));
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingRandomPersonalRegion, delegate
		{
			UIManager.MessageBox.Show(T._("무작위 항해를 하시겠습니까?"), T._("외부인의 방문을 허락한 누군가의 섬으로 항해합니다."), delegate(bool ok)
			{
				if (ok)
				{
					ExploreSystem.TravelToRandomPersonalRegion();
				}
			});
		});
	}

	private void OnRoutesUpdated()
	{
		if (base.IsOpened && GameSystem<ExploreSystem>.Instance().HasRoutes())
		{
			_routeViewer.OnLoad(_routeType);
		}
	}

	private void OnFoundArchipelago(Messages.Archipelago archipelago)
	{
		UIManager.ShowLoadingIcon(show: false);
		if (!string.IsNullOrEmpty(archipelago.Id) && _port != null)
		{
			GameSystem<ExploreSystem>.Instance().RequestRoutes(_port.Id, _port.Tile);
		}
	}

	private void OnFoundRegion(Durango.Logic.Explore.Region region)
	{
		UIManager.ShowLoadingIcon(show: false);
		if (region != null && _port != null)
		{
			GameSystem<ExploreSystem>.Instance().RequestRoutes(_port.Id, _port.Tile);
		}
	}

	public Transform GetIslandTransoform(Role role, Biome biome, int level)
	{
		return _routeViewer.GetIslandTransoform(role, biome, level);
	}

	public bool IsTargetToolTipVisible(Role role, Biome biome, int level)
	{
		if (_lastTooltip == null || !_lastTooltip.IsVisible || !_lastRoute.HasValue)
		{
			return false;
		}
		return _lastRoute.Value.IsTargetRegion(role, biome, level);
	}

	public Transform GetTooltipButtonTransoform()
	{
		if (_lastTooltip == null || _lastTooltip.Button == null)
		{
			return null;
		}
		return _lastTooltip.Button.transform;
	}
}
