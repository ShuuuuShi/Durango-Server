using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class KInfiniteScrollView : KScrollViewBase
{
	public interface IView
	{
		int Count { get; }

		int CurrentIndex { get; }

		void Reset();

		void Refresh();

		float GetNodeSize(int index);

		float GetNodeOffset(int index);

		int GetOffsetIndex(float offset);

		bool TryGetLastOffset(out float offset);
	}

	public class View<T, TC> : IView where TC : Component
	{
		private readonly KInfiniteScrollView _scroll;

		private readonly Action<TC, T> _setter;

		private readonly Action<TC> _initFunc;

		private readonly bool _fixedSize;

		private readonly LinkedList<TC> _nodes = new LinkedList<TC>();

		private readonly LinkedList<TC> _pools = new LinkedList<TC>();

		private readonly TC _baseObject;

		private readonly List<float> _offsets;

		[CanBeNull]
		private IList<T> _list;

		public LinkedList<TC> List => _nodes;

		public int Begin { get; private set; }

		public int Count => (_list != null) ? _list.Count : 0;

		public int CurrentIndex { get; private set; }

		public View([NotNull] KInfiniteScrollView scroll, [NotNull] Action<TC, T> setter, Action<TC> initFunc, bool fixedSize)
		{
			_scroll = scroll;
			_setter = setter;
			_initFunc = initFunc;
			_baseObject = _scroll._baseObject.GetComponent<TC>();
			_baseObject.gameObject.SetActive(value: false);
			_fixedSize = fixedSize;
			if (!fixedSize)
			{
				_offsets = new List<float>();
			}
			Begin = -1;
		}

		public int GetOffsetIndex(float offset)
		{
			if (_fixedSize)
			{
				return Mathf.FloorToInt(offset / (_scroll.GetSize(_scroll._baseObject.localSize) + (float)_scroll.Margin));
			}
			if (offset <= 0f)
			{
				return 0;
			}
			int size = KUtility.GetSize(_offsets);
			int num = -1;
			float num2 = 0f;
			for (int i = 1; i < size; i++)
			{
				float num3 = _offsets[i];
				if (num2 <= offset && offset < num3)
				{
					num = i - 1;
					break;
				}
				num2 = num3;
			}
			if (num < 0)
			{
				TC val = (TC)null;
				int count = Count;
				num = count;
				for (int j = size; j <= count; j++)
				{
					if (val == null)
					{
						val = PopNode();
					}
					SetNode(val, j - 1);
					UIWidget component = val.GetComponent<UIWidget>();
					float num4 = num2 + _scroll.GetSize(component) + (float)_scroll.Margin;
					_offsets.Add(num4);
					if (num2 <= offset && offset < num4)
					{
						num = j - 1;
						break;
					}
					num2 = num4;
				}
				if (val != null)
				{
					PushNode(val);
				}
				_scroll.UpdateScrollBounds();
			}
			return num;
		}

		public float GetNodeSize(int index)
		{
			if (_fixedSize)
			{
				return _scroll.GetSize(_scroll._baseObject.localSize);
			}
			float nodeOffset = GetNodeOffset(index);
			float nodeOffset2 = GetNodeOffset(index + 1);
			if (float.IsNaN(nodeOffset) || float.IsNaN(nodeOffset2))
			{
				return 0f;
			}
			return nodeOffset2 - nodeOffset - (float)_scroll.Margin;
		}

		public bool TryGetLastOffset(out float offset)
		{
			if (_fixedSize)
			{
				offset = (float)Count * (_scroll.GetSize(_scroll._baseObject) + (float)_scroll.Margin);
				return true;
			}
			int size = KUtility.GetSize(_offsets);
			int count = Count;
			offset = ((size <= 0) ? 0f : _offsets[size - 1]);
			return size > count;
		}

		public float GetNodeOffset(int index)
		{
			if (_fixedSize)
			{
				float size = _scroll.GetSize(_scroll._baseObject.localSize);
				return (float)index * (size + (float)_scroll.Margin);
			}
			int size2 = KUtility.GetSize(_offsets);
			if (index < size2)
			{
				return _offsets[index];
			}
			TC val = (TC)null;
			float num = _offsets[size2 - 1];
			for (int i = size2; i <= index; i++)
			{
				if (val == null)
				{
					val = PopNode();
				}
				SetNode(val, i - 1);
				UIWidget component = val.GetComponent<UIWidget>();
				float num2 = num + _scroll.GetSize(component) + (float)_scroll.Margin;
				_offsets.Add(num2);
				num = num2;
			}
			if (val != null)
			{
				PushNode(val);
			}
			_scroll.UpdateScrollBounds();
			return _offsets[index];
		}

		public void Redraw()
		{
			Begin = -1;
			Refresh();
		}

		public void Reset()
		{
			Begin = -1;
			if (_offsets != null)
			{
				_offsets.Clear();
				_offsets.Add(0f);
			}
		}

		public void Refresh()
		{
			if (!_scroll._isAwake)
			{
				return;
			}
			float num = _scroll.CurrentOffset - (float)_scroll.Padding;
			int num2 = Mathf.Max(0, GetOffsetIndex(num));
			if (Begin >= 0 && Begin < Count)
			{
				if (num2 < Begin)
				{
					float num3 = 0f;
					LinkedListNode<TC> linkedListNode = null;
					for (int i = num2; i < Begin; i++)
					{
						TC val = PopNode();
						SetNode(val, i);
						linkedListNode = ((linkedListNode != null) ? _nodes.AddAfter(linkedListNode, val) : _nodes.AddFirst(val));
						num3 += _scroll.GetSize(val.GetComponent<UIWidget>());
						if (num3 > _scroll.ViewLength)
						{
							break;
						}
					}
				}
				else if (Begin < num2)
				{
					for (int j = Begin; j < num2; j++)
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
			bool flag = Begin != num2;
			Begin = num2;
			float num4 = GetNodeOffset(num2);
			float num5 = num + _scroll.ViewLength;
			int num6 = 0;
			LinkedListNode<TC> linkedListNode2 = _nodes.First;
			int count = Count;
			while (num4 < num5 && num2 + num6 < count)
			{
				TC val2;
				if (linkedListNode2 == null)
				{
					val2 = PopNode();
					SetNode(val2, num2 + num6);
					_nodes.AddLast(val2);
					flag = true;
				}
				else
				{
					val2 = linkedListNode2.Value;
					linkedListNode2 = linkedListNode2.Next;
				}
				UIWidget component = val2.GetComponent<UIWidget>();
				num4 += _scroll.GetSize(component) + (float)_scroll.Margin;
				if (!_fixedSize && _offsets.Count <= num2 + num6 + 1)
				{
					_offsets.Add(num4);
					_scroll.UpdateScrollBounds();
				}
				num6++;
			}
			int num7 = _nodes.Count - num6;
			for (int k = 0; k < num7; k++)
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
			Vector2 vector = _scroll.Vector;
			Vector2 pivot = Vector2.one * 0.5f;
			if (vector.x > 0f)
			{
				pivot.x = 0f;
			}
			else if (vector.x < 0f)
			{
				pivot.x = 1f;
			}
			if (vector.y > 0f)
			{
				pivot.y = 0f;
			}
			else if (vector.y < 0f)
			{
				pivot.y = 1f;
			}
			basePosition += _scroll.Vector * GetNodeOffset(num2);
			foreach (TC node2 in _nodes)
			{
				TC current2 = node2;
				current2.GetComponent<UIWidget>().alpha = 1f;
			}
			foreach (TC pool in _pools)
			{
				TC current3 = pool;
				UIWidget component2 = current3.GetComponent<UIWidget>();
				component2.alpha = 0f;
				component2.SetPosition(basePosition, pivot);
			}
			if (_nodes.Count > 0)
			{
				UIUtility.WidgetsReposition(_nodes, _scroll.Vector, basePosition, _scroll.Margin);
			}
		}

		private TC PopNode()
		{
			TC val;
			if (_pools.Count == 0)
			{
				TC baseObject = _baseObject;
				GameObject gameObject = baseObject.gameObject;
				TC baseObject2 = _baseObject;
				val = UnityEngine.Object.Instantiate(gameObject, baseObject2.transform.parent).GetComponent<TC>();
				Transform transform = val.transform;
				TC baseObject3 = _baseObject;
				transform.localPosition = baseObject3.transform.localPosition;
				Transform transform2 = val.transform;
				TC baseObject4 = _baseObject;
				transform2.localScale = baseObject4.transform.localScale;
				Transform transform3 = val.transform;
				TC baseObject5 = _baseObject;
				transform3.localRotation = baseObject5.transform.localRotation;
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
				TC current = node;
				current.GetComponent<UIWidget>().SetDimensions(size.x, size.y);
			}
			foreach (TC pool in _pools)
			{
				TC current2 = pool;
				current2.GetComponent<UIWidget>().SetDimensions(size.x, size.y);
			}
		}
	}

	[SerializeField]
	private UIWidget _baseObject;

	[SerializeField]
	private bool _isDynamicSize;

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
		return (_view != null) ? _view.GetNodeSize(index) : 0f;
	}

	protected override int CalcNodeIndex(float offset)
	{
		return (_view != null) ? _view.GetOffsetIndex(offset) : 0;
	}

	public override UIWidget GetNode(int index)
	{
		throw new NotImplementedException();
	}

	public override int GetNodeCount()
	{
		return (_view != null) ? _view.Count : 0;
	}

	public override float GetNodeOffset(int index)
	{
		return (_view != null) ? _view.GetNodeOffset(index) : 0f;
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

	protected override void OnUpdatePositioMoveToOption(PositionOption option)
	{
		if (_isDynamicSize && option.MoveTo.HasValue)
		{
			MoveToOption value = option.MoveTo.Value;
			if (value.Type == MoveToType.Offset)
			{
				CalcNodeIndex(value.Offset);
			}
		}
		base.OnUpdatePositioMoveToOption(option);
	}

	private void UpdateScrollBounds()
	{
		float offset;
		if (_view == null)
		{
			base.MaxOffset = 0f;
			base.ContentsLength = 0f;
			Bounds scrollBounds = GetScrollBounds();
			base.ScrollView.SetFixedBounds(scrollBounds);
		}
		else if (_view.TryGetLastOffset(out offset))
		{
			base.ContentsLength = Mathf.Max(0f, offset - (float)base.Margin + (float)base.Padding + (float)base.EndPadding);
			base.MaxOffset = Mathf.Max(0f, base.ContentsLength - base.ViewLength);
			Bounds scrollBounds2 = GetScrollBounds();
			base.ScrollView.SetFixedBounds(scrollBounds2);
		}
		else
		{
			float b = offset + (float)base.Padding + (float)base.EndPadding;
			base.MaxOffset = Mathf.Max(0f, b);
			Bounds scrollBounds3 = GetScrollBounds();
			base.ScrollView.SetFixedBounds(scrollBounds3);
			base.MaxOffset = float.MaxValue;
			base.ContentsLength = float.MaxValue;
		}
	}

	public View<T, TC> Initialize<T, TC>([NotNull] Action<TC, T> setter, Action<TC> initFunc = null) where TC : Component
	{
		return (View<T, TC>)(_view = new View<T, TC>(this, setter, initFunc, !_isDynamicSize));
	}
}
