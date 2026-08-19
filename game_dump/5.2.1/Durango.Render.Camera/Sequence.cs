using System.Collections.Generic;
using UnityEngine;

namespace Durango.Render.Camera;

public abstract class Sequence
{
	protected readonly List<float> StartAt = new List<float>();

	protected int SequenceIndex;

	public abstract void Next(float startAt);

	public abstract void Delay(float delay);

	public virtual void Clear()
	{
		StartAt.Clear();
		SequenceIndex = 0;
	}
}
public class Sequence<T> : Sequence
{
	public delegate T LerpFunction(NgInterpolate.Function ease, T begin, T end, float elapsedTime, float duration);

	public delegate T SnapshotFunction(T value);

	private readonly List<Item<T>> _items = new List<Item<T>>();

	private LerpFunction _lerpFunc;

	private SnapshotFunction _snapFunc;

	private int _prevGetIndex = -1;

	private T _begin;

	public T Value { get; set; }

	public Sequence(LerpFunction lerp, SnapshotFunction snap = null)
	{
		_lerpFunc = lerp;
		_snapFunc = snap;
	}

	public override void Clear()
	{
		base.Clear();
		_items.Clear();
		_prevGetIndex = -1;
	}

	public override void Next(float startAt)
	{
		if (SequenceIndex < _items.Count)
		{
			StartAt.Add(startAt);
			SequenceIndex++;
		}
		else if (SequenceIndex < StartAt.Count)
		{
			StartAt[SequenceIndex] = startAt;
		}
		else
		{
			StartAt.Add(startAt);
		}
	}

	public override void Delay(float delay)
	{
		float num = ((_items.Count <= 0) ? Time.time : (StartAt[_items.Count - 1] + _items[_items.Count - 1].Duration));
		Next(num + delay);
	}

	public void Add(T value, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		float time = Time.time;
		if (StartAt.Count > 0 && StartAt[0] < time)
		{
			if (_prevGetIndex != -1)
			{
				Update(_prevGetIndex);
			}
			Clear();
		}
		if (_items.Count != 0 || !Value.Equals(value))
		{
			if (StartAt.Count == 0)
			{
				StartAt.Add(time);
			}
			Item<T> item = default(Item<T>);
			item.Value = value;
			item.Duration = duration;
			item.Ease = NgInterpolate.Ease(type);
			Item<T> item2 = item;
			if (SequenceIndex < _items.Count)
			{
				_items[SequenceIndex] = item2;
			}
			else
			{
				_items.Add(item2);
			}
		}
	}

	public T Update()
	{
		if (_items.Count == 0)
		{
			return Value;
		}
		float time = Time.time;
		int num = -1;
		for (int num2 = _items.Count - 1; num2 >= 0; num2--)
		{
			if (StartAt[num2] <= time)
			{
				num = num2;
				break;
			}
		}
		if (num == -1)
		{
			return Value;
		}
		return Update(num);
	}

	private T Update(int index)
	{
		if (_prevGetIndex != index)
		{
			int num = index - 1;
			T val = ((num >= 0) ? _items[num].Value : Value);
			_begin = ((_snapFunc != null) ? _snapFunc(val) : val);
			_prevGetIndex = index;
		}
		Item<T> item = _items[index];
		float duration = item.Duration;
		if (duration > 0f)
		{
			float num2 = Time.time - StartAt[index];
			if (num2 > duration)
			{
				Value = item.Value;
				if (index == _items.Count - 1)
				{
					Clear();
				}
			}
			else
			{
				T begin = _begin;
				T value = item.Value;
				NgInterpolate.Function ease = item.Ease;
				Value = _lerpFunc(ease, begin, value, num2, duration);
			}
		}
		else
		{
			Value = item.Value;
		}
		return Value;
	}
}
