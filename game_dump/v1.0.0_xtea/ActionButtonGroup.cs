using System;
using System.Collections.Generic;
using CombatData;
using Shared.Battle;
using UnityEngine;

public class ActionButtonGroup : UIBase
{
	[SerializeField]
	private ActionButtonContainer _actionButtons;

	private float _touchLockTime;

	public ActionButtonContainer ActionButtons => _actionButtons;

	private void Start()
	{
		_actionButtons.ActionClicked += OnClickActionButton;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnChangeCombatMode;
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		combatSystem.WaitForAttackStarted = (System.Action)Delegate.Combine(combatSystem.WaitForAttackStarted, new System.Action(OnWaitForAttackStarted));
		CombatSystem combatSystem2 = GameSystem<CombatSystem>.Instance();
		combatSystem2.LeavingBattleStarted = (Action<float>)Delegate.Combine(combatSystem2.LeavingBattleStarted, new Action<float>(OnLeavingBattleStarted));
		GameSystem<CombatSystem>.Instance().ActiveActionsUpdated += OnUpdateActiveActions;
		base.OnOpenSucceed += OnOpenSucceeded;
		TerrainA6.OnInitTerrain(delegate
		{
			OnChangeCombatMode(GameSystem<CombatSystem>.Instance().CombatMode);
		});
	}

	private void OnOpenSucceeded()
	{
		OnUpdateActiveActions();
		_actionButtons.ReserveAutoAction(Connections.Frontend.GetPredictedServerTime());
	}

	public Transform GetActionButtonTransform(string id)
	{
		ActionButton actionButton = _actionButtons.FindActionButton(id);
		return (!((Object)(object)actionButton != (Object)null)) ? null : ((Component)actionButton).transform;
	}

	public ActionButton FindActionButton(string id)
	{
		return _actionButtons.FindActionButton(id);
	}

	public void OnClickActionButton(string key)
	{
		if (!(Time.time < _touchLockTime))
		{
			ActionButton actionButton = FindActionButton(key);
			if (!((Object)(object)actionButton == (Object)null))
			{
				GameSystem<CombatSystem>.Instance().SendReserveAction(key);
				_touchLockTime = Time.time + 0.5f;
			}
		}
	}

	private void OnChangeCombatMode(bool isCombat)
	{
		if (isCombat)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	private void OnWaitForAttackStarted()
	{
		OnUpdateActiveActions();
	}

	private void OnLeavingBattleStarted(float remainTime)
	{
		OnUpdateActiveActions();
	}

	private void OnUpdateActiveActions()
	{
		if (!base.IsOpen)
		{
			return;
		}
		if (GameSystem<CombatSystem>.Instance().CombatState == CombatSystem.State.Battle)
		{
			_touchLockTime = 0f;
			List<CombatData.Action> currentActiveActions = GameSystem<CombatSystem>.Instance().CurrentActiveActions;
			int size = KUtility.GetSize(currentActiveActions);
			string[] array = new string[size];
			ActionGroup[] array2 = new ActionGroup[size];
			string[] array3 = new string[size];
			for (int i = 0; i < size; i++)
			{
				CombatData.Action action = currentActiveActions[i];
				if (action != null)
				{
					array[i] = action.Id;
					array2[i] = action.ActionGroup;
					array3[i] = action.Icon;
				}
			}
			_actionButtons.InitIconActionButtons(array, array2, array3);
			double val = -1.0;
			for (int j = 0; j < size; j++)
			{
				CombatData.Action action2 = currentActiveActions[j];
				if (action2 != null && (action2.State != ActionState.Cooling || !(action2.Until <= Connections.Frontend.GetPredictedServerTime())))
				{
					_actionButtons.SetActionButtonState(action2.Id, action2.State);
					_actionButtons.SetActionButtonDeactiveTime(action2.Id, action2.Since, action2.Until);
					val = Math.Max(val, action2.Until);
				}
			}
		}
		else
		{
			_actionButtons.HideAllActionButtons();
		}
	}
}
