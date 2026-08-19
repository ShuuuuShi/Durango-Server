using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Battle;

namespace Yaml;

public class PlayerActionMeta
{
	[JsonProperty(PropertyName = "action_length")]
	public float ActionLength;

	[JsonProperty(PropertyName = "active_condition")]
	public ActionActiveCondition ActiveCondition;

	[JsonProperty(PropertyName = "cooltime")]
	public float Cooldown;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "battle_action_type")]
	public BattleActionType BattleActionType;

	[JsonProperty(PropertyName = "hide_when_deactive")]
	public bool HideWhenDeactive;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "motion")]
	public string Motion;

	[JsonProperty(PropertyName = "playback_rate")]
	public float? PlaybackRate;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "prohibit_type")]
	public ProhibitType ProhibitType;

	[JsonProperty(PropertyName = "prohibited_time")]
	public Dictionary<ProhibitType, float> ProhibitedTime;

	[JsonProperty(PropertyName = "stamina")]
	public float Stamina;

	[JsonProperty(PropertyName = "slot")]
	public PlayerActionSlot Slot;

	[JsonProperty(PropertyName = "use_range")]
	public float UseRange;

	[JsonProperty(PropertyName = "casting_bar")]
	public Pair<float, float> CastingBar;
}
