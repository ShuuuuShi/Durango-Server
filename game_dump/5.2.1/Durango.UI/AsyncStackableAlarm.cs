using System;
using System.Linq;

namespace Durango.UI;

public class AsyncStackableAlarm<TK, TV> : StackableAlarmBase<TK, TV>
{
	private readonly Action<TK, Action<TK, TV, bool>> _requestFunc;

	private bool _isWait;

	public AsyncStackableAlarm(string alarmKey, Action<TK, Action<TK, TV, bool>> requestFunc, Func<TV, int, string> toString, string icon, bool majorAlarm, float duration, Action<TV> alarmOnClick)
		: base(alarmKey, toString, icon, (Func<TV, PortraitBuilder.Argument>)null, majorAlarm, duration, alarmOnClick)
	{
		_requestFunc = requestFunc;
	}

	public AsyncStackableAlarm(string alarmKey, Action<TK, Action<TK, TV, bool>> requestFunc, Func<TV, int, string> toString, Func<TV, PortraitBuilder.Argument> getPortrait, bool majorAlarm, float duration, Action<TV> alarmOnClick)
		: base(alarmKey, toString, (string)null, getPortrait, majorAlarm, duration, alarmOnClick)
	{
		_requestFunc = requestFunc;
	}

	public void Add(TK id)
	{
		if (_isWait)
		{
			_keys.Add(id);
		}
		else if (UIManager.Alarm.HasNotify(_alarmKey, _majorAlarm))
		{
			_keys.Add(id);
			RefreshAlarm();
		}
		else
		{
			_keys.Clear();
			_keys.Add(id);
			Request(id);
		}
	}

	private void Request(TK key)
	{
		if (!_isWait)
		{
			_isWait = true;
			_requestFunc(key, OnResponse);
		}
	}

	private void OnResponse(TK key, TV value, bool success)
	{
		_isWait = false;
		if (success)
		{
			_representation = value;
			RefreshAlarm();
			return;
		}
		_keys.Remove(key);
		if (_keys.Count > 0)
		{
			Request(_keys.First());
		}
	}
}
