using Newtonsoft.Json;
using Shared.Season2;

namespace Yaml;

public class CommodityCondition
{
	public enum Type
	{
		Unknown,
		Level,
		Resource
	}

	[JsonProperty(PropertyName = "min_level")]
	public int? MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int? MaxLevel;

	[JsonProperty(PropertyName = "season2_resource_type")]
	public ResourceType? ResourceType;

	[JsonProperty(PropertyName = "season2_supply_level")]
	public int? WarpRushSupplyLevel;

	public Type ConditionType
	{
		get
		{
			if (MinLevel.HasValue || MaxLevel.HasValue)
			{
				return Type.Level;
			}
			if (ResourceType.HasValue)
			{
				return Type.Resource;
			}
			return Type.Unknown;
		}
	}
}
