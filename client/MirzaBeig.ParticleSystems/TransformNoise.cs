using UnityEngine;

namespace MirzaBeig.ParticleSystems;

public class TransformNoise : MonoBehaviour
{
	public PerlinNoiseXYZ positionNoise;

	public PerlinNoiseXYZ rotationNoise;

	public bool unscaledTime;

	private float time;

	private void Start()
	{
		positionNoise.init();
		rotationNoise.init();
	}

	private void Update()
	{
		time = (unscaledTime ? Time.unscaledTime : Time.time);
		base.transform.localPosition = positionNoise.GetXYZ(time);
		base.transform.localEulerAngles = rotationNoise.GetXYZ(time);
	}
}
