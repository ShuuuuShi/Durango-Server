using UnityEngine;

namespace Durango.UI.Control;

public abstract class KeyLabelBase : MonoBehaviour
{
	public interface IContent
	{
	}

	[SerializeField]
	protected UILabel _valueLabel;

	[SerializeField]
	private UILabel _keyLabel;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private bool _useMinHeight;

	[SerializeField]
	private int _minHeight;

	[SerializeField]
	private int _keyValueMargin;

	private UIWidget _widget;

	private float _topPadding;

	private float _bottomPadding;

	private float _leftPadding;

	private float _rightPadding;

	private float _topBottomPaddingRatio;

	private bool _isInit;

	public float TopBottomPaddingRatio
	{
		get
		{
			return (!(_topBottomPaddingRatio <= 0f)) ? _topBottomPaddingRatio : 1f;
		}
		set
		{
			_topBottomPaddingRatio = value;
		}
	}

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	protected void Init()
	{
		TopBottomPaddingRatio = 0f;
		if (!_isInit)
		{
			_isInit = true;
			if (_keyLabel.isAnchored)
			{
				_keyLabel.UpdateAnchors();
			}
			if (_valueLabel.isAnchored)
			{
				_valueLabel.UpdateAnchors();
			}
			Vector3[] localCorners = Widget.localCorners;
			Vector3[] localCorners2 = _keyLabel.localCorners;
			Vector3[] localCorners3 = _valueLabel.localCorners;
			for (int i = 0; i < 4; i++)
			{
				localCorners2[i] += _keyLabel.transform.localPosition;
				localCorners3[i] += _valueLabel.transform.localPosition;
			}
			_leftPadding = localCorners2[0].x - localCorners[0].x;
			_topPadding = localCorners[1].y - localCorners2[1].y;
			_bottomPadding = localCorners2[0].y - localCorners[0].y;
			_rightPadding = localCorners[2].x - localCorners3[2].x;
			Vector2 pivotOffset = _keyLabel.pivotOffset;
			if (pivotOffset.y == 0f)
			{
				_topPadding = _bottomPadding;
			}
			else if (pivotOffset.y == 1f)
			{
				_bottomPadding = _topPadding;
			}
		}
	}

	public void SetFontSize(int size)
	{
		_keyLabel.fontSize = size;
		_valueLabel.fontSize = size;
	}

	public KeyLabelBase Set(SyncString key, IContent value)
	{
		return SetKey(key).SetValue(value);
	}

	public KeyLabelBase SetKey(SyncString key)
	{
		Init();
		if (_keyLabel == null)
		{
			return this;
		}
		if (_keyLabel.overflowMethod == UILabel.Overflow.ResizeFreely)
		{
			_keyLabel.overflowWidth = 0;
		}
		_keyLabel.SetText(key);
		return this;
	}

	private void Reset()
	{
		_minHeight = GetComponent<UIWidget>().height;
	}

	public abstract KeyLabelBase SetValue(IContent value);

	public void UpdateLayout(int width = 0)
	{
		Point2 point = new Point2(GetPreferredSize(width));
		if (width > 0)
		{
			point.x = width;
		}
		Widget.SetDimensions(point.x, point.y);
		if (_keyLabel.isAnchored)
		{
			_keyLabel.UpdateAnchors();
		}
		else
		{
			Vector3 pos = Widget.localCorners[1];
			pos += new Vector3(_leftPadding, (0f - _topPadding) * TopBottomPaddingRatio);
			_keyLabel.SetPosition(pos, 0f, 1f);
		}
		if (_valueLabel.isAnchored)
		{
			_valueLabel.UpdateAnchors();
			return;
		}
		Vector3 pos2 = Widget.localCorners[2];
		pos2 += new Vector3(0f - _rightPadding, (0f - _topPadding) * TopBottomPaddingRatio);
		_valueLabel.SetPosition(pos2, 1f, 1f);
	}

	public Vector2 GetPreferredSize(int limitWidth = 0)
	{
		Vector2 vector = ((!(_keyLabel == null) && !string.IsNullOrEmpty(_keyLabel.text)) ? _keyLabel.printedSize : Vector2.zero);
		Vector2 vector2 = ((!(_valueLabel == null) && !string.IsNullOrEmpty(_valueLabel.text)) ? _valueLabel.printedSize : Vector2.zero);
		bool flag = vector.x > 0f;
		bool flag2 = vector2.x > 0f;
		float num = vector.x + vector2.x + _leftPadding + _rightPadding;
		if (flag && flag2)
		{
			num += (float)_keyValueMargin;
		}
		Vector2 result = default(Vector2);
		if (limitWidth == 0)
		{
			result.x = num;
		}
		else
		{
			result.x = limitWidth;
			if (num > (float)limitWidth)
			{
				bool flag3 = flag && _keyLabel.overflowMethod == UILabel.Overflow.ResizeFreely;
				bool flag4 = flag2 && _valueLabel.overflowMethod == UILabel.Overflow.ResizeFreely;
				int num2 = 0;
				if (flag3 || flag4)
				{
					num2 = ((flag3 && flag4) ? ((vector.x > vector2.x) ? 1 : 2) : (flag3 ? 1 : 2));
				}
				switch (num2)
				{
				case 1:
				{
					int num4 = (int)((float)limitWidth - vector2.x - _leftPadding - _rightPadding);
					if (flag && flag2)
					{
						num4 -= _keyValueMargin;
					}
					_keyLabel.overflowWidth = num4;
					vector = _keyLabel.printedSize;
					break;
				}
				case 2:
				{
					int num3 = (int)((float)limitWidth - vector.x - _leftPadding - _rightPadding);
					if (flag && flag2)
					{
						num3 -= _keyValueMargin;
					}
					_valueLabel.overflowWidth = num3;
					vector2 = _valueLabel.printedSize;
					break;
				}
				}
				result.x = vector.x + vector2.x + _leftPadding + _rightPadding;
				if (flag && flag2)
				{
					result.x += _keyValueMargin;
				}
			}
			else
			{
				result.x = num;
			}
		}
		result.x = Mathf.Max(_minWidth, result.x);
		float num5 = Mathf.Max(vector.y, vector2.y) + (_bottomPadding + _topPadding) * TopBottomPaddingRatio;
		float y = ((!_useMinHeight) ? num5 : Mathf.Max(_minHeight, num5));
		result.y = y;
		return result;
	}
}
