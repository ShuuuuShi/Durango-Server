using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Item;

namespace Yaml;

public struct ConstantsItem
{
	[JsonProperty(PropertyName = "dye_recipe", Required = Required.Always)]
	public Dictionary<ColorChannel, string> DyeRecipe;

	[JsonProperty(PropertyName = "bleach_recipe", Required = Required.Always)]
	public Dictionary<ColorChannel, string> BleachRecipe;
}
