using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using L10N;
using Newtonsoft.Json;

namespace Durango.Logic.Clusters;

public class Maintenance
{
	private const string HHmm = "HH:mm";

	private const string YyyyMMddHHmm = "yyyy/MM/dd HH:mm";

	private readonly Dictionary<string, string> _names;

	private readonly DateTime _localStartTime;

	private readonly DateTime _locaEndTime;

	private readonly bool _hasValidTime;

	[JsonConstructor]
	public Maintenance(Dictionary<string, string> name, string utc_start, string utc_end)
	{
		_names = name;
		_hasValidTime = false;
		if (DateTime.TryParseExact(utc_start, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result) && DateTime.TryParseExact(utc_end, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result2))
		{
			_hasValidTime = true;
			_localStartTime = result.ToLocalTime();
			_locaEndTime = result2.ToLocalTime();
		}
	}

	public bool IsInMaintenance()
	{
		DateTime now = DateTime.Now;
		if (_hasValidTime && _localStartTime <= now)
		{
			return now < _locaEndTime;
		}
		return false;
	}

	public string GetMaintenanceText([CanBeNull] string locale, bool em)
	{
		return string.Format((!em) ? "{0}  {1}" : "{0}  <em>{1}</em>", GetName(locale), GetPeriodText());
	}

	private string GetName([CanBeNull] string locale)
	{
		if (_names == null)
		{
			return string.Empty;
		}
		if (string.IsNullOrEmpty(locale) || !_names.ContainsKey(locale))
		{
			locale = "en_US";
		}
		return _names.Get(locale);
	}

	private string GetPeriodText()
	{
		if (!_hasValidTime)
		{
			return string.Empty;
		}
		bool day = _locaEndTime.Day != _localStartTime.Day;
		return GetTimeString(_localStartTime, day) + " - " + GetTimeString(_locaEndTime, day);
	}

	private static string GetTimeString(DateTime time, bool day)
	{
		if (day)
		{
			return string.Format("{0} {1}", time.ToString("m", T.Culture), time.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo));
		}
		return time.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);
	}
}
