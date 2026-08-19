using Newtonsoft.Json;
using UnityEngine;

namespace Yaml;

public class PioneerRate
{
	[JsonProperty(PropertyName = "paid")]
	public bool Paid;

	[JsonProperty(PropertyName = "point")]
	public int Point;

	[JsonProperty(PropertyName = "rate")]
	public float Rate;

	public float GetRemainPoint(float exchagedPoint)
	{
		return (Point < 0) ? 1.7014117E+38f : Mathf.Max(0f, (float)Point - exchagedPoint);
	}
}
