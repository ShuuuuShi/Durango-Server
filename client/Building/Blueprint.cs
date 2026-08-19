using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Skill;
using Durango.Terrain;
using Durango.Utils.Extensions;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Building;
using Shared.Etc;
using Shared.Region;
using Yaml;

namespace Building;

public class Blueprint : CategoryItem
{
	public string SubCategory;

	public int RotatableDirections;

	public int EntityType;

	public bool Permanent;

	public string Description;

	public int PostprocessTime;

	public string[] Components;

	public string Preview;

	public string DefaultLook;

	public string ArtifactIcon;

	public bool TimeLimited;

	public bool IsShowCraftMode;

	public int MinLevel;

	public int MaxLevel;

	public string[] Musics;

	public bool InteriorSetEffect;

	public bool IsSizeVariable;

	public Point2 Size;

	public int Height;

	public Biome[] BuildableBiomes;

	public Exclusive Exclusive;

	public bool Exterior;

	public bool Interior;

	public bool TransparentSite;

	public int RepairRequirement;

	public ArtifactIndicatorData Indicator;

	public float? MinBuildableDepth;

	public float? MaxBuildableDepth;

	public BlueprintWater Water;

	public BlueprintSlot[] Slots;

	public OrTagFilter AllowedToolTag;

	public Scribblable Scribblable;

	public float BoundRadius;

	public HashSet<string> AbilityType = new HashSet<string>();

	public Derived? RequiredAbility;

	public string RequiredBlueprint;

	public List<Point2> UnoccupiableTiles;

	private bool _isValidOwnerSkill;

	private Node _ownerSkill;

	private List<Point2> _effectTiles;

	public string RemodelingParentId { get; private set; }

	public bool IsModular { get; private set; }

	public bool IsNatural { get; private set; }

	public bool IsRemodeling => RemodelingParentId != null;

	public Blueprint([NotNull] string id, [NotNull] Yaml.Blueprint yamlBlueprint, [CanBeNull] string remodelingId = null, [CanBeNull] RemodelingBlueprint yamlRemodeling = null)
	{
		if (yamlRemodeling == null)
		{
			Id = id;
			Name = yamlBlueprint.name;
			Icon = yamlBlueprint.icon;
			Description = yamlBlueprint.description;
			MinLevel = yamlBlueprint.min_level;
			MaxLevel = yamlBlueprint.max_level;
			PostprocessTime = yamlBlueprint.postprocess_time;
			SetSlots(yamlBlueprint.slots, yamlBlueprint.tool_tags);
			RemodelingParentId = null;
		}
		else
		{
			Id = remodelingId;
			Name = yamlRemodeling.Name;
			Icon = IconMap.Get(Interaction.RemodelArtifact);
			Description = yamlRemodeling.Description;
			MinLevel = yamlRemodeling.MinLevel;
			MaxLevel = yamlRemodeling.MaxLevel;
			PostprocessTime = yamlRemodeling.PostprocessTime;
			SetSlots(yamlRemodeling.Slots, yamlRemodeling.ToolTags);
			RemodelingParentId = id;
			base.Available = true;
		}
		Category = yamlBlueprint.category;
		SubCategory = yamlBlueprint.subcategory;
		Preview = yamlBlueprint.preview;
		Season = yamlBlueprint.season;
		RequiredAbility = yamlBlueprint.required_ability;
		RequiredBlueprint = yamlBlueprint.required_blueprint;
		DefaultLook = yamlBlueprint.default_look;
	}

	public Blueprint(BiomeSpriteInfo natural)
	{
		Id = $"natural_{natural.EntityType}";
		Name = natural.Name;
		Icon = natural.Icon;
		Description = ((KUtility.GetSize(natural.Survivability) <= 0) ? string.Empty : T._("{0:l:{}|, }", natural.Survivability.Select((Biome n) => n.GetName()).ToList()));
		SetSlots(null, null);
		RemodelingParentId = null;
		Category = "natural";
		SubCategory = (natural.EntityType / 10 * 10).ToString();
		EntityType = natural.EntityType;
		ArtifactIcon = natural.Icon;
		IsShowCraftMode = true;
		MaxBuildableDepth = 1f;
		Size.x = 1;
		Size.y = 1;
		Height = 1;
		Components = new string[0];
		IsModular = false;
		Exterior = true;
		Interior = false;
		TransparentSite = true;
		IsNatural = true;
	}

	public Blueprint()
	{
	}

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

	private void SetSlots(IList<Yaml.BlueprintSlot> slots, Dictionary<string, int> toolTags)
	{
		int size = KUtility.GetSize(slots);
		Slots = new BlueprintSlot[size];
		for (int i = 0; i < size; i++)
		{
			Slots[i] = new BlueprintSlot(slots[i]);
		}
		AllowedToolTag = new OrTagFilter(toolTags);
	}

	public void SetPrototypeInfo(int entityType, ArtifactPrototype json)
	{
		EntityType = entityType;
		Permanent = json.permanent;
		RotatableDirections = json.rotatable_directions;
		InteriorSetEffect = json.interior_set_effect;
		IsSizeVariable = json.is_size_variable;
		ArtifactIcon = json.icon;
		TimeLimited = json.time_limited;
		IsShowCraftMode = json.is_craft;
		Musics = json.musics;
		Size.x = json.size[0];
		Size.y = json.size[1];
		Height = json.height;
		BuildableBiomes = ((json.biomes == null || json.biomes.Length != 0) ? json.biomes : null);
		MinBuildableDepth = json.depth_min;
		MaxBuildableDepth = json.depth_max;
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
		IsModular = Array.IndexOf(Components, "Modular") != -1;
		Exclusive = json.exclusive;
		Exterior = json.exterior;
		Interior = json.interior;
		TransparentSite = json.transparent_site;
		RepairRequirement = json.repair_requirement;
		if (json.indicator != null)
		{
			Indicator = new ArtifactIndicatorData();
			Indicator.Active = json.indicator.active;
			Indicator.Icon = json.indicator.icon;
			Indicator.Size = json.indicator.size;
			Indicator.VisibleZoom = json.indicator.visible_zoom;
			Indicator.Color = json.indicator.color.ToColor();
		}
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
		if (json.unoccupiable_tiles != null)
		{
			UnoccupiableTiles = new List<Point2>();
			int[][] unoccupiable_tiles = json.unoccupiable_tiles;
			foreach (int[] array in unoccupiable_tiles)
			{
				UnoccupiableTiles.Add(new Point2(array[0], array[1]));
			}
		}
		else
		{
			UnoccupiableTiles = null;
		}
		if (json.effect_tiles != null)
		{
			_effectTiles = new List<Point2>();
			int[][] effect_tiles = json.effect_tiles;
			foreach (int[] array2 in effect_tiles)
			{
				_effectTiles.Add(new Point2(array2[0], array2[1]));
			}
		}
		else
		{
			_effectTiles = null;
		}
	}

	public bool HasRequiredBlueprint()
	{
		if (string.IsNullOrEmpty(RequiredBlueprint))
		{
			return true;
		}
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(RequiredBlueprint);
		if (blueprint == null)
		{
			return false;
		}
		return blueprint.Available && blueprint.HasRequiredBlueprint();
	}

	public bool HasComponent(string comp)
	{
		if (Components == null)
		{
			return false;
		}
		return Array.IndexOf(Components, comp) != -1;
	}

	public Node GetOwnerSkill()
	{
		if (_isValidOwnerSkill)
		{
			return _ownerSkill;
		}
		_isValidOwnerSkill = true;
		_ownerSkill = GameSystem<SkillSystem>.Instance().FindSkill(delegate(Node s)
		{
			int i = 0;
			for (int size = KUtility.GetSize(s.Rewards); i < size; i++)
			{
				Durango.Logic.Skill.Reward reward = s.Rewards[i];
				if (reward.BlueprintIds != null && reward.BlueprintIds.Contains(Id))
				{
					return true;
				}
			}
			return false;
		});
		return _ownerSkill;
	}

	public IEnumerable<Point2> GetEffectTiles(Rotation rotation)
	{
		if (_effectTiles == null)
		{
			return null;
		}
		Point2 xDir;
		Point2 yDir;
		Point2 pivot;
		switch (rotation)
		{
		case Rotation.None:
			return _effectTiles;
		case Rotation.Quarter:
			xDir = new Point2(0, 1);
			yDir = new Point2(-1, 0);
			pivot = new Point2(Size.y - 1, 0);
			break;
		case Rotation.Half:
			xDir = new Point2(-1, 0);
			yDir = new Point2(0, -1);
			pivot = new Point2(Size.x - 1, Size.y - 1);
			break;
		case Rotation.ThreeQuarter:
			xDir = new Point2(0, -1);
			yDir = new Point2(1, 0);
			pivot = new Point2(0, Size.x - 1);
			break;
		default:
			throw new ArgumentOutOfRangeException("rotation", rotation, null);
		}
		return _effectTiles.Select((Point2 o) => o.x * xDir + o.y * yDir + pivot);
	}

	public ArtifactDisplay GetDefaultDisplay()
	{
		ArtifactDisplay artifactDisplay = default(ArtifactDisplay);
		artifactDisplay.Condition = Shared.Building.Condition.Normal;
		ArtifactDisplay result = artifactDisplay;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!string.IsNullOrEmpty(DefaultLook))
		{
			dictionary.Add("common", DefaultLook);
		}
		BlueprintSlot[] slots = Slots;
		foreach (BlueprintSlot blueprintSlot in slots)
		{
			if (blueprintSlot.HasLook)
			{
				string value = null;
				if (!string.IsNullOrEmpty(blueprintSlot.DefaultLook) && blueprintSlot.Looks.TryGetValue(blueprintSlot.DefaultLook, out var value2))
				{
					value = value2.model_key;
				}
				if (string.IsNullOrEmpty(value))
				{
					value = blueprintSlot.Looks.First().Value.model_key;
				}
				dictionary.Add(blueprintSlot.Id, value);
			}
		}
		result.Parts = dictionary;
		if (IsModular)
		{
			result.AddOns = new Dictionary<int, Pair<string, string>> { 
			{
				0,
				new Pair<string, string>(null, "door")
			} };
		}
		return result;
	}
}
