using Newtonsoft.Json;

namespace Yaml;

public class Animal
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "bound_radius")]
	public float BoundRadius;

	[JsonProperty(PropertyName = "portrait")]
	public string Portrait;

	[JsonProperty(PropertyName = "model_path")]
	public string ModelPath;

	[JsonProperty(PropertyName = "tamable")]
	public bool Tamable;

	[JsonProperty(PropertyName = "life_gauge_ratio")]
	public float[] LifeGaugeRatio;

	public string PrefabPath => $"Models/Animals/{ModelPath}.prefab";
}
