using System;
using Durango.Network;
using L10N;
using UnityEngine;

namespace Durango.Utils;

public static class Times
{
	public interface IServerTimeProvider
	{
		double GetOffset();

		double GetServerTime();
	}

	private class DefaultServerTimeProvider : IServerTimeProvider
	{
		double IServerTimeProvider.GetOffset()
		{
			return OptionSystem.GetTimezoneOffset();
		}

		public double GetServerTime()
		{
			return Connections.Frontend.GetPredictedServerTime();
		}
	}

	private static IServerTimeProvider _serverTimeProvider = new DefaultServerTimeProvider();

	private static DateTime UnixTimeBegin => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static DateTimeOffset UnixTimeToServerTime(double unixTime)
	{
		DateTimeOffset dateTimeOffset = new DateTimeOffset(UnixTimeToDateTimeUtc(unixTime).Ticks, TimeSpan.Zero);
		TimeSpan offset = TimeSpan.FromHours(_serverTimeProvider.GetOffset());
		return dateTimeOffset.ToOffset(offset);
	}

	public static DateTime UnixTimeToDateTimeLocal(double unixTime)
	{
		return UnixTimeToDateTimeUtc(unixTime).ToLocalTime();
	}

	public static DateTime UnixTimeToDateTimeUtc(double unixTime)
	{
		long num = (long)(unixTime * 10000000.0);
		return new DateTime(UnixTimeBegin.Ticks + num, DateTimeKind.Utc);
	}

	public static float UnixTimeToUnityTime(double serverTime)
	{
		return Time.time + (float)(serverTime - _serverTimeProvider.GetServerTime());
	}

	public static double UnityTimeToUnixTime(float unityTime)
	{
		return _serverTimeProvider.GetServerTime() + (double)(unityTime - Time.time);
	}

	public static double UnixTimeNow()
	{
		return (DateTime.UtcNow - UnixTimeBegin).TotalSeconds;
	}

	public static double ToUnixTime(this DateTime targetTime)
	{
		return (targetTime.ToUniversalTime() - UnixTimeBegin).TotalSeconds;
	}

	public static double ToUnixTime(this DateTimeOffset targetTime)
	{
		return (targetTime.ToUniversalTime() - UnixTimeBegin).TotalSeconds;
	}

	public static string GetDateString(double since, double until, string timeFormat = "{0:m}", bool useClientTime = false)
	{
		bool flag = since > 0.0;
		bool flag2 = until > 0.0;
		string text = string.Empty;
		string text2 = string.Empty;
		if (flag)
		{
			DateTime dateTime = ((!useClientTime) ? UnixTimeToServerTime(since).DateTime : UnixTimeToDateTimeLocal(since));
			text = string.Format(T.Culture, timeFormat, dateTime);
		}
		if (flag2)
		{
			DateTime dateTime2 = ((!useClientTime) ? UnixTimeToServerTime(until).DateTime : UnixTimeToDateTimeLocal(until));
			text2 = string.Format(T.Culture, timeFormat, dateTime2);
		}
		if (flag && flag2)
		{
			return text + " - " + text2;
		}
		if (flag)
		{
			return text + " -";
		}
		if (flag2)
		{
			return "- " + text2;
		}
		return string.Empty;
	}

	public static string GetRemainTime(double until, int scope = 2, string granularity = "sec")
	{
		return TimedeltaFormatter.Format(Math.Max(until - Connections.Frontend.GetPredictedServerTime(), 0.0), scope, granularity);
	}

	public static string Timeago(double time)
	{
		int num = (int)(Connections.Frontend.GetPredictedServerTime() - time);
		if (num < 60)
		{
			return T._("방금");
		}
		return T._("{0} 전", TimedeltaFormatter.Format(num, 2, "min"));
	}

	public static double ParseDateTimeToUnixTime(string dateTime, double defaultValue = 0.0)
	{
		if (!string.IsNullOrEmpty(dateTime) && TryParse(dateTime, out var result))
		{
			return result.ToUnixTime();
		}
		return defaultValue;
	}

	public static bool TryParse(string at, out DateTimeOffset result)
	{
		if (DateTime.TryParse(at, out var result2))
		{
			if (result2.Kind == DateTimeKind.Unspecified)
			{
				TimeSpan offset = TimeSpan.FromHours(_serverTimeProvider.GetOffset());
				result = new DateTimeOffset(result2, offset);
			}
			else
			{
				result = new DateTimeOffset(result2);
			}
			return true;
		}
		result = DateTimeOffset.MinValue;
		return false;
	}

	public static void InstallServerTimeProvider(IServerTimeProvider provider)
	{
		_serverTimeProvider = provider;
	}
}
