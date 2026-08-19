using System;
using System.Collections.Generic;

namespace Durango.UI;

public abstract class StackableAlarmBase<TK, TV>
{
	protected readonly string _alarmKey;

	protected readonly Func<TV, int, string> _toString;

	protected readonly string _icon;

	protected readonly Func<TV, PortraitBuilder.Argument> _getPortrait;

	protected readonly bool _majorAlarm;

	protected readonly float _duration;

	protected readonly Action<TV> _alarmOnClick;

	protected TV _representation;

	protected readonly HashSet<TK> _keys = new HashSet<TK>();

	protected StackableAlarmBase(string alarmKey, Func<TV, int, string> toString, string icon, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)
	{
		_alarmKey = alarmKey;
		_toString = toString;
		_icon = icon;
		_getPortrait = getPortrait;
		_majorAlarm = majorAlarm;
		_duration = duration;
		_alarmOnClick = alarmOnClick;
	}

	protected void RefreshAlarm()
	{
		string text = _toString(_representation, _keys.Count);
		Action viewMoreAction = null;
		if (_alarmOnClick != null)
		{
			viewMoreAction = delegate
			{
				_alarmOnClick(_representation);
			};
		}
		if (_getPortrait != null)
		{
			UIManager.Alarm.ShowNotify(text, _getPortrait(_representation), _majorAlarm, _duration, viewMoreAction, _alarmKey);
		}
		else
		{
			UIManager.Alarm.ShowNotify(text, _icon, _majorAlarm, _duration, viewMoreAction, _alarmKey);
		}
	}
}
