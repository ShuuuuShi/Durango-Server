using System;
using System.Linq;
using Durango.UI.Control;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class PositionSharer : SelectableWidget
{
	private enum ButtonType
	{
		ShareCurrentPos,
		SelectSharePos
	}

	[SerializeField]
	[EnumList(typeof(ButtonType), false, 0, -1)]
	private SelectableWidget[] _buttons;

	[SerializeField]
	private TweenerPlayer _buttonsTweener;

	public bool IsAllChatChannel { get; set; }

	public ChannelType? SpecifiedChannelType { get; set; }

	public string SpecifiedConversationId { get; set; }

	public bool IsOpened => base.Selected;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPressMouse));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPressMouse));
	}

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
		Clicked = OnClickIcon;
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, delegate
		{
			Open(isOpen: false);
		});
		for (int i = 0; i < _buttons.Length; i++)
		{
			int idx = i;
			_buttons[idx].Clicked = delegate
			{
				OnClickButton(idx);
			};
		}
		ActivateButtons(isActive: false);
	}

	private void OnPressMouse(GameObject go, bool isPressed)
	{
		if (isPressed && IsOpened && !(go == base.gameObject) && !_buttons.Any((SelectableWidget x) => x.gameObject == go))
		{
			Open(isOpen: false);
		}
	}

	private void OnClickIcon()
	{
		if (!base.Disabled)
		{
			Open(!IsOpened);
		}
	}

	private void OnClickButton(int idx)
	{
		ChannelType? channelType = ((!IsAllChatChannel) ? null : SpecifiedChannelType);
		string conversationId = ((!IsAllChatChannel) ? null : SpecifiedConversationId);
		switch ((ButtonType)idx)
		{
		case ButtonType.SelectSharePos:
			UIManager.FindScript<WorldMapGroup>().OpenForSharePos(channelType, conversationId);
			break;
		case ButtonType.ShareCurrentPos:
			WorldMapGroup.ShareCurrentPos(channelType, conversationId);
			break;
		}
		Open(isOpen: false);
	}

	public void Open(bool isOpen)
	{
		if (IsOpened != isOpen)
		{
			ActivateButtons(isOpen);
			base.Selected = isOpen;
		}
	}

	public void SetEnabled(bool enable)
	{
		base.Disabled = !enable;
	}

	private void ActivateButtons(bool isActive)
	{
		_buttonsTweener.gameObject.SetActive(isActive);
	}
}
