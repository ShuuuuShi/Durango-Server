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

	private static int _currentMinUnitIndex;

	private static readonly Dictionary<string, int> Kwargs = new Dictionary<string, int>();

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
		double num = Convert.ToDouble(formattingInfo.CurrentValue);
		double seconds = num;
		string granularity = formatterName;
		string text = Format(seconds, 2, granularity);
		formattingInfo.Write(text);
		return true;
	}

	public static int CurrentMinUnit()
	{
		return TimedeltaUnits[_currentMinUnitIndex].Seconds;
	}

	public static float NextPeriod(double remain)
	{
		int num = CurrentMinUnit();
		float num2 = (float)(remain % (double)num);
		return (num2 != 0f) ? num2 : ((float)num);
	}

	public static string Format(double seconds, int scope = 2, string granularity = "sec")
	{
		Dictionary<string, int> kwargs = Kwargs;
		kwargs.Clear();
		int num = 0;
		float num2 = 1f;
		int num3 = -1;
		for (int i = 0; i < TimedeltaUnits.Length; i++)
		{
			TimedeltaUnit timedeltaUnit = TimedeltaUnits[i];
			double num4 = seconds / (double)timedeltaUnit.Seconds;
			seconds %= (double)timedeltaUnit.Seconds;
			bool flag = timedeltaUnit.Name == granularity;
			if (flag || i == TimedeltaUnits.Length - 1)
			{
				num3 = i;
			}
			bool flag2 = num == 0 && i == num3;
			if (flag2 || num4 >= (double)num2)
			{
				num4 = ((!flag && scope != 1) ? Math.Floor(num4) : Math.Ceiling(num4));
				if (flag2 || num4 > 0.0)
				{
					num |= 1 << i;
					kwargs[timedeltaUnit.Name] = (int)num4;
					if (num3 == -1 && scope > 0)
					{
						num3 = i + scope - 1;
					}
					num2 = 0f;
				}
			}
			if (i == num3)
			{
				break;
			}
		}
		for (int num5 = num3; num5 >= 1; num5--)
		{
			TimedeltaUnit timedeltaUnit2 = TimedeltaUnits[num5];
			TimedeltaUnit timedeltaUnit3 = TimedeltaUnits[num5 - 1];
			if (kwargs.TryGetValue(timedeltaUnit2.Name, out var value) && value >= timedeltaUnit3.Seconds / timedeltaUnit2.Seconds)
			{
				num &= ~(1 << num5);
				num |= 1 << num5 - 1;
				kwargs[timedeltaUnit2.Name] = 0;
				kwargs[timedeltaUnit3.Name] = kwargs.Get(timedeltaUnit3.Name, 0) + 1;
			}
		}
		_currentMinUnitIndex = num3;
		return T._(TimedeltaFormats[num - 1], kwargs);
	}

	public static string ColonFormat(double seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		int days = timeSpan.Days;
		int num = timeSpan.Hours + days * 24;
		int minutes = timeSpan.Minutes;
		int seconds2 = timeSpan.Seconds;
		return (num <= 0) ? $"{minutes:D2}:{seconds2:D2}" : $"{num:D2}:{minutes:D2}:{seconds2:D2}";
	}
}
