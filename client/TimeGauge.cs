using System;
using Durango.Network;
using Durango.Utils;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class TimeGauge : MonoBehaviour
{
	private const string DefaultDateTimeName = "default";

	private static float _timeRatio;

	private static float _normalizedTime;

	private static float _timeBegin;

	private static float _timeEnd = 1f;

	[ExposedInEditor(false, null)]
	public static int TotalTimeInSec = 1200;

	[ExposedInEditor(false, null)]
	public static int SunriseBegin = 6;

	[ExposedInEditor(false, null)]
	public static int SunsetEnd = 21;

	private static readonly Action[] TimeCallbacks = new Action[24];

	private static readonly int TimeFrac = Shader.PropertyToID("_TimeFrac");

	public static bool IsSunUp { get; private set; }

	public static DateTimeYaml DateTimeYaml { get; private set; }

	public static event Action IsSunUpChanged;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InstallEvent()
	{
		GameManager.Reset += delegate
		{
			TimeGauge.IsSunUpChanged = null;
			for (int i = 0; i < TimeCallbacks.Length; i++)
			{
				TimeCallbacks[i] = null;
			}
		};
		GameManager.Started += delegate
		{
			Durango.Utils.Singleton<GameManager>.Instance().WelcomeReceived += delegate
			{
				SetDateTimeBy(GameManager.Region.TemplateId);
			};
		};
	}

	private static void SetDateTimeBy(string regionTemplateId)
	{
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(regionTemplateId);
		if (regionTemplate == null)
		{
			return;
		}
		DateTimeYaml dateTimeYaml = null;
		foreach (string tag in regionTemplate.Tags)
		{
			dateTimeYaml = SingletonDict<string, DateTimeYaml>.Instance.Get(tag);
			if (dateTimeYaml != null)
			{
				break;
			}
		}
		if (dateTimeYaml == null)
		{
			dateTimeYaml = SingletonDict<string, DateTimeYaml>.Instance.Get("default");
			if (dateTimeYaml != null)
			{
			}
		}
		SetDateTime(dateTimeYaml);
	}

	private static void SetDateTime(DateTimeYaml yaml)
	{
		DateTimeYaml = yaml;
		if (yaml.Daytime != 0)
		{
			TotalTimeInSec = yaml.Daytime;
			_timeRatio = 1f / (float)TotalTimeInSec;
		}
		if (yaml.Sunrise != null && yaml.Sunrise.Length >= 2)
		{
			SunriseBegin = yaml.Sunrise[0];
		}
		if (yaml.Sunset != null && yaml.Sunset.Length >= 2)
		{
			SunsetEnd = yaml.Sunset[1];
		}
	}

	private void Update()
	{
		int totalTimeInSec = TotalTimeInSec;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float num = (float)(predictedServerTime % (double)totalTimeInSec);
		float normalizedTime = _normalizedTime;
		_normalizedTime = num / (float)totalTimeInSec;
		_normalizedTime = _timeBegin + _normalizedTime % (_timeEnd - _timeBegin);
		Shader.SetGlobalFloat(TimeFrac, Time.time % 1f);
		CheckIsSunUp();
		CheckTimeCallbacks(normalizedTime);
	}

	private static void CheckIsSunUp()
	{
		bool flag = CheckTime(SunriseBegin, SunsetEnd);
		if (flag != IsSunUp)
		{
			IsSunUp = flag;
			if (TimeGauge.IsSunUpChanged != null)
			{
				TimeGauge.IsSunUpChanged();
			}
		}
	}

	private static void CheckTimeCallbacks(float prevNormalizedTime)
	{
		int num = (int)(prevNormalizedTime * 24f);
		int num2 = (int)(_normalizedTime * 24f);
		if (num > num2)
		{
			num2 += 24;
		}
		for (int i = num; i < num2; i++)
		{
			TimeCallbacks[i % 24]?.Invoke();
		}
	}

	public static bool CheckTime(float begin, float end)
	{
		float num = begin / 24f;
		float num2 = end / 24f;
		if (num <= num2)
		{
			return _normalizedTime >= num && _normalizedTime < num2;
		}
		return _normalizedTime >= num || _normalizedTime < num2;
	}

	public static float GetNormalizedTime()
	{
		return _normalizedTime;
	}

	public static float GetNormalizedTimeForDayNight()
	{
		int num = ((SunsetEnd <= SunriseBegin) ? 24 : 0) + SunsetEnd - SunriseBegin;
		float num2 = _normalizedTime * 24f;
		if (IsSunUp)
		{
			float num3 = ((!(num2 > (float)SunriseBegin)) ? 24f : 0f) + num2 - (float)SunriseBegin;
			return num3 / (float)num;
		}
		float num4 = ((!(num2 > (float)SunsetEnd)) ? 24f : 0f) + num2 - (float)SunsetEnd;
		return num4 / (float)(24 - num);
	}

	public static float GetRemainTimeForDayOrNight()
	{
		float num = _normalizedTime * 24f;
		float num2 = ((!IsSunUp) ? ((float)SunriseBegin - num) : ((float)SunsetEnd - num));
		if (num2 < 0f)
		{
			num2 += 24f;
		}
		return GetRealTimeFromNormalizedTime(num2 / 24f);
	}

	public static float GetNormalizedTimeFromRealTime(float realTime)
	{
		return realTime * _timeRatio;
	}

	public static float GetRealTimeFromNormalizedTime(float ingameTime)
	{
		return ingameTime / _timeRatio;
	}

	public static void SetTimeZone(float begin, float end)
	{
		float timeBegin = begin / 24f;
		float timeEnd = end / 24f;
		_timeBegin = timeBegin;
		_timeEnd = timeEnd;
	}

	public static int DaysPassedFrom(double beginningUnixTime)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double durationTime = predictedServerTime - beginningUnixTime;
		return DaysPassedWhile(durationTime);
	}

	public static int DaysPassedWhile(double durationTime)
	{
		return 1 + (int)Math.Floor(durationTime / (double)TotalTimeInSec);
	}

	public static void RegisterTimeCallback(int time, Action action)
	{
		Action[] timeCallbacks;
		int num;
		(timeCallbacks = TimeCallbacks)[num = time % 24] = (Action)Delegate.Combine(timeCallbacks[num], action);
	}
}
