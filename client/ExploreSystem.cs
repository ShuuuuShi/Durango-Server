using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Explore;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ExploreSystem : GameSystem<ExploreSystem>
{
	private const string RecentlyVisitKey = "RecentlyVisitTemplateIds";

	public const int UrbanRoleLevel = 40;

	public static string LastFoundRegion;

	private Dictionary<Role, Dictionary<string, Route[]>> _routes;

	private ArchipelagoRoute[] _archipelagoRoutes;

	private readonly Dictionary<string, Durango.Logic.Explore.Region> _regions = new Dictionary<string, Durango.Logic.Explore.Region>();

	private readonly Dictionary<string, Archipelago> _archipelagoes = new Dictionary<string, Archipelago>();

	private AsyncCachedDictionary<string, Archipelago> _archipelagoAsyncDict;

	private Dictionary<Pair<int, Biome>, int> _clearedUnstableFactors;

	public LinkedList<string> RecentlyVisits { get; private set; }

	public static bool ReadyToUrbanExplore { get; private set; }

	public event Action RoutesUpdated;

	private void Awake()
	{
		Connections.Frontend.On<Routes>(OnRoutes);
		Connections.Frontend.On<RoutesOfArchipelago>(OnRoutesOfArchipelago);
		Connections.Frontend.On<RegionExpirationAlarm>(OnRegionExpirationAlarm);
		Connections.Frontend.On<RegionExpired>(OnRegionExpiration);
		Connections.Frontend.On<RegionMovedByExpiration>(OnRegionMovedByExpiration);
		Connections.Frontend.On<ClearedUnstableFactors>(OnClearedUnstableFactors);
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			LoadRecentlyVisits();
			if (GameManager.Region.Role() == Role.Risky && GameManager.Region.Template != null)
			{
				AddToRecentlyVisits(GameManager.Region.Template);
			}
		});
		_archipelagoAsyncDict = new AsyncCachedDictionary<string, Archipelago>(RequestArchipelago);
	}

	public int GetClearedUnstableFactor(int level, Biome biome)
	{
		return (_clearedUnstableFactors != null) ? _clearedUnstableFactors.Get(new Pair<int, Biome>(level, biome), 0) : 0;
	}

	public Archipelago GetArchipelago(string archipelagoId)
	{
		return _archipelagoes.Get(archipelagoId);
	}

	[CanBeNull]
	public List<Route> GetUrbanRoutes()
	{
		if (_routes == null)
		{
			return null;
		}
		Dictionary<string, Route[]> dictionary = _routes.Get(Role.Urban);
		if (dictionary == null)
		{
			return null;
		}
		List<Route> list = null;
		foreach (KeyValuePair<string, Route[]> item2 in dictionary)
		{
			Route[] value = item2.Value;
			foreach (Route item in value)
			{
				if (list == null)
				{
					list = new List<Route>();
				}
				list.Add(item);
			}
		}
		return list?.Distinct().ToList();
	}

	[CanBeNull]
	public Route[] GetOutpostRoute([NotNull] RegionTemplate template)
	{
		if (_routes == null)
		{
			return null;
		}
		return _routes.Get(Role.Outpost)?.Get(template.Id);
	}

	public bool HasOutpostRoute([NotNull] RegionTemplate template)
	{
		if (_routes == null)
		{
			return false;
		}
		return _routes.Get(Role.Outpost)?.ContainsKey(template.Id) ?? false;
	}

	public bool HasRoutes()
	{
		return _routes != null || _archipelagoRoutes != null;
	}

	public List<ArchipelagoRoute> GetArchipelagoRoutes(int level, Biome biome)
	{
		return _archipelagoRoutes.Where((ArchipelagoRoute route) => route.Biome == biome && route.Level == level).ToList();
	}

	public void AddRegion([NotNull] Durango.Logic.Explore.Region region)
	{
		_regions[region.Id] = region;
	}

	[NotNull]
	public Durango.Logic.Explore.Region GetRegion([NotNull] string regionId)
	{
		Durango.Logic.Explore.Region region = _regions.Get(regionId);
		return (region != null) ? region : Durango.Logic.Explore.Region.UnknownRegion;
	}

	public static void Withdraw(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new Withdraw
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	public static void TravelToRandomPersonalRegion()
	{
		Connections.Frontend.Send(default(TravelToRandomPersonalRegion));
	}

	public static void WarpToNextArchipelagoRegion(Messages.Region region)
	{
		GameSystem<MapSystem>.Instance().TryWarp(() => Connections.Frontend.Send(default(WarpToNextArchipelagoRegion)), MapSystem.WarpTo.Warp, region);
	}

	public void TravelRegion(Port port, string regionId, string partierId, bool goesNeighbor, float delay)
	{
		StartCoroutine(CoTravelRegion(port, regionId, partierId, goesNeighbor, delay));
	}

	private IEnumerator CoTravelRegion(Port port, string regionId, string partierId, bool goesNeighbor, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (goesNeighbor)
		{
			Connections.Frontend.Send(new TravelByRegionInArchipelago
			{
				EntityId = port.Id,
				Tile = port.Tile,
				RegionId = regionId
			});
		}
		else
		{
			Connections.Frontend.Send(new TravelByRegion
			{
				EntityId = port.Id,
				Tile = port.Tile,
				RegionId = regionId,
				PartierId = partierId
			});
		}
	}

	public void SailingBack(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new SailingBack
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	public void GetSailingBackCost([NotNull] Action<long> callback)
	{
		Connections.Frontend.Send(default(GetSailingBackCost)).On(delegate(SailingBackCost cost, PacketHeader header)
		{
			callback(cost.Cost);
		});
	}

	public void RecommendArchipelago(ArchipelagoRoute archipelagoRoute, Action<Archipelago> onResult)
	{
		Connections.Frontend.Send(new RecommendArchipelago
		{
			Level = archipelagoRoute.Level,
			Biome = archipelagoRoute.Biome,
			UnstableFactor = archipelagoRoute.UnstableFactor
		}).On(delegate(Archipelago msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
		});
	}

	public void RecommendRegion(Port port, Role role, string templateId, Action<Durango.Logic.Explore.Region> onResult)
	{
		Connections.Frontend.Send(new RecommendRegion
		{
			EntityId = port.Id,
			Tile = port.Tile,
			Role = role,
			TemplateId = templateId
		}).On(delegate(Messages.Region msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(new Durango.Logic.Explore.Region(msg));
			}
		}).On<Error>(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	private void AddToRecentlyVisits([NotNull] RegionTemplate template)
	{
		string id2 = template.Id;
		string value = RecentlyVisits.FirstOrDefault(delegate(string id)
		{
			RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(id);
			return regionTemplate != null && regionTemplate.Level == template.Level && regionTemplate.MajorBiome() == template.MajorBiome();
		});
		RecentlyVisits.Remove(value);
		RecentlyVisits.AddFirst(id2);
		if (RecentlyVisits.Count > 10)
		{
			RecentlyVisits.RemoveLast();
		}
		SaveRecentlyVisits();
	}

	private void LoadRecentlyVisits()
	{
		string @string = Preferences.GetString("RecentlyVisitTemplateIds", string.Empty, Preferences.Level.Player);
		RecentlyVisits = ((!string.IsNullOrEmpty(@string)) ? Json.Read<LinkedList<string>>(@string) : new LinkedList<string>());
	}

	private void SaveRecentlyVisits()
	{
		string value = Json.Write(RecentlyVisits);
		Preferences.SetString("RecentlyVisitTemplateIds", value, Preferences.Level.Player);
	}

	private void OnRoutesOfArchipelago(RoutesOfArchipelago archipelagoRoutes, PacketHeader header)
	{
		Routes routes = default(Routes);
		routes._Routes = null;
		routes.ArchipelagoRoutes = archipelagoRoutes.ArchipelagoRoutes;
		Routes routes2 = routes;
		OnRoutes(routes2, header);
	}

	private void OnRoutes(Routes routes, PacketHeader header)
	{
		_routes = routes._Routes;
		_archipelagoRoutes = routes.ArchipelagoRoutes;
		ReadyToUrbanExplore = false;
		List<string> discoveredRegionIds = new List<string>();
		List<string> list = new List<string>();
		if (_routes != null)
		{
			foreach (KeyValuePair<Role, Dictionary<string, Route[]>> route3 in _routes)
			{
				Role key = route3.Key;
				foreach (KeyValuePair<string, Route[]> item in route3.Value)
				{
					string key2 = item.Key;
					Route[] value = item.Value;
					if (key == Role.Urban)
					{
						ReadyToUrbanExplore = true;
					}
					if (string.IsNullOrEmpty(key2))
					{
						continue;
					}
					RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(key2);
					if (regionTemplate != null)
					{
						Route[] array = value;
						for (int i = 0; i < array.Length; i++)
						{
							Route route2 = array[i];
							discoveredRegionIds.Add(route2.RegionId);
						}
					}
				}
			}
		}
		ArchipelagoRoute[] archipelagoRoutes = routes.ArchipelagoRoutes;
		for (int j = 0; j < archipelagoRoutes.Length; j++)
		{
			ArchipelagoRoute archipelagoRoute = archipelagoRoutes[j];
			if (archipelagoRoute.IncludedRoutes != null)
			{
				list.Add(archipelagoRoute.ArchipelagoId);
				discoveredRegionIds.AddRange(archipelagoRoute.IncludedRoutes.Select((Route route) => route.RegionId));
			}
		}
		_regions.Clear();
		_archipelagoes.Clear();
		RequestArchipelagos(list, delegate(Archipelago[] archipelagoes)
		{
			if (archipelagoes != null)
			{
				for (int k = 0; k < archipelagoes.Length; k++)
				{
					Archipelago value2 = archipelagoes[k];
					if (!string.IsNullOrEmpty(value2.Id))
					{
						_archipelagoes.Add(value2.Id, value2);
					}
				}
			}
			RequestRegionsForExploreSystem(discoveredRegionIds, this.RoutesUpdated);
		});
	}

	public void RequestRegionsForExploreSystem(IList<string> regionIds, Action completed = null)
	{
		GameSystem<MapSystem>.Instance().GetRegions(regionIds, delegate(Messages.Region[] regions)
		{
			if (regions != null)
			{
				for (int i = 0; i < regions.Length; i++)
				{
					Messages.Region region = regions[i];
					if (!string.IsNullOrEmpty(region.Id))
					{
						AddRegion(new Durango.Logic.Explore.Region(region));
					}
				}
			}
			if (completed != null)
			{
				completed();
			}
		});
	}

	public void RequestRoutes(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetRoutes
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	public void RequestRoutesOfParty(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetRoutesOfParty
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	public void RequestRouteOfArchipelago(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetRouteOfArchipelago
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	public void RequestArchipelago([CanBeNull] string id, [NotNull] Action<Archipelago> onResult)
	{
		_archipelagoAsyncDict.Request(id, onResult);
	}

	public void RequestArchipelagos(IList<string> ids, [NotNull] Action<Archipelago[]> onResult)
	{
		_archipelagoAsyncDict.Request(ids, onResult);
	}

	private static void RequestArchipelago(string id, Archipelago cacheValue, Action<string, Archipelago> result)
	{
		Connections.Frontend.Send(new GetArchipelago
		{
			ArchipelagoId = id
		}).On(delegate(Archipelago msg, PacketHeader header)
		{
			result(msg.Id, msg);
		}).On<Error>(delegate
		{
			result(id, default(Archipelago));
		});
	}

	private void OnRegionExpirationAlarm(RegionExpirationAlarm msg, PacketHeader header)
	{
		string comment = T._("{0} 후 섬이 사라집니다.", TimedeltaFormatter.Format(msg.After, 1));
		UIManager.SystemMsg(comment, 4f);
	}

	private void OnRegionExpiration(RegionExpired msg, PacketHeader header)
	{
		UIManager.SystemMsg(T._("섬이 곧 사라집니다. {0} 후 귀환 지점으로 돌아갑니다.", TimedeltaFormatter.Format(msg.After, 1)), 4f);
	}

	private void OnRegionMovedByExpiration(RegionMovedByExpiration msg, PacketHeader header)
	{
		UIManager.MessageBox.Show(T._("이전에 있던 섬이 사라져 귀환 지점으로 돌아왔습니다."));
	}

	private void OnClearedUnstableFactors(ClearedUnstableFactors msg, PacketHeader header)
	{
		_clearedUnstableFactors = msg._ClearedUnstableFactors;
	}
}
