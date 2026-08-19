using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmControl : MonoBehaviour
{
	private struct AlarmStruct
	{
		public string Text;

		public string Icon;

		public bool IsPortriat;

		public PortraitBuilder.Argument Portrait;

		public float Duration;

		public Action ViewmoreAction;
	}

	[SerializeField]
	private AlarmWidget _baseAlarmWidget;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private int _margin = 10;

	private List<AlarmWidget> _alarmWidgets;

	private Stack<AlarmWidget> _alarmWidgetPool;

	private Queue<AlarmStruct> _alarmQueue = new Queue<AlarmStruct>();

	private void Update()
	{
		if (!UIManager.IsLoadingCurtain && _alarmQueue.Count > 0)
		{
			ShowAlarm(_alarmQueue.Dequeue());
		}
	}

	public void Init()
	{
		((Component)this).gameObject.SetActive(true);
		((Component)_baseAlarmWidget).gameObject.SetActive(false);
		_alarmWidgets = new List<AlarmWidget>();
		_alarmWidgetPool = new Stack<AlarmWidget>();
	}

	private AlarmWidget GetAlarmWidget()
	{
		AlarmWidget alarmWidget;
		if (_alarmWidgetPool.Count > 0)
		{
			alarmWidget = _alarmWidgetPool.Pop();
		}
		else
		{
			AlarmWidget component = ((Component)((Component)_baseAlarmWidget).transform.parent).gameObject.AddChild(((Component)_baseAlarmWidget).gameObject).GetComponent<AlarmWidget>();
			component.ShowFinished = OnShowFinish_AlarmWidget;
			component.HideFinished = OnHideFinish_AlarmWidget;
			alarmWidget = component;
		}
		_alarmWidgets.Insert(0, alarmWidget);
		return alarmWidget;
	}

	private void OnShowFinish_AlarmWidget(AlarmWidget widget)
	{
	}

	private void OnHideFinish_AlarmWidget(AlarmWidget widget)
	{
		ReturnAlarmWidget(widget);
		UpdatePosition();
	}

	private void ReturnAlarmWidget(AlarmWidget widget)
	{
		_alarmWidgets.Remove(widget);
		((Component)widget).gameObject.SetActive(false);
		_alarmWidgetPool.Push(widget);
	}

	public void ShowAlarm(string text, PortraitBuilder.Argument arg, float duration = 5f, Action viewMoreAction = null)
	{
		_alarmQueue.Enqueue(new AlarmStruct
		{
			Text = text,
			Portrait = arg,
			Duration = duration,
			ViewmoreAction = viewMoreAction,
			IsPortriat = true
		});
	}

	[ExposedInEditor(null)]
	public void ShowAlarm(string text, string icon, float duration = 5f, Action viewMoreAction = null)
	{
		_alarmQueue.Enqueue(new AlarmStruct
		{
			Text = text,
			Icon = icon,
			Duration = duration,
			ViewmoreAction = viewMoreAction,
			IsPortriat = false
		});
	}

	private void ShowAlarm(AlarmStruct arg)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		AlarmWidget alarmWidget = GetAlarmWidget();
		if (arg.IsPortriat)
		{
			arg.Portrait.Mask = _portraitMask;
			alarmWidget.Set(arg.Text, arg.Portrait, arg.ViewmoreAction);
		}
		else
		{
			string icon = arg.Icon;
			alarmWidget.Set(arg.Text, icon, arg.ViewmoreAction);
		}
		((Component)alarmWidget).transform.localPosition = GetPosition(0);
		alarmWidget.Show(arg.Duration);
		UpdatePosition();
	}

	public void ClearAlarms()
	{
		for (int num = _alarmWidgets.Count - 1; num >= 0; num--)
		{
			_alarmWidgets[num].Hide();
		}
	}

	private void UpdatePosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _alarmWidgets.Count; i < count; i++)
		{
			AlarmWidget alarmWidget = _alarmWidgets[i];
			alarmWidget.AnimWidget.Position = GetPosition(i);
		}
	}

	private Vector3 GetPosition(int index)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)_baseAlarmWidget).transform.localPosition;
		int i = 0;
		for (int count = _alarmWidgets.Count; i < count; i++)
		{
			if (i == index)
			{
				return localPosition;
			}
			localPosition.y += (float)(_alarmWidgets[i].GetHeight() + _margin);
		}
		return localPosition;
	}
}
