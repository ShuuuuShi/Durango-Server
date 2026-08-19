using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public struct Sailing
{
	[JsonProperty(PropertyName = "role_opening_levels", Required = Required.Always)]
	public Dictionary<int, int> RoleOpeningLevels;
}
