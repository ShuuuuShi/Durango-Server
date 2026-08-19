using System;
using System.Collections.Generic;
using TimerData;
using UnityEngine;

namespace InteractionData;

public class InteractionMenuList
{
	private List<InteractionMenuData> _menus = new List<InteractionMenuData>();

	private bool _sorted;

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
	}

	public event Action Updated;

	public event Action<InteractionMenuData> MenuTimerFinished;

	public int IndexOf(InteractionMenuData data)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].IsEqualKey(data))
			{
				return i;
			}
		}
		return -1;
	}

	public void Add(InteractionMenuData data)
	{
		int num = IndexOf(data);
		if (num == -1)
		{
			_menus.Add(data);
			_sorted = false;
		}
		else
		{
			if (_menus[num].Timer != null)
			{
				_menus[num].Timer.Finished -= OnFinishMenuTimer;
			}
			_menus[num] = data;
		}
		if (data.Timer != null)
		{
			data.Timer.Finished += OnFinishMenuTimer;
		}
	}

	public bool Remove(InteractionMenuData data)
	{
		int num = IndexOf(data);
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

	public void Apply()
	{
		if (!_sorted)
		{
			_sorted = true;
			_menus.Sort();
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

	public void Clear()
	{
		_name = null;
		_menus.Clear();
	}

	private void OnFinishMenuTimer(Timer timer)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_menus[i].Timer == timer && this.MenuTimerFinished != null)
			{
				this.MenuTimerFinished(_menus[i]);
			}
		}
	}
}
