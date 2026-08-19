using Newtonsoft.Json;
using Shared.System;

namespace Yaml;

public struct TimelineCategory
{
	[JsonProperty(PropertyName = "name", Required = Required.Always)]
	public Gettext Name;

	[JsonProperty(PropertyName = "types", Required = Required.Always)]
	public TimelineEvent[] Types;
}
