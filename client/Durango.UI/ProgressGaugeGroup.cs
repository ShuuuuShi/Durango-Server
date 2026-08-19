using System;
using System.Collections.Generic;
using Durango.Logic.Timer;
using UnityEngine;

namespace Durango.UI;

public class ProgressGaugeGroup : UIBase
{
	private readonly List<ProgressGauge> _progressGaugeBase = new List<ProgressGauge>();

	private readonly HashSet<ProgressGauge> _progressGauges = new HashSet<ProgressGauge>();

	private readonly Dictionary<Type, Stack<ProgressGauge>> _gaugePool = new Dictionary<Type, Stack<ProgressGauge>>();

	private void Awake()
	{
		ProgressGauge[] componentsInChildren = GetComponentsInChildren<ProgressGauge>(includeInactive: true);
		foreach (ProgressGauge progressGauge in componentsInChildren)
		{
			progressGauge.gameObject.SetActive(value: false);
			_progressGaugeBase.Add(progressGauge);
		}
		SetChildrenActive(activated: true);
	}

	public T Play<T>(Timer timer) where T : ProgressGauge
	{
		ProgressGauge gauge = GetGauge(typeof(T));
		if (gauge == null)
		{
			return (T)null;
		}
		gauge.Play(timer);
		return gauge as T;
	}

	private void AddGauge(ProgressGauge gauge)
	{
		if (!(gauge == null) && !_progressGauges.Contains(gauge))
		{
			_progressGauges.Add(gauge);
			gauge.Ended = ProgressGauge_Ended;
		}
	}

	private ProgressGauge GetGauge(Type type)
	{
		ProgressGauge progressGauge = Gauge_Pop(type);
		AddGauge(progressGauge);
		progressGauge.gameObject.SetActive(value: true);
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
			GameObject gameObject = null;
			for (int i = 0; i < _progressGaugeBase.Count; i++)
			{
				if (_progressGaugeBase[i].GetType() == type)
				{
					gameObject = _progressGaugeBase[i].gameObject;
					break;
				}
			}
			if (gameObject == null)
			{
				return null;
			}
			progressGauge = gameObject.transform.parent.gameObject.AddChild(gameObject).GetComponent<ProgressGauge>();
		}
		else
		{
			progressGauge = value.Pop();
		}
		progressGauge.IsPooledGauge = true;
		progressGauge.gameObject.SetActive(value: true);
		return progressGauge;
	}

	private void Gauge_Push(ProgressGauge gauge)
	{
		if (gauge.IsPooledGauge)
		{
			Type type = gauge.GetType();
			_gaugePool.TryGetValue(type, out var value);
			if (value == null)
			{
				value = new Stack<ProgressGauge>();
				_gaugePool.Add(type, value);
			}
			value.Push(gauge);
			gauge.gameObject.SetActive(value: false);
		}
	}

	private void ProgressGauge_Ended(ProgressGauge gauge)
	{
		_progressGauges.Remove(gauge);
		Gauge_Push(gauge);
	}
}
