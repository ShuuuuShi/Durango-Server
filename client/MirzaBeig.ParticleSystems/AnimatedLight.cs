using UnityEngine;

namespace MirzaBeig.ParticleSystems;

[RequireComponent(typeof(Light))]
public class AnimatedLight : MonoBehaviour
{
	private Light _light;

	public float duration = 1f;

	private bool evaluating = true;

	public Gradient colourOverLifetime;

	public AnimationCurve intensityOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	public bool loop = true;

	public bool autoDestruct;

	private Color startColour;

	private float startIntensity;

	public float time { get; set; }

	private void Awake()
	{
		_light = GetComponent<Light>();
	}

	private void Start()
	{
		startColour = _light.color;
		startIntensity = _light.intensity;
		_light.color = startColour * colourOverLifetime.Evaluate(0f);
		_light.intensity = startIntensity * intensityOverLifetime.Evaluate(0f);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		_light.color = startColour;
		_light.intensity = startIntensity;
		time = 0f;
		evaluating = true;
		_light.color = startColour * colourOverLifetime.Evaluate(0f);
		_light.intensity = startIntensity * intensityOverLifetime.Evaluate(0f);
	}

	private void Update()
	{
		if (!evaluating)
		{
			return;
		}
		if (time < duration)
		{
			time += Time.deltaTime;
			if (time > duration)
			{
				if (autoDestruct)
				{
					Object.Destroy(base.gameObject);
				}
				else if (loop)
				{
					time = 0f;
				}
				else
				{
					time = duration;
					evaluating = false;
				}
			}
		}
		if (time <= duration)
		{
			float num = time / duration;
			_light.color = startColour * colourOverLifetime.Evaluate(num);
			_light.intensity = startIntensity * intensityOverLifetime.Evaluate(num);
		}
	}
}
