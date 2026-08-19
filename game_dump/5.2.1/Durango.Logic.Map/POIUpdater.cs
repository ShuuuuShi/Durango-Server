using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Season2;
using Shared.System;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Map;

public class POIUpdater
{
	public struct NearbyPOI
	{
		public Shared.System.PointOfInterest Type;

		public Vector3 Position;

		public void Clear()
		{
			Type = Shared.System.PointOfInterest.Invalid;
			Position = Vector3.zero;
		}

		public void Set(Shared.System.PointOfInterest type, Vector3 position)
		{
			Type = type;
			Position = position;
		}
	}

	public const float DistanceForExplore = 500f;

	public const float MinDistanceForSearchNearby = 1600f;

	private readonly List<GameObject> _searchList = new List<GameObject>();

	private readonly Dictionary<Point2, Shared.System.PointOfInterest> _exploreredPOIs = new Dictionary<Point2, Shared.System.PointOfInterest>();

	private readonly HashSet<Point2> _justFoundPOIs = new HashSet<Point2>();

	private NearbyPOI _nearbyPOI;

	private Dictionary<Point2, bool> _justRefreshedCracks = new Dictionary<Point2, bool>();

	public int EntireWarpholeCount { get; private set; }

	public int EntireCraterCount { get; private set; }

	public int EntireRiftCount { get; private set; }

	private float DistanceForSearchNearby => 3200f + GetAdditionalNearbyDistance();

	private float DistanceForUpdateNearby => 1600f + GetAdditionalNearbyDistance();

	public event Action GetExploredPOIsRequested;

	public event Action<NearbyPOI?> NearbyPOIUpdated;

	public void Init()
	{
		GameSystem<WarpRushSystem>.Instance().SurvivorRegionChanged += WarpRushSystem_SurvivorRegionChanged;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += CombatSystem_ChangedCombatMode;
		_exploreredPOIs.Clear();
		_justFoundPOIs.Clear();
		_nearbyPOI.Clear();
		NotifyNearbyPOIUpdated();
		if (this.GetExploredPOIsRequested != null)
		{
			this.GetExploredPOIsRequested();
		}
		Connections.Frontend.Send(new GetPOICount
		{
			RegionId = GameManager.Region.Id
		}).On(delegate(POICount msg, PacketHeader _)
		{
			EntireWarpholeCount = msg.WarpholeCount;
			EntireCraterCount = msg.CraterCount;
			EntireRiftCount = msg.RiftCount;
		});
	}

	public bool ContainsPOI(Point2 tile)
	{
		return _exploreredPOIs.ContainsKey(tile);
	}

	public void AddPOI(Point2 tile, Shared.System.PointOfInterest poiType)
	{
		_justFoundPOIs.Remove(tile);
		_exploreredPOIs[tile] = poiType;
	}

	public void SearchPOIProp()
	{
		int mask = LayerHelper.PropMask;
		_searchList.Clear();
		InteractionSystem.GetNearObjectsInternal(_searchList, mask, DistanceForSearchNearby);
		NearbyPOI nearbyPOI = _nearbyPOI;
		_nearbyPOI.Clear();
		for (int i = 0; i < _searchList.Count; i++)
		{
			GameObject gameObject = InteractionSystem.ImmovableObjectFilter(_searchList[i]);
			NaturalObject naturalObject = ((!(gameObject != null)) ? null : gameObject.GetComponent<NaturalObject>());
			if (naturalObject != null)
			{
				if (Yaml.Util.Singleton<Constants>.Instance.NaturalComponents.TryGetValue(naturalObject.EntityType, out var value))
				{
					NearbyPOIFound(naturalObject, value.POIType);
				}
				continue;
			}
			Artifact artifact = ((!(gameObject != null)) ? null : gameObject.GetComponent<Artifact>());
			if (artifact == null)
			{
				continue;
			}
			if (artifact.ArtifactState.Crack.HasValue)
			{
				Messages.Crack value2 = artifact.ArtifactState.Crack.Value;
				double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
				double? activatedSince = value2.ActivatedSince;
				int num;
				if (activatedSince.HasValue && value2.ActivatedSince.Value <= bufferedServerTime)
				{
					double? activatedUntil = value2.ActivatedUntil;
					num = ((activatedUntil.HasValue && activatedUntil.GetValueOrDefault() > bufferedServerTime) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
				bool isActivated = (byte)num != 0;
				NearbyCrackFound(artifact, isActivated);
			}
			if (artifact.ArtifactState.StoneCrack.HasValue && GameManager.Region.IsWarpRush())
			{
				StoneCrack value3 = artifact.ArtifactState.StoneCrack.Value;
				WarpRushSystem warpRushSystem = GameSystem<WarpRushSystem>.Instance();
				int activePhase = value3.ActivePhase;
				bool isActivated2 = warpRushSystem.DaysPassed > 0 && activePhase <= warpRushSystem.PhaseNumber;
				NearbyCrackFound(artifact, isActivated2);
			}
			switch (artifact.BlueprintId)
			{
			case "dock":
				NearbyPOIFound(artifact, Shared.System.PointOfInterest.Port);
				break;
			case "neutral_warphole":
			case "cargo_warphole_in":
				NearbyPOIFound(artifact, Shared.System.PointOfInterest.CargoWarphole);
				break;
			case "warp_accelerator":
				NearbyPOIFound(artifact, Shared.System.PointOfInterest.Rift);
				break;
			}
		}
		if (nearbyPOI.Type != _nearbyPOI.Type || nearbyPOI.Position != _nearbyPOI.Position)
		{
			NotifyNearbyPOIUpdated();
		}
	}

	private void NearbyPOIFound(ImmovableBase obj, Shared.System.PointOfInterest poiType)
	{
		if (!_exploreredPOIs.ContainsKey(obj.WorldTile))
		{
			TryExplorePOI(obj, poiType);
		}
	}

	private void NearbyCrackFound(ImmovableBase obj, bool isActivated)
	{
		Point2 worldTile = obj.WorldTile;
		if (_exploreredPOIs.ContainsKey(worldTile))
		{
			if (!_justRefreshedCracks.ContainsKey(worldTile) || _justRefreshedCracks[worldTile] != isActivated)
			{
				SendExplorePOIMsg(worldTile, obj.EntityType, Shared.System.PointOfInterest.Crack);
				_justRefreshedCracks[worldTile] = isActivated;
			}
		}
		else
		{
			TryExplorePOI(obj, Shared.System.PointOfInterest.Crack);
		}
	}

	private void TryExplorePOI(ImmovableBase obj, Shared.System.PointOfInterest poiType)
	{
		Vector3 center = obj.Center;
		Point2 worldTile = obj.WorldTile;
		float num = Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, center);
		if (num <= 500f)
		{
			if (!_justFoundPOIs.Contains(worldTile))
			{
				_justFoundPOIs.Add(worldTile);
				SendExplorePOIMsg(worldTile, obj.EntityType, poiType);
			}
		}
		else if (poiType == Shared.System.PointOfInterest.Port || num <= DistanceForUpdateNearby)
		{
			TryToUpdateNearbyPOI(obj, poiType);
		}
	}

	private void SendExplorePOIMsg(Point2 tile, ushort? entityType, Shared.System.PointOfInterest poiType)
	{
		Connections.Frontend.Send(new ExplorePOI
		{
			Tile = tile,
			Type = poiType,
			EntityType = entityType
		}).On<OK>(delegate
		{
			if (this.GetExploredPOIsRequested != null)
			{
				this.GetExploredPOIsRequested();
			}
		});
	}

	private void TryToUpdateNearbyPOI(ImmovableBase obj, Shared.System.PointOfInterest poiType)
	{
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		Vector3 center = obj.Center;
		if (_nearbyPOI.Type == Shared.System.PointOfInterest.Invalid || !(Vector3.Distance(_nearbyPOI.Position, currentPosition) < Vector3.Distance(center, currentPosition)))
		{
			_nearbyPOI.Set(poiType, center);
		}
	}

	private static float GetAdditionalNearbyDistance()
	{
		return GameSystem<StatisticsSystem>.Instance().GetModifier("poi_discover_plus");
	}

	private void NotifyNearbyPOIUpdated()
	{
		if (this.NearbyPOIUpdated != null)
		{
			if (_nearbyPOI.Type != Shared.System.PointOfInterest.Invalid)
			{
				this.NearbyPOIUpdated(_nearbyPOI);
			}
			else
			{
				this.NearbyPOIUpdated(null);
			}
		}
	}

	private void CombatSystem_ChangedCombatMode(bool combatMode)
	{
		if (combatMode)
		{
			_nearbyPOI.Clear();
			NotifyNearbyPOIUpdated();
		}
	}

	private void WarpRushSystem_SurvivorRegionChanged(ResourceType resourceType)
	{
		_justRefreshedCracks.Clear();
	}
}
