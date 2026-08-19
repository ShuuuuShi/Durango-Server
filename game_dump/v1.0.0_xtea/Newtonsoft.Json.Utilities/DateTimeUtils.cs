using System;
using System.Globalization;

namespace Newtonsoft.Json.Utilities;

internal static class DateTimeUtils
{
	public static string GetLocalOffset(this DateTime d)
	{
		TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(d);
		return utcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + utcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
	}
}
