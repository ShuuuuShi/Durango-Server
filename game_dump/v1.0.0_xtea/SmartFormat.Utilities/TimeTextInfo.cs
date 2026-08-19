namespace SmartFormat.Utilities;

public class TimeTextInfo
{
	private PluralRules.PluralRuleDelegate PluralRule;

	private string[] week;

	private string[] day;

	private string[] hour;

	private string[] minute;

	private string[] second;

	private string[] millisecond;

	private string[] w;

	private string[] d;

	private string[] h;

	private string[] m;

	private string[] s;

	private string[] ms;

	private string lessThan;

	public TimeTextInfo(PluralRules.PluralRuleDelegate pluralRule, string[] week, string[] day, string[] hour, string[] minute, string[] second, string[] millisecond, string[] w, string[] d, string[] h, string[] m, string[] s, string[] ms, string lessThan)
	{
		PluralRule = pluralRule;
		this.week = week;
		this.day = day;
		this.hour = hour;
		this.minute = minute;
		this.second = second;
		this.millisecond = millisecond;
		this.w = w;
		this.d = d;
		this.h = h;
		this.m = m;
		this.s = s;
		this.ms = ms;
		this.lessThan = lessThan;
	}

	public TimeTextInfo(string week, string day, string hour, string minute, string second, string millisecond, string lessThan)
	{
		PluralRule = (decimal d, int c) => 0;
		this.week = new string[1] { week };
		this.day = new string[1] { day };
		this.hour = new string[1] { hour };
		this.minute = new string[1] { minute };
		this.second = new string[1] { second };
		this.millisecond = new string[1] { millisecond };
		this.lessThan = lessThan;
	}

	private static string getValue(PluralRules.PluralRuleDelegate pluralRule, int value, string[] units)
	{
		int num = ((units.Length != 1) ? pluralRule(value, units.Length) : 0);
		return string.Format(units[num], value);
	}

	public string GetLessThanText(string minimumValue)
	{
		return string.Format(lessThan, minimumValue);
	}

	public virtual string GetUnitText(TimeSpanFormatOptions unit, int value, bool abbr)
	{
		return unit switch
		{
			TimeSpanFormatOptions.RangeWeeks => getValue(PluralRule, value, (!abbr) ? week : w), 
			TimeSpanFormatOptions.RangeDays => getValue(PluralRule, value, (!abbr) ? day : d), 
			TimeSpanFormatOptions.RangeHours => getValue(PluralRule, value, (!abbr) ? hour : h), 
			TimeSpanFormatOptions.RangeMinutes => getValue(PluralRule, value, (!abbr) ? minute : m), 
			TimeSpanFormatOptions.RangeSeconds => getValue(PluralRule, value, (!abbr) ? second : s), 
			TimeSpanFormatOptions.RangeMilliSeconds => getValue(PluralRule, value, (!abbr) ? millisecond : ms), 
			_ => null, 
		};
	}
}
