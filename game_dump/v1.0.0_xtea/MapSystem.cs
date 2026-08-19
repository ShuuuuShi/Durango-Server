using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using K1Network;
using MapData;
using Messages;
using Shared.Faction;
using Shared.System;
using TimerData;
using UnityEngine;

public class MapSystem : GameSystem<MapSystem>
{
	private POIUpdater _poiUpdater;

	private bool _poiUpdaterInitialized;

	private Dictionary<Point2, FactionType> _craterFactions = new Dictionary<Point2, FactionType>();

	private List<Point2> _completedCraters = new List<Point2>();

	private readonly Dictionary<ulong, Region> _cachedRegions = new Dictionary<ulong, Region>();

	private readonly List<KeyValuePair<ulong, Action<Region>>> _regionsCallbacks = new List<KeyValuePair<ulong, Action<Region>>>();

	public int EntireWarpholeCount => _poiUpdater.EntireWarpholeCount;

	public MapData.Points Points { get; private set; }

	public event Action PointsUpdated;

	public event Action HomeAssigned;

	public event Action BaseAssigned;

	public event Action WarpholesUpdated;

	public event Action<Point2> OnExploreWarphole;

	public event Action<Point2> OnExploreCrater;

	public event Action<Point2> OnExploreCrack;

	public event Action<Point2> OnExplorePort;

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

	public int GetExploredWarpholeCount()
	{
		return _poiUpdater.GetExploredWarpholeCount();
	}

	private void Awake()
	{
		Connections.Frontend.On(delegate(Messages.Points msg, PacketHeader _)
		{
			OnPointUpdate(msg);
		});
		Connections.Frontend.On(delegate(DefoggedChunks msg, PacketHeader header)
		{
			MapContext mapContext = UIManager.MapContext;
			if ((Object)(object)mapContext != (Object)null)
			{
				mapContext.HandleDefoggedChunks(msg);
			}
		});
		Connections.Frontend.On<ExploredPOIs>(UpdatePois);
		Connections.Frontend.On<CompletedCraters>(UpdateCompletedCrators);
		_poiUpdater = new POIUpdater();
		_poiUpdater.GetExploredPOIsRequested += _poiUpdater_GetExploredPOIsRequested;
		Artifact.ArtifactStateChanged += OnChangeArtifactState;
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				PlayerBehavior.LocalPlayer.TileChanged += LocalPlayer_TileChanged;
				KSingleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
				KSingleton<AnimalManager>.Instance().AnimalDisappeared += OnDisappearAnimal;
				KSingleton<PlayerManager>.Instance().PlayerAppeared += OnAppearPlayer;
				KSingleton<PlayerManager>.Instance().PlayerDisappeared += OnDisappearPlayer;
				KSingleton<StaticObjectManager>.Instance().ArtifactAdded += StaticObjectManager_ArtifactAdded;
				KSingleton<StaticObjectManager>.Instance().ArtifactRemoved += StaticObjectManager_ArtifactRemoved;
			}
		};
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			_poiUpdaterInitialized = false;
			_poiUpdater.Init();
		};
	}

	private void OnDestroy()
	{
		Artifact.ArtifactStateChanged -= OnChangeArtifactState;
	}

	private void _poiUpdater_GetExploredPOIsRequested()
	{
		Connections.Frontend.Send(new GetExploredPOIs
		{
			RegionId = KSingleton<GameManager>.Instance().Region.Id
		});
	}

	private void UpdatePois(ExploredPOIs msg, PacketHeader header)
	{
		_craterFactions.Clear();
		for (int i = 0; i < msg.POIs.Length; i++)
		{
			Messages.PointOfInterest pointOfInterest = msg.POIs[i];
			switch (pointOfInterest.Type)
			{
			case Shared.System.PointOfInterest.Warphole:
				if (UpdatePoi(pointOfInterest.Tile, Shared.System.PointOfInterest.Warphole) && this.OnExploreWarphole != null)
				{
					this.OnExploreWarphole(pointOfInterest.Tile);
				}
				AddMapIconIndicator("icon_map_warphole", pointOfInterest.Tile, IndicatorType.Warphole, 30, string.Empty);
				break;
			case Shared.System.PointOfInterest.Crater:
				if (pointOfInterest.Faction.HasValue)
				{
					_craterFactions[pointOfInterest.Tile] = pointOfInterest.Faction.Value;
				}
				if (UpdatePoi(pointOfInterest.Tile, Shared.System.PointOfInterest.Crater) && this.OnExploreCrater != null)
				{
					this.OnExploreCrater(pointOfInterest.Tile);
				}
				AddFactionPOIIndicator(pointOfInterest.Icon, pointOfInterest.Tile, pointOfInterest.Title, (!pointOfInterest.Faction.HasValue) ? FactionType.Invalid : pointOfInterest.Faction.Value);
				break;
			case Shared.System.PointOfInterest.Crack:
				if (UpdatePoi(pointOfInterest.Tile, Shared.System.PointOfInterest.Crack) && this.OnExploreCrack != null)
				{
					this.OnExploreCrack(pointOfInterest.Tile);
				}
				AddMapIconIndicator(pointOfInterest.Icon, pointOfInterest.Tile, IndicatorType.POIBiocom, 30, pointOfInterest.Title);
				break;
			case Shared.System.PointOfInterest.Biocom:
				UpdatePoi(pointOfInterest.Tile, Shared.System.PointOfInterest.Biocom);
				AddMapIconIndicator(pointOfInterest.Icon, pointOfInterest.Tile, IndicatorType.POIBiocom, 30, pointOfInterest.Title);
				break;
			case Shared.System.PointOfInterest.Port:
				if (UpdatePoi(pointOfInterest.Tile, Shared.System.PointOfInterest.Port) && this.OnExplorePort != null)
				{
					this.OnExplorePort(pointOfInterest.Tile);
				}
				AddMapIconIndicator("icon_map_port", pointOfInterest.Tile, IndicatorType.Dock, 35, string.Empty);
				break;
			}
		}
		if (this.WarpholesUpdated != null)
		{
			this.WarpholesUpdated();
		}
		Connections.Frontend.Send(new GetCompletedCraters
		{
			RegionId = KSingleton<GameManager>.Instance().Region.Id
		});
		_poiUpdaterInitialized = true;
	}

	private void AddMapIconIndicator(string iconName, Point2 tile, IndicatorType type, int size = 30, string toolTip = "")
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		MapIconIndicator mapIconIndicator = MapIndicators.Add<MapIconIndicator>(tile, type);
		mapIconIndicator.SetTarget(tile);
		mapIconIndicator.SetIcon(iconName, Color.white, size, 10);
		if (!string.IsNullOrEmpty(toolTip))
		{
			mapIconIndicator.SetTooltip(toolTip);
		}
	}

	private void AddFactionPOIIndicator(string iconName, Point2 tile, string title, FactionType factionType)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(iconName))
		{
			return;
		}
		MapFactionIndicator mapFactionIndicator = MapIndicators.Add<MapFactionIndicator>(tile, IndicatorType.POIBiocom);
		mapFactionIndicator.SetTarget(tile);
		mapFactionIndicator.SetIcon(iconName, Color.white, 30, 10);
		mapFactionIndicator.SetTooltip(title);
		if (factionType == FactionType.Invalid)
		{
			return;
		}
		mapFactionIndicator.FactionType = factionType;
		if (!_completedCraters.Contains(tile))
		{
			switch (factionType)
			{
			case FactionType.ChlorophylForum:
				mapFactionIndicator.SetSubIcon("icon_map_chlorophy", PresetColor.UILightGray, 25);
				break;
			case FactionType.ChamberOfPioneer:
				mapFactionIndicator.SetSubIcon("icon_map_pioneer", PresetColor.UILightGray, 25);
				break;
			}
		}
	}

	private void UpdateCompletedCrators(CompletedCraters msg, PacketHeader header)
	{
		for (int i = 0; i < msg.Craters.Length; i++)
		{
			AddCompletedFactionIndicator(msg.Craters[i]);
		}
	}

	public bool IsExploredPoi(Point2 tile)
	{
		return _poiUpdater.ContainsPOI(tile);
	}

	private bool UpdatePoi(Point2 tile, Shared.System.PointOfInterest poiType)
	{
		if (!_poiUpdater.ContainsPOI(tile))
		{
			_poiUpdater.AddPOI(tile, poiType);
			return _poiUpdaterInitialized;
		}
		return false;
	}

	private void AddCompletedFactionIndicator(Point2 tile)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		_completedCraters.Add(tile);
		MapFactionIndicator mapFactionIndicator = MapIndicators.Add<MapFactionIndicator>(tile, IndicatorType.POIBiocom);
		if (!((Object)(object)mapFactionIndicator == (Object)null))
		{
			switch (mapFactionIndicator.FactionType)
			{
			case FactionType.ChlorophylForum:
				mapFactionIndicator.SetSubIcon("icon_map_chlorophy", PresetColor.UIDarkGray, 25);
				break;
			case FactionType.ChamberOfPioneer:
				mapFactionIndicator.SetSubIcon("icon_map_pioneer", PresetColor.UIDarkGray, 25);
				break;
			}
		}
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		MapAnimalIndicator mapAnimalIndicator = MapIndicators.Add<MapAnimalIndicator>(animal.EntityId, IndicatorType.Animal);
		mapAnimalIndicator.SetAnimal(animal);
		mapAnimalIndicator.VisibleType = IndicatorVisibleType.Reveal;
	}

	private void OnDisappearAnimal(AnimalBehavior animal)
	{
		MapIndicators.Remove(animal.EntityId, IndicatorType.Animal);
	}

	private void OnAppearPlayer(PlayerBehavior player)
	{
		MapPlayerIndicator mapPlayerIndicator = MapIndicators.Add<MapPlayerIndicator>(player.EntityId, IndicatorType.Player);
		mapPlayerIndicator.SetPlayer(player);
		mapPlayerIndicator.VisibleType = IndicatorVisibleType.Reveal;
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		MapIndicators.Remove(player.EntityId, IndicatorType.Player);
	}

	private void OnChangeArtifactState(Artifact artifact)
	{
		AddArtifactIndicator(artifact);
	}

	private void StaticObjectManager_ArtifactAdded(Artifact artifact)
	{
		AddArtifactIndicator(artifact);
	}

	private void AddArtifactIndicator(Artifact artifact)
	{
		if (MapArtifactIndicator.HasIndicator(artifact))
		{
			MapArtifactIndicator mapArtifactIndicator = MapIndicators.Add<MapArtifactIndicator>(artifact.EntityId, IndicatorType.Artifact);
			mapArtifactIndicator.SetArtifact(artifact);
		}
		else
		{
			MapIndicators.Remove(artifact.EntityId, IndicatorType.Artifact);
		}
	}

	private void StaticObjectManager_ArtifactRemoved(Artifact artifact)
	{
		MapIndicators.Remove(artifact.EntityId, IndicatorType.Artifact);
	}

	private void OnPointUpdate(Messages.Points point)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		Points = new MapData.Points(point);
		if (Points.HasReachableBasePoint())
		{
			EntityTile value = Points.BasePoint.Value;
			MapIconIndicator mapIconIndicator = MapIndicators.Add<MapIconIndicator>(value.Tile, IndicatorType.Base);
			mapIconIndicator.SetTarget(value.Tile);
			mapIconIndicator.SetIcon("icon_map_base", Color.white, 35, 3);
		}
		else
		{
			MapIndicators.Remove(IndicatorType.Base);
		}
		if (Points.HasReachableHomePoint())
		{
			EntityTile value2 = Points.HomePoint.Value;
			MapIconIndicator mapIconIndicator2 = MapIndicators.Add<MapIconIndicator>(value2.Tile, IndicatorType.MyHome);
			mapIconIndicator2.SetTarget(value2.Tile);
			mapIconIndicator2.SetIcon("icon_map_house", Color.white, 35, 3);
		}
		else
		{
			MapIndicators.Remove(IndicatorType.MyHome);
		}
		if (Points.HasReachableDeathPoint())
		{
			RegionTile value3 = Points.DeathPoint.Value;
			MapIconIndicator mapIconIndicator3 = MapIndicators.Add<MapIconIndicator>(value3.Tile, IndicatorType.Death);
			mapIconIndicator3.SetTarget(value3.Tile);
			mapIconIndicator3.SetIcon("icon_map_dead", PresetColor.UIRed, 35, 1);
		}
		else
		{
			MapIndicators.Remove(IndicatorType.Death);
		}
		if (this.PointsUpdated != null)
		{
			this.PointsUpdated();
		}
		if (Points.HasHome() && this.HomeAssigned != null)
		{
			this.HomeAssigned();
		}
		if (Points.HasBase() && this.BaseAssigned != null)
		{
			this.BaseAssigned();
		}
	}

	private void LocalPlayer_TileChanged(Point2 prev, Point2 current)
	{
		_poiUpdater.SearchPOIProp();
	}

	public void SearchNearPOIProp()
	{
		_poiUpdater.SearchPOIProp();
	}

	public FactionType FindFactionFromCraterTile(Point2 tile)
	{
		_craterFactions.TryGetValue(tile, out var value);
		return value;
	}

	public void RequestWarpCost(Point2 tile, [NotNull] Action<int> callback)
	{
		Connections.Frontend.Send(default(GetWarpCosts)).On(delegate(WarpCosts msg, PacketHeader header)
		{
			for (int i = 0; i < msg.Costs.Length; i++)
			{
				if (msg.Costs[i].Tile == tile)
				{
					callback(msg.Costs[i].Cost);
				}
			}
		});
	}

	public void RequestWarpBackCost([NotNull] Action<int> callback)
	{
		Connections.Frontend.Send(default(GetWarpBackCost)).On(delegate(WarpCosts msg, PacketHeader header)
		{
			callback(msg.Costs[0].Cost);
		});
	}

	[ExposedInEditor(null)]
	public void Warp(Point2 tile)
	{
		Warp(tile, returnToHome: false, returnToBase: false);
	}

	public void WarpBack()
	{
		Warp(-Point2.one);
	}

	public void ReturnToHome()
	{
		Warp(Point2.zero, returnToHome: true, returnToBase: false);
	}

	public void ReturnToBase()
	{
		Warp(Point2.zero, returnToHome: false, returnToBase: true);
	}

	private void Warp(Point2 tile, bool returnToHome, bool returnToBase)
	{
		TimerData.Timer timer = new TimerData.Timer("warp", 5f);
		timer.Finished += delegate(TimerData.Timer t)
		{
			if (!t.IsInterrupt)
			{
				if (returnToHome)
				{
					Connections.Frontend.Send(default(ReturnToHome)).On<OK>(delegate
					{
						GameSystem<PlayGuideSystem>.Instance().EventOccured("return_home", null);
					});
				}
				else if (returnToBase)
				{
					Connections.Frontend.Send(default(ReturnToBase));
				}
				else if (tile.x < 0 || tile.y < 0)
				{
					Connections.Frontend.Send(default(WarpBack));
				}
				else
				{
					Connections.Frontend.Send(new Warp
					{
						Tile = tile
					});
				}
			}
		};
		TimerData.Timer.Play<DefaultProgressGauge>(timer);
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().Motion("Warp_Begin");
		}
	}

	public static void GetExploredPOICount(ulong regionId, Connection.MessageHandler<ExploredPOIs> reslut)
	{
		Connections.Frontend.Send(new GetExploredPOIs
		{
			RegionId = regionId
		}).On(reslut);
	}

	public static void GetPOICount(ulong regionId, Connection.MessageHandler<POICount> reslut)
	{
		Connections.Frontend.Send(new GetPOICount
		{
			RegionId = regionId
		}).On(reslut);
	}

	private void OnRegion(Region region, PacketHeader header)
	{
		_cachedRegions[region.Id] = region;
		for (int i = 0; i < _regionsCallbacks.Count; i++)
		{
			if (_regionsCallbacks[i].Key == region.Id)
			{
				_regionsCallbacks[i].Value(region);
				_regionsCallbacks.RemoveAt(i);
				i--;
			}
		}
	}

	public void GetRegion(ulong id, [NotNull] Action<Region> onRegion)
	{
		if (id != 0L)
		{
			if (_cachedRegions.ContainsKey(id))
			{
				onRegion(_cachedRegions[id]);
				return;
			}
			_regionsCallbacks.Add(new KeyValuePair<ulong, Action<Region>>(id, onRegion));
			Connections.Frontend.Send(new GetRegion
			{
				RegionId = id
			}).On<Region>(OnRegion);
		}
	}
}
