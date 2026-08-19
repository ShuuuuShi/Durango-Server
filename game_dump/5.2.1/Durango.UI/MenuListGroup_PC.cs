using System;
using Durango.Logic;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Menu")]
public class MenuListGroup_PC : MenuListGroupBase
{
	[SerializeField]
	private UIWidget _menuButtonWidget;

	[SerializeField]
	private RectLayoutComponent _lastActionButtonsLayout;

	[SerializeField]
	private UIWidget _chattingSpacerWidget;

	[SerializeField]
	private UILabel _lastMenuShortcut;

	[SerializeField]
	private UILabel _lastAddedItemShortcut;

	[SerializeField]
	private UIWidget _communicationSpacerWidget;

	private ChattingGroup_PC _chatGroup;

	private BottomLeftMenuGroupBase _bottomLeftMenuGroup;

	protected override void Start()
	{
		base.Start();
		UIEventListener uIEventListener = UIEventListener.Get(_menuBtn);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHoverGameMenuButton));
		UIEventListener uIEventListener2 = UIEventListener.Get(_menuBtn);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickGameMenuButton));
		GameSystem<InputSystem>.Instance().On(InputCommand.ShowMenuList, OnReceiveMessage, InputSystem.Priority.Lower);
		_chatGroup = UIManager.FindScript<ChattingGroup_PC>();
		if (_chatGroup != null)
		{
			GameSystem<InputSystem>.Instance().On(InputCommand.PopChatImmediately, delegate
			{
				if (base.IsOpened)
				{
					Close();
					_chatGroup.Show();
				}
				RefreshLastButtonsLayout();
			});
		}
		InteractionGroup interactionGroup = UIManager.FindScript<InteractionGroup>();
		if (interactionGroup != null)
		{
			interactionGroup.InteractionMenuShowed = (Action)Delegate.Combine(interactionGroup.InteractionMenuShowed, (Action)delegate
			{
				Close();
			});
		}
		_bottomLeftMenuGroup = UIManager.FindScript<BottomLeftMenuGroupBase>();
		_lastAddedItemShortcut.text = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(InputCommand.ShowLastAddedItem);
		_lastMenuShortcut.text = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(InputCommand.RepeatLastMenu);
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnMouseClick));
	}

	private void OnDisable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnMouseClick));
	}

	private void OnMouseClick(GameObject go)
	{
		if (base.IsOpened && TouchBlockBox.gameObject.activeSelf && !UIUtility.IsWidgetContainsMousePointer(TouchBlockBox))
		{
			Close();
		}
	}

	protected override bool GetMenuLockState()
	{
		return false;
	}

	protected override void SetMenuLayout(MenuLayout layout)
	{
		base.SetMenuLayout(MenuLayout.Landscape);
	}

	private void ShowGameMenuButtonTooltip(bool show)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Sign = 1;
			if (show)
			{
				string description = T._("게임 메뉴");
				buttonInfoTooltip.Set(InputCommand.ShowMenuList, description);
				buttonInfoTooltip.Show(_menuButtonWidget, new Vector2(-5f, 20f), float.MaxValue);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}

	protected override bool HideUIFunc(VisibleController script)
	{
		if (script != base.VisibleController && (script.Flag & VisibleType.Base) != 0)
		{
			return (script.Flag & VisibleType.HideOnLeftMenu) != 0;
		}
		return false;
	}

	protected override bool IsButtonVisible()
	{
		if (!base.IsOpened)
		{
			return !IsMenuLocked();
		}
		return false;
	}

	private void OnHoverGameMenuButton(GameObject go, bool state)
	{
		ShowGameMenuButtonTooltip(state);
	}

	private void OnClickGameMenuButton(GameObject go)
	{
		ShowGameMenuButtonTooltip(show: false);
	}

	protected override void OnMenuClick(MenuType type)
	{
		UIBase.CloseAllUI();
		base.OnMenuClick(type);
	}

	private void OnReceiveMessage(InputCommandMessage message)
	{
		if (base.Visible)
		{
			ShowGameMenuButtonTooltip(show: false);
			if (base.IsOpened)
			{
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	public override void RefreshLastButtonsLayout()
	{
		if (!(_chatGroup == null) && !(_bottomLeftMenuGroup == null))
		{
			_chattingSpacerWidget.gameObject.SetActive(_chatGroup.Visibility != ChattingGroup_PC.ChatVisibility.Hide);
			_communicationSpacerWidget.gameObject.SetActive(_bottomLeftMenuGroup.BottomMenuWidget.IsEmotionSelectorVisible);
			_lastActionButtonsLayout.UpdateLayout();
		}
	}
}
