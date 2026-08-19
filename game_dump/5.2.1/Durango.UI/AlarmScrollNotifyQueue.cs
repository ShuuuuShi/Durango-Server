using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class AlarmScrollNotifyQueue : AlarmNotifyQueueBase
{
	[SerializeField]
	private int _margin = 10;

	private int _visibleHeight;

	private readonly Queue<AlarmStruct> _alarmQueue = new Queue<AlarmStruct>();

	private readonly List<AlarmNotifyWidget> _alarmWidgets = new List<AlarmNotifyWidget>();

	private readonly Stack<AlarmNotifyWidget> _alarmWidgetPool = new Stack<AlarmNotifyWidget>();

	private void Start()
	{
		_baseAlarmWidget.gameObject.SetActive(value: false);
		RefreshVisibleHeight();
	}

	private void Update()
	{
		if (!UIManager.IsLoadingCurtain)
		{
			if (_alarmQueue.Count > 0)
			{
				ShowAlarm(_alarmQueue.Dequeue());
			}
			if (_alarmQueue.Count == 0 && _alarmWidgets.Count == 0)
			{
				base.enabled = false;
			}
		}
	}

	public override bool HasAlarm(string key)
	{
		return IndexOf(key) != -1;
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
		int num = IndexOf(key);
		if (num != -1)
		{
			_alarmWidgets[num].Hide();
		}
	}

	public override void ClearAlarms()
	{
		for (int num = _alarmWidgets.Count - 1; num >= 0; num--)
		{
			_alarmWidgets[num].Hide();
		}
	}

	public void RefreshVisibleHeight()
	{
		_visibleHeight = GetComponent<UIWidget>().height;
	}

	private void AddAlarmQueue(AlarmStruct arg)
	{
		int num = IndexOf(arg.Key);
		if (num == -1)
		{
			_alarmQueue.Enqueue(arg);
			base.enabled = true;
			return;
		}
		AlarmNotifyWidget alarmNotifyWidget = _alarmWidgets[num];
		SetAlarmStruct(alarmNotifyWidget, arg);
		alarmNotifyWidget.SetVisibleDuration(arg.Duration);
		UpdatePosition();
	}

	private void ShowAlarm(AlarmStruct arg)
	{
		AlarmNotifyWidget alarmNotifyWidget = null;
		if (!string.IsNullOrEmpty(arg.Key))
		{
			for (int num = _alarmWidgets.Count - 1; num >= 0; num--)
			{
				if (_alarmWidgets[num].Key == arg.Key)
				{
					alarmNotifyWidget = _alarmWidgets[num];
					break;
				}
			}
		}
		if (alarmNotifyWidget == null)
		{
			alarmNotifyWidget = GetAlarmWidget();
		}
		SetAlarmStruct(alarmNotifyWidget, arg);
		alarmNotifyWidget.transform.localPosition = GetPosition(0);
		alarmNotifyWidget.Show(arg.Duration, Vector3.left * 40f);
		UpdatePosition();
	}

	private void SetAlarmStruct(AlarmNotifyWidget w, AlarmStruct arg)
	{
		if (arg.IsPortrait)
		{
			arg.Portrait.Mask = _portraitMask;
			w.Set(arg.Key, arg.Text, arg.Portrait, arg.ViewmoreAction);
		}
		else
		{
			string icon = arg.Icon;
			w.Set(arg.Key, arg.Text, icon, arg.ViewmoreAction, arg.IconColor);
		}
	}

	private int IndexOf(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return -1;
		}
		for (int num = _alarmWidgets.Count - 1; num >= 0; num--)
		{
			if (_alarmWidgets[num].Key == key)
			{
				return num;
			}
		}
		return -1;
	}

	private void UpdatePosition()
	{
		int i = 0;
		for (int count = _alarmWidgets.Count; i < count; i++)
		{
			AlarmNotifyWidget alarmNotifyWidget = _alarmWidgets[i];
			Vector3 position = GetPosition(i);
			if (position.y + (float)alarmNotifyWidget.GetHeight() < (float)_visibleHeight)
			{
				alarmNotifyWidget.AnimWidget.Position = position;
			}
			else
			{
				alarmNotifyWidget.SetVisibleDuration(0f);
			}
		}
	}

	private Vector3 GetPosition(int index)
	{
		Vector3 localPosition = _baseAlarmWidget.transform.localPosition;
		int i = 0;
		for (int count = _alarmWidgets.Count; i < count; i++)
		{
			if (i == index)
			{
				return localPosition;
			}
			localPosition.y += _alarmWidgets[i].GetHeight() + _margin;
		}
		return localPosition;
	}

	private AlarmNotifyWidget GetAlarmWidget()
	{
		AlarmNotifyWidget alarmNotifyWidget;
		if (_alarmWidgetPool.Count > 0)
		{
			alarmNotifyWidget = _alarmWidgetPool.Pop();
		}
		else
		{
			AlarmNotifyWidget component = _baseAlarmWidget.transform.parent.gameObject.AddChild(_baseAlarmWidget.gameObject).GetComponent<AlarmNotifyWidget>();
			component.ShowFinished = OnShowFinish_AlarmWidget;
			component.HideFinished = OnHideFinish_AlarmWidget;
			alarmNotifyWidget = component;
		}
		_alarmWidgets.Insert(0, alarmNotifyWidget);
		return alarmNotifyWidget;
	}

	private void ReturnAlarmWidget(AlarmNotifyWidget widget)
	{
		_alarmWidgets.Remove(widget);
		widget.gameObject.SetActive(value: false);
		_alarmWidgetPool.Push(widget);
	}

	private void OnShowFinish_AlarmWidget(AlarmNotifyWidget widget)
	{
	}

	private void OnHideFinish_AlarmWidget(AlarmNotifyWidget widget)
	{
		ReturnAlarmWidget(widget);
		UpdatePosition();
	}
}
