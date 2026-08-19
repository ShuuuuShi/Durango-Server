using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class RectLayout
{
	public enum Direction
	{
		Horizontal,
		Vertical
	}

	public enum ItemType
	{
		Pixel,
		Ratio,
		Weight
	}

	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	[Serializable]
	public struct LayoutArgument
	{
		public bool OverrideWidth;

		public float Width;

		public bool OverrideHeight;

		public float Height;

		public Direction Direction;

		public Pivot Pivot;

		public Vector2 GetPivotOffset()
		{
			return RectLayout.GetPivotOffset(Pivot);
		}
	}

	[Serializable]
	public struct RectArgument
	{
		public int Depth;

		public ItemArgument Size;

		public ItemArgument Breadth;

		public int PositionType;

		public Spacing Spacing;

		public Side Padding;

		[NonSerialized]
		public CompatibleDelegate Compatible;
	}

	[Serializable]
	public struct Spacing
	{
		public float Begin;

		public float End;

		public float Side1;

		public float Side2;

		public float Sum()
		{
			return Begin + End;
		}

		public float Breadth()
		{
			return Side1 + Side2;
		}
	}

	[Serializable]
	public struct Side
	{
		public int Left;

		public int Bottom;

		public int Right;

		public int Top;

		public int GetSize(Direction dir)
		{
			return dir switch
			{
				Direction.Horizontal => Left + Right, 
				Direction.Vertical => Bottom + Top, 
				_ => 0, 
			};
		}
	}

	[Serializable]
	public struct ItemArgument
	{
		public ItemType Type;

		public float Value;

		public float Min;

		public float Max;
	}

	[Serializable]
	public class WidgetItem
	{
		public UIWidget Widget;

		public RectArgument Argument;

		private bool _isInitializeCompatible;

		private CompatibleDelegate _compatible;

		public CompatibleDelegate GetCompatible()
		{
			if (!_isInitializeCompatible)
			{
				if (Application.isPlaying)
				{
					_isInitializeCompatible = true;
				}
				else
				{
					_compatible = null;
				}
				if ((bool)Widget)
				{
					ICompatible component = Widget.GetComponent<ICompatible>();
					if (component != null)
					{
						_compatible = (CompatibleDelegate)Delegate.Combine(_compatible, new CompatibleDelegate(component.UpdateLayout));
					}
				}
			}
			return _compatible;
		}

		public void AddCompatible(CompatibleDelegate func)
		{
			_compatible = (CompatibleDelegate)Delegate.Combine(_compatible, func);
		}
	}

	public delegate Vector2 CompatibleDelegate(float? x, float? y);

	public interface ICompatible
	{
		Vector2 UpdateLayout(float? x, float? y);
	}

	private struct WidgetSizeChange
	{
		private RectLayout _layout;

		private UIWidget _parent;

		private Point2 _size;

		private Action _onPostUpdate;

		public void Set([NotNull] RectLayout layout, Action onPostUpdate)
		{
			_layout = layout;
			_parent = layout.GetParentWidget();
			_onPostUpdate = onPostUpdate;
			if ((bool)_parent)
			{
				_parent.AddOnChange(OnChange);
			}
		}

		public void Reset()
		{
			if ((bool)_parent)
			{
				UIWidget parent = _parent;
				parent.onChange = (Action)Delegate.Remove(parent.onChange, new Action(OnChange));
			}
			_layout = null;
			_parent = null;
		}

		public bool Valid()
		{
			return _layout != null;
		}

		public bool IsEqual(RectLayout layout)
		{
			return _layout == layout;
		}

		private void OnChange()
		{
			Point2 point = new Point2(_parent.width, _parent.height);
			if (point != _size)
			{
				_size = point;
				_layout.UpdateLayout(_size.x, point.y);
				if (_onPostUpdate != null)
				{
					_onPostUpdate();
				}
			}
		}
	}

	public static bool DontResizeItems;

	[SerializeField]
	private LayoutArgument _layout;

	[SerializeField]
	private WidgetItem[] _items;

	private List<RectArgument> _rects;

	private readonly List<bool> _hiddenBuffer = new List<bool>();

	private List<Rect> _results;

	private WidgetSizeChange _widgetSizeChange;

	public bool HasItems()
	{
		if (_items != null)
		{
			return _items.Length != 0;
		}
		return false;
	}

	public UIWidget GetParentWidget()
	{
		if (_items != null)
		{
			int i = 0;
			for (int num = _items.Length; i < num; i++)
			{
				if (_items[i].Widget != null)
				{
					return _items[i].Widget.transform.parent.GetComponent<UIWidget>();
				}
			}
		}
		return null;
	}

	private void FillHiddenBuffer()
	{
		_hiddenBuffer.Clear();
		int size = KUtility.GetSize(_items);
		for (int i = 0; i < size; i++)
		{
			UIWidget widget = _items[i].Widget;
			bool item = widget != null && (!widget.enabled || !widget.gameObject.activeSelf);
			_hiddenBuffer.Add(item);
		}
	}

	public Vector2 UpdateLayout()
	{
		return UpdateLayout(null, null);
	}

	public Vector2 UpdateLayout(float? width, float? height)
	{
		FillHiddenBuffer();
		GetLayoutRects(ref _rects, out var layout, _hiddenBuffer);
		if (width.HasValue)
		{
			layout.Width = width.Value;
		}
		if (height.HasValue)
		{
			layout.Height = height.Value;
		}
		return UpdateLayout(layout);
	}

	private Vector2 UpdateLayout(LayoutArgument layout)
	{
		RectLayoutCalculator.CalcLayout(layout, _rects, ref _results, out var contentsSize, out var parentSize);
		if (DontResizeItems)
		{
			return parentSize;
		}
		UIWidget parentWidget = GetParentWidget();
		Vector2 vector = Vector2.zero;
		if ((bool)parentWidget)
		{
			parentWidget.SetDimensions((int)parentSize.x, (int)parentSize.y);
			vector = parentWidget.localCenter;
		}
		Vector2 pivotOffset = _layout.GetPivotOffset();
		Vector2 vector2 = parentSize - contentsSize;
		Vector2 vector3 = new Vector2(contentsSize.x * 0.5f, (0f - contentsSize.y) * 0.5f) - vector - new Vector2(vector2.x * (pivotOffset.x - 0.5f), vector2.y * (pivotOffset.y - 0.5f));
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_items); i < size; i++)
		{
			if (!_hiddenBuffer[i])
			{
				Rect rect = _results[num];
				num++;
				UIWidget widget = _items[i].Widget;
				if (!(widget == null))
				{
					Vector3 vector4 = rect.position - vector3;
					widget.SetDimensions((int)rect.width, (int)rect.height);
					Vector2 vector5 = -widget.pivotOffset;
					widget.transform.localPosition = vector4 - Vector3.Scale(vector5, rect.size);
				}
			}
		}
		return parentSize;
	}

	public void GetLayoutRects(ref List<RectArgument> rects, out LayoutArgument layout, IList<bool> isHidden)
	{
		if (rects == null)
		{
			rects = new List<RectArgument>();
		}
		rects.Clear();
		int size = KUtility.GetSize(_items);
		int size2 = KUtility.GetSize(isHidden);
		for (int i = 0; i < size; i++)
		{
			WidgetItem widgetItem = _items[i];
			RectArgument argument = widgetItem.Argument;
			bool num = i >= 0 && i < size2 && isHidden[i];
			UIWidget widget = widgetItem.Widget;
			if (num)
			{
				continue;
			}
			CompatibleDelegate compatibleDelegate = (argument.Compatible = widgetItem.GetCompatible());
			if (widget != null)
			{
				Direction direction = _layout.Direction;
				if (argument.Depth % 2 == 1)
				{
					direction = (Direction)((int)(direction + 1) % 2);
				}
				if (compatibleDelegate == null)
				{
					if (argument.Size.Type == ItemType.Pixel && Math.Abs(argument.Size.Value) < 0.0001f)
					{
						argument.Size.Type = ItemType.Pixel;
						argument.Size.Value = ((direction != 0) ? widget.height : widget.width);
					}
					if (argument.Breadth.Type == ItemType.Pixel && Math.Abs(argument.Breadth.Value) < 0.0001f)
					{
						argument.Breadth.Type = ItemType.Pixel;
						argument.Breadth.Value = ((direction != 0) ? widget.width : widget.height);
					}
				}
			}
			rects.Add(argument);
		}
		layout = _layout;
		UIWidget parentWidget = GetParentWidget();
		if ((bool)parentWidget)
		{
			if (!_layout.OverrideWidth || ((bool)parentWidget.leftAnchor.target && (bool)parentWidget.rightAnchor.target))
			{
				layout.Width = parentWidget.width;
			}
			if (!_layout.OverrideHeight || ((bool)parentWidget.bottomAnchor.target && (bool)parentWidget.topAnchor.target))
			{
				layout.Height = parentWidget.height;
			}
		}
	}

	public void AddCompatible([NotNull] UIWidget widget, CompatibleDelegate func)
	{
		int size = KUtility.GetSize(_items);
		for (int i = 0; i < size; i++)
		{
			WidgetItem widgetItem = _items[i];
			if (widgetItem.Widget == widget)
			{
				widgetItem.AddCompatible(func);
				break;
			}
		}
	}

	public void AddCompatible(int index, CompatibleDelegate func)
	{
		int size = KUtility.GetSize(_items);
		if (index >= 0 && index < size)
		{
			_items[index].AddCompatible(func);
		}
	}

	public void UpdateOnSizeChange(Action onPostUpdate = null)
	{
		if (_widgetSizeChange.Valid())
		{
			if (_widgetSizeChange.IsEqual(this))
			{
				return;
			}
			_widgetSizeChange.Reset();
		}
		_widgetSizeChange.Set(this, onPostUpdate);
	}

	public static Vector2 GetPivotOffset(Pivot pv)
	{
		Vector2 zero = Vector2.zero;
		switch (pv)
		{
		case Pivot.Top:
		case Pivot.Center:
		case Pivot.Bottom:
			zero.x = 0.5f;
			break;
		case Pivot.TopRight:
		case Pivot.Right:
		case Pivot.BottomRight:
			zero.x = 1f;
			break;
		default:
			zero.x = 0f;
			break;
		}
		switch (pv)
		{
		case Pivot.Left:
		case Pivot.Center:
		case Pivot.Right:
			zero.y = 0.5f;
			break;
		case Pivot.TopLeft:
		case Pivot.Top:
		case Pivot.TopRight:
			zero.y = 1f;
			break;
		default:
			zero.y = 0f;
			break;
		}
		return zero;
	}
}
