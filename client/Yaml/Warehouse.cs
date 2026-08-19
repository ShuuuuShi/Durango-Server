using Newtonsoft.Json;

namespace Yaml;

public struct Warehouse
{
	[JsonProperty(PropertyName = "section_size", Required = Required.Always)]
	public int SectionSize;
}
