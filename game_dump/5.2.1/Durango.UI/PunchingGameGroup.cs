using System;
using Durango.Logic.Combat;
using Durango.UI.Control;
using InteractionData;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class PunchingGameGroup : UIBase
{
	[SerializeField]
	private SelectableButton _challengeButton;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SoundEventType _insertCoinAudio;

	[SerializeField]
	private SoundEventType _startGameAudio;

	[SerializeField]
	private long _gameMoney = 100L;

	private Artifact _target;

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.OnePunch, delegate(InteractionObject target)
		{
			if (target != null)
			{
				_target = target.GetTargetComponent<Artifact>();
				Open();
			}
		});
		SelectableButton challengeButton = _challengeButton;
		challengeButton.Clicked = (Action)Delegate.Combine(challengeButton.Clicked, new Action(ChallengeButtonClicked));
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(CancelButton_Clicked));
		base.OnOpenSucceed += delegate
		{
			SoundManager.PlayEvent(_insertCoinAudio);
		};
		base.OnCloseSucceed += delegate
		{
			_target = null;
		};
		SoundManager.PrepareEvent(_insertCoinAudio);
		SoundManager.PrepareEvent(_startGameAudio);
		SetChildrenActive(activated: false);
	}

	private void ChallengeButtonClicked()
	{
		if (_target == null)
		{
			Close();
			return;
		}
		if (InventorySystem.Wallet.GetBalance(Currency.TStone) >= _gameMoney)
		{
			SoundManager.PlayEvent(_startGameAudio);
		}
		Artifact target = _target;
		Close();
		BattleAction[] actionSlots = GameSystem<CombatSystem>.Instance().ActionSlots;
		if (actionSlots != null && actionSlots.Length >= 2)
		{
			BattleAction battleAction = actionSlots[1];
			if (battleAction != null)
			{
				GameSystem<CombatSystem>.Instance().UseBattleAction(battleAction.Data.Id, new ArtifactDamageableEntity(target), targetSelect: false);
				Invoke("ExitPunchingBattle", battleAction.Data.Meta.ActionLength);
			}
		}
	}

	private void ExitPunchingBattle()
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			CombatSystem.ExitBattle();
		}
	}

	private void CancelButton_Clicked()
	{
		Close();
	}
}
