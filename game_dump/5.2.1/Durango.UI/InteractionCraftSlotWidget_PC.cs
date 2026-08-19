using UnityEngine;

namespace Durango.UI;

public class InteractionCraftSlotWidget_PC : InteractionCraftSlotWidget
{
	private int CommandCount = 5;

	[SerializeField]
	private UILabel _keyLabel;

	private int _index;

	private bool _initQuickKey;

	protected override void Awake()
	{
		base.Awake();
		_keyLabel.text = string.Empty;
	}

	public override void SetIndex(int index)
	{
		InitQuickKey(index);
	}

	private void InitQuickKey(int index)
	{
		if (_initQuickKey)
		{
			return;
		}
		_index = index;
		if (_index < 0 || _index >= CommandCount)
		{
			_keyLabel.text = string.Empty;
		}
		else
		{
			InputCommand command = GetCommand(_index);
			if (command != 0)
			{
				GameSystem<InputSystem>.Instance().On(command, OnQuickKey);
			}
			_keyLabel.text = InputSystem.GetKeyCaption(command);
		}
		_initQuickKey = true;
	}

	private void OnQuickKey(InputCommandMessage message)
	{
		if (base.gameObject.activeInHierarchy)
		{
			OnClick();
		}
	}

	private InputCommand GetCommand(int index)
	{
		if (index < 0 || index >= CommandCount)
		{
			return InputCommand.None;
		}
		return (InputCommand)(44 + index);
	}
}
