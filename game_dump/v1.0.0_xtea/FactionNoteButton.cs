using UnityEngine;

public class FactionNoteButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconArrow;

	[SerializeField]
	private UILabel _label;

	private bool _isEnabled;

	private bool _isPressed;

	private Color _colorNormal;

	private Color _colorPressed;

	private Color _colorDisabled;

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			_isEnabled = value;
			RefreshColor();
		}
	}

	public void SetColors(Color normal, Color pressed, Color disabled)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_colorNormal = normal;
		_colorPressed = pressed;
		_colorDisabled = disabled;
	}

	private void RefreshColor()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			if (_isPressed)
			{
				_iconArrow.color = _colorPressed;
				_label.color = _colorPressed;
			}
			else
			{
				_iconArrow.color = _colorNormal;
				_label.color = _colorNormal;
			}
		}
		else
		{
			_iconArrow.color = _colorDisabled;
			_label.color = _colorDisabled;
		}
	}

	private void OnPress(bool press)
	{
		_isPressed = press;
		RefreshColor();
	}
}
