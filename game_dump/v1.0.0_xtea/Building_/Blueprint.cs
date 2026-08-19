using System;
using System.Collections.Generic;
using Crafting;
using ItemSystem;
using TerrainData;
using Yaml;

namespace Building_;

public class Blueprint : CategoryItem
{
	public string SubCategory;

	public bool RotationDisabled;

	public int EntityType;

	public bool Permanent;

	public string Description;

	public bool HasDefaultLook;

	public int PostprocessTime;

	public string[] Components;

	public string Preview;

	public string DefaultLook;

	public string ArtifactIcon;

	public bool IsSizeVariable;

	public Point2 Size;

	public Biome[] BuildableBiomes;

	public bool Exterior;

	public bool Interior;

	public bool TransparentSite;

	public int Floor;

	public float MinBuildableDepth;

	public float MaxBuildableDepth;

	public BlueprintWater Water;

	public BlueprintSlot[] Slots;

	public TagFilter[] ToolTags;

	public Scribblable Scribblable;

	public float BoundRadius;

	public HashSet<string> AbilityType = new HashSet<string>();

	public override string LocalizedName => (!string.IsNullOrEmpty(Name)) ? Name : Id;

	public bool ToolRequired => ToolTags.Length > 0;

	public bool IsEstateFlag { get; private set; }

	public bool IsClanEstateFlag { get; private set; }

	public bool IsModular { get; private set; }

	public bool IsLookChangeable()
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].HasLook)
			{
				return true;
			}
		}
		return false;
	}

	public void SetBlueprintInfo(string key, Yaml.Blueprint json)
	{
		Name = json.name;
		Icon = json.icon;
		Description = json.description;
		Category = json.category;
		SubCategory = json.subcategory;
		PostprocessTime = json.postprocess_time;
		Preview = json.preview;
		HasDefaultLook = !string.IsNullOrEmpty(json.default_look);
		DefaultLook = json.default_look;
		Slots = new BlueprintSlot[json.slots.Count];
		int num = 0;
		foreach (KeyValuePair<string, Yaml.BlueprintSlot> slot in json.slots)
		{
			BlueprintSlot blueprintSlot = new BlueprintSlot();
			Slots[num] = blueprintSlot;
			num++;
			Yaml.BlueprintSlot value = slot.Value;
			blueprintSlot.Id = slot.Key;
			blueprintSlot.Name = value.slot_name;
			blueprintSlot.RequiredCount = value.count;
			blueprintSlot.SizeFactor = ExpressionParser.Parse(value.size_factor);
			blueprintSlot.RequiredTags = TagFilter.CreateTagFilters(value.required_tags);
			blueprintSlot.RequiredMaterials = TagFilter.CreateTagFilters(value.required_materials);
			blueprintSlot.Looks = value.looks;
		}
		ToolTags = TagFilter.CreateTagFilters(json.tool_tags);
	}

	public void SetPrototypeInfo(int entityType, ArtifactPrototype json)
	{
		EntityType = entityType;
		Permanent = json.permanent;
		RotationDisabled = json.rotation_disabled;
		IsSizeVariable = json.is_size_variable;
		ArtifactIcon = json.icon;
		Size.x = json.size[0];
		Size.y = json.size[1];
		BuildableBiomes = ((json.biomes == null || json.biomes.Length != 0) ? json.biomes : null);
		Floor = json.floor;
		MinBuildableDepth = json.depth_min;
		MaxBuildableDepth = ((!(json.depth_max > 0f)) ? 1f : json.depth_max);
		int size = KUtility.GetSize(json.components);
		int size2 = KUtility.GetSize(json.client_only_components);
		Components = new string[size + size2];
		for (int i = 0; i < Components.Length; i++)
		{
			if (i < size)
			{
				Components[i] = json.components[i];
			}
			else
			{
				Components[i] = json.client_only_components[i - size];
			}
		}
		IsEstateFlag = Array.IndexOf(Components, "Estate") != -1;
		IsClanEstateFlag = Array.IndexOf(Components, "ClanEstate") != -1;
		IsModular = Array.IndexOf(Components, "Modular") != -1;
		Exterior = json.exterior;
		Interior = json.interior;
		TransparentSite = json.transparent_site;
		Scribblable = null;
		if (json.scribble != null)
		{
			Scribblable = new Scribblable();
			Scribblable.Text = json.scribble.text;
			Scribblable.CanvasSize.x = json.scribble.canvas.width;
			Scribblable.CanvasSize.y = json.scribble.canvas.height;
			Scribblable.LimitFrame = json.scribble.canvas.frame;
		}
		BoundRadius = json.bound_radius;
	}

	public bool HasComponent(string comp)
	{
		if (Components == null)
		{
			return false;
		}
		return Array.IndexOf(Components, comp) != -1;
	}
}
