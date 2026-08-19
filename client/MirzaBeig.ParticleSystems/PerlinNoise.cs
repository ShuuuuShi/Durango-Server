using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems;

[Serializable]
public class PerlinNoise
{
	private Vector2 offset;

	public float amplitude = 1f;

	public float frequency = 1f;

	public bool unscaledTime;

	public void init()
	{
		offset.x = UnityEngine.Random.Range(-32f, 32f);
		offset.y = UnityEngine.Random.Range(-32f, 32f);
	}

	public float GetValue(float time)
	{
		float num = time * frequency;
		return (Mathf.PerlinNoise(num + offset.x, num + offset.y) - 0.5f) * amplitude;
	}
}
