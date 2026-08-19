using System;
using System.Collections.Generic;
using ExploreData;
using ItemSystem;
using L10N;
using Messages;
using Shared.Region;
using Shared.System;
using UnityEngine;

public class ExploreGroup : UIBase
{
	public Action RequestExplore;

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private RoutesViewer _routeViewer;

	[SerializeField]
	private UISpriteLabel _portInfoLabel;

	private Port _port;

	private ExploreData.Route _selectedUnknownRoute;

	private string _portInfoFormat;

	private void Start()
	{
		AddInteractionHandlers();
		_titleWidget.OnClose += base.ForceClose;
		_titleWidget.OnBack += Close;
		_routeViewer.RouteClicked += OnClickRoute;
		ExploreData.Region region = KSingleton<GameManager>.Instance().Region;
		if (region != null && region.Id == ExploreSystem.LastFoundRegion)
		{
			UIManager.FindScript<RewardAlarmGroup>().Show(T._("<em>{0}</em> 섬 발견", KSingleton<GameManager>.Instance().Region.Name), null, RewardAlarmGroup.RewardEffectType.SkillCategoryLevelUp);
		}
		ExploreSystem.LastFoundRegion = 0uL;
		base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<ExploreSystem>.Instance().RoutesUpdated += OnRoutesUpdated;
		GameSystem<ExploreSystem>.Instance().FoundRegion += OnFoundRegion;
	}

	private void OnDisable()
	{
		GameSystem<ExploreSystem>.Instance().RoutesUpdated -= OnRoutesUpdated;
		GameSystem<ExploreSystem>.Instance().FoundRegion += OnFoundRegion;
	}

	protected override bool OnOpen()
	{
		if (_port == null)
		{
			return false;
		}
		KSingleton<PlayerController>.Instance().IsGestureProcessed += OnGestureProcess;
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		_port = null;
		KSingleton<PlayerController>.Instance().IsGestureProcessed -= OnGestureProcess;
		return base.OnClose();
	}

	private void OnGestureProcess(PlayerController.Gesture gesture, Vector3 pos, bool touchUI, ref bool result)
	{
		if (gesture != PlayerController.Gesture.Zoom || !_routeViewer.IsOpen)
		{
			return;
		}
		if (pos.z > 0f)
		{
			if (!_routeViewer.IsInner)
			{
				_routeViewer.ShowInner(instant: false);
			}
		}
		else if (_routeViewer.IsInner)
		{
			_routeViewer.ShowOutter(instant: false);
		}
	}

	private void OnClickRoute(ExploreData.Route route)
	{
		InfoTooltip tooltip = UIManager.Popup.Tooltip<InfoTooltip>();
		RouteInfoTooltip.Show(tooltip, route, TravelRegion);
	}

	private void TravelRegion(ExploreData.Route route)
	{
		if (route.Region.Id == 0L)
		{
			UIManager.MessageBox.Show(T._("{0:lv:} 미지의 섬을 찾으시겠습니까?", route.Region.Level), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<ExploreSystem>.Instance().RecommendRegion(_port, route.Region.TemplateId);
					UIManager.Popup.IsLoading = true;
				}
			});
			return;
		}
		string text = null;
		text = ((route.Price <= 0) ? T._("{0} 섬으로 이동하시겠습니까?", route.Region.Name) : T._("{0} 섬으로 이동하시겠습니까?\n{1} 의 비용이 필요합니다", route.Region.Name, ItemSystem.Inventory.CurrencyFormat(route.Price, route.CurrencyType)));
		UIManager.MessageBox.Show(text, delegate(bool ok)
		{
			if (ok)
			{
				if (route.Price > GameSystem<InventorySystem>.Instance().PlayerInventory.GetBalance(route.CurrencyType))
				{
					UIManager.SystemMsg(T._("비용이 부족합니다"));
				}
				else
				{
					_routeViewer.DrawLine(route, delegate
					{
						KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
						{
							GameSystem<ExploreSystem>.Instance().TravelRegion(_port, route);
						}, 0.5f);
					});
				}
			}
		});
	}

	public bool IsRouteMode()
	{
		return true;
	}

	public Transform GetEnterButtonTransform()
	{
		return null;
	}

	public Transform GetExploreButtonTransform()
	{
		return null;
	}

	public void Open(ulong entityId, Point2 tile)
	{
		_port = new Port
		{
			Id = entityId,
			Tile = tile,
			Region = KSingleton<GameManager>.Instance().Region
		};
		((Component)_routeViewer).gameObject.SetActive(false);
		_portInfoLabel.text = string.Empty;
		GameSystem<ExploreSystem>.Instance().RequestPort(entityId, tile);
		Open();
	}

	private void AddInteractionHandlers()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingRoutes, SailingRoutesHandler);
	}

	private void OnRoutesUpdated(Routes routes)
	{
		List<ExploreData.Route> inner = new List<ExploreData.Route>();
		List<ExploreData.Route> outter = new List<ExploreData.Route>();
		_routeViewer.Active(KSingleton<GameManager>.Instance().Region);
		Dictionary<ulong, Messages.Route> queryRoutes = new Dictionary<ulong, Messages.Route>();
		Dictionary<Role, Dictionary<string, KeyValuePair<bool, Messages.Route[]>>>.Enumerator enumerator = routes._Routes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<string, KeyValuePair<bool, Messages.Route[]>>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if ((enumerator.Current.Key == Role.Risky || enumerator.Current.Key == Role.Troubled) && enumerator2.Current.Value.Key)
				{
					outter.Add(MakeEmptyRoute(enumerator2.Current.Key));
				}
				Messages.Route[] value = enumerator2.Current.Value.Value;
				for (int i = 0; i < value.Length; i++)
				{
					Messages.Route value2 = value[i];
					queryRoutes[value2.RegionId] = value2;
				}
			}
		}
		int queryRouteCount = queryRoutes.Count;
		if (queryRouteCount > 0)
		{
			Dictionary<ulong, Messages.Route>.Enumerator enumerator3 = queryRoutes.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				GameSystem<MapSystem>.Instance().GetRegion(enumerator3.Current.Key, delegate(Messages.Region region)
				{
					ExploreData.Route item = new ExploreData.Route(region, queryRoutes[region.Id].Price);
					if (region.Role == Role.Risky || region.Role == Role.Troubled)
					{
						outter.Add(item);
					}
					else
					{
						inner.Add(item);
					}
					if (outter.Count + inner.Count >= queryRouteCount)
					{
						_routeViewer.SetInnerOutter(KSingleton<GameManager>.Instance().Region, inner, outter, _port.Id.GetHashCode());
					}
				});
			}
		}
		else
		{
			_routeViewer.SetInnerOutter(KSingleton<GameManager>.Instance().Region, inner, outter, _port.Id.GetHashCode());
		}
		if (_portInfoFormat == null)
		{
			_portInfoFormat = _portInfoLabel.text;
			_portInfoFormat = _portInfoFormat.Replace("{part_label}", T._("출발 항구"));
			_portInfoFormat = _portInfoFormat.Replace("{island}", T._("섬"));
		}
		_portInfoLabel.text = string.Format(_portInfoFormat, _port.Name, _port.Region.Name);
	}

	private ExploreData.Route MakeEmptyRoute(string templateId)
	{
		return new ExploreData.Route(new ExploreData.Region(templateId));
	}

	private void OnFoundRegion(ExploreData.Region region)
	{
		if (region != null)
		{
			_selectedUnknownRoute = null;
			UIManager.Popup.IsLoading = false;
			GameSystem<ExploreSystem>.Instance().RequestPort(_port.Id, _port.Tile);
		}
	}

	private void SailingRoutesHandler(InteractionObject target)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Open(target.EntityId, new Point2(target.Tile));
	}
}
