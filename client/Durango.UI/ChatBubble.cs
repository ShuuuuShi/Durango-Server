using System;
using Durango.Render.Camera;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ChatBubble : MonoBehaviour
{
	public enum ChatBubbleAlign
	{
		Auto,
		Left,
		Right
	}

	public enum TargetPivot
	{
		Up,
		Down,
		Left,
		Right
	}

	public Action<ChatBubble> Disabled;

	[SerializeField]
	private UIWidget _mainWidget;

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
	private UISprite _portaitIcon;

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
	private Vector3 _worldOffset;

	[SerializeField]
	private Vector2 _boxSize;

	[SerializeField]
	private int _arrowSize;

	private int _maxXPos;

	private int _maxYPos;

	private float _hideAt;

	private Vector3 _targetPosition;

	private TargetPivot? _mainPivot;

	private readonly TargetPivot[] _pivotOrder = new TargetPivot[4];

	private Vector3 _defaultCommentPos;

	private bool _isInit;

	private Vector3? _posOffset;

	public string Id => (Chatter != null) ? Chatter.EntityId : string.Empty;

	private ChatableBase Chatter { get; set; }

	public ChatBubbleAlign Align { get; set; }

	public bool AlwaysInScreen { get; set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_defaultCommentPos = _commentLabel.transform.localPosition;
		}
	}

	public void Refresh()
	{
		if (Chatter == null)
		{
			Hide();
			return;
		}
		UpdatePosition();
		if (_hideAt > 0f && Time.time > _hideAt)
		{
			Hide();
		}
	}

	private void UpdatePosition()
	{
		Vector3 localPosition = CalcPosition();
		base.transform.localPosition = localPosition;
		UpdateArrowPosition();
	}

	private Vector3 GetTargetPosition(Vector3 offset)
	{
		return MainCamera.WorldToNGUIPos(Chatter.ChatterPosition + _posOffset.GetValueOrDefault(_worldOffset) + offset);
	}

	private Vector3 CalcPosition()
	{
		if (Chatter == null)
		{
			return base.transform.localPosition;
		}
		Vector3 targetPosition = GetTargetPosition(Vector3.zero);
		int safeWidth = UIManager.SafeWidth;
		int safeHeight = UIManager.SafeHeight;
		float ratio = (float)safeHeight / (float)safeWidth;
		float num = Mathf.Max((float)safeHeight * 0.1f, (float)safeWidth * 0.1f);
		TargetPivot? mainPivot = _mainPivot;
		if (mainPivot.HasValue)
		{
			switch (mainPivot.Value)
			{
			case TargetPivot.Up:
				targetPosition.y += num;
				break;
			case TargetPivot.Down:
				targetPosition.y -= num;
				break;
			case TargetPivot.Right:
				targetPosition.x += num;
				break;
			case TargetPivot.Left:
				targetPosition.x -= num;
				break;
			}
		}
		Vector3 result = (_targetPosition = CalcPivotPosition(targetPosition, ratio));
		TargetPivot? mainPivot2 = _mainPivot;
		if (mainPivot2.HasValue)
		{
			switch (mainPivot2.Value)
			{
			case TargetPivot.Up:
				result.y += (float)_mainWidget.height * 0.5f + (float)_arrowSize;
				break;
			case TargetPivot.Left:
				result.x -= (float)_mainWidget.width * 0.5f + (float)_arrowSize;
				break;
			case TargetPivot.Right:
				result.x += (float)_mainWidget.width * 0.5f + (float)_arrowSize;
				break;
			case TargetPivot.Down:
				result.y -= (float)_mainWidget.height * 0.5f + (float)_arrowSize;
				break;
			}
		}
		if (AlwaysInScreen)
		{
			result.x = Mathf.Clamp(result.x, -_maxXPos, _maxXPos);
			result.y = Mathf.Clamp(result.y, -_maxYPos, _maxYPos);
		}
		return result;
	}

	private Vector3 CalcPivotPosition(Vector3 center, float ratio)
	{
		if (_mainPivot.HasValue)
		{
			return GetPivotPosition(_mainPivot.Value);
		}
		if (Mathf.Abs(center.y) / Mathf.Abs(center.x) > ratio)
		{
			if (center.y > 0f)
			{
				if (center.x > 0f)
				{
					SetPivotOrder(TargetPivot.Up, TargetPivot.Right, TargetPivot.Left, TargetPivot.Down);
				}
				else
				{
					SetPivotOrder(TargetPivot.Up, TargetPivot.Left, TargetPivot.Right, TargetPivot.Down);
				}
			}
			else if (center.x > 0f)
			{
				SetPivotOrder(TargetPivot.Right, TargetPivot.Up, TargetPivot.Left, TargetPivot.Down);
			}
			else
			{
				SetPivotOrder(TargetPivot.Left, TargetPivot.Up, TargetPivot.Right, TargetPivot.Down);
			}
		}
		else if (center.x > 0f)
		{
			SetPivotOrder(TargetPivot.Right, TargetPivot.Up, TargetPivot.Left, TargetPivot.Down);
		}
		else
		{
			SetPivotOrder(TargetPivot.Left, TargetPivot.Up, TargetPivot.Right, TargetPivot.Down);
		}
		bool flag = false;
		int i = 0;
		for (int num = _pivotOrder.Length; i < num; i++)
		{
			TargetPivot targetPivot = _pivotOrder[i];
			Vector3 pivotPosition = GetPivotPosition(targetPivot);
			switch (targetPivot)
			{
			case TargetPivot.Up:
			case TargetPivot.Down:
				if (Mathf.Abs(pivotPosition.y) < (float)_maxYPos)
				{
					flag = true;
				}
				break;
			case TargetPivot.Left:
			case TargetPivot.Right:
				if (Mathf.Abs(pivotPosition.x) < (float)_maxXPos)
				{
					flag = true;
				}
				break;
			}
			if (flag)
			{
				_mainPivot = targetPivot;
				return pivotPosition;
			}
		}
		_mainPivot = TargetPivot.Up;
		return GetPivotPosition(_mainPivot.Value);
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
		Vector3 localPosition = base.transform.localPosition;
		Vector3 vector = _targetPosition - localPosition;
		float num = Mathf.Abs(vector.x);
		float num2 = Mathf.Abs(vector.y);
		if (!IsInScreen(localPosition))
		{
			_arrowWidget.gameObject.SetActive(value: false);
			return;
		}
		float num3 = (float)(_mainWidget.width - _paddingLeft - _paddingRight) / 2f - (float)_arrowSize;
		float num4 = (float)(_mainWidget.height - _paddingBottom - _paddingTop) / 2f - (float)_arrowSize;
		if (num < num3 && num2 >= num4)
		{
			_arrowWidget.gameObject.SetActive(value: true);
			Vector3 localPosition2 = default(Vector3);
			localPosition2.x = vector.x;
			localPosition2.y = (float)_mainWidget.height * 0.5f - 2f;
			localPosition2.z = 0f;
			if (vector.y > 0f)
			{
				_arrowWidget.eulerAngles = Vector3.forward * 180f;
			}
			else
			{
				_arrowWidget.eulerAngles = Vector3.forward * 0f;
				localPosition2.y = 0f - localPosition2.y;
			}
			_arrowWidget.localPosition = localPosition2;
		}
		else if (num >= num3 && num2 < num4)
		{
			_arrowWidget.gameObject.SetActive(value: true);
			Vector3 localPosition3 = default(Vector3);
			localPosition3.x = (float)_mainWidget.width * 0.5f - 2f;
			localPosition3.y = vector.y;
			localPosition3.z = 0f;
			if (vector.x > 0f)
			{
				_arrowWidget.eulerAngles = Vector3.forward * 90f;
			}
			else
			{
				_arrowWidget.eulerAngles = Vector3.forward * 270f;
				localPosition3.x = 0f - localPosition3.x;
			}
			_arrowWidget.localPosition = localPosition3;
		}
		else
		{
			_arrowWidget.gameObject.SetActive(value: false);
		}
	}

	public void Set(ChatableBase chatter, string comment, PortraitBuilder.Argument? portraitArgs, string portraitIcon, Color portraitColor, TargetPivot? direction, Vector3? offset, bool showSttIcon)
	{
		Init();
		Chatter = chatter;
		string chatterName = chatter.ChatterName;
		_posOffset = offset;
		Transform transform = _commentLabel.transform;
		if (string.IsNullOrEmpty(chatterName))
		{
			_nameLabel.gameObject.SetActive(value: false);
			Vector3 localPosition = transform.localPosition;
			localPosition.y = _nameLabel.transform.localPosition.y;
			transform.localPosition = localPosition;
		}
		else
		{
			_nameLabel.gameObject.SetActive(value: true);
			_nameLabel.text = chatterName;
			transform.localPosition = _defaultCommentPos;
		}
		_commentLabel.text = string.Format("[{1}]{0}[-]", comment, (!showSttIcon) ? "000000" : "5E553E");
		_mainPivot = direction;
		_sttIcon.gameObject.SetActive(showSttIcon);
		if (portraitArgs.HasValue)
		{
			_portraitWidget.gameObject.SetActive(value: true);
			_portraitTexture.gameObject.SetActive(value: true);
			_portaitIcon.gameObject.SetActive(value: false);
			PortraitBuilder.Set(portraitArgs.Value, _portraitTexture);
			_portraitTexture.color = portraitColor;
		}
		else if (!string.IsNullOrEmpty(portraitIcon))
		{
			_portraitWidget.gameObject.SetActive(value: true);
			_portraitTexture.gameObject.SetActive(value: false);
			_portaitIcon.gameObject.SetActive(value: true);
			_portaitIcon.spriteName = portraitIcon;
			_portaitIcon.color = portraitColor;
		}
		else
		{
			_portraitWidget.gameObject.SetActive(value: false);
		}
		CalcWidgetSize();
	}

	private void CalcWidgetSize()
	{
		int num = 0;
		if (_nameLabel.gameObject.activeSelf)
		{
			num += _nameLabel.height;
			num += _betweenNameAndComment;
		}
		Vector2 printedSize = _commentLabel.printedSize;
		num += (int)printedSize.y;
		num = Mathf.Max(_portraitWidget.height, num);
		_mainWidget.height = num + _paddingTop + _paddingBottom;
		int num2 = (_nameLabel.gameObject.activeSelf ? _nameLabel.width : 0);
		if (_sttIcon.gameObject.activeSelf)
		{
			num2 += 20 + _sttIcon.width;
		}
		int num3 = Mathf.Max((int)printedSize.x, num2);
		if (_portraitWidget.gameObject.activeSelf)
		{
			if ((float)num3 > 0f)
			{
				num3 += _betweenPortraitAndText;
			}
			num3 += _portraitWidget.width;
		}
		_mainWidget.width = num3 + _paddingLeft + _paddingRight;
		_background.UpdateAnchors();
		_maxXPos = UIManager.SafeWidth / 2 - (_mainWidget.width / 2 + _screenPadding * 2);
		_maxYPos = UIManager.SafeHeight / 2 - (_mainWidget.height / 2 + _screenPadding * 2);
	}

	private void UpdateLayout()
	{
		if (Align == ChatBubbleAlign.Auto)
		{
			Align = (((_targetPosition - base.transform.localPosition).x > 0f) ? ChatBubbleAlign.Left : ChatBubbleAlign.Right);
		}
		int width = _mainWidget.width;
		int height = _mainWidget.height;
		if (_portraitWidget.gameObject.activeSelf)
		{
			int num = width / 2 - _portraitWidget.width / 2;
			int num2 = height / 2 - _paddingTop - _portraitWidget.height / 2;
			Vector3 localPosition = new Vector3(0f, num2, 0f);
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
			_portraitWidget.transform.localPosition = localPosition;
		}
		float x;
		if (_portraitWidget.gameObject.activeSelf)
		{
			x = _portraitWidget.transform.localPosition.x;
			x += (float)((Align != ChatBubbleAlign.Right) ? 1 : (-1)) * ((float)_betweenPortraitAndText + (float)_portraitWidget.width / 2f);
		}
		else
		{
			x = 0f;
			switch (Align)
			{
			case ChatBubbleAlign.Left:
				x -= (float)width * 0.5f - (float)_paddingLeft;
				break;
			case ChatBubbleAlign.Right:
				x += (float)width * 0.5f - (float)_paddingRight;
				break;
			}
		}
		float num3 = height / 2 - _paddingTop;
		if (_nameLabel.gameObject.activeSelf)
		{
			_nameLabel.pivot = ((Align == ChatBubbleAlign.Right) ? UIWidget.Pivot.TopRight : UIWidget.Pivot.TopLeft);
			_nameLabel.transform.localPosition = new Vector3(x, num3);
			num3 -= (float)_nameLabel.height;
			num3 -= (float)_betweenNameAndComment;
		}
		_commentLabel.pivot = ((Align == ChatBubbleAlign.Right) ? UIWidget.Pivot.TopRight : UIWidget.Pivot.TopLeft);
		_commentLabel.transform.localPosition = new Vector3(x, num3);
		if (_sttIcon.gameObject.activeSelf)
		{
			switch (Align)
			{
			case ChatBubbleAlign.Left:
				_sttIcon.SetPosition(_mainWidget.localCorners[2] + new Vector3(-16f, -9f), 1f, 1f);
				break;
			case ChatBubbleAlign.Right:
				_sttIcon.SetPosition(_mainWidget.localCorners[1] + new Vector3(16f, -9f), 0f, 1f);
				break;
			}
		}
		Align = ChatBubbleAlign.Auto;
	}

	public void Show(float? duration)
	{
		UpdatePosition();
		UpdateLayout();
		base.gameObject.SetActive(value: true);
		TweenScale component = _mainWidget.GetComponent<TweenScale>();
		component.tweenFactor = 0f;
		component.PlayForward();
		_hideAt = (duration.HasValue ? (Time.time + duration.Value) : 0f);
	}

	public void Hide()
	{
		_hideAt = 0f;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}
}
