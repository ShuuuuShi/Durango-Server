using System;
using UnityEngine;

public class ChatBubble : MonoBehaviour
{
	public enum ChatBubbleAlign
	{
		Auto,
		Left,
		Right
	}

	private enum TargetPivot
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	public Action<ChatBubble> Disabled;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private UIWidget _portraitWidget;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISprite _sttIcon;

	[SerializeField]
	private Transform _arrowWidget;

	[SerializeField]
	private int _paddingTop;

	[SerializeField]
	private int _paddingBottom;

	[SerializeField]
	private int _paddingLeft;

	[SerializeField]
	private int _paddingRight;

	[SerializeField]
	private int _betweenNameAndComment;

	[SerializeField]
	private int _betweenPortraitAndText;

	[SerializeField]
	private int _screenPadding;

	[SerializeField]
	private float _moveSpeed;

	[SerializeField]
	private Vector3 _worldOffset;

	[SerializeField]
	private Vector3 _uiOffset;

	[SerializeField]
	private Vector2 _boxSize;

	[SerializeField]
	private int _arrowSize;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	private ChatableBase _chatter;

	private int _maxXPos;

	private int _maxYPos;

	private float _hideAt;

	private Vector3 _targetPosition;

	private bool _isMoving;

	private TargetPivot _mainPivot;

	private TargetPivot[] _pivotOrder = new TargetPivot[4];

	private Vector3 _defaultCommentPos;

	private bool _isInit;

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

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public ulong Id => (!((Object)(object)Chatter == (Object)null)) ? Chatter.EntityId : 0;

	private ChatableBase Chatter
	{
		get
		{
			return _chatter;
		}
		set
		{
			_chatter = value;
		}
	}

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.localPosition = value;
		}
	}

	public int Depth { get; private set; }

	public ChatBubbleAlign Align { get; set; }

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_defaultCommentPos = ((Component)_commentLabel).transform.localPosition;
		}
	}

	private void Update()
	{
		if ((Object)(object)Chatter == (Object)null)
		{
			Hide();
			return;
		}
		UpdatePosition(instant: false);
		if (Time.time > _hideAt)
		{
			Hide();
		}
	}

	private void UpdatePosition(bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CalcPosition();
		_isMoving = false;
		if (instant || _moveSpeed <= 0f)
		{
			Position = val;
		}
		else if (val != Position)
		{
			Vector3 val2 = val - Position;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			float num = Mathf.Max(_moveSpeed, magnitude * 10f) * Time.deltaTime;
			if (magnitude < num)
			{
				Position = val;
			}
			else
			{
				_isMoving = true;
				Position += ((Vector3)(ref val2)).normalized * num;
			}
		}
		UpdateArrowPosition();
	}

	private Vector3 GetTargetPosition(Vector3 offset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return MainCamera.WorldToNGUIPos(Chatter.ChatterPosition + _worldOffset + offset) + _uiOffset;
	}

	private Vector3 CalcPosition()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Chatter == (Object)null)
		{
			return Position;
		}
		Vector3 targetPosition = GetTargetPosition(Vector3.zero);
		int screenHeight = UIManager.ScreenHeight;
		int screenWidth = UIManager.ScreenWidth;
		float num = (float)screenHeight / (float)screenWidth;
		float num2 = Mathf.Max((float)screenHeight * 0.1f, (float)screenWidth * 0.1f);
		switch (_mainPivot)
		{
		case TargetPivot.Up:
			targetPosition.y += num2;
			break;
		case TargetPivot.Down:
			targetPosition.y -= num2;
			break;
		case TargetPivot.Right:
			targetPosition.x += num2;
			break;
		case TargetPivot.Left:
			targetPosition.x -= num2;
			break;
		}
		Vector3 val = Vector3.zero;
		if (_mainPivot == TargetPivot.None)
		{
			if (Mathf.Abs(targetPosition.y) / Mathf.Abs(targetPosition.x) > num)
			{
				if (targetPosition.y > 0f)
				{
					if (targetPosition.x > 0f)
					{
						SetPivotOrder(TargetPivot.Up, TargetPivot.Right, TargetPivot.Left, TargetPivot.Down);
					}
					else
					{
						SetPivotOrder(TargetPivot.Up, TargetPivot.Left, TargetPivot.Right, TargetPivot.Down);
					}
				}
				else if (targetPosition.x > 0f)
				{
					SetPivotOrder(TargetPivot.Right, TargetPivot.Up, TargetPivot.Left, TargetPivot.Down);
				}
				else
				{
					SetPivotOrder(TargetPivot.Left, TargetPivot.Up, TargetPivot.Right, TargetPivot.Down);
				}
			}
			else if (targetPosition.x > 0f)
			{
				SetPivotOrder(TargetPivot.Right, TargetPivot.Up, TargetPivot.Left, TargetPivot.Down);
			}
			else
			{
				SetPivotOrder(TargetPivot.Left, TargetPivot.Up, TargetPivot.Right, TargetPivot.Down);
			}
			bool flag = false;
			int i = 0;
			for (int num3 = _pivotOrder.Length; i < num3; i++)
			{
				TargetPivot targetPivot = _pivotOrder[i];
				val = GetPivotPosition(targetPivot);
				switch (targetPivot)
				{
				case TargetPivot.Up:
				case TargetPivot.Down:
					if (Mathf.Abs(val.y) < (float)_maxYPos)
					{
						flag = true;
					}
					break;
				case TargetPivot.Left:
				case TargetPivot.Right:
					if (Mathf.Abs(val.x) < (float)_maxXPos)
					{
						flag = true;
					}
					break;
				}
				if (flag)
				{
					_mainPivot = targetPivot;
					break;
				}
			}
			if (!flag)
			{
				_mainPivot = TargetPivot.Up;
				val = GetPivotPosition(_mainPivot);
			}
		}
		else
		{
			val = GetPivotPosition(_mainPivot);
		}
		_targetPosition = val;
		switch (_mainPivot)
		{
		case TargetPivot.Up:
			val.y += (float)Widget.height * 0.5f + (float)_arrowSize;
			break;
		case TargetPivot.Left:
			val.x -= (float)Widget.width * 0.5f + (float)_arrowSize;
			break;
		case TargetPivot.Right:
			val.x += (float)Widget.width * 0.5f + (float)_arrowSize;
			break;
		case TargetPivot.Down:
			val.y -= (float)Widget.height * 0.5f + (float)_arrowSize;
			break;
		}
		val.x = Mathf.Clamp(val.x, (float)(-_maxXPos), (float)_maxXPos);
		val.y = Mathf.Clamp(val.y, (float)(-_maxYPos), (float)_maxYPos);
		return val;
	}

	private void SetPivotOrder(TargetPivot _1, TargetPivot _2, TargetPivot _3, TargetPivot _4)
	{
		_pivotOrder[0] = _1;
		_pivotOrder[1] = _2;
		_pivotOrder[2] = _3;
		_pivotOrder[3] = _4;
	}

	private Vector3 GetPivotPosition(TargetPivot pivot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = Vector3.zero;
		switch (pivot)
		{
		case TargetPivot.Up:
			result = GetTargetPosition(Vector3.up * _boxSize.y / 2f);
			break;
		case TargetPivot.Left:
			result = GetTargetPosition((Vector3.left + Vector3.forward) * _boxSize.x / 2f);
			break;
		case TargetPivot.Right:
			result = GetTargetPosition((Vector3.right + Vector3.back) * _boxSize.x / 2f);
			break;
		case TargetPivot.Down:
			result = GetTargetPosition(Vector3.down * _boxSize.y / 2f);
			break;
		}
		return result;
	}

	private bool IsInScreen(Vector3 pos)
	{
		return Mathf.Abs(pos.x) < (float)_maxXPos && Mathf.Abs(pos.y) < (float)_maxYPos;
	}

	private void UpdateArrowPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _targetPosition - Position;
		float num = Mathf.Abs(val.x);
		float num2 = Mathf.Abs(val.y);
		if (!IsInScreen(Position) || _isMoving)
		{
			((Component)_arrowWidget).gameObject.SetActive(false);
			return;
		}
		float num3 = (float)(Widget.width - _paddingLeft - _paddingRight) / 2f - (float)_arrowSize;
		float num4 = (float)(Widget.height - _paddingBottom - _paddingTop) / 2f - (float)_arrowSize;
		if (num < num3 && num2 >= num4)
		{
			((Component)_arrowWidget).gameObject.SetActive(true);
			Vector3 localPosition = default(Vector3);
			localPosition.x = val.x;
			localPosition.y = (float)Widget.height * 0.5f - 2f;
			localPosition.z = 0f;
			if (val.y > 0f)
			{
				_arrowWidget.eulerAngles = Vector3.forward * 180f;
			}
			else
			{
				_arrowWidget.eulerAngles = Vector3.forward * 0f;
				localPosition.y = 0f - localPosition.y;
			}
			_arrowWidget.localPosition = localPosition;
		}
		else if (num >= num3 && num2 < num4)
		{
			((Component)_arrowWidget).gameObject.SetActive(true);
			Vector3 localPosition2 = default(Vector3);
			localPosition2.x = (float)Widget.width * 0.5f - 2f;
			localPosition2.y = val.y;
			localPosition2.z = 0f;
			if (val.x > 0f)
			{
				_arrowWidget.eulerAngles = Vector3.forward * 90f;
			}
			else
			{
				_arrowWidget.eulerAngles = Vector3.forward * 270f;
				localPosition2.x = 0f - localPosition2.x;
			}
			_arrowWidget.localPosition = localPosition2;
		}
		else
		{
			((Component)_arrowWidget).gameObject.SetActive(false);
		}
	}

	public void Set(ChatableBase chatter, string playerName, string comment, PortraitBuilder.Argument portrait, bool showSTTIcon)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Init();
		Chatter = chatter;
		if (string.IsNullOrEmpty(playerName))
		{
			((Component)_nameLabel).gameObject.SetActive(false);
			Vector3 localPosition = ((Component)_commentLabel).transform.localPosition;
			localPosition.y = ((Component)_nameLabel).transform.localPosition.y;
			((Component)_commentLabel).transform.localPosition = localPosition;
		}
		else
		{
			((Component)_nameLabel).gameObject.SetActive(true);
			_nameLabel.text = playerName;
			((Component)_commentLabel).transform.localPosition = _defaultCommentPos;
		}
		_commentLabel.text = string.Format("[{1}]{0}[-]", comment, (!showSTTIcon) ? "000000" : "5E553E");
		_mainPivot = TargetPivot.None;
		((Component)_sttIcon).gameObject.SetActive(showSTTIcon);
		PortraitBuilder.Set(portrait, _portraitTexture);
		CalcWidgetSize();
	}

	private void CalcWidgetSize()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (((Component)_nameLabel).gameObject.activeSelf)
		{
			num += _nameLabel.height - _nameLabel.spacingY;
			num += _betweenNameAndComment;
		}
		num += _commentLabel.Label.height - _commentLabel.Label.spacingY;
		num = Mathf.Max(_portraitWidget.height, num);
		Widget.height = num + _paddingTop + _paddingBottom;
		int num2 = (((Component)_nameLabel).gameObject.activeSelf ? _nameLabel.width : 0);
		if (((Component)_sttIcon).gameObject.activeSelf)
		{
			num2 += 20 + _sttIcon.width;
		}
		int num3 = Mathf.Max((int)_commentLabel.Label.printedSize.x, num2);
		num3 += _portraitWidget.width;
		num3 += _betweenPortraitAndText;
		Widget.width = num3 + _paddingLeft + _paddingRight;
		_background.UpdateAnchors();
		_maxXPos = UIManager.ScreenWidth / 2 - (Widget.width / 2 + _screenPadding * 2);
		_maxYPos = UIManager.ScreenHeight / 2 - (Widget.height / 2 + _screenPadding * 2);
	}

	private void UpdateLayout()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		if (Align == ChatBubbleAlign.Auto)
		{
			Align = (((_targetPosition - Position).x > 0f) ? ChatBubbleAlign.Left : ChatBubbleAlign.Right);
		}
		int width = Widget.width;
		int height = Widget.height;
		int num = width / 2 - _portraitWidget.width / 2;
		int num2 = height / 2 - _paddingTop - _portraitWidget.height / 2;
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(0f, (float)num2, 0f);
		switch (Align)
		{
		case ChatBubbleAlign.Left:
			num -= _paddingLeft;
			localPosition.x = -num;
			break;
		case ChatBubbleAlign.Right:
			num -= _paddingRight;
			localPosition.x = num;
			break;
		}
		((Component)_portraitWidget).transform.localPosition = localPosition;
		float x = ((Component)_portraitWidget).transform.localPosition.x;
		x += (float)((Align != ChatBubbleAlign.Right) ? 1 : (-1)) * ((float)_betweenPortraitAndText + (float)_portraitWidget.width / 2f);
		float num3 = height / 2 - _paddingTop;
		if (((Component)_nameLabel).gameObject.activeSelf)
		{
			_nameLabel.pivot = ((Align == ChatBubbleAlign.Right) ? UIWidget.Pivot.TopRight : UIWidget.Pivot.TopLeft);
			((Component)_nameLabel).transform.localPosition = new Vector3(x, num3);
			num3 -= (float)_nameLabel.height;
			num3 -= (float)_betweenNameAndComment;
		}
		_commentLabel.Label.pivot = ((Align == ChatBubbleAlign.Right) ? UIWidget.Pivot.TopRight : UIWidget.Pivot.TopLeft);
		((Component)_commentLabel).transform.localPosition = new Vector3(x, num3);
		if (((Component)_sttIcon).gameObject.activeSelf)
		{
			switch (Align)
			{
			case ChatBubbleAlign.Left:
				_sttIcon.SetPosition(Widget.localCorners[2] + new Vector3(-16f, -9f), 1f, 1f);
				break;
			case ChatBubbleAlign.Right:
				_sttIcon.SetPosition(Widget.localCorners[1] + new Vector3(16f, -9f), 0f, 1f);
				break;
			}
		}
		Align = ChatBubbleAlign.Auto;
	}

	public void Show(float duration)
	{
		UpdatePosition(instant: true);
		UpdateLayout();
		((Component)this).gameObject.SetActive(true);
		TweenScale component = ((Component)this).GetComponent<TweenScale>();
		component.tweenFactor = 0f;
		component.PlayForward();
		_hideAt = Time.time + duration;
		TweenAlpha tweener = AnimWidget.GetTweener<TweenAlpha>();
		if (((Behaviour)tweener).enabled)
		{
			((Behaviour)tweener).enabled = false;
			AnimWidget.Alpha = 1f;
		}
	}

	public void Hide()
	{
		_hideAt = float.MaxValue;
		AnimWidget.Alpha = 0f;
	}

	private void OnEnable()
	{
		Widget.alpha = 0f;
		AnimWidget.Alpha = 1f;
		_mainPivot = TargetPivot.None;
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}

	public void SetDepth(ChatBubble origin, int depth)
	{
		Depth = depth;
		depth *= 10;
		Widget.depth = origin.Widget.depth + depth;
		_background.depth = origin._background.depth + depth;
		_nameLabel.depth = origin._nameLabel.depth + depth;
		_sttIcon.depth = origin._sttIcon.depth + depth;
		_commentLabel.Depth = origin._commentLabel.Label.depth + depth;
		_portraitWidget.depth = origin._portraitWidget.depth + depth;
		_portraitTexture.depth = origin._portraitTexture.depth + depth;
		((Component)_arrowWidget).GetComponent<UIWidget>().depth = ((Component)origin._arrowWidget).GetComponent<UIWidget>().depth + depth;
	}
}
