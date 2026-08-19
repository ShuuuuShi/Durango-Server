using System;
using System.Linq;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ChatRoomMaker : SelectableWidget
{
	public Action<int> TabClicked;

	[SerializeField]
	[EnumList(typeof(PlayerSearchGroup.Tab), false, 0, -1)]
	private SelectableWidget[] _buttons;

	[SerializeField]
	private TweenerPlayer _buttonsTweener;

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
				if (TabClicked != null)
				{
					TabClicked(idx);
				}
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
		Open(!IsOpened);
	}

	public void Open(bool isOpen)
	{
		if (IsOpened != isOpen)
		{
			ActivateButtons(isOpen);
			base.Selected = isOpen;
		}
	}

	private void ActivateButtons(bool isActive)
	{
		_buttonsTweener.gameObject.SetActive(isActive);
		if (isActive)
		{
			SelectableWidget selectableWidget = _buttons[2];
			selectableWidget.gameObject.SetActive(PlayerBehavior.LocalPlayer.HasClan);
		}
	}
}
