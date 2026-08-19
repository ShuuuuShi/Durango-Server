using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartFormat.Utilities;

public static class TimeSpanUtility
{
	public static TimeSpanFormatOptions DefaultFormatOptions { get; set; }

	public static TimeSpanFormatOptions AbsoluteDefaults { get; private set; }

	static TimeSpanUtility()
	{
		DefaultFormatOptions = TimeSpanFormatOptions.AbbreviateOff | TimeSpanFormatOptions.LessThan | TimeSpanFormatOptions.TruncateAuto | TimeSpanFormatOptions.RangeSeconds | TimeSpanFormatOptions.RangeDays;
		AbsoluteDefaults = DefaultFormatOptions;
	}

	public static string ToTimeString(this TimeSpan FromTime, TimeSpanFormatOptions options, TimeTextInfo timeTextInfo)
	{
		options = options.Merge(DefaultFormatOptions).Merge(AbsoluteDefaults);
		TimeSpanFormatOptions timeSpanFormatOptions = options.Mask(TimeSpanFormatOptions._Range).AllFlags().Last();
		TimeSpanFormatOptions timeSpanFormatOptions2 = options.Mask(TimeSpanFormatOptions._Range).AllFlags().First();
		TimeSpanFormatOptions timeSpanFormatOptions3 = options.Mask(TimeSpanFormatOptions._Truncate).AllFlags().First();
		bool flag = options.Mask(TimeSpanFormatOptions._LessThan) != TimeSpanFormatOptions.LessThanOff;
		bool abbr = options.Mask(TimeSpanFormatOptions._Abbreviate) != TimeSpanFormatOptions.AbbreviateOff;
		Func<double, double> func = ((!flag) ? new Func<double, double>(Math.Ceiling) : new Func<double, double>(Math.Floor));
		switch (timeSpanFormatOptions2)
		{
		case TimeSpanFormatOptions.RangeWeeks:
			FromTime = TimeSpan.FromDays(func(FromTime.TotalDays / 7.0) * 7.0);
			break;
		case TimeSpanFormatOptions.RangeDays:
			FromTime = TimeSpan.FromDays(func(FromTime.TotalDays));
			break;
		case TimeSpanFormatOptions.RangeHours:
			FromTime = TimeSpan.FromHours(func(FromTime.TotalHours));
			break;
		case TimeSpanFormatOptions.RangeMinutes:
			FromTime = TimeSpan.FromMinutes(func(FromTime.TotalMinutes));
			break;
		case TimeSpanFormatOptions.RangeSeconds:
			FromTime = TimeSpan.FromSeconds(func(FromTime.TotalSeconds));
			break;
		case TimeSpanFormatOptions.RangeMilliSeconds:
			FromTime = TimeSpan.FromMilliseconds(func(FromTime.TotalMilliseconds));
			break;
		}
		bool flag2 = false;
		StringBuilder stringBuilder = new StringBuilder();
		for (TimeSpanFormatOptions timeSpanFormatOptions4 = timeSpanFormatOptions; timeSpanFormatOptions4 >= timeSpanFormatOptions2; timeSpanFormatOptions4 = (TimeSpanFormatOptions)((int)timeSpanFormatOptions4 >> 1))
		{
			int num;
			switch (timeSpanFormatOptions4)
			{
			case TimeSpanFormatOptions.RangeWeeks:
				num = (int)Math.Floor(FromTime.TotalDays / 7.0);
				FromTime -= TimeSpan.FromDays(num * 7);
				break;
			case TimeSpanFormatOptions.RangeDays:
				num = (int)Math.Floor(FromTime.TotalDays);
				FromTime -= TimeSpan.FromDays(num);
				break;
			case TimeSpanFormatOptions.RangeHours:
				num = (int)Math.Floor(FromTime.TotalHours);
				FromTime -= TimeSpan.FromHours(num);
				break;
			case TimeSpanFormatOptions.RangeMinutes:
				num = (int)Math.Floor(FromTime.TotalMinutes);
				FromTime -= TimeSpan.FromMinutes(num);
				break;
			case TimeSpanFormatOptions.RangeSeconds:
				num = (int)Math.Floor(FromTime.TotalSeconds);
				FromTime -= TimeSpan.FromSeconds(num);
				break;
			case TimeSpanFormatOptions.RangeMilliSeconds:
				num = (int)Math.Floor(FromTime.TotalMilliseconds);
				FromTime -= TimeSpan.FromMilliseconds(num);
				break;
			default:
				throw new InvalidEnumArgumentException();
			}
			bool flag3 = false;
			bool flag4 = false;
			switch (timeSpanFormatOptions3)
			{
			case TimeSpanFormatOptions.TruncateShortest:
				if (flag2)
				{
					flag4 = true;
				}
				else if (num > 0)
				{
					flag3 = true;
				}
				break;
			case TimeSpanFormatOptions.TruncateAuto:
				if (num > 0)
				{
					flag3 = true;
				}
				break;
			case TimeSpanFormatOptions.TruncateFill:
				if (flag2 || num > 0)
				{
					flag3 = true;
				}
				break;
			case TimeSpanFormatOptions.TruncateFull:
				flag3 = true;
				break;
			}
			if (flag4)
			{
				break;
			}
			if (timeSpanFormatOptions4 == timeSpanFormatOptions2 && !flag2)
			{
				flag3 = true;
				if (flag && num < 1)
				{
					string unitText = timeTextInfo.GetUnitText(timeSpanFormatOptions2, 1, abbr);
					stringBuilder.Append(timeTextInfo.GetLessThanText(unitText));
					flag3 = false;
				}
			}
			if (flag3)
			{
				if (flag2)
				{
					stringBuilder.Append(" ");
				}
				string unitText2 = timeTextInfo.GetUnitText(timeSpanFormatOptions4, num, abbr);
				stringBuilder.Append(unitText2);
				flag2 = true;
			}
		}
		return stringBuilder.ToString();
	}

	public static TimeSpan Floor(this TimeSpan FromTime, long intervalTicks)
	{
		long num = FromTime.Ticks % intervalTicks;
		return TimeSpan.FromTicks(FromTime.Ticks - num);
	}

	public static TimeSpan Ceiling(this TimeSpan FromTime, long intervalTicks)
	{
		long num = FromTime.Ticks % intervalTicks;
		if (num == 0L)
		{
			return FromTime;
		}
		return TimeSpan.FromTicks(FromTime.Ticks - num + intervalTicks);
	}

	public static TimeSpan Round(this TimeSpan FromTime, long intervalTicks)
	{
		long num = FromTime.Ticks % intervalTicks;
		if (num >= intervalTicks >> 1)
		{
			num -= intervalTicks;
		}
		return TimeSpan.FromTicks(FromTime.Ticks - num);
	}
}
