using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Durango.AssetTest_PC;

public class TestLightHandler : MonoBehaviour
{
	public Light mainLight;

	public Light subLight;

	public Gradient mainLightGradient;

	public Gradient subLightGradient;

	public Gradient pointsLightGradient;

	[SerializeField]
	[Range(0f, 50f)]
	private float _rotationSpeed = 5f;

	[SerializeField]
	private AnimationCurve _lightCurveX;

	[SerializeField]
	private AnimationCurve _lightCurveY;

	[SerializeField]
	private float _dayAngleOffset;

	[SerializeField]
	private BloomData _bloomModifier = new BloomData
	{
		Default = new SimpleBloom
		{
			Intensity = 2f,
			Threshold = 1.3f
		},
		Modified = new SimpleBloom
		{
			Intensity = 3f,
			Threshold = 1.8f
		},
		StartModifyingTime = 0.5f,
		ModifyingTimeLength = 0.05f
	};

	private List<Light> _pointLights = new List<Light>();

	private PostProcessProfile _ppProfile;

	private bool _isAutoPlaying = true;

	public float Angle { get; set; }

	public bool IsAutoPlaying
	{
		get
		{
			return _isAutoPlaying;
		}
		set
		{
			_isAutoPlaying = value;
		}
	}

	public float DayNightFactor { get; private set; }

	public TestLightHandler()
	{
		DayNightFactor = 0f;
	}

	private void Start()
	{
		Light[] array = Resources.FindObjectsOfTypeAll<Light>();
		foreach (Light light in array)
		{
			if (light.type == LightType.Point)
			{
				_pointLights.Add(light);
				light.gameObject.SetActive(value: true);
			}
		}
		PostProcessVolume postProcessVolume = UnityEngine.Object.FindObjectOfType<PostProcessVolume>();
		if (postProcessVolume != null)
		{
			_ppProfile = postProcessVolume.profile;
		}
	}

	private void Update()
	{
		if (!(mainLight != null))
		{
			return;
		}
		if (_isAutoPlaying)
		{
			float num = Time.deltaTime * _rotationSpeed;
			Angle += num;
		}
		while (Angle >= 360f)
		{
			Angle -= 360f;
		}
		float time = Angle / 360f;
		DayNightFactor = Mathf.Clamp((Mathf.Sin((Angle + _dayAngleOffset) * ((float)Math.PI / 180f)) + 1f) * 0.5f, 0f, 1f);
		if (_lightCurveX != null && _lightCurveY != null)
		{
			mainLight.transform.localRotation = Quaternion.Euler(new Vector3(_lightCurveX.Evaluate(time) * 360f, _lightCurveY.Evaluate(time) * 360f, 0f));
		}
		else
		{
			mainLight.transform.localRotation = Quaternion.Euler(new Vector3(Angle, Angle - 30f, 0f));
		}
		subLight.transform.localRotation = Quaternion.Euler(new Vector3(Angle + 120f, Angle + 100f, 0f));
		mainLight.color = mainLightGradient.Evaluate(time);
		subLight.color = subLightGradient.Evaluate(time);
		foreach (Light pointLight in _pointLights)
		{
			pointLight.color = pointsLightGradient.Evaluate(time);
		}
		if (!(_ppProfile != null) || !_ppProfile.TryGetSettings<Bloom>(out var outSetting) || !(_bloomModifier.ModifyingTimeLength > 0f))
		{
			return;
		}
		if (DayNightFactor >= _bloomModifier.StartModifyingTime && DayNightFactor <= _bloomModifier.StartModifyingTime + _bloomModifier.ModifyingTimeLength * 2f)
		{
			float num2 = DayNightFactor - _bloomModifier.StartModifyingTime;
			num2 /= _bloomModifier.ModifyingTimeLength;
			if (num2 > 1f)
			{
				num2 = 2f - num2;
			}
			SimpleBloom simpleBloom = SimpleBloom.Lerp(_bloomModifier.Default, _bloomModifier.Modified, num2);
			outSetting.intensity.Override(simpleBloom.Intensity);
			outSetting.threshold.Override(simpleBloom.Threshold);
		}
		else
		{
			outSetting.intensity.Override(_bloomModifier.Default.Intensity);
			outSetting.threshold.Override(_bloomModifier.Default.Threshold);
		}
	}
}
