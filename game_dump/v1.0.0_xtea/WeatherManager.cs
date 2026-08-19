using System;
using System.Collections;
using K1Network;
using Messages;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
	public enum Weather
	{
		Invalid = -1,
		Sunny,
		Cloudy,
		Rainy,
		HeavyRainy,
		Snowy,
		HeavySnowy,
		VolcanicAsh
	}

	[Serializable]
	private class WeatherParameter
	{
		[SerializeField]
		public AudioClip Sound;

		[SerializeField]
		public ParticleType Particle;

		[SerializeField]
		public float Cloudiness;
	}

	private const float RainyScreenAlpha = 0.35f;

	private const float HeavyRainyScreenAlpha = 0.6f;

	[SerializeField]
	private GameObject _audioTemplete;

	[SerializeField]
	private float _weatherChangeTransitionTime = 2f;

	[SerializeField]
	private WeatherParameter _sunny;

	[SerializeField]
	private WeatherParameter _cloudy;

	[SerializeField]
	private WeatherParameter _rainy;

	[SerializeField]
	private WeatherParameter _heavyRainy;

	[SerializeField]
	private WeatherParameter _snowy;

	[SerializeField]
	private WeatherParameter _heavySnowy;

	[SerializeField]
	private WeatherParameter _volcanicAsh;

	private WeatherParameter[] _weatherParameters;

	private Weather _curWeather;

	private GameObject _weatherParicle;

	private string _weatherPariclePath;

	private AudioSource _audioSource;

	private static Weather GetWeatherFromString(string weatherStr)
	{
		switch (weatherStr)
		{
		case "sunny":
			return Weather.Sunny;
		case "cloudy":
			return Weather.Cloudy;
		case "rainy":
			return Weather.Rainy;
		case "heavy_rainy":
			return Weather.HeavyRainy;
		case "snowy":
			return Weather.Snowy;
		case "heavy_snowy":
			return Weather.HeavySnowy;
		case "volcanic_ash":
			return Weather.VolcanicAsh;
		default:
			Debug.LogError((object)("Unknown Weather: " + weatherStr));
			return Weather.Invalid;
		}
	}

	private void Awake()
	{
		_weatherParameters = new WeatherParameter[7] { _sunny, _cloudy, _rainy, _heavyRainy, _snowy, _heavySnowy, _volcanicAsh };
		for (int i = 0; i < _weatherParameters.Length; i++)
		{
			ParticleManager.Cache(_weatherParameters[i].Particle);
		}
		Connections.Frontend.On(delegate(Messages.Weather msg, PacketHeader header)
		{
			SetWeather(msg._Weather);
		});
		_audioSource = ((Component)this).gameObject.AddChild(_audioTemplete).GetComponent<AudioSource>();
		_audioSource.loop = true;
		_audioSource.volume = 1f;
	}

	public void RefreshWeather()
	{
		SetWeather(_curWeather);
	}

	private void SetWeather(string weatherStr)
	{
		Weather weatherFromString = GetWeatherFromString(weatherStr);
		if (weatherFromString == Weather.Invalid)
		{
			Debug.LogError((object)("Invalid Weather string: " + weatherStr));
		}
		else if (weatherFromString != _curWeather)
		{
			SetWeather(weatherFromString);
		}
	}

	private void SetWeather(Weather weather)
	{
		_curWeather = weather;
		WeatherParameter weatherParameter = GetWeatherParameter(weather);
		if (weatherParameter == null)
		{
			Debug.LogError((object)("Invalid Weather Parameter - " + weather));
			return;
		}
		((MonoBehaviour)this).StartCoroutine(SetCloudiness(weatherParameter.Cloudiness));
		SetWeatherSound(weatherParameter.Sound);
		SetWeatherEffect(weatherParameter.Particle.Path);
		SetWeatherFullscreenEffect(weather);
		OnWeatherChanged(weather);
	}

	private void SetWeatherSound(AudioClip weatherAudioClip)
	{
		if ((Object)(object)_audioSource != (Object)(object)weatherAudioClip)
		{
			_audioSource.clip = weatherAudioClip;
			((MonoBehaviour)this).StartCoroutine(CoFadeWeatherSoundVolume((!((Object)(object)weatherAudioClip == (Object)null)) ? 1f : 0f, 2f));
		}
	}

	private IEnumerator CoFadeWeatherSoundVolume(float targetVolume, float transitionTime)
	{
		float startTime = Time.realtimeSinceStartup;
		float startVolume = _audioSource.volume;
		if (!((Behaviour)_audioSource).enabled)
		{
			_audioSource.volume = 0f;
			startVolume = 0f;
		}
		if (!_audioSource.isPlaying)
		{
			((Behaviour)_audioSource).enabled = true;
			_audioSource.Play();
		}
		yield return null;
		while (true)
		{
			float curTime = Time.realtimeSinceStartup;
			float elapsed = curTime - startTime;
			if (elapsed >= transitionTime)
			{
				break;
			}
			float volume = startVolume + (targetVolume - startVolume) * elapsed / transitionTime;
			_audioSource.volume = volume;
			yield return null;
		}
		_audioSource.volume = targetVolume;
		if (_audioSource.volume <= 0f)
		{
			((Behaviour)_audioSource).enabled = false;
		}
	}

	private void SetWeatherEffect(string path)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!(_weatherPariclePath == path))
		{
			if ((Object)(object)_weatherParicle != (Object)null)
			{
				_weatherParicle.transform.parent = ((Component)KSingleton<ParticleManager>.Instance()).transform;
				ParticleManager.Stop(_weatherParicle, immediately: false);
				_weatherParicle = null;
			}
			_weatherPariclePath = path;
			if (!string.IsNullOrEmpty(_weatherPariclePath))
			{
				_weatherParicle = ParticleManager.EmitSync(_weatherPariclePath, Vector3.zero, Quaternion.identity, KSingleton<MainCamera>.Instance().TargetTransform);
			}
		}
	}

	private IEnumerator SetCloudiness(float targetValue)
	{
		CustomColorCorrectionEffect colorCorrection = KSingleton<CustomColorCorrectionEffect>.Instance();
		float startTime = Time.realtimeSinceStartup;
		float curTime = startTime;
		float startCloudiness = colorCorrection.Cloudiness;
		while (startTime + _weatherChangeTransitionTime > curTime)
		{
			float elapsed = curTime - startTime;
			colorCorrection.Cloudiness = startCloudiness + (targetValue - startCloudiness) * elapsed / _weatherChangeTransitionTime;
			curTime = Time.realtimeSinceStartup;
			yield return null;
		}
		colorCorrection.Cloudiness = targetValue;
	}

	private void SetWeatherFullscreenEffect(Weather weather)
	{
		OverlayCamera overlayCamera = KSingleton<OverlayCamera>.Instance();
		if (!((Object)(object)overlayCamera == (Object)null))
		{
			switch (weather)
			{
			case Weather.Rainy:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.Rainy, 0.35f);
				break;
			case Weather.HeavyRainy:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.Rainy, 0.6f);
				break;
			default:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.None);
				break;
			}
		}
	}

	private void OnWeatherChanged(Weather weather)
	{
		string key = $"#weather_changed_{weather.ToString().ToLower()}";
		if (LocalizeSystem.Has(key))
		{
			string text = LocalizeSystem.Get(key);
			if (!string.IsNullOrEmpty(text))
			{
				UIManager.SystemMsg(text);
			}
		}
	}

	private WeatherParameter GetWeatherParameter(Weather weather)
	{
		if (weather < Weather.Sunny || (int)weather >= _weatherParameters.Length)
		{
			return null;
		}
		return _weatherParameters[(int)weather];
	}
}
