using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InventoryMenuBar_PC : InventoryMenuBarBase
{
	private void Awake()
	{
		_lockButton.OnHovered = OnHoverLockButton;
		_removeButton.OnHovered = OnHoverRemoveButton;
		_filterButton.OnHovered = OnHoverFilterButton;
		GameSystem<InputSystem>.Instance().On(InputCommand.InventoryMenuBarLock, OnInputShortcut);
		GameSystem<InputSystem>.Instance().On(InputCommand.InventoryMenuBarRemove, OnInputShortcut);
		GameSystem<InputSystem>.Instance().On(InputCommand.InventoryMenuBarFilter, OnInputShortcut);
	}

	private void OnHoverLockButton(bool hover)
	{
		ShowTooltip(hover, _lockButton.Widget, InputCommand.InventoryMenuBarLock, T._("잠금/잠금 해제"));
	}

	private void OnHoverRemoveButton(bool hover)
	{
		ShowTooltip(hover, _removeButton.Widget, InputCommand.InventoryMenuBarRemove, T._("버리기"));
	}

	private void OnHoverFilterButton(bool hover)
	{
		ShowTooltip(hover, _filterButton.Widget, InputCommand.InventoryMenuBarFilter, T._("필터"));
	}

	private void ShowTooltip(bool show, UIWidget parent, InputCommand command, string description)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Direction = TooltipBase.TooltipDirection.Vertical;
			buttonInfoTooltip.Sign = 1;
			if (show)
			{
				buttonInfoTooltip.Set(command, description);
				buttonInfoTooltip.Show(parent, Vector3.up * 10f, float.MaxValue);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}

	private void OnInputShortcut(InputCommandMessage message)
	{
		switch (message.Command)
		{
		case InputCommand.InventoryMenuBarLock:
			if (!_lockButton.Disabled)
			{
				OnClickLockButton();
			}
			break;
		case InputCommand.InventoryMenuBarRemove:
			if (!_removeButton.Disabled)
			{
				OnClickRemoveButton();
			}
			break;
		case InputCommand.InventoryMenuBarFilter:
			if (!_filterButton.Disabled)
			{
				OnFilterButton();
			}
			break;
		}
	}
}
