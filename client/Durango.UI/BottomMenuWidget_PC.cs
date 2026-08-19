using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class BottomMenuWidget_PC : BottomMenuWidgetBase
{
	[CanBeNull]
	[SerializeField]
	private UISprite _chatNotification;

	[SerializeField]
	private RectLayoutComponent _buttonLayout;

	private ChattingGroup_PC _chattingGroup;

	private ChattingGroup_PC.ChatVisibility _lastChatVisibility;

	private MenuListGroup_PC _menuListGroup;

	private bool _existEmotionTooltip;

	public bool ExistEmotionTooltip
	{
		set
		{
			_existEmotionTooltip = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		UIEventListener uIEventListener = UIEventListener.Get(_communicationButton.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHoverCommunicationButton));
		if (_quickChatButton != null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(_quickChatButton.gameObject);
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHoverChatButton));
		}
		GameSystem<InputSystem>.Instance().On(InputCommand.CommunicationMenuButtonAction, OnDoCommunicationButtonAction);
		_chattingGroup = UIManager.FindScript<ChattingGroup_PC>();
		_menuListGroup = UIManager.FindScript<MenuListGroup_PC>();
		EmotionSelector emotionSelector = _emotionSelector;
		emotionSelector.TooltipOpened = (Action)Delegate.Combine(emotionSelector.TooltipOpened, (Action)delegate
		{
			_lastChatVisibility = _chattingGroup.Visibility;
			_chattingGroup.Hide();
			_chattingGroup.HoldShowState = true;
			_existEmotionTooltip = true;
		});
		EmotionSelector emotionSelector2 = _emotionSelector;
		emotionSelector2.TooltipClosed = (Action)Delegate.Combine(emotionSelector2.TooltipClosed, (Action)delegate
		{
			_chattingGroup.HoldShowState = false;
			switch (_lastChatVisibility)
			{
			case ChattingGroup_PC.ChatVisibility.Full:
				_chattingGroup.Show(isFocus: false);
				break;
			case ChattingGroup_PC.ChatVisibility.Shrink:
				_chattingGroup.Shrink();
				break;
			}
			_menuListGroup.RefreshLastButtonsLayout();
		});
		EmotionSelector emotionSelector3 = _emotionSelector;
		emotionSelector3.OnVisible = (Action)Delegate.Combine(emotionSelector3.OnVisible, (Action)delegate
		{
			_menuListGroup.RefreshLastButtonsLayout();
		});
		if (_quickChatButton != null && _quickChatSelector != null)
		{
			_quickChatButton.Initailize(_chattingGroup.OnPressEnter, _quickChatSelector.Show);
			_quickChatSelector.QuickChatClicked += OnClickQuickChat;
			ChattingGroup_PC chattingGroup = _chattingGroup;
			chattingGroup.ShowStateChanged = (Action<ChattingGroup_PC.ChatVisibility>)Delegate.Combine(chattingGroup.ShowStateChanged, (Action<ChattingGroup_PC.ChatVisibility>)delegate(ChattingGroup_PC.ChatVisibility showState)
			{
				SelectableWidget component = _quickChatButton.GetComponent<SelectableWidget>();
				Selectable.State state = ((showState == ChattingGroup_PC.ChatVisibility.Full) ? Selectable.State.Pressed : Selectable.State.Normal);
				if (state != component.GetState())
				{
					component.SetState(state);
				}
				switch (showState)
				{
				case ChattingGroup_PC.ChatVisibility.Full:
					_quickChatButton.ToggleOn = true;
					break;
				case ChattingGroup_PC.ChatVisibility.Shrink:
					_quickChatButton.ToggleOn = false;
					break;
				}
				_menuListGroup.RefreshLastButtonsLayout();
			});
		}
		if (_chatNotification != null)
		{
			_chatNotification.gameObject.SetActive(value: false);
			ChattingGroup_PC chattingGroup2 = _chattingGroup;
			chattingGroup2.TabNotificationStateChaged = (Action<bool>)Delegate.Combine(chattingGroup2.TabNotificationStateChaged, (Action<bool>)delegate(bool hasActiveNotification)
			{
				_chatNotification.gameObject.SetActive(hasActiveNotification);
			});
		}
		UIBase.UIOpened += RefreshCommunicationButton;
		UIBase.UIClosed += RefreshCommunicationButton;
	}

	protected override void OnClickCommunicationButton()
	{
		CommunicationButton_PC communicationButton_PC = (CommunicationButton_PC)_communicationButton;
		if (_existEmotionTooltip)
		{
			communicationButton_PC.ToggleOn = false;
			_emotionSelector.Hide();
			_existEmotionTooltip = false;
		}
		else if (_emotionSelector.State == TooltipBase.VisibleState.Show)
		{
			communicationButton_PC.ToggleOn = false;
			_emotionSelector.Hide();
		}
		else
		{
			communicationButton_PC.ToggleOn = true;
			_emotionSelector.AddOnFinished(OnCloseEmotionSelector);
			_emotionSelector.Show();
		}
	}

	protected override void OnClickQuickChat(string chat)
	{
		base.OnClickQuickChat(chat);
		ShowChatButtonTooltip(show: false);
	}

	private void ShowCommunicationButtonTooltip(bool show)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Sign = 1;
			if (show)
			{
				string description = T._("이모티콘");
				buttonInfoTooltip.Set(InputCommand.CommunicationMenuButtonAction, description);
				buttonInfoTooltip.Show(_communicationButton, new Vector2(-5f, 20f), float.MaxValue);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}

	private void ShowChatButtonTooltip(bool show)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Sign = 1;
			if (show)
			{
				string description = T._("채팅");
				buttonInfoTooltip.Set(InputCommand.PopChatImmediately, description);
				buttonInfoTooltip.Show(_quickChatButton, new Vector2(-5f, 20f), float.MaxValue);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}

	private void OnCloseEmotionSelector()
	{
		CommunicationButton_PC communicationButton_PC = _communicationButton as CommunicationButton_PC;
		if (communicationButton_PC != null)
		{
			communicationButton_PC.ToggleOn = false;
		}
	}

	private void OnHoverCommunicationButton(GameObject go, bool state)
	{
		ShowCommunicationButtonTooltip(state);
	}

	private void OnDoCommunicationButtonAction(InputCommandMessage message)
	{
		OnClickCommunicationButton();
	}

	private void OnHoverChatButton(GameObject go, bool state)
	{
		ShowChatButtonTooltip(state);
	}

	public override void RefreshCommunicationButton()
	{
		SetCommunicationButtonActive(UIBase.CurrentUI == null || UIBase.CurrentUI.Anchor != UIBase.AnchorType.Fullscreen);
		_buttonLayout.UpdateLayout();
	}

	private void OnDestroy()
	{
		UIBase.UIOpened -= RefreshCommunicationButton;
		UIBase.UIClosed -= RefreshCommunicationButton;
	}
}
