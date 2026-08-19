using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class PersonalRegion
{
	[JsonProperty(PropertyName = "region_template_ids", Required = Required.Always)]
	public List<string> RegionTemplateIds;
}
