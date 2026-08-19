using System;

namespace Durango.UI;

public class StackableAlarm<TK, TV> : StackableAlarmBase<TK, TV>
{
	private readonly Func<TV, TK> _getKey;

	public StackableAlarm(string alarmKey, Func<TV, TK> getKey, Func<TV, int, string> toString, string icon, bool majorAlarm, float duration, Action<TV> alarmOnClick)
		: base(alarmKey, toString, icon, (Func<TV, PortraitBuilder.Argument>)null, majorAlarm, duration, alarmOnClick)
	{
		_getKey = getKey;
	}

	public StackableAlarm(string alarmKey, Func<TV, TK> getKey, Func<TV, int, string> toString, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)
		: base(alarmKey, toString, (string)null, getPortrait, majorAlarm, duration, alarmOnClick)
	{
		_getKey = getKey;
	}

	public void Add(TV value)
	{
		TK item = _getKey(value);
		if (UIManager.Alarm.HasNotify(_alarmKey, _majorAlarm))
		{
			_keys.Add(item);
			RefreshAlarm();
			return;
		}
		_keys.Clear();
		_representation = value;
		_keys.Add(item);
		RefreshAlarm();
	}
}
