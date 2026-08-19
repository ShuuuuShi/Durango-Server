using Durango.Network;
using Durango.UI.Control;
using L10N;

public struct SyncString : KeyLabelBase.IContent
{
	public delegate void UpdateDelegate(out string text, out float period);

	private readonly UpdateDelegate _func;

	private readonly string _text;

	public SyncString(string value)
	{
		_func = null;
		_text = value;
	}

	public SyncString(UpdateDelegate value)
	{
		_func = value;
		_text = null;
	}

	public static implicit operator SyncString(string value)
	{
		return new SyncString(value);
	}

	public bool HasText()
	{
		if (_func == null)
		{
			return !string.IsNullOrEmpty(_text);
		}
		return true;
	}

	public string Get(out float period)
	{
		if (_func == null)
		{
			period = 0f;
			return _text;
		}
		_func(out var text, out period);
		return text;
	}

	public static double UpdateRemainTimeMsg(double endAt, out string text, out float period, string expired = "")
	{
		return UpdateRemainTimeMsg(endAt, null, out text, out period, expired);
	}

	public static double UpdateRemainTimeMsg(double endAt, string format, out string text, out float period, string expired = "", int scope = 2, string granularity = "sec")
	{
		double num = endAt - Connections.Frontend.GetPredictedServerTime();
		if (num > 0.0)
		{
			text = ((!string.IsNullOrEmpty(format)) ? T._(format, TimedeltaFormatter.Format(num, scope, granularity)) : TimedeltaFormatter.Format(num));
			period = TimedeltaFormatter.NextPeriod(num);
		}
		else
		{
			text = expired;
			period = 0f;
		}
		return num;
	}

	public static double UpdateRemainTimeColonMsg(double endAt, out string text, out float period, string expired = "")
	{
		double num = endAt - Connections.Frontend.GetPredictedServerTime();
		if (num > 0.0)
		{
			text = TimedeltaFormatter.ColonFormat(num);
			period = (float)(num % 1.0);
			if (period == 0f)
			{
				period = 1f;
			}
		}
		else
		{
			text = expired;
			period = 0f;
		}
		return num;
	}
}
