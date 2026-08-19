using System;
using UnityEngine;

public abstract class KScrollViewBase : MonoBehaviour
{
	public enum Direction
	{
		Auto,
		Vertical,
		Horizontal
	}

	private struct RepositionFlag
	{
		public bool Flag;

		public bool Resetposition;
	}

	public Action DragFinishedOnFirst;

	public Action DragFinishedOnLast;

	public Action DragFinshed;

	[SerializeField]
	private Direction _direction;

	[SerializeField]
	private bool _alwaysIntOffset;

	[SerializeField]
	private int _enableResetIndex = -1;

	[SerializeField]
	protected UIScrollView _scrollView;

	[SerializeField]
	protected int _margin;

	[SerializeField]
	private int _padding;

	[SerializeField]
	private int _scrollEndPadding;

	private UIWidget _widget;

	protected Vector3 _basePosition;

	protected Vector2 _baseOffset;

	protected UIWidget _boxWidget;

	protected UIWidget _endPaddingWidget;

	private Action _moveToOnFinish;

	private RepositionFlag _repositionFlag;

	private bool _isEnable;

	private float _totalOffset;

	private float _limitOffset;

	private int _limitIndex = -1;

	public UIScrollView ScrollView => _scrollView;

	public UIPanel Panel
	{
		get
		{
			UIPanel uIPanel = _scrollView.panel;
			if ((Object)(object)uIPanel == (Object)null)
			{
				uIPanel = ((Component)_scrollView).GetComponent<UIPanel>();
			}
			return uIPanel;
		}
	}

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public int Margin => _margin;

	public float MaxOffset { get; private set; }

	public Direction Dir
	{
		get
		{
			if (_direction == Direction.Auto)
			{
				switch (_scrollView.movement)
				{
				case UIScrollView.Movement.Vertical:
					return Direction.Vertical;
				case UIScrollView.Movement.Horizontal:
					return Direction.Horizontal;
				}
			}
			return _direction;
		}
	}

	public float CurrentOffset => CalcOffset(((Component)_scrollView).transform.localPosition);

	public float GoalOffset
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			SpringPanel component = ((Component)_scrollView).GetComponent<SpringPanel>();
			Vector3 pos = ((!((Object)(object)component == (Object)null) && ((Behaviour)component).enabled) ? component.target : ((Component)_scrollView).transform.localPosition);
			return CalcOffset(pos);
		}
	}

	public int CurrentNodeIndex => CalcNodeIndex(CurrentOffset);

	public int GoalNodeIndex => CalcNodeIndex(GoalOffset);

	public float BoxSize => (!((Object)(object)_boxWidget == (Object)null)) ? GetSize(_boxWidget) : 0f;

	public float BoxBreadth => (!((Object)(object)_boxWidget == (Object)null)) ? UIUtility.GetSize(_boxWidget, new Vector2(Vector.y, Vector.x)) : 0f;

	protected Vector3 Vector => (Vector3)(Dir switch
	{
		Direction.Vertical => Vector3.up * (float)GetSign(), 
		Direction.Horizontal => Vector3.right * (float)GetSign(), 
		_ => Vector3.zero, 
	});

	private void Start()
	{
		_scrollView.onDragFinished = OnDragFinished;
	}

	private float CalcOffset(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector2 rawOffset = GetRawOffset(pos);
		float num = 0f;
		switch (Dir)
		{
		case Direction.Vertical:
			num = rawOffset.y;
			break;
		case Direction.Horizontal:
			num = rawOffset.x;
			break;
		}
		return num * (float)GetSign();
	}

	private int CalcNodeIndex(float offset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		int result = GetNodeCount() - 1;
		float num = 0f;
		int i = 0;
		for (int nodeCount = GetNodeCount(); i < nodeCount; i++)
		{
			if (num >= offset)
			{
				result = Mathf.Max(0, i - 1);
				break;
			}
			num += GetSize(GetNodeSize(i));
		}
		return result;
	}

	private void OnDragFinished()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		float currentOffset = CurrentOffset;
		if (currentOffset < 0f)
		{
			if (DragFinishedOnFirst != null)
			{
				DragFinishedOnFirst();
			}
		}
		else if (currentOffset > MaxOffset + (float)_scrollEndPadding && DragFinishedOnLast != null)
		{
			DragFinishedOnLast();
		}
		if (_alwaysIntOffset)
		{
			int num = 0;
			Vector2 val = Vector2.op_Implicit(ScrollView.currentMomentum);
			if (val != Vector2.zero)
			{
				num = ((Vector.x == 0f) ? (num - (int)Mathf.Sign(Vector.y * val.y)) : (num - (int)Mathf.Sign(Vector.x * val.x)));
			}
			int currentNodeIndex = CurrentNodeIndex;
			currentNodeIndex = Mathf.Max(currentNodeIndex, currentNodeIndex + num);
			MoveToNode(currentNodeIndex, instant: false);
		}
		if (DragFinshed != null)
		{
			DragFinshed();
		}
	}

	protected virtual void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_isEnable = true;
		_boxWidget = UIUtility.SetScrollViewInvisibleBox(_scrollView, _boxWidget);
		UIPanel panel = Panel;
		_basePosition = ((Component)panel).transform.localPosition;
		_baseOffset = panel.clipOffset;
		if (_repositionFlag.Flag)
		{
			Reposition(_repositionFlag.Resetposition, tween: false);
		}
		else if (_enableResetIndex >= 0)
		{
			UpdateLayout();
			MoveToNode(_enableResetIndex, instant: true);
		}
	}

	private void OnDisable()
	{
		_isEnable = false;
	}

	public void MoveTo(float offset, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = Vector;
		_moveToOnFinish = onFinish;
		if (restrictWithinPanel)
		{
			offset = Mathf.Clamp(offset, 0f, MaxOffset);
		}
		UIPanel panel = Panel;
		Vector3 basePosition = _basePosition;
		Vector2 baseOffset = _baseOffset;
		switch (Dir)
		{
		case Direction.Vertical:
			basePosition.x = ((Component)panel).transform.localPosition.x;
			baseOffset.x = panel.clipOffset.x;
			break;
		case Direction.Horizontal:
			basePosition.y = ((Component)panel).transform.localPosition.y;
			baseOffset.y = panel.clipOffset.y;
			break;
		}
		Vector3 val = basePosition - vector * offset;
		if (instant)
		{
			_scrollView.DisableSpring();
			((Component)panel).transform.localPosition = val;
			panel.clipOffset = baseOffset + Vector2.op_Implicit(vector * offset);
			_scrollView.UpdateScrollbars(recalculateBounds: false);
			OnFinishMoveTo();
		}
		else
		{
			SpringPanel springPanel = SpringPanel.Begin(((Component)panel).gameObject, val, 8f);
			springPanel.onFinished = OnFinishMoveTo;
		}
	}

	public void MoveToNode(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		index = Mathf.Clamp(index, 0, GetNodeCount());
		if (index >= 0)
		{
			MoveTo(GetNodeOffset(index), instant, restrictWithinPanel, onFinish);
		}
	}

	public void MoveToEnd(int index, bool instant, bool restrictWithinPanel = true, Action onFinish = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		index = Mathf.Clamp(index, 0, GetNodeCount());
		if (index >= 0)
		{
			float offset = GetNodeOffset(index) - BoxSize + GetSize(GetNodeSize(index));
			MoveTo(offset, instant, restrictWithinPanel, onFinish);
		}
	}

	public void MoveToVisibleArea(int index, bool instant, float beginPadding = 0f, float endPadding = 0f, bool restrictWithinPanel = true, Action onFinish = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float currentOffset = CurrentOffset;
		float num = currentOffset + beginPadding;
		float nodeOffset = GetNodeOffset(index);
		if (nodeOffset < num)
		{
			MoveTo(nodeOffset - beginPadding, instant, restrictWithinPanel, onFinish);
			return;
		}
		endPadding += (float)_scrollEndPadding;
		float num2 = currentOffset + BoxSize - endPadding;
		float num3 = nodeOffset + GetSize(GetNodeSize(index));
		if (num3 > num2)
		{
			MoveTo(num3 - BoxSize + endPadding, instant, restrictWithinPanel, onFinish);
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
		if (!_isEnable)
		{
			_repositionFlag.Flag = true;
			_repositionFlag.Resetposition = resetPosition;
			return;
		}
		_repositionFlag.Flag = false;
		float num = UpdateLayout();
		if (resetPosition)
		{
			MoveToNode(0, !tween);
			return;
		}
		float currentOffset = CurrentOffset;
		if (currentOffset < 0f)
		{
			MoveToNode(0, !tween);
		}
		else if (currentOffset >= num)
		{
			MoveToNode(GetNodeCount() - 1, !tween);
		}
	}

	public float UpdateLayout(bool instant = true)
	{
		_totalOffset = OnUpdateLayout(instant);
		MakeEndPaddingWidget(_scrollEndPadding);
		CalcLimitOffset();
		return _totalOffset;
	}

	protected virtual void MakeEndPaddingWidget(int padding)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (padding > 0)
		{
			if ((Object)(object)_endPaddingWidget == (Object)null)
			{
				_endPaddingWidget = ((Component)_scrollView).gameObject.AddChild<UIWidget>();
			}
			Vector3 basePosition = GetBasePosition();
			_endPaddingWidget.width = ((Vector.x == 0f) ? _boxWidget.width : padding);
			_endPaddingWidget.height = ((Vector.y == 0f) ? _boxWidget.height : padding);
			((Component)_endPaddingWidget).transform.localPosition = basePosition + Vector * (_totalOffset + GetSize(_endPaddingWidget) * 0.5f);
		}
		else if ((Object)(object)_endPaddingWidget != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_endPaddingWidget).gameObject);
		}
	}

	public void PanelResized()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_isEnable && !((Object)(object)_boxWidget == (Object)null))
		{
			Vector2 vec = UIUtility.PanelInnerSize(Panel);
			ResizeBox(new Point2(vec));
		}
	}

	public void RefreshBox()
	{
		_boxWidget = UIUtility.SetScrollViewInvisibleBox(_scrollView, _boxWidget);
		CalcLimitOffset();
	}

	public void ResizeBox(int size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = Vector;
		Point2 size2 = new Point2(UIUtility.PanelInnerSize(Panel));
		if (vector.x != 0f)
		{
			size2.x = size;
		}
		else if (vector.y != 0f)
		{
			size2.y = size;
		}
		ResizeBox(size2);
	}

	private void ResizeBox(Point2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = Vector;
		Vector2 pivot = default(Vector2);
		((Vector2)(ref pivot))._002Ector((!(vector.x > 0f)) ? 1f : 0f, (!(vector.y > 0f)) ? 1f : 0f);
		_boxWidget.Resize(size, pivot);
		CalcLimitOffset();
	}

	private void CalcLimitOffset()
	{
		_limitOffset = Mathf.Max(0f, _totalOffset - (BoxSize - (float)(_padding * 2)) - (float)_margin);
		SetLimitIndex(_limitIndex);
	}

	public void SetLimitIndex(int index)
	{
		_limitIndex = index;
		MaxOffset = ((_limitIndex >= 0) ? Mathf.Min(_limitOffset, GetNodeOffset(_limitIndex)) : _limitOffset);
	}

	public Vector2 GetCurrentRawOffset()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return GetRawOffset(((Component)_scrollView).transform.localPosition);
	}

	public Vector2 GetRawOffset(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(_basePosition - pos);
	}

	protected virtual float OnUpdateLayout(bool instant)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_boxWidget == (Object)null)
		{
			return 0f;
		}
		Vector3 basePosition = GetBasePosition();
		return UIUtility.WidgetsReposition(GetNode, GetNodeCount(), Vector, basePosition, _margin, instant);
	}

	protected Vector3 GetBasePosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)_boxWidget).transform.localPosition - Vector * (BoxSize * 0.5f - (float)_padding);
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
			}
		}
		return result;
	}

	protected float GetSize(UIWidget widget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return UIUtility.GetSize(widget, Vector2.op_Implicit(Vector));
	}

	protected float GetSize(Vector2 size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return UIUtility.GetSize(size, Vector2.op_Implicit(Vector));
	}

	public virtual float GetNodeOffset(int index)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < index; i++)
		{
			num += GetSize(GetNodeSize(i)) + (float)_margin;
		}
		return num;
	}

	protected virtual Vector2 GetNodeSize(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetNode(index).localSize;
	}

	public abstract UIWidget GetNode(int index);

	public abstract int GetNodeCount();
}
