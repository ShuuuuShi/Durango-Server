using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ListScroll : KScrollViewBase
{
	private interface IView
	{
		int Count { get; }

		void Reset();

		void Refresh();
	}

	public class View<T, TC> : IView where TC : Component
	{
		private readonly ListScroll _scroll;

		private readonly Action<TC, T> _setter;

		private readonly Action<TC> _initFunc;

		private readonly GameObjectPool<TC> _nodes;

		private List<T> _list;

		private int _begin;

		private int _end;

		public int Count => (_list != null) ? _list.Count : 0;

		public List<TC> List => _nodes.List;

		public View([NotNull] ListScroll scroll, [NotNull] Action<TC, T> setter, Action<TC> initFunc)
		{
			_scroll = scroll;
			_setter = setter;
			_initFunc = initFunc;
			_nodes = new GameObjectPool<TC>(((Component)_scroll._baseObject).GetComponent<TC>(), InitNodeFunc);
			UIPanel panel = _scroll.Panel;
			panel.onClipMove = (UIPanel.OnClippingMoved)Delegate.Combine(panel.onClipMove, new UIPanel.OnClippingMoved(OnClicpMove));
			_begin = -1;
			_end = -1;
		}

		private void InitNodeFunc(TC comp)
		{
			if (_initFunc != null)
			{
				_initFunc(comp);
			}
			((Component)comp).gameObject.SetActive(true);
		}

		private void OnClicpMove(UIPanel panel)
		{
			Refresh();
		}

		public void SetList(List<T> list)
		{
			_list = list;
			Reset();
			Refresh();
		}

		public int IndexOf(TC obj)
		{
			List<TC> list = _nodes.List;
			int num = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if ((Object)(object)list[i] == (Object)(object)obj)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				return -1;
			}
			return _begin + num;
		}

		public void Reset()
		{
			_begin = -1;
			_end = -1;
		}

		public void Refresh()
		{
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			if (_list == null)
			{
				return;
			}
			float currentOffset = _scroll.CurrentOffset;
			float num = currentOffset / _scroll.NodeSize;
			float num2 = (currentOffset + _scroll.BoxSize) / _scroll.NodeSize;
			int num3 = Mathf.Max(Mathf.FloorToInt(num) - 1, 0);
			int num4 = Mathf.Min(Mathf.CeilToInt(num2) + 1, _list.Count);
			if (_begin == num3 && _end == num4)
			{
				return;
			}
			int num5 = Mathf.Max(0, num4 - num3);
			if (_begin < 0 || _end < 0 || _end <= num3 || _begin >= num4)
			{
				_nodes.Clear();
				for (int i = 0; i < num5; i++)
				{
					TC arg = _nodes.Pop();
					_setter(arg, _list[num3 + i]);
				}
			}
			else
			{
				if (_end > num3 && _end <= num4 && _begin <= num3)
				{
					for (int j = 0; j < num3 - _begin; j++)
					{
						_nodes.PushAt(0);
					}
					for (int k = 0; k < num4 - _end; k++)
					{
						TC arg2 = _nodes.Pop();
						_setter(arg2, _list[_end + k]);
					}
				}
				if (_begin >= num3 && _begin < num4 && _end >= num4)
				{
					for (int l = 0; l < _end - num4; l++)
					{
						_nodes.PushAt(num4 - _begin);
					}
					for (int m = 0; m < _begin - num3; m++)
					{
						TC arg3 = _nodes.Insert(m);
						_setter(arg3, _list[num3 + m]);
					}
				}
			}
			Vector3 basePosition = _scroll.GetBasePosition();
			basePosition += _scroll.Vector * (((float)num3 + 0.5f) * _scroll.NodeSize);
			for (int n = 0; n < _nodes.Count; n++)
			{
				((Component)_nodes[n]).GetComponent<UIWidget>().alpha = 1f;
			}
			for (int num6 = 0; num6 < _nodes.Pool.Count; num6++)
			{
				UIWidget component = ((Component)_nodes.Pool[num6]).GetComponent<UIWidget>();
				component.alpha = 0f;
				component.SetPosition(basePosition, 0.5f, 0.5f);
			}
			if (num5 > 0)
			{
				UIWidget component2 = ((Component)_nodes[0]).GetComponent<UIWidget>();
				component2.SetPosition(basePosition, 0.5f, 0.5f);
				UIUtility.WidgetsReposition(_nodes.Get, num5, _scroll.Vector, component2, _scroll.Margin);
			}
			_end = num4;
			_begin = num3;
		}
	}

	[SerializeField]
	private UIWidget _baseObject;

	private Vector2 _nodeSize;

	private IView _view;

	public float NodeSize { get; private set; }

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_nodeSize = _baseObject.localSize;
		NodeSize = GetSize(_nodeSize);
	}

	private void OnDisable()
	{
		if (_view != null)
		{
			_view.Reset();
		}
	}

	protected override Vector2 GetNodeSize(int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _nodeSize;
	}

	public override UIWidget GetNode(int index)
	{
		throw new NotImplementedException();
	}

	public override int GetNodeCount()
	{
		return (_view != null) ? _view.Count : 0;
	}

	protected override float OnUpdateLayout(bool instant)
	{
		if (_view == null)
		{
			return 0f;
		}
		_view.Refresh();
		return (float)_view.Count * NodeSize;
	}

	protected override void MakeEndPaddingWidget(int padding)
	{
		base.MakeEndPaddingWidget(Mathf.Max(2, padding));
	}

	public View<T, TC> Initialize<T, TC>([NotNull] Action<TC, T> setter, Action<TC> initFunc = null) where TC : Component
	{
		return (View<T, TC>)(_view = new View<T, TC>(this, setter, initFunc));
	}
}
