using L10N;
using UnityEngine;

public class CombatTargetWidgetSelectButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private GameObject _selection;

	[SerializeField]
	private SpriteData _iconForNonCombatMode;

	[SerializeField]
	private SpriteData _iconForCombatMode;

	[LocalizableString]
	[SerializeField]
	private string _textForNonCombatMode;

	[SerializeField]
	[LocalizableString]
	private string _textForCombatMode;

	private bool _combatMode;

	public bool IsSelected
	{
		get
		{
			return _selection.activeSelf;
		}
		set
		{
			_selection.SetActive(value);
		}
	}

	private void Awake()
	{
		SetNonCombatMode();
	}

	public void Refresh(bool combatMode)
	{
		if (_combatMode != combatMode)
		{
			_combatMode = combatMode;
			if (_combatMode)
			{
				SetCombatMode();
			}
			else
			{
				SetNonCombatMode();
			}
		}
	}

	private void SetCombatMode()
	{
		_iconForCombatMode.Set(_icon);
		_text.text = T._(_textForCombatMode);
	}

	private void SetNonCombatMode()
	{
		_iconForNonCombatMode.Set(_icon);
		_text.text = T._(_textForNonCombatMode);
	}
}
