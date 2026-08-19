using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Durango.Utils.Extensions;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Offline;

public class ArtifactManager
{
	public readonly Dictionary<string, AppearArtifact> _artifacts;

	private readonly Dictionary<string, AddOns> _addOns;

	private readonly Dictionary<string, Messages.Mannequin> _mannequins;

	public static readonly string[] AddOnTags = new string[4] { "door", "window", "wall_deco", "empty_door" };

	public World world;

	public Dictionary<string, List<Item>> _boxInventories;

	public Dictionary<Role, Dictionary<string, Route[]>> _sailingRoutes;

	public event Action<ArtifactDisplay> ArtifactDisplayUpdated;

	public event Action<ArtifactState> ArtifactStateUpdated;

	public void AddArtifact(AppearArtifact artifact)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(artifact.EntityType);
		List<Item> value = new List<Item>();
		_artifacts.Add(artifact.EntityId, artifact);
		if (blueprint.Components.Contains("Inventory"))
		{
			_boxInventories.Add(artifact.EntityId, value);
		}
	}

	public AppearArtifact? Get(string entityId)
	{
		if (_artifacts.TryGetValue(entityId, out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerable<AppearArtifact> Enumerable(Predicate<AppearArtifact> func)
	{
		return _artifacts.Where(delegate(KeyValuePair<string, AppearArtifact> pair)
		{
			Predicate<AppearArtifact> predicate = func;
			KeyValuePair<string, AppearArtifact> keyValuePair2 = pair;
			return predicate(keyValuePair2.Value);
		}).Select(delegate(KeyValuePair<string, AppearArtifact> pair)
		{
			KeyValuePair<string, AppearArtifact> keyValuePair = pair;
			return keyValuePair.Value;
		});
	}

	public AppearArtifact? RemoveArtifact(string entityId)
	{
		_addOns.Remove(entityId);
		if (_artifacts.TryGetValue(entityId, out var value))
		{
			_artifacts.Remove(entityId);
			_boxInventories.Remove(entityId);
			return value;
		}
		return null;
	}

	public Messages.Mannequin? GetMannequin(string entityId)
	{
		if (_mannequins.TryGetValue(entityId, out var value))
		{
			return value;
		}
		return null;
	}

	public void SeedPlant(string entityId, string prototypeId)
	{
		Crop crop = CropYaml.Get(prototypeId);
		if (crop == null)
		{
			return;
		}
		Farming value = default(Farming);
		if (_artifacts.TryGetValue(entityId, out var value2))
		{
			value.PlantName = prototypeId;
			value.PlantedAt = Gauge.CurrentTime;
			value2.States.Farming = value;
			value2.Display.Crop = crop.GrownLooks[KUtility.GetRandomHash(value2.Tile.x, value2.Tile.y) % crop.GrownLooks.Length];
			_artifacts[entityId] = value2;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value2.Display);
			}
			if (this.ArtifactStateUpdated != null)
			{
				this.ArtifactStateUpdated(value2.States);
			}
		}
	}

	public void ChargeEffect(string entityId)
	{
		if (_artifacts.TryGetValue(entityId, out var value))
		{
			value.States.Effector = new Effector
			{
				RemainCount = 100
			};
			value.Display.Decorations = new Dictionary<string, Pair<string, string>>();
			value.Display.Decorations.Add("incense", new Pair<string, string>("clan_thurible_incense", string.Empty));
			_artifacts[entityId] = value;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
		}
	}

	public void Scribble(Scribble scribble)
	{
		if (_artifacts.TryGetValue(scribble.EntityId, out var value))
		{
			value.States.EntityId = value.EntityId;
			value.States.Scribble = new ScribbleContent
			{
				Data = scribble.Data,
				Type = scribble.Type
			};
			_artifacts[scribble.EntityId] = value;
			if (this.ArtifactStateUpdated != null)
			{
				this.ArtifactStateUpdated(value.States);
			}
		}
	}

	public void OpenGate(PropKey key, bool open)
	{
		if (_artifacts.TryGetValue(key.EntityId, out var value) && value.States.GateOpened != open)
		{
			value.States.EntityId = value.EntityId;
			value.States.GateOpened = open;
			_artifacts[key.EntityId] = value;
			if (this.ArtifactStateUpdated != null)
			{
				this.ArtifactStateUpdated(value.States);
			}
		}
	}

	public void ChangeDecoration(string entityId)
	{
		if (!_artifacts.TryGetValue(entityId, out var value))
		{
			return;
		}
		List<string[]> list = RecipeDict.GetDecorations(GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(value.EntityType).Id);
		if (KUtility.GetSize(list) == 0)
		{
			return;
		}
		if (value.Display.Decorations == null)
		{
			value.Display.Decorations = new Dictionary<string, Pair<string, string>>();
		}
		if (value.Display.Decorations.TryGetValue("deco", out var curDeco) && string.IsNullOrEmpty(curDeco.Item2))
		{
			List<string[]> list2 = list.Where((string[] o) => o[0] != curDeco.Item1 || !string.IsNullOrEmpty(o[1])).ToList();
			if (list2.Count > 0)
			{
				list = list2;
			}
		}
		string[] array = list[UnityEngine.Random.Range(0, list.Count)];
		string item = ((!string.IsNullOrEmpty(array[1])) ? UnityEngine.Random.ColorHSV().ToHex() : string.Empty);
		value.Display.Decorations["deco"] = new Pair<string, string>(array[0], item);
		_artifacts[entityId] = value;
		if (this.ArtifactDisplayUpdated != null)
		{
			this.ArtifactDisplayUpdated(value.Display);
		}
	}

	public AddOns GetAddons(string entityId)
	{
		if (!_addOns.ContainsKey(entityId))
		{
			_addOns.Add(entityId, default(AddOns));
		}
		return _addOns[entityId];
	}

	public AppearArtifact? PlaceAddOns(string entityId, Dictionary<int, Item> placements)
	{
		if (_artifacts.TryGetValue(entityId, out var value))
		{
			Dictionary<int, Pair<string, string>> dictionary = new Dictionary<int, Pair<string, string>>();
			foreach (KeyValuePair<int, Item> placement in placements)
			{
				Performance performance = placement.Value.Performance.FirstOrDefault((Performance o) => o.Id == "add_on");
				if (performance.Strs == null)
				{
					continue;
				}
				string item = performance.Strs.Get("add_on_model_key");
				Messages.Tag[] tags = placement.Value.Tags;
				string item2 = AddOnTags.FirstOrDefault((string o) => tags.Any((Messages.Tag p) => o == p.Id));
				dictionary.Add(placement.Key, new Pair<string, string>(item, item2));
			}
			value.Display.AddOns = dictionary;
			_artifacts[entityId] = value;
			_addOns[entityId] = new AddOns
			{
				_AddOns = placements
			};
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
			return value;
		}
		return null;
	}

	public void UpdateArtifactDisplay(ArtifactDisplay display)
	{
		if (_artifacts.TryGetValue(display.EntityId, out var value))
		{
			value.Display = display;
			_artifacts[display.EntityId] = value;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
		}
	}

	public AppearArtifact? ExtendFloor(string entityId, bool withRoof)
	{
		if (!_artifacts.TryGetValue(entityId, out var value))
		{
			return null;
		}
		int? stories = value.Stories;
		if (!stories.HasValue)
		{
			return null;
		}
		value.Stories++;
		value.HasRoof = withRoof;
		_artifacts[entityId] = value;
		return value;
	}

	public void TurnOnMusic(string entityId)
	{
		if (!_artifacts.TryGetValue(entityId, out var value))
		{
			return;
		}
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(value.EntityType);
		if (blueprint != null)
		{
			string[] musics = blueprint.Musics;
			if (KUtility.GetSize(musics) > 0)
			{
				int num = UnityEngine.Random.Range(0, musics.Length);
				value.Display.Music = new Pair<string, double>(musics[num], 0.0);
				_artifacts[entityId] = value;
			}
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
		}
	}

	public void TurnOffMusic(string entityId)
	{
		if (_artifacts.TryGetValue(entityId, out var value) && value.Display.Music.HasValue)
		{
			value.Display.Music = null;
			_artifacts[entityId] = value;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
		}
	}

	public bool TakeOutItems(string entityId, string[] ids)
	{
		if (ids == null)
		{
			return false;
		}
		if (!_artifacts.TryGetValue(entityId, out var _))
		{
			return false;
		}
		Messages.Mannequin? mannequin = GetMannequin(entityId);
		if (!mannequin.HasValue)
		{
			return false;
		}
		Messages.Mannequin value2 = mannequin.Value;
		string text = null;
		Item? head = value2.Head;
		if (head.HasValue && ids.Contains(value2.Head.Value.Id))
		{
			text = "head";
			value2.Head = null;
		}
		else
		{
			Item? body = value2.Body;
			if (body.HasValue && ids.Contains(value2.Body.Value.Id))
			{
				text = "body";
				value2.Body = null;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (value2.Head.HasValue || value2.Body.HasValue)
		{
			_mannequins[entityId] = value2;
		}
		else
		{
			_mannequins.Remove(entityId);
		}
		TakeOffMannequin(entityId, text);
		return true;
	}

	public bool TakeOffMannequin(string entityId, string slot)
	{
		if (!_artifacts.TryGetValue(entityId, out var value))
		{
			return false;
		}
		ArtifactDisplay display = value.Display;
		MannequinDisplayInfo valueOrDefault = display.MannequinInfo.GetValueOrDefault();
		Messages.Mannequin value2 = _mannequins.Get(entityId);
		switch (slot)
		{
		default:
			return false;
		case "body":
			valueOrDefault.Body = null;
			valueOrDefault.BodyColor = null;
			value2.Body = null;
			goto IL_009b;
		case "head":
			valueOrDefault.Head = null;
			valueOrDefault.HeadColor = null;
			value2.Head = null;
			goto IL_009b;
		case null:
			{
				return false;
			}
			IL_009b:
			value2.EntityId = entityId;
			_mannequins[entityId] = value2;
			display.MannequinInfo = valueOrDefault;
			value.Display = display;
			_artifacts[entityId] = value;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
			return true;
		}
	}

	public bool ChangeMannequin(string entityId, string slot, Item item)
	{
		if (!_artifacts.TryGetValue(entityId, out var value))
		{
			return false;
		}
		if (!SingletonDict<int, ArtifactPrototype>.TryGetValue(value.EntityType, out var value2))
		{
			return false;
		}
		string gender = value2.gender;
		if (gender == null)
		{
			return false;
		}
		bool flag;
		if (!(gender == "male"))
		{
			if (!(gender == "female"))
			{
				return false;
			}
			flag = false;
		}
		else
		{
			flag = true;
		}
		string text = null;
		string key = ((!flag) ? "female_model" : "male_model");
		if (item.Performance != null)
		{
			Performance[] performance = item.Performance;
			for (int i = 0; i < performance.Length; i++)
			{
				Performance performance2 = performance[i];
				if (performance2.Strs != null && performance2.Strs.TryGetValue(key, out var value3))
				{
					text = value3;
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text) || text.Equals("None", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		ArtifactDisplay display = value.Display;
		MannequinDisplayInfo valueOrDefault = display.MannequinInfo.GetValueOrDefault();
		Messages.Mannequin value4 = _mannequins.Get(entityId);
		switch (slot)
		{
		default:
			return false;
		case "body":
			valueOrDefault.Body = text;
			valueOrDefault.BodyColor = new string[3] { item.ColorR, item.ColorG, item.ColorB };
			value4.Body = item;
			goto IL_019c;
		case "head":
			valueOrDefault.Head = text;
			valueOrDefault.HeadColor = new string[3] { item.ColorR, item.ColorG, item.ColorB };
			value4.Head = item;
			goto IL_019c;
		case null:
			{
				return false;
			}
			IL_019c:
			value4.EntityId = entityId;
			_mannequins[entityId] = value4;
			display.MannequinInfo = valueOrDefault;
			value.Display = display;
			_artifacts[entityId] = value;
			if (this.ArtifactDisplayUpdated != null)
			{
				this.ArtifactDisplayUpdated(value.Display);
			}
			return true;
		}
	}

	public List<Item> GetBoxItems(string entityId)
	{
		if (_boxInventories.TryGetValue(entityId, out var value))
		{
			return new List<Item>(value);
		}
		return null;
	}

	public ArtifactManager(Dictionary<string, AppearArtifact> artifacts, Dictionary<string, AddOns> addons, Dictionary<string, Messages.Mannequin> mannequins, Dictionary<string, List<Item>> boxInventories, Dictionary<Role, Dictionary<string, Route[]>> sailingRoutes)
	{
		_artifacts = artifacts;
		_addOns = addons;
		_mannequins = mannequins;
		_boxInventories = boxInventories;
		_sailingRoutes = sailingRoutes;
	}

	public void AddArchitect(string artifactId, string entityId)
	{
		if (_artifacts.TryGetValue(artifactId, out var value) && !string.IsNullOrEmpty(entityId) && !value.ArchitectEntityIds.Contains(entityId))
		{
			List<string> list = value.ArchitectEntityIds.ToList();
			list.Add(entityId);
			value.ArchitectEntityIds = list.ToArray();
			_artifacts[artifactId] = value;
			UIManager.SystemMsg($"Successfully Added to Architect List: <em>{entityId}</em>");
		}
	}
}
