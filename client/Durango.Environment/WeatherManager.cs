using System;
using System.Collections;
using System.Linq;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Render.Screen;
using Durango.Terrain;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.Environment;

public class WeatherManager : Singleton<WeatherManager>
{
	public enum Weather
	{
		Invalid = -1,
		[T.EnumName("맑음")]
		[EnumIcon("icon_se_hot")]
		Sunny,
		[T.EnumName("흐림")]
		[EnumIcon("icon_env_gale")]
		Cloudy,
		[T.EnumName("비")]
		[EnumIcon("icon_se_pvp_rain")]
		Rainy,
		[T.EnumName("폭풍우")]
		[EnumIcon("icon_se_heavyrain")]
		HeavyRainy,
		[T.EnumName("눈")]
		[EnumIcon("icon_se_cold")]
		Snowy,
		[T.EnumName("눈보라")]
		[EnumIcon("icon_se_heavysnow")]
		HeavySnowy,
		VolcanicAsh,
		VolcanicSign,
		[T.EnumName("화산폭풍")]
		VolcanicStorm,
		PVPStorm,
		PVPFog,
		ClimateSunny,
		ClimateSnowy,
		ClimateStorm
	}

	[Serializable]
	private class WeatherSound
	{
		public SoundEventType Start;

		public SoundEventType End;
	}

	[Serializable]
	private class WeatherParameter
	{
		[SerializeField]
		public WeatherSound Sound;

		[SerializeField]
		public ParticleType Particle;

		[SerializeField]
		public float Cloudiness;
	}

	[Serializable]
	private class WaterFog
	{
		public string TileSet;

		public ParticleType Particle;
	}

	public Action<string> WeatherChanged;

	private const float RainyScreenAlpha = 0.35f;

	private const float HeavyRainyScreenAlpha = 0.6f;

	[SerializeField]
	private float _weatherChangeTransitionTime = 2f;

	[EnumList(typeof(Weather), false, 0, -1)]
	[SerializeField]
	private WeatherParameter[] _weatherParameters;

	[SerializeField]
	private WaterFog[] _waterFogs;

	[SerializeField]
	private float _fadeOutDuration = 2f;

	private Weather _currentWeather;

	private string _curWeatherString;

	private int _weatherParicleId;

	private string _weatherPariclePath;

	private WeatherSound _currentWeatherSound;

	private uint _soundInstanceId;

	public Weather CurrentWeather => _currentWeather;

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
		case "volcanic_sign":
			return Weather.VolcanicSign;
		case "volcanic_storm":
			return Weather.VolcanicStorm;
		case "pvp_fog":
			return Weather.PVPFog;
		case "pvp_rain":
			return Weather.Rainy;
		case "pvp_storm_sign":
			return Weather.Rainy;
		case "pvp_storm_01":
			return Weather.PVPStorm;
		case "pvp_storm_02":
			return Weather.PVPStorm;
		case "climate_sunny":
			return Weather.ClimateSunny;
		case "climate_snowy":
			return Weather.ClimateSnowy;
		case "climate_storm":
			return Weather.ClimateStorm;
		default:
			Debug.LogError("Unknown Weather: " + weatherStr);
			return Weather.Invalid;
		}
	}

	protected override void OnAwake()
	{
		for (int i = 0; i < _weatherParameters.Length; i++)
		{
			ParticleManager.Cache(_weatherParameters[i].Particle);
		}
		Connections.Frontend.On(delegate(Messages.Weather msg, PacketHeader header)
		{
			Weather weather = GetWeatherFromString(msg._Weather);
			if (weather == Weather.Invalid)
			{
				return;
			}
			if (!(_curWeatherString == msg._Weather))
			{
				_curWeatherString = msg._Weather;
				SetWeather(weather);
				if (WeatherChanged != null)
				{
					WeatherChanged(msg._Weather);
				}
			}
		});
		WaterFog waterFog = _waterFogs.FirstOrDefault((WaterFog o) => o.TileSet == TerrainMeta.TileSet);
		if (waterFog != null)
		{
			ParticleManager.EmitFollow(waterFog.Particle, Vector3.zero, Quaternion.identity, Singleton<PlayerController>.Instance().transform);
			CloudUpdater component = GetComponent<CloudUpdater>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	public void RefreshWeather()
	{
		SetWeatherPopup();
	}

	[ExposedInEditor(null)]
	private void SetWeather(Weather weather)
	{
		_currentWeather = weather;
		WeatherParameter weatherParameter = GetWeatherParameter(weather);
		if (weatherParameter == null)
		{
			Debug.LogError("Invalid Weather Parameter - " + weather);
			return;
		}
		StartCoroutine(SetCloudiness(weatherParameter.Cloudiness));
		SetWeatherSound(weatherParameter.Sound);
		SetWeatherEffect(weatherParameter.Particle.Path);
		SetWeatherFullscreenEffect(weather);
		OnWeatherChanged(weather);
	}

	private void SetWeatherSound(WeatherSound weatherSound)
	{
		if (_currentWeatherSound != weatherSound)
		{
			if (_currentWeatherSound != null && !string.IsNullOrEmpty(_currentWeatherSound.End.Path))
			{
				SoundManager.PlayEvent(_currentWeatherSound.End, SoundPosition.Empty, exclusive: true);
			}
			_currentWeatherSound = weatherSound;
			SoundManager.StopEvent(_soundInstanceId, _fadeOutDuration);
			_soundInstanceId = 0u;
			if (_currentWeatherSound != null && !string.IsNullOrEmpty(_currentWeatherSound.Start.Path))
			{
				_soundInstanceId = SoundManager.PlayEvent(_currentWeatherSound.Start, SoundPosition.Empty, exclusive: true);
			}
		}
	}

	private void SetWeatherEffect(string path)
	{
		if (_weatherPariclePath == path)
		{
			return;
		}
		if (_weatherParicleId != 0)
		{
			Singleton<ParticleManager>.Instance().RegisterAction(_weatherParicleId, delegate(GameObject go)
			{
				go.transform.parent = Singleton<ParticleManager>.Instance().transform;
				ParticleManager.Stop(_weatherParicleId, immediately: false);
			});
			_weatherParicleId = 0;
		}
		_weatherPariclePath = path;
		if (!string.IsNullOrEmpty(_weatherPariclePath))
		{
			_weatherParicleId = ParticleManager.EmitFollow(_weatherPariclePath, Vector3.zero, Quaternion.identity, Singleton<MainCamera>.Instance().TargetTransform);
		}
	}

	private IEnumerator SetCloudiness(float targetValue)
	{
		CustomColorCorrectionEffect colorCorrection = Singleton<CustomColorCorrectionEffect>.Instance();
		float startTime = Time.time;
		float startCloudiness = colorCorrection.Cloudiness;
		while (true)
		{
			float num = (Time.time - startTime) / _weatherChangeTransitionTime;
			colorCorrection.Cloudiness = Mathf.Lerp(startCloudiness, targetValue, num);
			if (!(num >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private void SetWeatherFullscreenEffect(Weather weather)
	{
		OverlayCamera overlayCamera = Singleton<OverlayCamera>.Instance();
		if (!(overlayCamera == null))
		{
			switch (weather)
			{
			case Weather.Rainy:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.Rainy, 0.35f);
				break;
			default:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.None);
				break;
			case Weather.HeavyRainy:
				overlayCamera.SetFullscreenEffect(OverlayCamera.ScreenParticleEffect.Rainy, 0.6f);
				break;
			}
		}
	}

	private void OnWeatherChanged(Weather weather)
	{
	}

	private WeatherParameter GetWeatherParameter(Weather weather)
	{
		if (weather < Weather.Sunny || (int)weather >= _weatherParameters.Length)
		{
			return null;
		}
		return _weatherParameters[(int)weather];
	}

	private void SetWeatherPopup()
	{
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		genericSelector.SetTitle(T._("날씨 변경"));
		genericSelector.AddItem("맑음");
		genericSelector.AddItem("흐림");
		genericSelector.AddItem("비");
		genericSelector.AddItem("폭풍우");
		genericSelector.AddItem("눈");
		genericSelector.AddItem("눈보라");
		genericSelector.AddItem("화산폭풍");
		genericSelector.SetSelected(delegate(int index)
		{
			switch (index)
			{
			case 0:
				UIManager.SystemMsg(T._("날씨를 <em>맑음</em>으로 변경합니다.."));
				SetWeather(Weather.Sunny);
				break;
			case 1:
				UIManager.SystemMsg(T._("날씨를 <em>흐림</em>으로 변경합니다.."));
				SetWeather(Weather.Cloudy);
				break;
			case 2:
				UIManager.SystemMsg(T._("날씨를 <em>비</em>로 변경합니다.."));
				SetWeather(Weather.Rainy);
				break;
			case 3:
				UIManager.SystemMsg(T._("날씨를 <em>폭풍우</em>로 변경합니다.."));
				SetWeather(Weather.HeavyRainy);
				break;
			case 4:
				UIManager.SystemMsg(T._("날씨를 <em>눈</em>으로 변경합니다.."));
				SetWeather(Weather.Snowy);
				break;
			case 5:
				UIManager.SystemMsg(T._("날씨를 <em>눈보라</em>로 변경합니다.."));
				SetWeather(Weather.HeavySnowy);
				break;
			case 6:
				UIManager.SystemMsg(T._("날씨를 <em>화산폭풍</em>으로 변경합니다.."));
				SetWeather(Weather.VolcanicStorm);
				break;
			}
		});
		genericSelector.Show();
	}
}
