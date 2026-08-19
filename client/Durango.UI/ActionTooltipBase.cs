using Durango.Logic.Combat;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public abstract class ActionTooltipBase : TooltipBase
{
	[SerializeField]
	protected KeyValueLabel _titleLabel;

	[SerializeField]
	protected UILabel _descriptionLabel;

	[SerializeField]
	protected RectLayout _layout;

	private BattleAction _playerAction;

	private PlayerAction _defaultPlayerAction;

	private PetActiveSkill _petAction;

	private ContextActionButtonBase _contextActionButton;

	private bool _isTamingHelp;

	private void ResetArgument()
	{
		_playerAction = null;
		_defaultPlayerAction = null;
		_petAction = null;
		_contextActionButton = null;
		_isTamingHelp = false;
	}

	public void Set(BattleAction action)
	{
		_playerAction = action;
	}

	public void Set(PlayerAction action)
	{
		_defaultPlayerAction = action;
	}

	public void Set(PetActiveSkill action)
	{
		_petAction = action;
	}

	public void Set(ContextActionButtonBase contextActionButton)
	{
		_contextActionButton = contextActionButton;
	}

	public void SetTamingHelp()
	{
		_isTamingHelp = true;
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetArgument();
	}

	private void Fill([NotNull] BattleAction action)
	{
		string text = null;
		if (action.Stamina > 0f)
		{
			text = $"[icon=img_character_energy] {action.Stamina:0}";
		}
		_titleLabel.Set(action.Data.Meta.Name.ToString(), text);
		_descriptionLabel.text = action.Data.Meta.Description;
	}

	private void Fill([NotNull] PlayerAction action)
	{
		string text = null;
		if (action.Meta.Stamina > 0f)
		{
			text = $"[icon=img_character_energy] {action.Meta.Stamina:0}";
		}
		_titleLabel.Set(action.Meta.Name.ToString(), text);
		Node parentSkill = action.GetParentSkill();
		if (parentSkill == null)
		{
			_descriptionLabel.text = action.Meta.Description;
			return;
		}
		_descriptionLabel.text = string.Format("{0}<br>10</br>{1}", action.Meta.Description, T._("<em>{0}</em> 스킬이 필요합니다.", parentSkill.Name));
	}

	private void Fill([NotNull] PetActiveSkill action)
	{
		_titleLabel.Set(action.Name.ToString(), string.Empty);
		_descriptionLabel.text = action.Description;
	}

	private void Fill([NotNull] ContextActionButtonBase contextActionButton)
	{
		_titleLabel.Set(contextActionButton.Menu.Action.GetName(), string.Empty);
		_descriptionLabel.text = contextActionButton.Description;
	}

	private void FillTamingHelp()
	{
		_titleLabel.Set(T._("포획"), null);
		_descriptionLabel.text = T._("동물 포획은 포획용 아이템이 필요하며, 동물 체력이 일정 이하일 때 사용할 수 있습니다.");
	}

	protected override void FillData()
	{
		if (_isTamingHelp)
		{
			FillTamingHelp();
		}
		else if (_playerAction != null)
		{
			Fill(_playerAction);
		}
		else if (_defaultPlayerAction != null)
		{
			Fill(_defaultPlayerAction);
		}
		else if (_petAction != null)
		{
			Fill(_petAction);
		}
		else if (_contextActionButton != null)
		{
			Fill(_contextActionButton);
		}
	}
}
