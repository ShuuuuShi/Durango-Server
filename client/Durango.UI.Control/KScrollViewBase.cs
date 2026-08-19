using System;
using UnityEngine;

namespace Durango.UI.Control;

public abstract class KScrollViewBase : MonoBehaviour
{
	public enum Direction
	{
		Auto,
		Vertical,
		Horizontal
	}

	protected struct PositionOption
	{
		public bool UpdateLayout;

		public MoveToOption? MoveTo;
	}

	protected struct MoveToOption
	{
		public MoveToType Type;

		public float Offset;

		public int Index;

		public float BeginPadding;

		public float EndPadding;

		public bool Instant;

		public bool RestrictWithinPanel;

		public Action OnFinish;
	}

	protected enum MoveToType
	{
		Offset,
		BeginIndex,
		EndIndex,
		VisibleArea
	}

	[SerializeField]
	private Direction _direction;

	[SerializeField]
	private bool _alwaysIntOffset;

	[SerializeField]
	private int _enableResetIndex = -1;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private int _margin;

	[SerializeField]
	private int _padding;

	[SerializeField]
	private int _scrollEndPadding;

	[SerializeField]
	private int _dragOnLastMargin;

	[SerializeField]
	private float _resetOffset;

	private Action _moveToOnFinish;

	private PositionOption _positionOption;

	private Vector4 _panelClip;

	private Vector2? _viewSize;

	private bool _isEnable;

	public UIScrollView ScrollView => _scrollView;

	public UIPanel Panel
	{
		get
		{
			UIPanel uIPanel = _scrollView.panel;
			if (uIPanel == null)
			{
				uIPanel = _scrollView.GetComponent<UIPanel>();
			}
			return uIPanel;
		}
	}

	public int Margin => _margin;

	public int Padding
	{
		get
		{
			return _padding;
		}
		set
		{
			_padding = value;
		}
	}

	public int EndPadding
	{
		get
		{
			return _scrollEndPadding;
		}
		set
		{
			_scrollEndPadding = value;
		}
	}

	public float MaxOffset { get; protected set; }

	public float ContentsLength { get; protected set; }

	public Direction Dir
	{
		get
		{
			if (_direction == Direction.Auto)
			{
				return _scrollView.movement switch
				{
					UIScrollView.Movement.Vertical => Direction.Vertical, 
					UIScrollView.Movement.Horizontal => Direction.Horizontal, 
					_ => throw new ArgumentException("Invalid Direction"), 
				};
			}
			return _direction;
		}
	}

	public float OffsetRatio => CurrentOffset / (GetNodeSize(0) + (float)Margin);

	public float CurrentOffset => CalcOffset(_scrollView.transform.localPosition);

	public float GoalOffset
	{
		get
		{
			SpringPanel component = _scrollView.GetComponent<SpringPanel>();
			Vector3 pos = ((!(component == null) && component.enabled) ? component.target : _scrollView.transform.localPosition);
			return CalcOffset(pos);
		}
	}

	public Vector2 ViewSize
	{
		get
		{
			Vector2? viewSize = _viewSize;
			if (!viewSize.HasValue)
			{
				UpdateViewSize();
			}
			return _viewSize.Value;
		}
	}

	public float ViewLength => GetSize(ViewSize);

	public float ViewBreadth
	{
		get
		{
			Vector2 vc = new Vector2(Vector.y, Vector.x);
			return UIUtility.GetSize(ViewSize, vc);
		}
	}

	protected Vector3 Vector => Dir switch
	{
		Direction.Vertical => Vector3.up * GetSign(), 
		Direction.Horizontal => Vector3.right * GetSign(), 
		_ => throw new ArgumentException("Invalid Vector"), 
	};

	public event Action DragFinishedOnFirst;

	public event Action DragFinishedOnLast;

	public event Action DragFinshed;

	public int GetCurrentNodeIndex()
	{
		return CalcNodeIndex(CurrentOffset);
	}

	public int GetGoalNodeIndex()
	{
		return CalcNodeIndex(GoalOffset);
	}

	private void Start()
	{
		_scrollView.onDragFinished = OnDragFinished;
		UpdateViewSize();
	}

	private void Reset()
	{
		_scrollView = GetComponent<UIScrollView>();
	}

	private float CalcOffset(Vector3 pos)
	{
		Vector2 vector = -pos;
		float num = 0f;
		switch (Dir)
		{
		case Direction.Vertical:
			num = vector.y;
			break;
		case Direction.Horizontal:
			num = vector.x;
			break;
		}
		return num * (float)GetSign();
	}

	protected virtual int CalcNodeIndex(float offset)
	{
		int nodeCount = GetNodeCount();
		int result = nodeCount - 1;
		float num = 0f;
		for (int i = 0; i < nodeCount; i++)
		{
			if (num > offset)
			{
				result = Mathf.Max(0, i - 1);
				break;
			}
			num += GetNodeSize(i) + (float)_margin;
		}
		return result;
	}

	protected virtual void OnClipMove(UIPanel panel)
	{
		UpdateViewSize();
	}

	public void UpdateViewSize()
	{
		UIPanel panel = Panel;
		Vector4 baseClipRegion = panel.baseClipRegion;
		if (baseClipRegion == _panelClip)
		{
			return;
		}
		_panelClip = baseClipRegion;
		Vector2 value = UIUtility.PanelInnerSize(panel);
		bool flag = true;
		bool flag2 = true;
		if (_viewSize.HasValue)
		{
			flag = _viewSize.Value.x != value.x;
			flag2 = _viewSize.Value.y != value.y;
		}
		else
		{
			panel.onClipMove = (UIPanel.OnClippingMoved)Delegate.Combine(panel.onClipMove, new UIPanel.OnClippingMoved(OnClipMove));
		}
		_viewSize = value;
		_positionOption.UpdateLayout = true;
		if ((flag || flag2) && flag && flag2)
		{
			panel.transform.localPosition = Vector3.zero;
			panel.clipOffset = Vector2.zero;
			panel.UpdateAnchors();
			MoveToOption? moveTo = _positionOption.MoveTo;
			if (!moveTo.HasValue)
			{
				_positionOption.MoveTo = new MoveToOption
				{
					Type = MoveToType.Offset,
					Offset = _resetOffset,
					RestrictWithinPanel = true,
					Instant = true
				};
			}
		}
		OnUpdateViewSize();
		UpdatePositionOption();
	}

	protected virtual void OnUpdateViewSize()
	{
	}

	private void OnDragFinished()
	{
		float currentOffset = CurrentOffset;
		if (currentOffset < 0f)
		{
			if (this.DragFinishedOnFirst != null)
			{
				this.DragFinishedOnFirst();
			}
		}
		else if (currentOffset > MaxOffset + (float)_scrollEndPadding - (float)_dragOnLastMargin && this.DragFinishedOnLast != null)
		{
			this.DragFinishedOnLast();
		}
		if (_alwaysIntOffset)
		{
			int sign = 0;
			Vector2 vector = _scrollView.currentMomentum;
			if (Vector.x != 0f)
			{
				if (Mathf.Abs(vector.x) > 0.01f)
				{
					sign = -(int)Mathf.Sign(Vector.x * vector.x);
				}
			}
			else if (Mathf.Abs(vector.y) > 0.01f)
			{
				sign = -(int)Mathf.Sign(Vector.y * vector.y);
			}
			MoveToNode(ToIntOffset(GetCurrentNodeIndex(), sign), instant: false);
		}
		if (this.DragFinshed != null)
		{
			this.DragFinshed();
		}
	}

	protected virtual int ToIntOffset(int currentIndex, int sign)
	{
		return Mathf.Max(currentIndex, currentIndex + sign);
	}

	protected virtual void OnEnable()
	{
		if (_enableResetIndex >= 0)
		{
			_positionOption.UpdateLayout = true;
			MoveToNode(_enableResetIndex, instant: true);
		}
		_isEnable = true;
		UpdatePositionOption();
	}

	protected virtual void OnDisable()
	{
		_isEnable = false;
	}

	public void MoveTo(float offset, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		MoveToOption moveToOption = default(MoveToOption);
		moveToOption.Type = MoveToType.Offset;
		moveToOption.Offset = offset;
		moveToOption.RestrictWithinPanel = restrictWithinPanel;
		moveToOption.OnFinish = onFinish;
		moveToOption.Instant = instant;
		MoveToOption value = moveToOption;
		_positionOption.MoveTo = value;
		UpdatePositionOption();
	}

	public void MoveToNode(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		MoveToOption moveToOption = default(MoveToOption);
		moveToOption.Type = MoveToType.BeginIndex;
		moveToOption.Index = index;
		moveToOption.RestrictWithinPanel = restrictWithinPanel;
		moveToOption.OnFinish = onFinish;
		moveToOption.Instant = instant;
		MoveToOption value = moveToOption;
		_positionOption.MoveTo = value;
		UpdatePositionOption();
	}

	public void MoveToEnd(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		MoveToOption moveToOption = default(MoveToOption);
		moveToOption.Type = MoveToType.EndIndex;
		moveToOption.Index = index;
		moveToOption.RestrictWithinPanel = restrictWithinPanel;
		moveToOption.OnFinish = onFinish;
		moveToOption.Instant = instant;
		MoveToOption value = moveToOption;
		_positionOption.MoveTo = value;
		UpdatePositionOption();
	}

	public void MoveToVisibleArea(int index, bool instant, float beginPadding = 0f, float endPadding = 0f, bool restrictWithinPanel = true, Action onFinish = null)
	{
		MoveToOption moveToOption = default(MoveToOption);
		moveToOption.Type = MoveToType.VisibleArea;
		moveToOption.Index = index;
		moveToOption.BeginPadding = beginPadding;
		moveToOption.EndPadding = endPadding;
		moveToOption.RestrictWithinPanel = restrictWithinPanel;
		moveToOption.OnFinish = onFinish;
		moveToOption.Instant = instant;
		MoveToOption value = moveToOption;
		_positionOption.MoveTo = value;
		UpdatePositionOption();
	}

	private void DoMoveTo(MoveToOption option)
	{
		switch (option.Type)
		{
		case MoveToType.Offset:
			MoveToImpl(option.Offset, option.Instant, option.RestrictWithinPanel, option.OnFinish);
			break;
		case MoveToType.BeginIndex:
		{
			int num6 = Mathf.Clamp(option.Index, 0, GetNodeCount() - 1);
			if (num6 >= 0)
			{
				MoveToImpl(GetNodeOffset(num6), option.Instant, option.RestrictWithinPanel, option.OnFinish);
			}
			break;
		}
		case MoveToType.EndIndex:
		{
			int num7 = Mathf.Clamp(option.Index, 0, GetNodeCount() - 1);
			if (num7 >= 0)
			{
				float offset = GetNodeOffset(num7) - ViewLength + GetNodeSize(num7);
				MoveToImpl(offset, option.Instant, option.RestrictWithinPanel, option.OnFinish);
			}
			break;
		}
		case MoveToType.VisibleArea:
		{
			int num = Mathf.Clamp(option.Index, 0, GetNodeCount() - 1);
			if (num < 0)
			{
				break;
			}
			float currentOffset = CurrentOffset;
			float num2 = currentOffset + option.BeginPadding;
			float nodeOffset = GetNodeOffset(num);
			if (nodeOffset < num2)
			{
				MoveToImpl(nodeOffset - option.BeginPadding, option.Instant, option.RestrictWithinPanel, option.OnFinish);
				break;
			}
			float num3 = option.EndPadding + (float)_scrollEndPadding;
			float num4 = currentOffset + ViewLength - num3;
			float num5 = nodeOffset + GetNodeSize(num);
			if (num5 > num4)
			{
				MoveToImpl(num5 - ViewLength + num3, option.Instant, option.RestrictWithinPanel, option.OnFinish);
			}
			break;
		}
		}
	}

	private void MoveToImpl(float offset, bool instant, bool restrictWithinPanel, Action onFinish)
	{
		Vector3 vector = Vector;
		_moveToOnFinish = onFinish;
		if (restrictWithinPanel)
		{
			offset = Mathf.Min(offset, MaxOffset);
			offset = Mathf.Max(0f, offset);
		}
		UIPanel panel = Panel;
		Vector3 zero = Vector3.zero;
		Vector2 zero2 = Vector2.zero;
		switch (Dir)
		{
		case Direction.Vertical:
			zero.x = panel.transform.localPosition.x;
			zero2.x = panel.clipOffset.x;
			break;
		case Direction.Horizontal:
			zero.y = panel.transform.localPosition.y;
			zero2.y = panel.clipOffset.y;
			break;
		}
		Vector3 vector2 = zero - vector * offset;
		if (instant)
		{
			_scrollView.DisableSpring();
			panel.transform.localPosition = vector2;
			panel.clipOffset = zero2 + (Vector2)(vector * offset);
			_scrollView.currentMomentum = Vector3.zero;
			_scrollView.UpdateScrollbars(recalculateBounds: false);
			OnFinishMoveTo();
		}
		else
		{
			SpringPanel springPanel = SpringPanel.Begin(panel.gameObject, vector2, 8f);
			springPanel.onFinished = OnFinishMoveTo;
		}
	}

	private void OnFinishMoveTo()
	{
		if (_moveToOnFinish != null)
		{
			_moveToOnFinish();
		}
	}

	public void ResetPosition()
	{
		Reposition(resetPosition: true, tween: false);
	}

	public void Reposition(bool resetPosition = false, bool tween = true)
	{
		_positionOption.UpdateLayout = true;
		_positionOption.MoveTo = new MoveToOption
		{
			Type = MoveToType.Offset,
			Offset = ((!resetPosition) ? CurrentOffset : _resetOffset),
			RestrictWithinPanel = true,
			Instant = !tween
		};
		UpdatePositionOption();
	}

	private void UpdatePositionOption()
	{
		int num;
		if (_isEnable)
		{
			Vector2? viewSize = _viewSize;
			num = (viewSize.HasValue ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		if (_positionOption.UpdateLayout && flag)
		{
			OnUpdatePositionLayoutOption(_positionOption);
		}
		if (_positionOption.MoveTo.HasValue)
		{
			if (flag)
			{
				OnUpdatePositioMoveToOption(_positionOption);
			}
			else
			{
				MoveToOption value = _positionOption.MoveTo.Value;
				value.Instant = true;
				_positionOption.MoveTo = value;
			}
		}
		if (flag)
		{
			_positionOption = default(PositionOption);
		}
	}

	protected virtual void OnUpdatePositionLayoutOption(PositionOption option)
	{
		UpdateLayout();
	}

	protected virtual void OnUpdatePositioMoveToOption(PositionOption option)
	{
		MoveToOption? moveTo = option.MoveTo;
		if (moveTo.HasValue)
		{
			DoMoveTo(option.MoveTo.Value);
		}
	}

	public virtual void UpdateLayout(bool instant = true)
	{
		float num = OnUpdateLayout(instant);
		ContentsLength = num + (float)_padding + (float)_scrollEndPadding;
		MaxOffset = Mathf.Max(0f, ContentsLength - ViewLength);
		Bounds scrollBounds = GetScrollBounds();
		_scrollView.SetFixedBounds(scrollBounds);
	}

	protected virtual Bounds GetScrollBounds()
	{
		UIPanel panel = Panel;
		Vector4 baseClipRegion = panel.baseClipRegion;
		Vector3 center = new Vector3(baseClipRegion.x, baseClipRegion.y);
		Vector3 size = new Vector3(baseClipRegion.z, baseClipRegion.w);
		if (panel.softBorderPadding)
		{
			size -= (Vector3)(panel.clipSoftness * 2f);
		}
		float num = Mathf.Floor(MaxOffset + ViewLength);
		if (Vector.x == 0f)
		{
			if (num > size.y)
			{
				center.y += Vector.y * (num - size.y) * 0.5f;
				size.y = num;
			}
		}
		else if (num > size.x)
		{
			center.x += Vector.x * (num - size.x) * 0.5f;
			size.x = num;
		}
		return new Bounds(center, size);
	}

	public Vector3 GetBasePosition()
	{
		UIPanel panel = Panel;
		Vector4 baseClipRegion = panel.baseClipRegion;
		Vector3 vector = new Vector3(baseClipRegion.x, baseClipRegion.y);
		return vector - Vector * (ViewLength * 0.5f - (float)_padding);
	}

	private int GetSign()
	{
		int result = 0;
		if (Dir == Direction.Vertical)
		{
			switch (_scrollView.contentPivot)
			{
			case UIWidget.Pivot.BottomLeft:
			case UIWidget.Pivot.Bottom:
			case UIWidget.Pivot.BottomRight:
				result = 1;
				break;
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.TopRight:
				result = -1;
				break;
			default:
				result = -1;
				break;
			}
		}
		else if (Dir == Direction.Horizontal)
		{
			switch (_scrollView.contentPivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.BottomLeft:
				result = 1;
				break;
			case UIWidget.Pivot.TopRight:
			case UIWidget.Pivot.Right:
			case UIWidget.Pivot.BottomRight:
				result = -1;
				break;
			default:
				result = 1;
				break;
			}
		}
		return result;
	}

	protected float GetSize(UIWidget widget)
	{
		return UIUtility.GetSize(widget.localSize, Vector);
	}

	protected float GetSize(Vector2 size)
	{
		return UIUtility.GetSize(size, Vector);
	}

	public virtual float GetNodeOffset(int index)
	{
		float num = 0f;
		for (int i = 0; i < index; i++)
		{
			num += GetNodeSize(i) + (float)_margin;
		}
		return num;
	}

	protected virtual float GetNodeSize(int index)
	{
		return GetSize(GetNode(index).localSize);
	}

	protected abstract float OnUpdateLayout(bool instant);

	public abstract UIWidget GetNode(int index);

	public abstract int GetNodeCount();
}
