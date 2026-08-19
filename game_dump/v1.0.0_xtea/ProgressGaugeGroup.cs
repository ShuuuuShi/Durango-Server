using System;
using System.Collections.Generic;
using TimerData;
using UnityEngine;

public class ProgressGaugeGroup : UIBase
{
	[SerializeField]
	private Transform _inGameProgressGaugeContainer;

	[SerializeField]
	private Transform _uiProgressGaugeContainer;

	private readonly List<ProgressGauge> _progressGaugeBase = new List<ProgressGauge>();

	private readonly HashSet<ProgressGauge> _progressGauges = new HashSet<ProgressGauge>();

	private readonly Dictionary<Type, Stack<ProgressGauge>> _gaugePool = new Dictionary<Type, Stack<ProgressGauge>>();

	private void Awake()
	{
		Transform[] array = (Transform[])(object)new Transform[2] { _inGameProgressGaugeContainer, _uiProgressGaugeContainer };
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			Transform val = array[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			int j = 0;
			for (int childCount = val.childCount; j < childCount; j++)
			{
				ProgressGauge component = ((Component)val.GetChild(j)).GetComponent<ProgressGauge>();
				if (!((Object)(object)component == (Object)null))
				{
					((Component)component).gameObject.SetActive(false);
					_progressGaugeBase.Add(component);
				}
			}
		}
	}

	public ProgressGauge Play<T>(Timer timer)
	{
		ProgressGauge gauge = GetGauge(typeof(T));
		if ((Object)(object)gauge == (Object)null)
		{
			return null;
		}
		gauge.Play(timer);
		return gauge;
	}

	private void AddGauge(ProgressGauge gauge)
	{
		if (!((Object)(object)gauge == (Object)null) && !_progressGauges.Contains(gauge))
		{
			_progressGauges.Add(gauge);
			gauge.Ended = ProgressGauge_Ended;
		}
	}

	private ProgressGauge GetGauge(Type type)
	{
		ProgressGauge progressGauge = Gauge_Pop(type);
		AddGauge(progressGauge);
		((Component)progressGauge).gameObject.SetActive(true);
		return progressGauge;
	}

	private ProgressGauge Gauge_Pop(Type type)
	{
		if (!_gaugePool.TryGetValue(type, out var value))
		{
			value = new Stack<ProgressGauge>();
			_gaugePool.Add(type, value);
		}
		ProgressGauge progressGauge;
		if (value.Count == 0)
		{
			GameObject val = null;
			for (int i = 0; i < _progressGaugeBase.Count; i++)
			{
				if ((object)((object)_progressGaugeBase[i]).GetType() == type)
				{
					val = ((Component)_progressGaugeBase[i]).gameObject;
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				return null;
			}
			progressGauge = ((Component)val.transform.parent).gameObject.AddChild(val).GetComponent<ProgressGauge>();
		}
		else
		{
			progressGauge = value.Pop();
		}
		progressGauge.IsPooledGauge = true;
		((Component)progressGauge).gameObject.SetActive(true);
		return progressGauge;
	}

	private void Gauge_Push(ProgressGauge gauge)
	{
		if (gauge.IsPooledGauge)
		{
			Type type = ((object)gauge).GetType();
			_gaugePool.TryGetValue(type, out var value);
			if (value == null)
			{
				value = new Stack<ProgressGauge>();
				_gaugePool.Add(type, value);
			}
			value.Push(gauge);
			((Component)gauge).gameObject.SetActive(false);
		}
	}

	private void ProgressGauge_Ended(ProgressGauge gauge)
	{
		_progressGauges.Remove(gauge);
		Gauge_Push(gauge);
	}
}
