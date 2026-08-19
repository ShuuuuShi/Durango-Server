using System;
using System.Collections.Generic;

namespace Durango.Logic.Notification;

public sealed class Container : Notification
{
	private readonly List<Notification> _children = new List<Notification>();

	private bool _isTypeDirty = true;

	private bool _isCountDirty = true;

	private int _count;

	public List<Notification> Children => _children;

	public override Type Type
	{
		get
		{
			if (_isTypeDirty)
			{
				base.Type = CalcType();
				_isTypeDirty = false;
			}
			return base.Type;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override int Count
	{
		get
		{
			if (_isCountDirty)
			{
				_count = CalcCount();
				_isCountDirty = false;
			}
			return _count;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override bool On
	{
		get
		{
			return Count > 0;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		int i = 0;
		for (int size = KUtility.GetSize(Children); i < size; i++)
		{
			Children[i].Refresh();
		}
	}

	private Type CalcType()
	{
		Type type = Type.Normal;
		int i = 0;
		for (int count = _children.Count; i < count; i++)
		{
			if (_children[i].On && _children[i].Type > type)
			{
				type = _children[i].Type;
			}
		}
		return type;
	}

	private int CalcCount()
	{
		int num = 0;
		int i = 0;
		for (int count = _children.Count; i < count; i++)
		{
			num += _children[i].Count;
		}
		return num;
	}

	protected override void OnChanged()
	{
		_isTypeDirty = true;
		_isCountDirty = true;
		base.OnChanged();
	}

	public void AddChild(Notification obj)
	{
		if (obj != null)
		{
			if (!_children.Contains(obj))
			{
				_children.Add(obj);
			}
			obj.AddParent(this);
			if (obj.On)
			{
				OnChanged();
			}
		}
	}

	public void AddChild(INotificationable obj)
	{
		AddChild(obj.Notification);
	}

	public void ClearChild()
	{
		int i = 0;
		for (int count = _children.Count; i < count; i++)
		{
			_children[i].RemoveParent(this);
		}
		_children.Clear();
		OnChanged();
	}
}
