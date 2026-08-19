using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionListItem : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _background;

	[SerializeField]
	private Color _colorNormal;

	[SerializeField]
	private Color _colorSelected;

	[SerializeField]
	private Color _colorPressed;

	private bool _isSelected;

	private bool _isPressed;

	public FactionType FactionType { get; private set; }

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			RefreshColor();
		}
	}

	public void SetFaction(Faction msg)
	{
		FactionType = msg.Type;
		string id = $"#faction_{msg.Type.ToString()}";
		UIUtility.SetSpriteName(_icon, IconMap.Get(id));
		IsSelected = false;
	}

	private void RefreshColor()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (IsSelected)
		{
			_icon.color = _colorSelected;
			_background.SetActive(true);
		}
		else
		{
			_icon.color = ((!_isPressed) ? _colorNormal : _colorPressed);
			_background.SetActive(false);
		}
	}

	private void OnPress(bool press)
	{
		_isPressed = press;
		RefreshColor();
	}
}
