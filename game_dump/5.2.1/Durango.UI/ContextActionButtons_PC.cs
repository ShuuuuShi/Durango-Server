using System.Collections.Generic;
using Durango.Logic.InputSystem;
using Durango.UI.Control;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class ContextActionButtons_PC : ContextActionButtonsBase
{
	private const int MaxActionCount = 8;

	public override void SetActions(List<InteractionMenuData> menus)
	{
		base.SetActions(menus);
		int num = Mathf.Min(menus.Count, 8);
		for (int i = 0; i < 8; i++)
		{
			GameSystem<InputSystem>.Instance().Off((InputCommand)(70 + i), OnDoContextAction);
		}
		for (int j = 0; j < num; j++)
		{
			ContextActionButton_PC contextActionButton_PC = _actionButtons[j] as ContextActionButton_PC;
			if (contextActionButton_PC != null)
			{
				InputCommand inputCommand = (InputCommand)(70 + j);
				contextActionButton_PC.SetShortcut(GameSystem<InputSystem>.Instance().Keyboard.GetFirstKeySet(inputCommand).Code);
				GameSystem<InputSystem>.Instance().On(inputCommand, OnDoContextAction);
			}
		}
	}

	private void OnDoContextAction(InputCommandMessage message)
	{
		ContextActionButtonBase contextActionButtonBase = _actionButtons[(int)(message.Command - 70)];
		switch (message.CurrentTrigger)
		{
		case Trigger.Down:
			contextActionButtonBase.GetComponent<SelectableWidget>().SetState(Selectable.State.Pressed);
			contextActionButtonBase.OnPress(press: true);
			break;
		case Trigger.Up:
			contextActionButtonBase.GetComponent<SelectableWidget>().SetState(Selectable.State.Normal);
			contextActionButtonBase.OnPress(press: false);
			OnClickActionButton(contextActionButtonBase);
			break;
		}
	}
}
