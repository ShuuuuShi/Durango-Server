using System;
using System.Collections.Generic;
using InteractionData;
using UnityEngine;

internal class PrologueSelectable : SelectableObject
{
	[Serializable]
	public class Action
	{
		public enum Command
		{
			None,
			DoFindDropItem,
			DoGetAxe
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
		case Action.Command.DoFindDropItem:
			KUtility.DelayedCall((MonoBehaviour)(object)this, DoFindDropItem, delay);
			break;
		case Action.Command.DoGetAxe:
			KUtility.DelayedCall((MonoBehaviour)(object)this, KSingleton<PrologueManager>.Instance().DoGetAxe, delay);
			KUtility.DelayedCall((MonoBehaviour)(object)this, InteractMotionCompleted, delay);
			break;
		}
	}

	private void DoFindDropItem()
	{
		SelectableObject.ShowInteractionButton(show: true);
		InteractMotionCompleted();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.LostAndFoundSuccess);
		InteractionGroupHelper.HideInteractionButton();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
	}

	private void InteractMotionCompleted()
	{
		SelectableObject.ShowInteractionButton(show: true);
		if (GameSystem<InteractionSystem>.Instance().LastInteractionTarget.IsValid())
		{
			SelectionsEnded();
		}
	}
}
