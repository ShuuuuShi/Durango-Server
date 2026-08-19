using Newtonsoft.Json;

namespace Yaml;

public class BonusPrototypes
{
	[JsonProperty(PropertyName = "rate")]
	public float Rate;

	[JsonProperty(PropertyName = "prototypes")]
	public BonusPrototype[] Prototypes;
}
