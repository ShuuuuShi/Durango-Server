using System;
using System.Collections.Generic;
using L10N;
using SmartFormat.Core.Extensions;

public class TimedeltaFormatter : IFormatter
{
	private struct TimedeltaUnit
	{
		public string Name;

		public int Seconds;
	}

	private static readonly string[] TimedeltaFormats = new string[15]
	{
		T.N_("{day}일"),
		T.N_("{hour}시간"),
		T.N_("{day}일 {hour}시간"),
		T.N_("{min}분"),
		T.N_("{day}일 {min}분"),
		T.N_("{hour}시간 {min}분"),
		T.N_("{day}일 {hour}시간 {min}분"),
		T.N_("{sec}초"),
		T.N_("{day}일 {sec}초"),
		T.N_("{hour}시간 {sec}초"),
		T.N_("{day}일 {hour}시간 {sec}초"),
		T.N_("{min}분 {sec}초"),
		T.N_("{day}일 {min}분 {sec}초"),
		T.N_("{hour}시간 {min}분 {sec}초"),
		T.N_("{day}일 {hour}시간 {min}분 {sec}초")
	};

	private static readonly TimedeltaUnit[] TimedeltaUnits = new TimedeltaUnit[4]
	{
		new TimedeltaUnit
		{
			Name = "day",
			Seconds = 86400
		},
		new TimedeltaUnit
		{
			Name = "hour",
			Seconds = 3600
		},
		new TimedeltaUnit
		{
			Name = "min",
			Seconds = 60
		},
		new TimedeltaUnit
		{
			Name = "sec",
			Seconds = 1
		}
	};

	private string[] _names = new string[5] { "sec", "min", "hour", "day", "xxx" };

	public string[] Names
	{
		get
		{
			return _names;
		}
		set
		{
			_names = value;
		}
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string formatterName = formattingInfo.Placeholder.FormatterName;
		double seconds = Convert.ToDouble(formattingInfo.CurrentValue);
		string granularity = formatterName;
		string text = FormatTimedelta(seconds, 2, granularity);
		formattingInfo.Write(text);
		return true;
	}

	public static string FormatTimedelta(double seconds, int scope = 2, string granularity = "sec", float threshold = 0.85f)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < TimedeltaUnits.Length; i++)
		{
			TimedeltaUnit timedeltaUnit = TimedeltaUnits[i];
			double num3 = seconds / (double)timedeltaUnit.Seconds;
			seconds %= (double)timedeltaUnit.Seconds;
			if (timedeltaUnit.Name == granularity)
			{
				num2 = i;
			}
			if (num3 >= (double)threshold || (num == 0 && i == num2))
			{
				num |= 1 << (i & 0x1F);
				if (num2 == -1 && scope != -1)
				{
					num2 = i + scope - 1;
				}
				if (i == num2 && num3 > 0.0)
				{
					num3 = Math.Max(1.0, Math.Round(num3));
				}
				dictionary[timedeltaUnit.Name] = (int)num3;
			}
			if (i == num2)
			{
				break;
			}
		}
		return T._(TimedeltaFormats[num - 1], dictionary);
	}
}
