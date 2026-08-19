using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class Emotions
{
	[JsonProperty(PropertyName = "emoticons")]
	public Emoticon[] Emoticons;

	[JsonProperty(PropertyName = "motions")]
	public Dictionary<string, Motion> Motions;
}
