using UnityEngine;

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

	public UILabel Label => _label;

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

	private void Init()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_defaultColor = _bg.color;
		}
	}

	private void OnPress(bool press)
	{
		Select(press);
	}

	public void Select(bool select)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_bg.color = ((!select) ? _defaultColor : _selectColor);
	}

	public void UpdateAnchor()
	{
		Widget.ResetAndUpdateAnchors();
		_label.ResetAndUpdateAnchors();
		_bg.ResetAndUpdateAnchors();
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
	}
}
