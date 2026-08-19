using System;
using System.Collections.Generic;
using System.IO;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Newtonsoft.Json;
using Shared.Region;

namespace Durango.Offline;

public class WorldContext
{
	[JsonProperty("player_slot")]
	public int PlayerSlot;

	[JsonProperty("terrain_id")]
	public string TerrainId;

	[JsonProperty("artifacts")]
	public Dictionary<string, AppearArtifact> Artifacts;

	[JsonProperty("artifact_addOns")]
	public Dictionary<string, AddOns> ArtifactAddOns;

	[JsonProperty("artifact_mannequins")]
	public Dictionary<string, Messages.Mannequin> ArtifactMannequins;

	[JsonProperty("added_natural")]
	public List<NaturalInfo> AddedNatural;

	[JsonProperty("removed_natural")]
	public List<Point2> RemovedNatural;

	[JsonProperty("grazed_pet_list")]
	public List<Pet> GrazedPetList;

	[JsonProperty("garden")]
	public byte[] Garden;

	[JsonProperty("persistent")]
	public bool Persistent;

	[JsonProperty("collected_from")]
	public Dictionary<string, Collectible> CollectedFrom;

	[JsonProperty("action_timer")]
	public Dictionary<string, global::System.DateTime> ActionTimer;

	[JsonProperty("box_inventories")]
	public Dictionary<string, List<Item>> BoxInventories;

	[JsonProperty("wild_animal_list")]
	public List<AppearAnimal> WildAnimalList;

	[JsonProperty("sailing_routes")]
	public Dictionary<Role, Dictionary<string, Route[]>> SailingRoutes;

	[JsonProperty("is_sailing_unstable")]
	public bool IsSailingUnstable;

	[JsonProperty("stable_terrain_id")]
	public string StableTerrainId;

	[JsonIgnore]
	public string Path { get; private set; }

	public void Initialize(string path)
	{
		if (Artifacts == null)
		{
			Artifacts = new Dictionary<string, AppearArtifact>();
		}
		if (BoxInventories == null)
		{
			BoxInventories = new Dictionary<string, List<Item>>();
		}
		if (ArtifactAddOns == null)
		{
			ArtifactAddOns = new Dictionary<string, AddOns>();
		}
		if (ArtifactMannequins == null)
		{
			ArtifactMannequins = new Dictionary<string, Messages.Mannequin>();
		}
		if (AddedNatural == null)
		{
			AddedNatural = new List<NaturalInfo>();
		}
		if (RemovedNatural == null)
		{
			RemovedNatural = new List<Point2>();
		}
		if (GrazedPetList == null)
		{
			GrazedPetList = new List<Pet>();
		}
		if (CollectedFrom == null)
		{
			CollectedFrom = new Dictionary<string, Collectible>();
		}
		if (ActionTimer == null)
		{
			ActionTimer = new Dictionary<string, global::System.DateTime>();
		}
		if (WildAnimalList == null)
		{
			WildAnimalList = new List<AppearAnimal>();
		}
		if (SailingRoutes == null)
		{
			SailingRoutes = new Dictionary<Role, Dictionary<string, Route[]>>();
		}
		Path = path;
	}

	[CanBeNull]
	public static WorldContext Load(string path)
	{
		WorldContext worldContext = null;
		try
		{
			worldContext = Json.Read<WorldContext>(File.ReadAllBytes(path));
			if (worldContext == null)
			{
				return null;
			}
			worldContext.Initialize(path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return worldContext;
	}

	public void Save(bool persistent = false)
	{
		if (Persistent && !persistent)
		{
			return;
		}
		if (persistent)
		{
			Persistent = true;
		}
		try
		{
			byte[] bytes = Json.WriteToBytes(this, indented: true);
			File.WriteAllBytes(Path, bytes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static string MakePath(int slot, string clusterKey)
	{
		return global::System.IO.Path.Combine(AppData.CombinePath(GetBasePath(clusterKey)), slot + ".world");
	}

	public static string GetBasePath(string clusterKey)
	{
		return global::System.IO.Path.Combine("offline", clusterKey);
	}
}
