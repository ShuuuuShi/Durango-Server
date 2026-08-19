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
		if (Point >= 0)
		{
			return Mathf.Max(0f, (float)Point - exchagedPoint);
		}
		return 1.7014117E+38f;
	}
}
