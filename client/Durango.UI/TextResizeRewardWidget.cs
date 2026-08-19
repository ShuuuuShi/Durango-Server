using UnityEngine;

namespace Durango.UI;

public class TextResizeRewardWidget : TweenerRewardWidget
{
	[SerializeField]
	private UIWidget _textWidget;

	[SerializeField]
	private int _leftPadding;

	[SerializeField]
	private int _rightPadding;

	[SerializeField]
	private float _centerYPos;

	private Vector3 _baseMainTextPos;

	private Vector3 _baseSubTextPos;

	protected override void OnInit()
	{
		base.OnInit();
		if (_mainLabel != null)
		{
			_baseMainTextPos = _mainLabel.transform.localPosition;
		}
		if (_subLabel != null)
		{
			_baseSubTextPos = _subLabel.transform.localPosition;
		}
	}

	protected override void UpdateLayout()
	{
		Vector3 baseMainTextPos = _baseMainTextPos;
		Vector3 baseSubTextPos = _baseSubTextPos;
		Vector2 vector;
		if (_mainLabel == null || string.IsNullOrEmpty(_mainLabel.text))
		{
			vector = Vector2.zero;
			baseSubTextPos.y = _centerYPos;
		}
		else
		{
			vector = _mainLabel.printedSize;
		}
		Vector2 vector2;
		if (_subLabel == null || string.IsNullOrEmpty(_subLabel.text))
		{
			vector2 = Vector2.zero;
			baseMainTextPos.y = _centerYPos;
		}
		else
		{
			_subLabel.width = Mathf.FloorToInt((float)UIManager.ScreenWidth * 0.6f);
			vector2 = _subLabel.printedSize;
		}
		if (_mainLabel != null)
		{
			_mainLabel.transform.localPosition = baseMainTextPos;
		}
		if (_subLabel != null)
		{
			_subLabel.transform.localPosition = baseSubTextPos;
		}
		_textWidget.width = (int)Mathf.Max(vector.x, vector2.x) + _leftPadding + _rightPadding;
		UIUtility.UpdateAnchors(base.transform);
	}
}
