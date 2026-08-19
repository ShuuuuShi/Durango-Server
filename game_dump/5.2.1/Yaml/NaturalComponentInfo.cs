using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.System;

namespace Yaml;

public class NaturalComponentInfo
{
	[JsonProperty(PropertyName = "components", Required = Required.Always)]
	public List<string> Components;

	[JsonProperty(PropertyName = "poi_type", Required = Required.Always)]
	public PointOfInterest POIType;
}
