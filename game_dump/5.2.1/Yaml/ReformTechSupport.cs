using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class ReformTechSupport
{
	[JsonProperty(PropertyName = "r_piece")]
	public int RandomNumberPiece;

	[JsonProperty(PropertyName = "tags")]
	public Dictionary<string, ReformTechSupportTag> Tags;
}
