using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Logic.Notification;

public abstract class Notification
{
	[CanBeNull]
	private List<Notification> _parents;

	private bool _ignoreOnChanged;

	private int _prevCount;

	private Type _prevType;

	public abstract int Count { get; set; }

	public abstract bool On { get; set; }

	public virtual Type Type { get; set; }

	public ViewType ViewType { get; set; }

	public event Action Changed;

	public void BeginSetting()
	{
		_prevCount = Count;
		_prevType = Type;
		_ignoreOnChanged = true;
	}

	public void EndSetting()
	{
		_ignoreOnChanged = false;
		if (_prevCount != Count || _prevType != Type)
		{
			OnChanged();
		}
	}

	protected virtual void OnChanged()
	{
		if (_ignoreOnChanged)
		{
			return;
		}
		if (this.Changed != null)
		{
			this.Changed();
		}
		if (_parents != null)
		{
			int i = 0;
			for (int count = _parents.Count; i < count; i++)
			{
				_parents[i].OnChanged();
			}
		}
	}

	public void AddParent([NotNull] Notification parent)
	{
		if (_parents == null)
		{
			_parents = new List<Notification>();
		}
		else if (_parents.Contains(parent))
		{
			return;
		}
		_parents.Add(parent);
	}

	public void RemoveParent([NotNull] Notification parent)
	{
		if (_parents != null)
		{
			_parents.Remove(parent);
		}
	}

	public virtual void Refresh()
	{
	}

	public static Color GetTypeColor(Type type)
	{
		return type switch
		{
			Type.Normal => new Color32(150, 153, 158, byte.MaxValue), 
			Type.Important => new Color32(184, 46, 46, byte.MaxValue), 
			_ => Color.white, 
		};
	}
}
