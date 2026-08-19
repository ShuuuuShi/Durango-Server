using System;
using L10N;
using UnityEngine;

public class CombatModeButton : MonoBehaviour
{
	[SerializeField]
	private UIWidget _button;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _buttonText;

	[SerializeField]
	private SpriteData _iconForLeave;

	[SerializeField]
	private SpriteData _iconForAttack;

	[SerializeField]
	private Color _textColorForLeave;

	[SerializeField]
	private Color _textColorForAttack;

	private void Awake()
	{
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		UIEventListener.Get(((Component)_button).gameObject).onClick = OnClickButton;
		combatSystem.WaitForAttackStarted = (Action)Delegate.Combine(combatSystem.WaitForAttackStarted, new Action(CombatSystem_WaitForAttackStarted));
		combatSystem.LeavingBattleStarted = (Action<float>)Delegate.Combine(combatSystem.LeavingBattleStarted, new Action<float>(CombatSystem_LeavingBattleStarted));
		combatSystem.ServerSideBattleBegun = (Action)Delegate.Combine(combatSystem.ServerSideBattleBegun, new Action(CombatSystem_ServerSideBattleBegun));
		RefreshButtons();
	}

	private void RefreshButtons()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		if (combatSystem.CombatState == CombatSystem.State.Battle)
		{
			_iconForLeave.Set(_icon);
			_buttonText.text = T._("전투 중지");
			_buttonText.color = _textColorForLeave;
		}
		else
		{
			_iconForAttack.Set(_icon);
			_buttonText.text = T._("반격!");
			_buttonText.color = _textColorForAttack;
		}
		((Component)_button).gameObject.SetActive(combatSystem.CombatState != CombatSystem.State.None);
	}

	private void OnClickButton(GameObject go)
	{
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		switch (combatSystem.CombatState)
		{
		case CombatSystem.State.Battle:
			combatSystem.RequestServerSideBattleLeaving();
			break;
		case CombatSystem.State.Leaving:
			combatSystem.ReEnterBattle();
			break;
		case CombatSystem.State.Waiting:
			combatSystem.RequestAttack();
			break;
		}
	}

	private void CombatSystem_WaitForAttackStarted()
	{
		RefreshButtons();
	}

	private void CombatSystem_LeavingBattleStarted(float remainTime)
	{
		RefreshButtons();
	}

	private void CombatSystem_ServerSideBattleBegun()
	{
		RefreshButtons();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		_button.ResetAnchors();
	}
}
