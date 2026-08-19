using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using Messages;
using Shared.Building;
using Shared.Etc;
using UnityEngine;
using Yaml;

namespace Durango.Offline;

public static class Cheats
{
	public static Item? MakeItem(string prototypeId, int level)
	{
		prototypeId = prototypeId.Replace(".", "_");
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId);
		if (itemPrototype == null)
		{
			return null;
		}
		Item value = default(Item);
		value.Id = Guid.NewGuid().ToString();
		value.FounderId = null;
		value.FounderCategory = itemPrototype.Category;
		value.Durability = new Gauge(1f, 0f, new GaugeNode[1]
		{
			new GaugeNode(0.0, 1f)
		});
		value.Size = itemPrototype.Size;
		value.Unstable = false;
		value.ModifiableCount = 0;
		value.ModifiedCount = 0;
		int hashCode = value.Id.GetHashCode();
		ItemIconTex.TryGetDefaultColor(itemPrototype.ColorR, out var col, hashCode, Color.white);
		ItemIconTex.TryGetDefaultColor(itemPrototype.ColorG, out var col2, hashCode, Color.white);
		ItemIconTex.TryGetDefaultColor(itemPrototype.ColorB, out var col3, hashCode, Color.white);
		value.ColorR = col.ToHex();
		value.ColorG = col2.ToHex();
		value.ColorB = col3.ToHex();
		value.Icon = itemPrototype.Icon;
		value.Prototype = prototypeId;
		value.Level = level;
		value.Name = itemPrototype.Name;
		value.Description = itemPrototype.Description;
		List<Messages.Tag> list = new List<Messages.Tag>();
		foreach (KeyValuePair<string, string> tag in itemPrototype.Tags)
		{
			list.Add(new Messages.Tag
			{
				Level = level,
				Id = tag.Key
			});
		}
		value.Tags = list.ToArray();
		List<Performance> list2 = new List<Performance>();
		if (PerformanceYaml.TryGetAddOnModelKey(prototypeId, out var modelKey))
		{
			list2.Add(new Performance
			{
				Id = "add_on",
				Strs = new Dictionary<string, string> { { "add_on_model_key", modelKey } }
			});
		}
		PerformanceYaml.Weapon weapon = PerformanceYaml.GetWeapon(prototypeId);
		if (weapon != null)
		{
			list2.Add(new Performance
			{
				Id = "weapon",
				Strs = new Dictionary<string, string>
				{
					{ "weapon_framework", weapon.WeaponFramework },
					{ "model", weapon.Model },
					{ "slot", weapon.Slot }
				}
			});
		}
		PerformanceYaml.Armor armor = PerformanceYaml.GetArmor(prototypeId);
		if (armor != null)
		{
			list2.Add(new Performance
			{
				Id = "armor",
				Strs = new Dictionary<string, string>
				{
					{ "female_model", armor.FemaleModel },
					{ "male_model", armor.MaleModel },
					{ "slot", armor.Slot }
				}
			});
		}
		PerformanceYaml.Instrument instrument = PerformanceYaml.GetInstrument(prototypeId);
		if (instrument != null)
		{
			list2.Add(new Performance
			{
				Id = "instrument",
				Strs = new Dictionary<string, string> { { "timbre", instrument.Timbre } }
			});
		}
		value.Performance = list2.ToArray();
		return value;
	}

	public static AppearArtifact? MakeAppearArtifact(string[] arguments, out AddOns? addons)
	{
		addons = null;
		AppearArtifact appearArtifact = default(AppearArtifact);
		bool flag = arguments[0] == "immortal";
		appearArtifact.EntityId = Guid.NewGuid().ToString();
		string s2 = ((!flag) ? arguments[1] : arguments[2]);
		appearArtifact.EntityType = ushort.Parse(s2);
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(int.Parse(s2));
		if (blueprint == null)
		{
			return null;
		}
		appearArtifact.Display.Condition = Shared.Building.Condition.Normal;
		appearArtifact.Display.EntityId = appearArtifact.EntityId;
		appearArtifact.Display.Parts = new Dictionary<string, string>();
		for (int i = ((!flag) ? 2 : 3); i < arguments.Length; i++)
		{
			string[] array = arguments[i].Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			if (array[0] == "floor" && array.Length == 2 && int.TryParse(array[1], out var result))
			{
				appearArtifact.Floor = result;
				continue;
			}
			switch (array[0])
			{
			case "rotation":
				appearArtifact.Rotation = (Rotation)Enum.Parse(typeof(Rotation), array[1]);
				continue;
			case "position":
			{
				string[] array3 = array[1].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				appearArtifact.Tile = new Point2(int.Parse(array3[0]), int.Parse(array3[1]));
				continue;
			}
			case "size":
			{
				string[] array2 = array[1].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				appearArtifact.Size = new Point2(int.Parse(array2[0]), int.Parse(array2[1]));
				continue;
			}
			case "stories":
				if (array.Length == 2)
				{
					appearArtifact.Stories = array[1].ToInt();
				}
				continue;
			case "level":
				continue;
			}
			if (array.Length != 2)
			{
				continue;
			}
			string slotId = array[0];
			if (blueprint.Slots == null)
			{
				continue;
			}
			Building.BlueprintSlot blueprintSlot = blueprint.Slots.FirstOrDefault((Building.BlueprintSlot s) => s.Id == slotId);
			if (blueprintSlot != null)
			{
				Dictionary<string, ArtifactLook> looks = blueprintSlot.Looks;
				if (looks != null)
				{
					appearArtifact.Display.Parts.Add(slotId, looks[array[1]].model_key);
				}
			}
		}
		if (appearArtifact.Rotation == Rotation.Quarter || appearArtifact.Rotation == Rotation.ThreeQuarter)
		{
			appearArtifact.Size = new Point2(appearArtifact.Size.y, appearArtifact.Size.x);
		}
		for (int j = 0; j < KUtility.GetSize(blueprint.Components); j++)
		{
			switch (blueprint.Components[j])
			{
			case "Modular":
			{
				appearArtifact.Display.AddOns = new Dictionary<int, Pair<string, string>>();
				int? stories = appearArtifact.Stories;
				if (!stories.HasValue)
				{
					appearArtifact.Stories = 1;
				}
				break;
			}
			case "Landmark":
			case "FourSideEnterable":
				appearArtifact.Stories = 1;
				SetDisplayParts(appearArtifact, blueprint);
				break;
			default:
				SetDisplayParts(appearArtifact, blueprint);
				break;
			}
		}
		appearArtifact.States.BuildingState = BuildingState.Completed;
		appearArtifact.States.Durability = new Gauge(1f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 1f
			}
		});
		if (appearArtifact.Display.Parts.Count == 0)
		{
			appearArtifact.Display.Parts.Add("common", (!blueprint.Components.Contains("Burnable")) ? blueprint.DefaultLook : (blueprint.DefaultLook + "_burning"));
		}
		if (appearArtifact.Display.AddOns != null)
		{
			Dictionary<string, string> parts = appearArtifact.Display.Parts;
			if (parts != null && !string.IsNullOrEmpty(parts.Get("wall")))
			{
				Item? item = MakeItem("door_01_none", 1);
				if (item.HasValue)
				{
					addons = new AddOns
					{
						_AddOns = new Dictionary<int, Item> { { 0, item.Value } }
					};
				}
			}
		}
		return appearArtifact;
	}

	private static void SetDisplayParts(AppearArtifact artifact, Building.Blueprint blueprint)
	{
		if (artifact.Display.Parts.Count != 0)
		{
			return;
		}
		Building.BlueprintSlot[] slots = blueprint.Slots;
		foreach (Building.BlueprintSlot blueprintSlot in slots)
		{
			Dictionary<string, ArtifactLook> looks = blueprintSlot.Looks;
			if (looks == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, ArtifactLook> item in looks)
			{
				artifact.Display.Parts.Add(blueprintSlot.Id, item.Value.model_key);
			}
		}
	}
}
