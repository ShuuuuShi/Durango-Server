using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Ability;
using Yaml.Util;

namespace Yaml;

public class Constants : Singleton<Constants>
{
	[JsonProperty(PropertyName = "newbie_level", Required = Required.Always)]
	public int NewbieLevel;

	[JsonProperty(PropertyName = "dash", Required = Required.Always)]
	public Dash Dash;

	[JsonProperty(PropertyName = "explorer", Required = Required.Always)]
	public Explorer Explorer;

	[JsonProperty(PropertyName = "item", Required = Required.Always)]
	public ConstantsItem Item;

	[JsonProperty(PropertyName = "market", Required = Required.Always)]
	public Market Market;

	[JsonProperty(PropertyName = "max_levels", Required = Required.Always)]
	public MaxLevels MaxLevels;

	[JsonProperty(PropertyName = "natural_components", Required = Required.Always)]
	public Dictionary<int, NaturalComponentInfo> NaturalComponents;

	[JsonProperty(PropertyName = "repair", Required = Required.Always)]
	public Repair Repair;

	[JsonProperty(PropertyName = "faction", Required = Required.Always)]
	public FactionInfo Faction;

	[JsonProperty(PropertyName = "estate", Required = Required.Always)]
	public Estate Estate;

	[JsonProperty(PropertyName = "ally", Required = Required.Always)]
	public Ally Ally;

	[JsonProperty(PropertyName = "war", Required = Required.Always)]
	public War War;

	[JsonProperty(PropertyName = "build", Required = Required.Always)]
	public Build Build;

	[JsonProperty(PropertyName = "warehouse", Required = Required.Always)]
	public Warehouse Warehouse;

	[JsonProperty(PropertyName = "sailing", Required = Required.Always)]
	public Sailing Sailing;

	[JsonProperty(PropertyName = "season2", Required = Required.Always)]
	public Season2 Season2;

	[JsonProperty(PropertyName = "resistance", Required = Required.Always)]
	public Resistance Resistance;

	[JsonProperty(PropertyName = "skill_untrain", Required = Required.Always)]
	public SkillUntrain SkillUntrain;

	[JsonProperty(PropertyName = "skill", Required = Required.Always)]
	public SkillConstants Skill;

	[JsonProperty(PropertyName = "pet", Required = Required.Always)]
	public ConstantPet Pet;

	[JsonProperty(PropertyName = "attendance", Required = Required.Always)]
	public Attendance Attendance;

	[JsonProperty(PropertyName = "faction_support", Required = Required.Always)]
	public FactionSupport FactionSupport;

	[JsonProperty(PropertyName = "sprinkler", Required = Required.Always)]
	public Sprinkler Sprinkler;

	[JsonProperty(PropertyName = "crack", Required = Required.Always)]
	public Crack Crack;

	[JsonProperty(PropertyName = "personal_region", Required = Required.Always)]
	public PersonalRegion PersonalRegion;

	[JsonProperty(PropertyName = "represent_powers", Required = Required.Always)]
	public Dictionary<RepresentType, Dictionary<Derived, float>> RepresentAbilities;

	[JsonProperty(PropertyName = "skip_tutorial_missions", Required = Required.Always)]
	public Dictionary<string, SkipTutorialMissions> SkipTutorialMissionses;

	[JsonProperty(PropertyName = "durability", Required = Required.Always)]
	public Durability Durability;

	[JsonProperty(PropertyName = "taming", Required = Required.Always)]
	public Taming Taming;

	[JsonProperty(PropertyName = "mini_game_dance", Required = Required.Always)]
	public Dictionary<string, float> MiniGameDance;

	[JsonProperty(PropertyName = "warp_accelerator", Required = Required.Always)]
	public WarpAccelerator WarpAccelerator;

	[JsonProperty(PropertyName = "put_in_container_infos", Required = Required.Always)]
	public Dictionary<string, PutInContainerInfo> PutInContainerInfos;

	[JsonProperty(PropertyName = "musician", Required = Required.Always)]
	public Musician Musician;

	[JsonProperty(PropertyName = "artifact_floor", Required = Required.Always)]
	public ArtifactFloor ArtifactFloor;
}
