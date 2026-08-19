using System;
using System.Collections.Generic;
using InteractionData;
using UnityEngine;

internal class PrologueSelectableEatAndDrink : SelectableObject
{
	[Serializable]
	public class Action
	{
		public enum Command
		{
			None,
			EatCompleted,
			DrinkCompleted
		}

		public string name = string.Empty;

		public string icon = string.Empty;

		public string motion = string.Empty;

		public float motionDuration;

		public float commandDelay;

		public Command command;
	}

	[SerializeField]
	private string _propName = string.Empty;

	[SerializeField]
	private List<Action> _actionList = new List<Action>();

	[SerializeField]
	private bool _removeAfterInteraction;

	private bool _eatCompleted;

	private bool _drinkCompleted;

	public void SelectionsEnded()
	{
		if (_removeAfterInteraction)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public override void InteractionTouched()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<UIManager>.Instance().PlayGuideHelper.FinishArrowTargetIf(((Component)this).transform.position);
		KUtility.DelayedCall((MonoBehaviour)(object)this, MakeInteractionMenuList, 0.1f);
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		int count = _actionList.Count;
		for (int i = 0; i < count; i++)
		{
			InteractionMenuData data = new InteractionMenuData(Interaction.ClientSidePropAction);
			data.Id = (ulong)i;
			data.Name = LocalizeSystem.Get(_actionList[i].name);
			data.Icon = _actionList[i].icon;
			menuList.Add(data);
		}
		string name = LocalizeSystem.Get(_propName);
		menuList.Name = name;
		menuList.Apply();
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)this).gameObject);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		int num = (int)menu.Id;
		if (0 > num || num >= _actionList.Count)
		{
			return false;
		}
		Action action = _actionList[num];
		SelectableObject.PlayMotion(action.motion, action.motionDuration);
		if (action.command != 0)
		{
			DispatchCommand(action.command, action.commandDelay);
		}
		return true;
	}

	private void DispatchCommand(Action.Command cmd, float delay)
	{
		switch (cmd)
		{
		case Action.Command.EatCompleted:
			KUtility.DelayedCall((MonoBehaviour)(object)this, EatCompleted, delay);
			break;
		case Action.Command.DrinkCompleted:
			KUtility.DelayedCall((MonoBehaviour)(object)this, DrinkCompleted, delay);
			break;
		}
	}

	private void EatCompleted()
	{
		SelectableObject.ShowInteractionButton(show: true);
		if (!_eatCompleted)
		{
			GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("eat", completed: true);
			GameSystem<PlayerStatusEffectSystem>.Instance().RemoveStatusEffectPrologue("hungry");
			_eatCompleted = true;
			if (_eatCompleted && _drinkCompleted)
			{
				EatAndDrinkCompleted();
			}
			else
			{
				GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.RequireDrink);
			}
		}
	}

	private void DrinkCompleted()
	{
		SelectableObject.ShowInteractionButton(show: true);
		if (!_drinkCompleted)
		{
			GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("drink", completed: true);
			GameSystem<PlayerStatusEffectSystem>.Instance().RemoveStatusEffectPrologue("thirst");
			_drinkCompleted = true;
			if (_eatCompleted && _drinkCompleted)
			{
				EatAndDrinkCompleted();
			}
			else
			{
				GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.RequireFood);
			}
		}
	}

	private static void EatAndDrinkCompleted()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.RequestFromClerk);
		GameSystem<InteractionSystem>.Instance().LastInteractionTarget?.GetTargetComponent<PrologueSelectableEatAndDrink>().SelectionsEnded();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		InteractionGroupHelper.HideInteractionButton();
	}
}
