using UnityEngine;
using Yaml;

public class TimeGauge : MonoBehaviour
{
	private static float _timeRatio;

	private static float _normalizedTime;

	private static float _timeBegin;

	private static float _timeEnd = 1f;

	[ExposedInEditor(false, null)]
	private static int _totalTimeInSec = 1200;

	[ExposedInEditor(false, null)]
	private static int _dayBegin = 6;

	[ExposedInEditor(false, null)]
	private static int _nightBegin = 21;

	public static DateTimeYaml DateTimeYaml { get; private set; }

	public static void Initialize(DateTimeYaml yaml)
	{
		DateTimeYaml = yaml;
		if (yaml.daytime != 0)
		{
			_totalTimeInSec = yaml.daytime;
		}
		if (yaml.sunrise != null && yaml.sunrise.Length >= 2)
		{
			_dayBegin = yaml.sunrise[0];
		}
		if (yaml.sunset != null && yaml.sunset.Length >= 2)
		{
			_nightBegin = yaml.sunset[1];
		}
	}

	private void Update()
	{
		int totalTimeInSec = _totalTimeInSec;
		_timeRatio = 1f / (float)totalTimeInSec;
		bool flag = CheckTime(_dayBegin, _nightBegin);
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		float num = (float)(bufferedServerTime % (double)totalTimeInSec);
		_normalizedTime = num / (float)totalTimeInSec;
		_normalizedTime = _timeBegin + _normalizedTime % (_timeEnd - _timeBegin);
	}

	public static bool IsDay()
	{
		return CheckTime(_dayBegin, _nightBegin);
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
}
