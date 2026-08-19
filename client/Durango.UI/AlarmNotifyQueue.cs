using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class AlarmNotifyQueue : AlarmNotifyQueueBase
{
	private Vector3 _pos;

	private readonly List<AlarmStruct> _alarmWaiting = new List<AlarmStruct>();

	private void Start()
	{
		_pos = _baseAlarmWidget.transform.localPosition;
		_baseAlarmWidget.ShowFinished = OnShowFinish_AlarmWidget;
		_baseAlarmWidget.HideFinished = OnHideFinish_AlarmWidget;
		_baseAlarmWidget.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!UIManager.IsLoadingCurtain && base.enabled && !_baseAlarmWidget.gameObject.activeSelf)
		{
			AlarmStruct? alarmStruct = DequeueWaitingAlarm();
			if (alarmStruct.HasValue)
			{
				SetAlarmStruct(alarmStruct.Value);
				_baseAlarmWidget.transform.localPosition = _pos;
				_baseAlarmWidget.Show(alarmStruct.Value.Duration, Vector3.left * 30f);
			}
			else
			{
				base.enabled = false;
			}
		}
	}

	private void OnShowFinish_AlarmWidget(AlarmNotifyWidget widget)
	{
	}

	private void OnHideFinish_AlarmWidget(AlarmNotifyWidget widget)
	{
		widget.gameObject.SetActive(value: false);
	}

	public override bool HasAlarm(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			return IsCurrentAlarm(key) || HasWaitingAlarm(key);
		}
		return false;
	}

	public override void ShowAlarm(string key, string text, PortraitBuilder.Argument arg, float duration, Action viewMoreAction)
	{
		AddAlarmQueue(new AlarmStruct
		{
			Key = key,
			Text = text,
			Portrait = arg,
			Duration = duration,
			ViewmoreAction = viewMoreAction,
			IsPortrait = true
		});
	}

	[ExposedInEditor(null)]
	public override void ShowAlarm(string key, string text, string icon, Color32 iconColor, float duration, Action viewMoreAction)
	{
		AddAlarmQueue(new AlarmStruct
		{
			Key = key,
			Text = text,
			Icon = icon,
			IconColor = iconColor,
			Duration = duration,
			ViewmoreAction = viewMoreAction,
			IsPortrait = false
		});
	}

	public override void HideAlarm(string key)
	{
		if (IsCurrentAlarm(key))
		{
			_baseAlarmWidget.Hide();
		}
		else
		{
			RemoveWaitingAlarm(key);
		}
	}

	public override void ClearAlarms()
	{
		if (_baseAlarmWidget.gameObject.activeSelf)
		{
			_baseAlarmWidget.Hide();
		}
		ClearWaitingAlarms();
	}

	private void AddAlarmQueue(AlarmStruct arg)
	{
		if (IsCurrentAlarm(arg.Key))
		{
			SetAlarmStruct(arg);
			_baseAlarmWidget.SetVisibleDuration(arg.Duration);
		}
		else
		{
			EnqueueWaitingAlarm(arg);
			base.enabled = true;
		}
	}

	private bool IsCurrentAlarm(string key)
	{
		if (_baseAlarmWidget.gameObject.activeSelf)
		{
			return _baseAlarmWidget.Key == key;
		}
		return false;
	}

	private void SetAlarmStruct(AlarmStruct arg)
	{
		if (arg.IsPortrait)
		{
			arg.Portrait.Mask = _portraitMask;
			_baseAlarmWidget.Set(arg.Key, arg.Text, arg.Portrait, arg.ViewmoreAction);
		}
		else
		{
			string icon = arg.Icon;
			_baseAlarmWidget.Set(arg.Key, arg.Text, icon, arg.ViewmoreAction, arg.IconColor);
		}
	}

	private bool HasWaitingAlarm(string key)
	{
		foreach (AlarmStruct item in _alarmWaiting)
		{
			if (item.Key == key)
			{
				return true;
			}
		}
		return false;
	}

	private void EnqueueWaitingAlarm(AlarmStruct arg)
	{
		RemoveWaitingAlarm(arg.Key);
		_alarmWaiting.Add(arg);
	}

	private AlarmStruct? DequeueWaitingAlarm()
	{
		AlarmStruct? result = null;
		if (_alarmWaiting.Count > 0)
		{
			result = _alarmWaiting[0];
			_alarmWaiting.RemoveAt(0);
		}
		return result;
	}

	private void ClearWaitingAlarms()
	{
		_alarmWaiting.Clear();
	}

	private void RemoveWaitingAlarm(string key)
	{
		foreach (AlarmStruct item in _alarmWaiting)
		{
			if (item.Key == key)
			{
				_alarmWaiting.Remove(item);
				break;
			}
		}
	}
}
