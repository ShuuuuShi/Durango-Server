using Newtonsoft.Json;

namespace Yaml;

public class ShopUIOption
{
	[JsonProperty(PropertyName = "show_tradable")]
	public ItemTextCondition ShowTradable;

	[JsonProperty(PropertyName = "show_repairable")]
	public ItemTextCondition ShowRepairable;

	[JsonProperty(PropertyName = "show_dyeable")]
	public ItemTextCondition ShowDyeable;

	[JsonProperty(PropertyName = "show_dumpable")]
	public ItemTextCondition ShowDumpable;

	[JsonProperty(PropertyName = "show_avatar")]
	public ItemTextCondition ShowAvatar;
}
