using System;
using Durango.Logic.Item;
using Durango.Utils;
using Newtonsoft.Json;

namespace Yaml;

public class PrototypePreset
{
	[JsonIgnore]
	public string PrototypeId;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "max_durability")]
	public float MaxDurability;

	[JsonProperty(PropertyName = "modifiable_count")]
	public int ModifiableCount;

	[JsonProperty(PropertyName = "size")]
	public int Size;

	[JsonProperty(PropertyName = "color_r")]
	public string ColorR;

	[JsonProperty(PropertyName = "color_g")]
	public string ColorG;

	[JsonProperty(PropertyName = "color_b")]
	public string ColorB;

	[JsonProperty(PropertyName = "tags")]
	public PrototypePresetTag[] Tags;

	[JsonProperty(PropertyName = "performance")]
	public PrototypePresetPerformance[] Performances;

	[JsonProperty(PropertyName = "repair_requirement")]
	public PrototypePresetRepair RepairRequirement;

	[JsonProperty(PropertyName = "immune_to_time")]
	public bool ImmuneToTime;

	[JsonProperty(PropertyName = "trade_locked")]
	public bool TradeLocked;

	[JsonProperty(PropertyName = "dump_locked")]
	public bool DumpLocked;

	[JsonProperty(PropertyName = "emotional_motions")]
	public string[] EmotionalMotions;

	[JsonProperty(PropertyName = "ext_class_name")]
	public string ExtClassName;

	[JsonProperty(PropertyName = "ext_class_args")]
	public string[] ExtClassArgs;

	public ItemData ToItem()
	{
		ItemData itemData = new ItemData();
		itemData.Set(this);
		return itemData;
	}

	public static void Request(string prototypeId, int level, Action<PrototypePreset> response)
	{
		if (response == null || string.IsNullOrEmpty(prototypeId))
		{
			return;
		}
		string url = $"{GameManager.GatewayUrl}/prototypes/{prototypeId}/{level}";
		Http.RequestYml(url, delegate(PrototypePreset preset)
		{
			if (preset != null)
			{
				preset.PrototypeId = prototypeId;
			}
			response(preset);
		});
	}
}
