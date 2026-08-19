using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Timer;
using UnityEngine;

namespace InteractionData;

public class InteractionMenuList : IEnumerable<InteractionMenuData>, IEnumerable
{
	public class InteractionTimerData
	{
		public Interaction Type { get; private set; }

		public Timer Timer { get; private set; }

		public Func<string> GetIdFunc { get; private set; }

		public InteractionTimerData(Interaction type, Timer timer, Func<string> getIdFunc)
		{
			Type = type;
			Timer = timer;
			GetIdFunc = getIdFunc;
		}
	}

	private readonly List<InteractionMenuData> _menus = new List<InteractionMenuData>();

	private bool _sorted;

	public readonly List<InteractionTimerData> _timers = new List<InteractionTimerData>();

	private string _name;

	public int ResetFrame { get; private set; }

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public int Count => _menus.Count;

	public InteractionMenuData this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				return default(InteractionMenuData);
			}
			return _menus[index];
		}
		set
		{
			if (index >= 0 && index < Count)
			{
				_menus[index] = value;
			}
		}
	}

	public event Action Updated;

	public event Action Cleared;

	public event Action TimerEnded;

	public int IndexOf(Interaction type)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Action == type)
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf(Interaction type, string id)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].IsEqualKey(type, id))
			{
				return i;
			}
		}
		return -1;
	}

	public void Add(InteractionMenuData data)
	{
		int num = IndexOf(data.Action, data.Id);
		if (num == -1)
		{
			_menus.Add(data);
			_sorted = false;
		}
		else
		{
			data.Parent = this;
			_menus[num] = data;
		}
	}

	public bool Remove(Interaction type, string id)
	{
		int num = IndexOf(type, id);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public void RemoveAt(int index)
	{
		_menus.RemoveAt(index);
	}

	public void RegisterTimer(Interaction type, PredictTimer timer, Func<string> getIdFunc)
	{
		_timers.Add(new InteractionTimerData(type, timer.Timer, getIdFunc));
		timer.Started += Timer_Started;
		timer.Ended += Timer_Ended;
	}

	public bool HasPlayingTimer()
	{
		return _timers.Any((InteractionTimerData o) => !o.Timer.IsStop);
	}

	private void Timer_Started(PredictTimer timer)
	{
		Apply();
	}

	private void Timer_Ended(PredictTimer timer)
	{
		if (this.TimerEnded != null)
		{
			this.TimerEnded();
		}
	}

	public void Apply()
	{
		if (!_sorted)
		{
			_sorted = true;
			_menus.Sort();
		}
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			InteractionMenuData data = _menus[i];
			InteractionTimerData interactionTimerData = _timers.Find((InteractionTimerData o) => data.IsEqualKey(o.Type, (o.GetIdFunc == null) ? null : o.GetIdFunc()));
			data.SetTimer(interactionTimerData?.Timer);
			_menus[i] = data;
		}
		if (this.Updated != null)
		{
			this.Updated();
		}
	}

	public void Reset()
	{
		Clear();
		ResetFrame = Time.frameCount;
	}

	public void ResetAndDontClear()
	{
		ResetFrame = Time.frameCount;
	}

	public void Clear()
	{
		_name = null;
		_menus.Clear();
		if (this.Cleared != null)
		{
			this.Cleared();
		}
		foreach (InteractionTimerData timer in _timers)
		{
			timer.Timer.Stop();
		}
	}

	public IEnumerator<InteractionMenuData> GetEnumerator()
	{
		return _menus.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
