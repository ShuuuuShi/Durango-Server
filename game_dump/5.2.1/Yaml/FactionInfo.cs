using Newtonsoft.Json;

namespace Yaml;

public struct FactionInfo
{
	public struct MissionData
	{
		public struct ShuffleData
		{
			[JsonProperty(PropertyName = "max_count", Required = Required.Always)]
			public int MaxCount;

			[JsonProperty(PropertyName = "recharge_cooltime", Required = Required.Always)]
			public double RechargeCooltime;
		}

		[JsonProperty(PropertyName = "activation_level", Required = Required.Always)]
		public int ActivationLevel;

		[JsonProperty(PropertyName = "shuffle", Required = Required.Always)]
		public ShuffleData Shuffle;
	}

	[JsonProperty(PropertyName = "mission", Required = Required.Always)]
	public MissionData Mission;
}
