using System;
using System.Collections.Generic;
using K1Network;
using Messages;
using Shared.System;
using UnityEngine;

public class POIUpdater
{
	public struct NearbyPOI
	{
		public Shared.System.PointOfInterest Type;

		public Vector3 Position;

		public void Clear()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Type = Shared.System.PointOfInterest.Invalid;
			Position = Vector3.zero;
		}

		public void Set(Shared.System.PointOfInterest type, Vector3 position)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Type = type;
			Position = position;
		}
	}

	private const string NearbyDiscoverModifierName = "poi_discover_plus";

	private const float DistanceForExplore = 500f;

	private const float MinDistanceForSearchNearby = 1600f;

	private const int WarpHoleEntityType = 15001;

	private const int CraterEntityType = 15002;

	private const string ArtifactIdPort = "dock";

	private const string ArtifactIdCrack = "crack_01";

	private readonly List<GameObject> _searchList = new List<GameObject>();

	private readonly Dictionary<Point2, Shared.System.PointOfInterest> _exploreredPOIs = new Dictionary<Point2, Shared.System.PointOfInterest>();

	private readonly HashSet<Point2> _justFoundPOIs = new HashSet<Point2>();

	private NearbyPOI _nearbyPOI;

	private Dictionary<Point2, bool> _justRefreshedCracks = new Dictionary<Point2, bool>();

	public int EntireWarpholeCount { get; private set; }

	private float DistanceForSearchNearby => 3200f + GetAdditionalNearbyDistance();

	private float DistanceForUpdateNearby => 1600f + GetAdditionalNearbyDistance();

	public event Action GetExploredPOIsRequested;

	public event Action<NearbyPOI?> NearbyPOIUpdated;

	public void Init()
	{
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
			RegionId = KSingleton<GameManager>.Instance().Region.Id
		}).On(delegate(POICount msg, PacketHeader _)
		{
			EntireWarpholeCount = msg.WarpholeCount;
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

	public int GetExploredWarpholeCount()
	{
		int num = 0;
		Dictionary<Point2, Shared.System.PointOfInterest>.Enumerator enumerator = _exploreredPOIs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value == Shared.System.PointOfInterest.Warphole)
			{
				num++;
			}
		}
		return num;
	}

	public void SearchPOIProp()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		int mask = LayerMask.op_Implicit(LayerHelper.PropMask);
		InteractionSystem.GetNearObjectsInternal(_searchList, mask, DistanceForSearchNearby);
		NearbyPOI nearbyPOI = _nearbyPOI;
		_nearbyPOI.Clear();
		for (int i = 0; i < _searchList.Count; i++)
		{
			GameObject val = _searchList[i];
			if (((Object)val).name.Contains("BiocomMarker"))
			{
				NearbyPOIFound(tile: new Point2(TerrainA6.ClientPositionToTilePosition(val.transform.position)), posArtifactCenter: val.transform.position, poiType: Shared.System.PointOfInterest.Biocom);
				continue;
			}
			GameObject val2 = InteractionSystem.ImmovableObjectFilter(val);
			NaturalObject naturalObject = ((!((Object)(object)val2 != (Object)null)) ? null : val2.GetComponent<NaturalObject>());
			if ((Object)(object)naturalObject != (Object)null)
			{
				switch (naturalObject.EntityType)
				{
				case 15001:
					NearbyPOIFound(naturalObject.Center, naturalObject.WorldTile, Shared.System.PointOfInterest.Warphole);
					break;
				case 15002:
					NearbyPOIFound(naturalObject.Center, naturalObject.WorldTile, Shared.System.PointOfInterest.Crater);
					break;
				}
				continue;
			}
			Artifact artifact = ((!((Object)(object)val2 != (Object)null)) ? null : val2.GetComponent<Artifact>());
			if ((Object)(object)artifact == (Object)null)
			{
				continue;
			}
			switch (artifact.ArtifactId)
			{
			case "dock":
				NearbyPOIFound(artifact.Center, artifact.WorldTile, Shared.System.PointOfInterest.Port);
				break;
			case "crack_01":
				if (artifact.ArtifactState.Crack.HasValue)
				{
					Crack value = artifact.ArtifactState.Crack.Value;
					double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
					double? activatedSince = value.ActivatedSince;
					int num;
					if (activatedSince.HasValue && value.ActivatedSince.Value <= bufferedServerTime_Enhanced)
					{
						double? activatedUntil = value.ActivatedUntil;
						num = ((activatedUntil.HasValue && activatedUntil.Value > bufferedServerTime_Enhanced) ? 1 : 0);
					}
					else
					{
						num = 0;
					}
					bool isActivated = (byte)num != 0;
					NearbyCrackFound(artifact.Center, artifact.WorldTile, isActivated);
				}
				break;
			}
		}
		if (nearbyPOI.Type != _nearbyPOI.Type || nearbyPOI.Position != _nearbyPOI.Position)
		{
			NotifyNearbyPOIUpdated();
		}
	}

	private void NearbyPOIFound(Vector3 posArtifactCenter, Point2 tile, Shared.System.PointOfInterest poiType)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!_exploreredPOIs.ContainsKey(tile))
		{
			TryExplorePOI(posArtifactCenter, tile, poiType);
		}
	}

	private void NearbyCrackFound(Vector3 posArtifactCenter, Point2 tile, bool isActivated)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (_exploreredPOIs.ContainsKey(tile))
		{
			if (!_justRefreshedCracks.ContainsKey(tile) || _justRefreshedCracks[tile] != isActivated)
			{
				SendExplorePOIMsg(tile, Shared.System.PointOfInterest.Crack);
				_justRefreshedCracks[tile] = isActivated;
			}
		}
		else
		{
			TryExplorePOI(posArtifactCenter, tile, Shared.System.PointOfInterest.Crack);
		}
	}

	private void TryExplorePOI(Vector3 posArtifactCenter, Point2 tile, Shared.System.PointOfInterest poiType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, posArtifactCenter);
		if (num <= 500f)
		{
			if (!_justFoundPOIs.Contains(tile))
			{
				_justFoundPOIs.Add(tile);
				SendExplorePOIMsg(tile, poiType);
			}
		}
		else if (poiType == Shared.System.PointOfInterest.Port || num <= DistanceForUpdateNearby)
		{
			TryToUpdateNearbyPOI(posArtifactCenter, poiType);
		}
	}

	private void SendExplorePOIMsg(Point2 tile, Shared.System.PointOfInterest poiType)
	{
		Connections.Frontend.Send(new ExplorePOI
		{
			Tile = tile,
			Type = poiType
		}).On<OK>(delegate
		{
			if (this.GetExploredPOIsRequested != null)
			{
				this.GetExploredPOIsRequested();
			}
		});
	}

	private void TryToUpdateNearbyPOI(Vector3 posArtifactCenter, Shared.System.PointOfInterest poiType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		if (_nearbyPOI.Type == Shared.System.PointOfInterest.Invalid || !(Vector3.Distance(_nearbyPOI.Position, currentPosition) < Vector3.Distance(posArtifactCenter, currentPosition)))
		{
			_nearbyPOI.Set(poiType, posArtifactCenter);
		}
	}

	private float GetAdditionalNearbyDistance()
	{
		Dictionary<string, float> modifiers = GameSystem<StatisticsSystem>.Instance().Modifiers;
		if (modifiers != null && modifiers.TryGetValue("poi_discover_plus", out var value))
		{
			return value * 200f * 2f;
		}
		return 0f;
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
}
