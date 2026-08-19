using UnityEngine;

public class TabItem : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private Color _defaultBGColor = UIManager.UIBlack;

	[SerializeField]
	private Color _selectBGColor = UIManager.UIYellow;

	[SerializeField]
	private Color _defaultTextColor = UIManager.UIMoreLightGray;

	[SerializeField]
	private Color _selectTextColor = UIManager.UIBlack;

	[SerializeField]
	private int _margin = 20;

	[SerializeField]
	private int _minWidth = 200;

	private bool _isSelect;

	private UIWidget _widget;

	public bool IsSelect
	{
		get
		{
			return _isSelect;
		}
		set
		{
			Select(value);
		}
	}

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	public string LocalizeKey { get; set; }

	public string Format { get; set; }

	public void Localize()
	{
		string text = ((Format != null) ? string.Format(Format, LocalizeSystem.Get(LocalizeKey)) : LocalizeSystem.Get(LocalizeKey));
		UISpriteLabel component = ((Component)_text).GetComponent<UISpriteLabel>();
		if ((Object)(object)component == (Object)null)
		{
			_text.text = text;
		}
		else
		{
			component.text = text;
		}
	}

	public int CalcMinWidth()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_text.UpdateNGUIText();
		NGUIText.regionWidth = 1280;
		float x = NGUIText.CalculatePrintedSize(_text.text).x;
		return (int)Mathf.Max(x + (float)_margin * 2f, (float)_minWidth);
	}

	public void Select(bool select)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		_isSelect = select;
		_text.color = ((!select) ? _defaultTextColor : _selectTextColor);
		_background.color = ((!select) ? _defaultBGColor : _selectBGColor);
	}
}
