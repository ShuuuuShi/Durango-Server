using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Estate;

namespace Yaml;

public struct Estate
{
	[JsonProperty(PropertyName = "activation_period", Required = Required.Always)]
	public Dictionary<OwnerType, int> ActivationPeriod;
}
