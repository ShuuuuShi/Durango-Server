using UnityEngine;

public class PrologueCustomButton : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _labelBg;

	[SerializeField]
	private UIWidget _labelWidget;

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _colorSprite;

	private Color _defaultColor = Color.clear;

	public string Text
	{
		get
		{
			return GetText();
		}
		set
		{
			_label.text = value;
		}
	}

	public void SetText(string text)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_label.UpdateNGUIText();
		NGUIText.regionWidth = UIManager.ScreenWidth;
		NGUIText.finalSize = _label.fontSize;
		Vector2 val = NGUIText.CalculatePrintedSize(text);
		_labelWidget.width = (int)val.x + 30;
		_label.text = text;
		_label.UpdateAnchors();
		_labelBg.UpdateAnchors();
	}

	public string GetText()
	{
		return _label.text;
	}

	public void SetColor(Color color)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_colorSprite != (Object)null)
		{
			_colorSprite.color = color;
		}
	}

	public Color GetColor()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_colorSprite != (Object)null)
		{
			return _colorSprite.color;
		}
		return Color.clear;
	}

	public static void SetButtonsText(string[] localizeKey, PrologueCustomButton[] buttons)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min((localizeKey != null) ? localizeKey.Length : 0, (buttons != null) ? buttons.Length : 0);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			buttons[i]._label.UpdateNGUIText();
			NGUIText.regionWidth = UIManager.ScreenWidth;
			NGUIText.finalSize = buttons[i]._label.fontSize;
			string text = localizeKey[i];
			Vector2 val = NGUIText.CalculatePrintedSize(text);
			buttons[i]._label.text = text;
			num2 = Mathf.Max(num2, (int)val.x + 30);
		}
		for (int j = 0; j < num; j++)
		{
			buttons[j]._labelWidget.width = num2;
			buttons[j]._label.UpdateAnchors();
			buttons[j]._labelBg.UpdateAnchors();
		}
	}

	public void PressAnimation(bool press)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (_defaultColor == Color.clear)
		{
			_defaultColor = _icon.color;
		}
		if (press)
		{
			Color uIYellow = UIManager.UIYellow;
			_bg.color = uIYellow;
			_labelBg.color = uIYellow;
			_icon.color = uIYellow;
		}
		else
		{
			Color defaultColor = _defaultColor;
			_bg.color = defaultColor;
			_labelBg.color = defaultColor;
			_icon.color = defaultColor;
		}
	}
}
