using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class Dialogue
{
	[JsonProperty(PropertyName = "blur")]
	public bool Blur;

	[JsonProperty(PropertyName = "remote")]
	public bool Remote;

	[JsonProperty(PropertyName = "talks")]
	public List<MissionTalk> Talks;
}
