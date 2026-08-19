using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems;

[Serializable]
public class PerlinNoiseXYZ
{
	public PerlinNoise x;

	public PerlinNoise y;

	public PerlinNoise z;

	public bool unscaledTime;

	public float amplitudeScale = 1f;

	public float frequencyScale = 1f;

	public void init()
	{
		x.init();
		y.init();
		z.init();
	}

	public Vector3 GetXYZ(float time)
	{
		float time2 = time * frequencyScale;
		return new Vector3(x.GetValue(time2), y.GetValue(time2), z.GetValue(time2)) * amplitudeScale;
	}
}
