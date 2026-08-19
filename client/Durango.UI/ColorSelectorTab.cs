using UnityEngine;

namespace Durango.UI;

public class ColorSelectorTab : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private Color _selectColor;

	private Color _defaultColor;

	private UIWidget _widget;

	private bool _isInit;

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

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_defaultColor = _bg.color;
		}
	}

	public float Set(string text)
	{
		_label.text = text;
		return _label.printedSize.x + 30f;
	}

	private void OnPress(bool press)
	{
		Select(press);
	}

	public void Select(bool select)
	{
		Init();
		_bg.color = ((!select) ? _defaultColor : _selectColor);
	}

	public void UpdateAnchor()
	{
		Widget.ResetAndUpdateAnchors();
		_label.ResetAndUpdateAnchors();
		_bg.ResetAndUpdateAnchors();
	}
}
