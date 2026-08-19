using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class KGridInfiniteScrollView : KScrollViewBase
{
	public interface IView
	{
		int Count { get; }

		int CurrentIndex { get; }

		int RowItemCount { get; }

		void Reset();

		void Refresh();

		float GetNodeSize();

		float GetNodeOffset(int index, float pivot);

		int GetOffsetIndex(float offset);
	}

	public class View<T, TC> : IView where TC : Component
	{
		private readonly KGridInfiniteScrollView _scroll;

		private readonly Action<TC, T> _setter;

		private readonly Action<TC> _initFunc;

		private readonly LinkedList<TC> _nodes = new LinkedList<TC>();

		private readonly LinkedList<TC> _pools = new LinkedList<TC>();

		private readonly TC _baseObject;

		private Vector2 _nodeSize;

		private float _colSize;

		[CanBeNull]
		private IList<T> _list;

		public LinkedList<TC> List => _nodes;

		public int Begin { get; private set; }

		public int Count
		{
			get
			{
				if (_list == null)
				{
					return 0;
				}
				return _list.Count;
			}
		}

		public int CurrentIndex { get; private set; }

		public int RowItemCount { get; private set; }

		public View([NotNull] KGridInfiniteScrollView scroll, [NotNull] Action<TC, T> setter, Action<TC> initFunc)
		{
			_scroll = scroll;
			_setter = setter;
			_initFunc = initFunc;
			_baseObject = _scroll._baseObject.GetComponent<TC>();
			_baseObject.gameObject.SetActive(value: false);
			Begin = -1;
			SetNodeSize(_scroll._baseObject.localSize);
		}

		public int GetOffsetIndex(float offset)
		{
			if (RowItemCount > 0)
			{
				float nodeSize = GetNodeSize();
				return Mathf.FloorToInt((offset + (float)_scroll._rowMargin) / (nodeSize + (float)_scroll._rowMargin)) * RowItemCount;
			}
			return 0;
		}

		public float GetNodeSize()
		{
			return _colSize;
		}

		public float GetNodeOffset(int index, float pivot)
		{
			int num = Mathf.FloorToInt((float)index / (float)RowItemCount);
			float nodeSize = GetNodeSize();
			return (float)num * (nodeSize + (float)_scroll._rowMargin) + nodeSize * pivot;
		}

		public void Reset()
		{
			Begin = -1;
			CalcRowItemCount();
		}

		public void SetNodeSize(Vector2 size)
		{
			_nodeSize = size;
			CalcRowItemCount();
			Point2 point = new Point2(size);
			_baseObject.GetComponent<UIWidget>().SetDimensions(point.x, point.y);
			foreach (TC node in _nodes)
			{
				node.GetComponent<UIWidget>().SetDimensions(point.x, point.y);
			}
			foreach (TC pool in _pools)
			{
				pool.GetComponent<UIWidget>().SetDimensions(point.x, point.y);
			}
		}

		private void CalcRowItemCount()
		{
			Vector2 nodeSize = _nodeSize;
			Vector3 vector = ((((Vector2)_scroll.Vector).x != 0f) ? Vector3.down : Vector3.right);
			float size = UIUtility.GetSize(nodeSize, vector);
			float viewBreadth = _scroll.ViewBreadth;
			float f = (viewBreadth + (float)_scroll.Margin) / (size + (float)_scroll.Margin);
			RowItemCount = Mathf.Max(1, Mathf.RoundToInt(f));
			_colSize = UIUtility.GetSize(_nodeSize, _scroll.Vector) * ((viewBreadth - (float)(_scroll.Margin * (RowItemCount - 1))) / ((float)RowItemCount * size));
		}

		public void Refresh()
		{
			if (!_scroll._isAwake)
			{
				return;
			}
			float offset = _scroll.CurrentOffset - (float)_scroll.Padding;
			int num = Mathf.Max(0, GetOffsetIndex(offset));
			if (Begin >= 0 && Begin < Count)
			{
				if (num < Begin)
				{
					float num2 = 0f;
					LinkedListNode<TC> linkedListNode = null;
					for (int i = num; i < Begin; i++)
					{
						TC val = PopNode();
						SetNode(val, i);
						linkedListNode = ((linkedListNode != null) ? _nodes.AddAfter(linkedListNode, val) : _nodes.AddFirst(val));
						num2 += _scroll.GetSize(val.GetComponent<UIWidget>());
						if (num2 > _scroll.ViewLength)
						{
							break;
						}
					}
				}
				else if (Begin < num)
				{
					for (int j = Begin; j < num; j++)
					{
						if (_nodes.Count == 0)
						{
							break;
						}
						PushNode(_nodes.First.Value);
						_nodes.RemoveFirst();
					}
				}
			}
			else
			{
				foreach (TC node in _nodes)
				{
					PushNode(node);
				}
				_nodes.Clear();
			}
			bool flag = Begin != num;
			Begin = num;
			LinkedListNode<TC> linkedListNode2 = _nodes.First;
			int num3 = Mathf.CeilToInt(_scroll.ViewLength / (GetNodeSize() + (float)_scroll._rowMargin) + 1f) * RowItemCount;
			int num4 = Mathf.Min(Count, num + num3);
			int k;
			for (k = 0; num + k < num4; k++)
			{
				if (linkedListNode2 == null)
				{
					TC val2 = PopNode();
					SetNode(val2, num + k);
					_nodes.AddLast(val2);
					flag = true;
				}
				else
				{
					linkedListNode2 = linkedListNode2.Next;
				}
			}
			int num5 = _nodes.Count - k;
			for (int l = 0; l < num5; l++)
			{
				PushNode(_nodes.Last.Value);
				_nodes.RemoveLast();
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			Vector3 basePosition = _scroll.GetBasePosition();
			basePosition += _scroll.Vector * GetNodeOffset(num, 0f);
			foreach (TC node2 in _nodes)
			{
				node2.GetComponent<UIWidget>().alpha = 1f;
			}
			foreach (TC pool in _pools)
			{
				pool.GetComponent<UIWidget>().alpha = 0f;
			}
			if (_nodes.Count > 0)
			{
				if (_scroll.Vector.x == 0f)
				{
					basePosition.x -= _scroll.ViewBreadth * 0.5f;
				}
				else
				{
					basePosition.y += _scroll.ViewBreadth * 0.5f;
				}
				UIUtility.WidgetsGridReposition(_nodes, null, _scroll.Vector, basePosition, UIUtility.GetBreadth(_scroll.ViewSize, _scroll.Vector), _nodeSize, _scroll._rowMargin, _scroll.Margin);
			}
		}

		private TC PopNode()
		{
			TC val;
			if (_pools.Count == 0)
			{
				GameObject gameObject = _baseObject.gameObject;
				TC baseObject = _baseObject;
				val = UnityEngine.Object.Instantiate(gameObject, baseObject.transform.parent).GetComponent<TC>();
				Transform transform = val.transform;
				TC baseObject2 = _baseObject;
				transform.localPosition = baseObject2.transform.localPosition;
				Transform transform2 = val.transform;
				TC baseObject3 = _baseObject;
				transform2.localScale = baseObject3.transform.localScale;
				Transform transform3 = val.transform;
				TC baseObject4 = _baseObject;
				transform3.localRotation = baseObject4.transform.localRotation;
				if (_initFunc != null)
				{
					_initFunc(val);
				}
				val.gameObject.SetActive(value: true);
			}
			else
			{
				val = _pools.Last.Value;
				_pools.RemoveLast();
			}
			return val;
		}

		private void PushNode(TC node)
		{
			_pools.AddLast(node);
		}

		private void SetNode(TC node, int index)
		{
			CurrentIndex = index;
			_setter(node, _list[index]);
		}

		public void SetList(IList<T> list)
		{
			_list = list;
			Reset();
			Refresh();
		}

		public int IndexOf(TC obj)
		{
			int num = -1;
			int num2 = 0;
			foreach (TC node in _nodes)
			{
				if (node == obj)
				{
					num = num2;
					break;
				}
				num2++;
			}
			if (num == -1)
			{
				return -1;
			}
			return Begin + num;
		}

		public void NodeResize(Point2 size)
		{
			_scroll._baseObject.SetDimensions(size.x, size.y);
			foreach (TC node in _nodes)
			{
				node.GetComponent<UIWidget>().SetDimensions(size.x, size.y);
			}
			foreach (TC pool in _pools)
			{
				pool.GetComponent<UIWidget>().SetDimensions(size.x, size.y);
			}
		}
	}

	[SerializeField]
	private int _rowMargin;

	[SerializeField]
	private UIWidget _baseObject;

	private bool _isAwake;

	private IView _view;

	private void Awake()
	{
		_isAwake = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_view != null)
		{
			_view.Reset();
		}
	}

	protected override void OnClipMove(UIPanel panel)
	{
		base.OnClipMove(panel);
		if (_view != null)
		{
			_view.Refresh();
		}
	}

	protected override float GetNodeSize(int index)
	{
		if (_view == null)
		{
			return 0f;
		}
		return _view.GetNodeSize();
	}

	protected override int CalcNodeIndex(float offset)
	{
		if (_view == null)
		{
			return 0;
		}
		return _view.GetOffsetIndex(offset);
	}

	public override UIWidget GetNode(int index)
	{
		throw new NotImplementedException();
	}

	public override int GetNodeCount()
	{
		if (_view == null)
		{
			return 0;
		}
		return _view.Count;
	}

	public override float GetNodeOffset(int index)
	{
		if (_view == null)
		{
			return 0f;
		}
		return _view.GetNodeOffset(index, 0f);
	}

	public override void UpdateLayout(bool instant = true)
	{
		if (_view != null)
		{
			_view.Reset();
			_view.Refresh();
		}
		UpdateScrollBounds();
	}

	protected override float OnUpdateLayout(bool instant)
	{
		throw new NotImplementedException();
	}

	protected override int ToIntOffset(int currentIndex, int sign)
	{
		if (_view.RowItemCount > 0)
		{
			int num = Mathf.Max(Mathf.FloorToInt((float)currentIndex / (float)_view.RowItemCount), 0);
			return Mathf.Max(num, num + sign) * _view.RowItemCount;
		}
		return base.ToIntOffset(currentIndex, sign);
	}

	private void UpdateScrollBounds()
	{
		if (_view == null)
		{
			base.MaxOffset = 0f;
			base.ScrollView.SetFixedBounds(GetScrollBounds());
			return;
		}
		float num = _view.GetNodeOffset(_view.Count - 1, 1f) + (float)base.Padding + (float)base.EndPadding;
		base.MaxOffset = Mathf.Max(0f, num - base.ViewLength);
		Bounds scrollBounds = GetScrollBounds();
		base.ScrollView.SetFixedBounds(scrollBounds);
	}

	public View<T, TC> Initialize<T, TC>([NotNull] Action<TC, T> setter, Action<TC> initFunc = null) where TC : Component
	{
		return (View<T, TC>)(_view = new View<T, TC>(this, setter, initFunc));
	}
}
