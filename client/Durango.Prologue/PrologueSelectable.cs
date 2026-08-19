using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.UI.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using UnityEngine;

namespace Durango.Prologue;

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
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void InteractionTouched()
	{
		Singleton<PrologueManager>.Instance().PlayGuideHelper.FinishTargetIf();
		KUtility.DelayedCall(this, MakeInteractionMenuList, 0.1f);
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		int count = _actionList.Count;
		for (int i = 0; i < count; i++)
		{
			InteractionMenuData data = new InteractionMenuData(Interaction.ClientSidePropAction);
			data.Id = i.ToString();
			data.Name = LocalizeSystem.Get(_actionList[i].name);
			data.Icon = _actionList[i].icon;
			menuList.Add(data);
		}
		string text = GetName();
		menuList.Name = text;
		menuList.Apply();
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		int num = menu.Id.ToInt();
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

	public override string GetName()
	{
		return LocalizeSystem.Get(_propName);
	}

	private void DispatchCommand(Action.Command cmd, float delay)
	{
		switch (cmd)
		{
		case Action.Command.DoFindDropItem:
			KUtility.DelayedCall(this, DoFindDropItem, delay);
			break;
		case Action.Command.DoGetAxe:
			KUtility.DelayedCall(this, Singleton<PrologueManager>.Instance().DoGetAxe, delay);
			KUtility.DelayedCall(this, InteractMotionCompleted, delay);
			break;
		}
	}

	private void DoFindDropItem()
	{
		InteractMotionCompleted();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.LostAndFoundSuccess);
		PrologueInteractionButtonGroupBase.HideInteractionButton();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
	}

	private void InteractMotionCompleted()
	{
		SelectableObject.OnPlayMotionFinished();
		if (GameSystem<InteractionSystem>.Instance().LastInteractionTarget.IsValid())
		{
			SelectionsEnded();
		}
	}
}
