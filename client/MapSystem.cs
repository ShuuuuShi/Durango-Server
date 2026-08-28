using System;
using System.Collections.Generic;
using BestHTTP;
using Building;
using Durango.Logic.Map;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using Shared.System;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class MapSystem : GameSystem<MapSystem>
{
	public enum WarpTo
	{
		Home,
		UrbanEstate,
		PersonalEstate,
		ClanEstate,
		ClanWarphole,
		Camp,
		Port,
		Warp,
		WarpBack
	}

	public const string PortFoundEffect = "Particle/Prefabs/Durango_01/06_Structure/FX_Port_Find_01.prefab";

	public const string WarpholeFoundEffect = "Particle/FX_Found_Warphole_01.prefab";

	public const string CraterFoundEffect = "Particle/Prefabs/Durango_01/06_Structure/FX_Crater_Find_01.prefab";

	public const string FoundSound = "Prop_Found_Mineral_01";

	private POIUpdater _poiUpdater;

	private bool _poiUpdaterInitialized;

	private AsyncCachedDictionary<string, Region> _regionAsyncDict;

	private AsyncCachedDictionary<string, Messages.DiscoveryInfo> _discoveryInfoAsyncDict;

	private DeathPointHelper _deathPointhelper;

	private List<RegionTemplate> _unstableAreaList;

	private ExploredPOIs _exploredPOIs;

	public PredictTimer WarpTimer { get; private set; }

	public DiscoverInfo Discover { get; private set; }

	public List<RegionTemplate> UnstableAreaList
	{
		get
		{
			if (_unstableAreaList == null)
			{
				InitUnstableAreaList();
			}
			return _unstableAreaList;
		}
	}

	public int EntireWarpholeCount => _poiUpdater.EntireWarpholeCount;

	public int EntireCraterCount => _poiUpdater.EntireCraterCount;

	public int EntireRiftCount => _poiUpdater.EntireRiftCount;

	public Points Points { get; private set; }

	public bool CanPurchaseMap => GameManager.Region.Role() == Role.Risky && !_exploredPOIs.IsOpenedMap;

	public event Action PointsUpdated;

	public event Action HomeAssigned;

	public event Action ExploredPOIsUpdated;

	public event Action<Shared.System.PointOfInterest, Point2> ExploredPOI;

	public event Action<POIUpdater.NearbyPOI?> NearbyPOIUpdated
	{
		add
		{
			_poiUpdater.NearbyPOIUpdated += value;
		}
		remove
		{
			_poiUpdater.NearbyPOIUpdated -= value;
		}
	}

	public event Action IndicatorsInitialized;

	public event Action WhenReturnToHome;

	public event Action WhenReturnToCamp;

	public event Action<Role> TriedTravelToStableRegion;

	public event Action MapPurchased;

	public bool HasRewarded()
	{
		return _exploredPOIs.FullCountRewarded;
	}

	public Messages.Cost? GetRewardtCost()
	{
		return _exploredPOIs.RewardCost;
	}

	public int GetPOICount(Shared.System.PointOfInterest type)
	{
		int num = 0;
		Messages.PointOfInterest[] pOIs = _exploredPOIs.POIs;
		int i = 0;
		for (int size = KUtility.GetSize(pOIs); i < size; i++)
		{
			if (pOIs[i].Type == type && pOIs[i].IsExplored)
			{
				num++;
			}
		}
		return num;
	}

	public void PurchaseMap(string voucherId, float tweenDuration)
	{
		Connections.Frontend.Send(new OpenMap
		{
			VoucherId = voucherId
		}).On(delegate(ExploredPOIs msg, PacketHeader header)
		{
			DoExploredPOIs(msg, byPurchase: true, tweenDuration);
			_exploredPOIs = msg;
			if (this.MapPurchased != null)
			{
				this.MapPurchased();
			}
		});
	}

	private void Awake()
	{
		_poiUpdater = new POIUpdater();
		_poiUpdater.GetExploredPOIsRequested += _poiUpdater_GetExploredPOIsRequested;
		Discover = new DiscoverInfo();
		_regionAsyncDict = new AsyncCachedDictionary<string, Region>(RequestRegion);
		_discoveryInfoAsyncDict = new AsyncCachedDictionary<string, Messages.DiscoveryInfo>(RequestDiscoverInfo);
		Connections.Frontend.On(delegate(Points msg, PacketHeader _)
		{
			OnPointUpdate(msg);
		});
		Connections.Frontend.On<ExploredPOIs>(OnExploredPOIs);
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				PlayerBehavior.LocalPlayer.TileChanged += LocalPlayer_TileChanged;
				WarpTimer = new PredictTimer(GameManager.PlayerId, "warp");
				_deathPointhelper = new DeathPointHelper();
				_deathPointhelper.Init(this);
			}
		};
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
	}

	private void LateUpdate()
	{
		Discover.Process();
	}

	private void OnReady()
	{
		_discoveryInfoAsyncDict.Request(GameManager.Region.TemplateId, delegate(Messages.DiscoveryInfo info)
		{
			Discover.Set(info);
		});
		_poiUpdaterInitialized = false;
		_poiUpdater.Init();
		for (int i = 0; i < TerrainMeta.Indicators.Count; i++)
		{
			Indicator indicator = TerrainMeta.Indicators[i];
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(indicator.EntityType);
			if (blueprint != null)
			{
				ArtifactIndicatorData indicator2 = blueprint.Indicator;
				if (indicator2 != null)
				{
					AddMapIconIndicator(tile: new Point2(indicator.Tile[0], indicator.Tile[1]), iconName: indicator2.Icon, type: (!blueprint.HasComponent("Warphole")) ? IndicatorType.Artifact : IndicatorType.Warphole, size: indicator2.Size, isExplored: true, toolTip: string.Empty);
				}
			}
		}
		if (this.IndicatorsInitialized != null)
		{
			this.IndicatorsInitialized();
		}
	}

	private void InitUnstableAreaList()
	{
		_unstableAreaList = new List<RegionTemplate>();
		Dictionary<string, RegionTemplate> instance = SingletonDict<string, RegionTemplate>.Instance;
		foreach (KeyValuePair<string, RegionTemplate> item in instance)
		{
			RegionTemplate value = item.Value;
			if (value != null && value.Active && value.Role == Role.Outpost)
			{
				_unstableAreaList.Add(value);
			}
		}
		foreach (KeyValuePair<string, ArchipelagoTemplate> item2 in SingletonDict<string, ArchipelagoTemplate>.Instance)
		{
			ArchipelagoTemplate value2 = item2.Value;
			if (value2 != null && value2.Active)
			{
				RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Instance.Get(value2.FirstRegionTemplateId);
				if (regionTemplate != null && !_unstableAreaList.Contains(regionTemplate))
				{
					_unstableAreaList.Add(regionTemplate);
				}
			}
		}
		_unstableAreaList.Sort((RegionTemplate a1, RegionTemplate a2) => a1.Level - a2.Level);
	}

	private void _poiUpdater_GetExploredPOIsRequested()
	{
		Connections.Frontend.Send(new GetExploredPOIs
		{
			RegionId = GameManager.Region.Id
		});
	}

	private void OnExploredPOIs(ExploredPOIs msg, PacketHeader header)
	{
		DoExploredPOIs(msg, byPurchase: false);
		_exploredPOIs = msg;
		if (this.ExploredPOIsUpdated != null)
		{
			this.ExploredPOIsUpdated();
		}
		_poiUpdaterInitialized = true;
	}

	private void DoExploredPOIs(ExploredPOIs msg, bool byPurchase, float tweenDuration = 0f)
	{
		List<MapIconIndicator> list = ((!byPurchase) ? null : new List<MapIconIndicator>());
		for (int i = 0; i < msg.POIs.Length; i++)
		{
			Messages.PointOfInterest poi = msg.POIs[i];
			MapIconIndicator mapIconIndicator = null;
			if (poi.IsExplored && UpdatePOI(poi.Type, poi.Tile) && !byPurchase)
			{
				DoFoundEvent(poi.Type, poi.Tile);
			}
			switch (poi.Type)
			{
			case Shared.System.PointOfInterest.Warphole:
			case Shared.System.PointOfInterest.CargoWarphole:
				mapIconIndicator = AddWarpholeIndicator(poi, byPurchase);
				break;
			case Shared.System.PointOfInterest.Crater:
			case Shared.System.PointOfInterest.Crack:
				mapIconIndicator = AddCraterOrCrackIndicator(poi, byPurchase);
				break;
			case Shared.System.PointOfInterest.Port:
				mapIconIndicator = AddPortIndicator(poi, byPurchase);
				break;
			}
			if (list != null && mapIconIndicator != null)
			{
				list.Add(mapIconIndicator);
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		float num = tweenDuration / (float)list.Count;
		float num2 = 0f;
		foreach (MapIconIndicator item in list)
		{
			item.StartTweener(num2);
			num2 += num;
		}
	}

	private static MapIconIndicator AddWarpholeIndicator(Messages.PointOfInterest poi, bool byPurchase)
	{
		if (byPurchase && ContainsMapIndicator(poi.Tile, IndicatorType.Warphole))
		{
			return null;
		}
		return AddMapIconIndicator("icon_map_warphole", poi.Tile, IndicatorType.Warphole, 30, poi.IsExplored, string.Empty);
	}

	private static MapIconIndicator AddCraterOrCrackIndicator(Messages.PointOfInterest poi, bool byPurchase)
	{
		if (byPurchase && ContainsMapIndicator(poi.Tile, IndicatorType.POIBiocom))
		{
			return null;
		}
		return AddMapIconIndicator(poi.Icon, poi.Tile, IndicatorType.POIBiocom, 30, poi.IsExplored, poi.Title);
	}

	private static MapIconIndicator AddPortIndicator(Messages.PointOfInterest poi, bool byPurchase)
	{
		if (byPurchase && ContainsMapIndicator(poi.Tile, IndicatorType.Dock))
		{
			return null;
		}
		return AddMapIconIndicator("icon_map_port", poi.Tile, IndicatorType.Dock, 35, poi.IsExplored, string.Empty);
	}

	private static void DoFoundEvent(Shared.System.PointOfInterest type, Point2 tile)
	{
		switch (type)
		{
		case Shared.System.PointOfInterest.Warphole:
		case Shared.System.PointOfInterest.CargoWarphole:
			DoFoundEvent("Particle/FX_Found_Warphole_01.prefab", tile);
			break;
		case Shared.System.PointOfInterest.Crater:
			DoFoundEvent("Particle/Prefabs/Durango_01/06_Structure/FX_Crater_Find_01.prefab", tile);
			break;
		case Shared.System.PointOfInterest.Crack:
			DoFoundEvent("Particle/FX_Found_Warphole_01.prefab", tile);
			break;
		case Shared.System.PointOfInterest.Port:
			DoFoundEvent("Particle/Prefabs/Durango_01/06_Structure/FX_Port_Find_01.prefab", tile);
			break;
		case Shared.System.PointOfInterest.FactionProp:
			break;
		}
	}

	private static void DoFoundEvent(string particle, Point2 tile)
	{
		ImmovableBase immovableBase = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(tile)?.GetImmovable();
		Vector3 pos = ((!(immovableBase == null)) ? immovableBase.Center : Util.TilePositionToClientPosition(tile));
		ParticleManager.Emit(particle, pos, Quaternion.identity);
		SoundManager.PlayEvent("Prop_Found_Mineral_01");
	}

	private static bool ContainsMapIndicator(Point2 tile, IndicatorType type)
	{
		return Durango.Utils.Singleton<MapIndicators>.Instance().ContainsIndicator(tile.ToString(), type);
	}

	private static MapIconIndicator AddMapIconIndicator(string iconName, Point2 tile, IndicatorType type, int size, bool isExplored, string toolTip = "")
	{
		Color white = Color.white;
		Color color = new Color32(100, 100, 100, byte.MaxValue);
		MapIconIndicator orAdd = MapIndicators.GetOrAdd<MapIconIndicator>(tile.ToString(), type);
		orAdd.SetTarget(tile);
		orAdd.SetIcon(iconName, (!isExplored) ? color : white, size, 10);
		if (!string.IsNullOrEmpty(toolTip))
		{
			orAdd.SetTooltip(toolTip);
		}
		return orAdd;
	}

	private bool UpdatePOI(Shared.System.PointOfInterest type, Point2 tile)
	{
		if (_poiUpdater.ContainsPOI(tile))
		{
			return false;
		}
		_poiUpdater.AddPOI(tile, type);
		if (!_poiUpdaterInitialized)
		{
			return false;
		}
		if (this.ExploredPOI != null)
		{
			this.ExploredPOI(type, tile);
		}
		return true;
	}

	private void OnPointUpdate(Points point)
	{
		Points = point;
		if (this.PointsUpdated != null)
		{
			this.PointsUpdated();
		}
		if (Points.HasHome() && this.HomeAssigned != null)
		{
			this.HomeAssigned();
		}
	}

	private void LocalPlayer_TileChanged(Point2 prev, Point2 current)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode && _poiUpdaterInitialized)
		{
			_poiUpdater.SearchPOIProp();
		}
	}

	public static void RequestFullCountPOIsReward()
	{
		Connections.Frontend.Send(new RequestFullCountPOIsReward
		{
			RegionId = GameManager.Region.Id
		});
	}

	public static void RequestWarpBackCost([NotNull] Action<long> callback)
	{
		Connections.Frontend.Send(default(GetWarpBackCost)).On(delegate(WarpCosts msg, PacketHeader header)
		{
			callback(msg.Costs[0].Cost);
		});
	}

	[ExposedInEditor(null)]
	public void Warp(Point2 tile)
	{
		TryWarp(() => Connections.Frontend.Send(new Warp
		{
			Tile = tile
		}), WarpTo.Warp);
	}

	public void WarpBack(Region region)
	{
		TryWarp(() => Connections.Frontend.Send(default(WarpBack)), WarpTo.WarpBack, region);
	}

	public void ReturnToHome()
	{
		EntityTile? homePoint = GameSystem<MapSystem>.Instance().Points.HomePoint;
		Region region = ((!homePoint.HasValue) ? GameSystem<MapSystem>.Instance().Points.ReturningPoint.Region : homePoint.Value.Region);
		TryWarp(delegate
		{
			if (this.WhenReturnToHome != null)
			{
				this.WhenReturnToHome();
			}
			return Connections.Frontend.Send(default(ReturnToHome));
		}, WarpTo.Home, region);
	}

	public void ReturnToCamp()
	{
		TryWarp(delegate
		{
			if (this.WhenReturnToCamp != null)
			{
				this.WhenReturnToCamp();
			}
			return Connections.Frontend.Send(default(ReturnToCamp));
		}, WarpTo.Camp);
	}

	public void WarpToPort()
	{
		TryWarp(() => Connections.Frontend.Send(default(WarpToPort))
			.On<OK>(delegate
			{
				RequestIslandTravelOptions();
			}), WarpTo.Port);
	}

	private void RequestIslandTravelOptions()
	{
		Point2 portTile = new Point2(
			(int)(PlayerBehavior.LocalPlayer.CurrentPosition.x / 200f),
			(int)(PlayerBehavior.LocalPlayer.CurrentPosition.z / 200f));
		Connections.Frontend.Send(new GetIslandTravelOptions
		{
			EntityId = string.Empty,
			Tile = portTile
		}).On(delegate(IslandTravelOptions options, PacketHeader _)
		{
			int count = Math.Min(options.Ids?.Length ?? 0, options.Names?.Length ?? 0);
			if (count == 0)
			{
				UIManager.SystemMsg(T._("ไม่มีเกาะปลายทางที่เดินทางได้ในตอนนี้"));
				return;
			}

			GenericSelector selector = UIManager.Popup.Tooltip<GenericSelector>();
			selector.ResetArguments();
			selector.SetTitle(T._("ย้ายเกาะที่ท่าเรือ"));
			selector.SetInfo(T._("เลือกเกาะปลายทาง — การย้ายเกาะจะโหลดเซิร์ฟเวอร์ปลายทาง"));
			for (int i = 0; i < count; i++)
			{
				int required = (options.RequiredLevels != null && i < options.RequiredLevels.Length)
					? options.RequiredLevels[i]
					: 0;
				selector.AddItem($"{options.Names[i]}\n[size=20]Lv. {required}+[/size]");
			}
			selector.SetSelected(delegate(int index)
			{
				if (index < 0 || index >= count)
				{
					return;
				}
				Connections.Frontend.Send(new TravelByRegion
				{
					EntityId = string.Empty,
					Tile = portTile,
					RegionId = options.Ids[index],
					PartierId = null
				});
			});
			selector.Show();
		});
	}

	public void TryWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, WarpTo to, Region region = default(Region))
	{
		string warpMsg = GetWarpMsg(region, to);
		bool flag = false;
		switch (to)
		{
		case WarpTo.Home:
		case WarpTo.UrbanEstate:
		case WarpTo.PersonalEstate:
		case WarpTo.ClanEstate:
		case WarpTo.ClanWarphole:
			flag = true;
			break;
		}
		Action action = delegate
		{
			UnmountAndDoWarp(onFinished, warpMsg);
		};
		if (flag && GameManager.Region.Id != region.Id)
		{
			CheckUnstableItem(action);
		}
		else
		{
			action();
		}
	}

	public static void CheckUnstableItem(Action action)
	{
		if (GameManager.Region.IsUnstable)
		{
			Connections.Frontend.Send(default(CheckUnstableItem)).On(delegate(ResultCheckUnstableItem msg, PacketHeader header)
			{
				if (msg.TotalUnstableCount > 0)
				{
					UIManager.MessageBox.Show(T._("가방 또는 동물의 가방에 불안정 아이템이 남아 있습니다.\n화물 워프홀에서 사유지로 전송하지 않은 불안정 아이템은 이 섬 밖으로 나갈 때 모두 사라집니다.\n그래도 섬을 이동하시겠습니까?"), delegate(bool ok)
					{
						if (ok)
						{
							action();
						}
					});
				}
				else
				{
					action();
				}
			});
		}
		else
		{
			action();
		}
	}

	private static string GetWarpMsg(Region region, WarpTo to)
	{
		string text = string.Empty;
		switch (to)
		{
		case WarpTo.Home:
			text = T._("집");
			break;
		case WarpTo.UrbanEstate:
			text = T._("도시섬 사유지");
			break;
		case WarpTo.PersonalEstate:
			text = T._("개인섬 사유지");
			break;
		case WarpTo.ClanEstate:
			text = T._("부족영토");
			break;
		case WarpTo.ClanWarphole:
			text = T._("거점");
			break;
		case WarpTo.Camp:
			text = T._("캠프");
			break;
		case WarpTo.Port:
			text = T._("항구");
			break;
		}
		if (string.IsNullOrEmpty(region.Id))
		{
			return (!string.IsNullOrEmpty(text)) ? T._("{0:으로} 워프합니다.", text) : string.Empty;
		}
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
		if (string.IsNullOrEmpty(text))
		{
			return T._("{0} {1:lv:} \n{2:으로} 워프합니다.", region.Name, regionTemplate?.Level ?? 0, region.Role.GetName());
		}
		return T._("{0} {1:lv:} \n{2} {3:으로} 워프합니다.", region.Name, regionTemplate?.Level ?? 0, region.Role.GetName(), text);
	}

	private void UnmountAndDoWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, string warpMsg)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				DoWarp(onFinished, warpMsg);
			});
		}
		else
		{
			DoWarp(onFinished, warpMsg);
		}
	}

	private void DoWarp([NotNull] Func<ReplyMessageHandlerRegistrar> onFinished, string warpMsg)
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return;
		}
		if (Durango.Utils.Singleton<PlayerController>.HasInstance())
		{
			Durango.Utils.Singleton<PlayerController>.Instance().StopMove();
			WarpTimer.SetMotion("Warp_Begin", null, default(ItemColor), 1f);
		}
		WarpTimer.Play(Connections.Frontend.Ping + 2f);
		onFinished().On(delegate(Messages.Timer msg, PacketHeader header)
		{
			if (!WarpTimer.Timer.IsStop)
			{
				if (!string.IsNullOrEmpty(warpMsg))
				{
					UIManager.SystemMsg(warpMsg);
				}
				WarpTimer.Play(msg.Duration);
			}
		}).Rest(delegate
		{
			WarpTimer.Stop();
		});
	}

	public static void GetExploredPOICount(string regionId, Connection.MessageHandler<ExploredPOIs> reslut)
	{
		Connections.Frontend.Send(new GetExploredPOIs
		{
			RegionId = regionId
		}).On(reslut);
	}

	public static void GetPOICount(string regionId, Connection.MessageHandler<POICount> reslut)
	{
		Connections.Frontend.Send(new GetPOICount
		{
			RegionId = regionId
		}).On(reslut);
	}

	public void GetRegion(string id, [NotNull] Action<Region> onRegion)
	{
		_regionAsyncDict.Request(id, onRegion);
	}

	public void GetRegions(IList<string> ids, [NotNull] Action<Region[]> onRegions)
	{
		_regionAsyncDict.Request(ids, onRegions);
	}

	private static void RequestRegion(string id, Region cacheValue, Action<string, Region> result)
	{
		Connections.Frontend.Send(new GetRegion
		{
			RegionId = id
		}).On(delegate(Region msg, PacketHeader header)
		{
			result(msg.Id, msg);
		}).On<Error>(delegate
		{
			result(id, default(Region));
		});
	}

	public static void RemoveDeathPoint()
	{
		Connections.Frontend.Send(default(RemoveDeathPoint));
	}

	public void GetDiscoveryInfos(IList<string> templateIds, [NotNull] Action<Messages.DiscoveryInfo[]> onResult)
	{
		_discoveryInfoAsyncDict.Request(templateIds, onResult);
	}

	private static void RequestDiscoverInfo(string templateId, Messages.DiscoveryInfo cachedValue, Action<string, Messages.DiscoveryInfo> onResult)
	{
		Connections.Frontend.Send(new GetDiscoveryInfo
		{
			TemplateId = templateId
		}).On(delegate(Messages.DiscoveryInfo msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg.TemplateId, msg);
			}
		});
	}

	public static void DiscoverAnimalType(string entityId, Action<bool> onResult)
	{
		Connections.Frontend.Send(new DiscoverAnimal
		{
			EntityId = entityId
		}).All(delegate(Packet packet)
		{
			bool obj = Packet.IsSuccess(packet);
			if (onResult != null)
			{
				onResult(obj);
			}
		});
	}

	public static void RecommendStableRegions([NotNull] Action<Route[]> onResult)
	{
		Connections.Frontend.Send(default(RecommendStableRegions)).On(delegate(RecommendedStableRegions msg, PacketHeader header)
		{
			int size = KUtility.GetSize(msg.Routes);
			if (size > 0)
			{
				string[] array = new string[size];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = msg.Routes[i].RegionId;
				}
				GameSystem<ExploreSystem>.Instance().RequestRegionsForExploreSystem(array, delegate
				{
					onResult(msg.Routes);
				});
			}
			else
			{
				onResult(null);
			}
		}).Rest(delegate
		{
			onResult(null);
		});
	}

	public void TravelToStableRegion(string regionId, Role role)
	{
		if (this.TriedTravelToStableRegion != null)
		{
			this.TriedTravelToStableRegion(role);
			KUtility.DelayedCall(this, delegate
			{
				TravelToStableRegion(regionId);
			}, 2f);
		}
		else
		{
			TravelToStableRegion(regionId);
		}
	}

	private static void TravelToStableRegion(string regionId)
	{
		Connections.Frontend.Send(new TravelToStableRegion
		{
			RegionId = regionId
		}).All(delegate(Packet packet)
		{
			if (!Packet.IsSuccess(packet))
			{
			}
		});
	}

	public static void RequestBiomes(string terrainId, Action<byte[]> onSucceeded, Action onFailed)
	{
		string url = $"{GameManager.GatewayUrl}/terrains/{terrainId}/whole_biomes";
		Http.Request(url, delegate(byte[] bytes, HTTPResponse response)
		{
			if (bytes == null)
			{
				if (onFailed != null)
				{
					onFailed();
				}
			}
			else if (onSucceeded != null)
			{
				onSucceeded(bytes);
			}
		}, disableCache: false, addSession: true);
	}
}
