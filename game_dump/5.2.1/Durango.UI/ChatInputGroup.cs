using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using L10N;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChatInputGroup : UIBase
{
	private const string HiddenKey = "ChatInput";

	[SerializeField]
	private UISprite _backgroundSprite;

	[SerializeField]
	private UIWidget _viewerWidget;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private UIWidget _textListContainer;

	[SerializeField]
	private ChatInputLineTextWidget _textLineBase;

	[SerializeField]
	private int _textListCount;

	[SerializeField]
	private UILabel _channelLabel;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private RectLayout _layout;

	private ListObjectPool<ChatInputLineTextWidget> _textList;

	private UIBase[] _hiddenWhenOpened;

	private void Start()
	{
		_textList = new ListObjectPool<ChatInputLineTextWidget>();
		_textList.BaseObject = _textLineBase;
		_hiddenWhenOpened = new UIBase[5]
		{
			UIManager.FindScript<BottomLeftMenuGroupBase>(),
			UIManager.FindScript<MenuListGroupBase>(),
			UIManager.FindScript<CombatGroup>(),
			UIManager.FindScript<InteractionHelperGroupBase>(),
			UIManager.FindScript<ContextActionGroupBase>()
		};
		_textInput.defaultText = string.Empty;
		EventDelegate.Set(_textInput.onSubmit, OnSubmit);
		UIEventListener uIEventListener = UIEventListener.Get(_textInput.gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject obj, bool selected)
		{
			if (!selected)
			{
				Close();
			}
		});
		UIWidget component = _textInput.GetComponent<UIWidget>();
		if (TouchScreenKeyboard.isSupported)
		{
			component.alpha = 0f;
			component.enabled = false;
		}
		else
		{
			component.alpha = 1f;
			component.enabled = true;
		}
		_textList.Set(_textListCount);
		if (_textListCount > 0)
		{
			_textListContainer.gameObject.SetActive(value: true);
			_textListContainer.height = (int)UIUtility.WidgetsReposition(_textList, _textListContainer, Vector3.up);
		}
		else
		{
			_textListContainer.gameObject.SetActive(value: false);
		}
		GameSystem<InputSystem>.Instance().On(InputCommand.PopChat, PopChat);
		GameSystem<InputSystem>.Instance().On(InputCommand.PopChatImmediately, PopChatImmediately);
		GameSystem<InputSystem>.Instance().On(InputCommand.NextChannel, NextChannel);
		GameSystem<InputSystem>.Instance().On(InputCommand.PreviousChannel, PrevChannel);
		GameSystem<SocialSystem>.Instance().ChatAdded += OnChatAdded;
		UIEventListener uIEventListener2 = UIEventListener.Get(_touchBox);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(_touchBox);
		uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, (UIEventListener.VectorDelegate)delegate
		{
			UIManager.SetCurrentUITouchEvent(enable: false);
			Close();
		});
		base.VisibleController.Changed += delegate(bool visible)
		{
			if (!visible)
			{
				Close();
			}
		};
		SetChildrenActive(activated: false);
	}

	private void OnDisable()
	{
		KeyboardHeightChecker.KeyboardHeightUpdated -= OnUpdateKeyboardHeight;
		Conversation.MessagesUpdated -= OnMessageUpdated;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(_viewerWidget.transform);
		OnUpdateKeyboardHeight(KeyboardHeightChecker.Height);
	}

	protected override bool TryOpen()
	{
		KeyboardHeightChecker.KeyboardHeightUpdated += OnUpdateKeyboardHeight;
		Conversation.MessagesUpdated += OnMessageUpdated;
		OnUpdateKeyboardHeight(KeyboardHeightChecker.Height);
		int i = 0;
		for (int size = KUtility.GetSize(_hiddenWhenOpened); i < size; i++)
		{
			if (!(_hiddenWhenOpened[i] == null))
			{
				_hiddenWhenOpened[i].SetVisible(visible: false, "ChatInput");
			}
		}
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		_textInput.isSelected = false;
		KeyboardHeightChecker.KeyboardHeightUpdated -= OnUpdateKeyboardHeight;
		Conversation.MessagesUpdated -= OnMessageUpdated;
		int i = 0;
		for (int size = KUtility.GetSize(_hiddenWhenOpened); i < size; i++)
		{
			if (!(_hiddenWhenOpened[i] == null))
			{
				_hiddenWhenOpened[i].SetVisible(visible: true, "ChatInput");
			}
		}
		return base.TryClose();
	}

	private void PopChat(InputCommandMessage msg)
	{
		ShowTextInput(immediately: false);
	}

	private void PopChatImmediately(InputCommandMessage msg)
	{
		ShowTextInput(immediately: true);
	}

	public override bool Open()
	{
		ShowTextInput(immediately: true);
		return true;
	}

	private void ShowTextInput(bool immediately)
	{
		if (base.Visible)
		{
			base.Open();
			SwitchChannel(0);
			_textInput.value = string.Empty;
			if (immediately)
			{
				_textInput.isSelected = true;
			}
		}
	}

	private void NextChannel(InputCommandMessage msg)
	{
		SwitchChannel(1);
	}

	private void PrevChannel(InputCommandMessage msg)
	{
		SwitchChannel(-1);
	}

	private void SetChannelName(string text)
	{
		uint num = 0u;
		SocialSystem.Channel currentChannel = GameSystem<SocialSystem>.Instance().CurrentChannel;
		Color c;
		if (currentChannel.Type == ChannelType.Conversation)
		{
			c = PresetColor.UIFriendlyPink;
		}
		else
		{
			c = PresetColor.UISunglowYellow;
			num = GameSystem<SocialSystem>.Instance().GetSubscriptionCount(currentChannel.Type);
		}
		_channelLabel.text = ((num == 0) ? string.Format("[{1}]{0}[-]", text, NGUIText.EncodeColor(c)) : string.Format("[{1}]{0}[-] [icon=friends_list] {2}", text, NGUIText.EncodeColor(c), num));
	}

	private void OnUpdateKeyboardHeight(int height)
	{
		height = Mathf.Max(height, 20);
		height = Mathf.Max(height, (int)((float)UIManager.ScreenHeight * UIManager.SafeArea.y));
		Vector3 vector = Vector3.down * ((float)UIManager.ScreenHeight * 0.5f);
		_viewerWidget.SetPosition(vector + Vector3.up * height, 0.5f, 0f);
		_backgroundSprite.height = height + _viewerWidget.height - _textListContainer.height;
		_backgroundSprite.SetPosition(vector, 0.5f, 0f);
	}

	private void SwitchChannel(int amount)
	{
		GameSystem<SocialSystem>.Instance().SwitchChannel(amount);
		SocialSystem.Channel currentChannel = GameSystem<SocialSystem>.Instance().CurrentChannel;
		_textList.BeginLoad();
		if (currentChannel.Type == ChannelType.Conversation)
		{
			Conversation conversation = GameSystem<SocialSystem>.Instance().Conversations.Get(currentChannel.Id);
			if (conversation == null)
			{
				SetChannelName(string.Empty);
				return;
			}
			int size = KUtility.GetSize(conversation.Messages);
			for (int i = 0; i < _textListCount; i++)
			{
				int num = size - 1 - i;
				if (num < 0)
				{
					break;
				}
				_textList.GetNext().Set(conversation.Messages[num]);
			}
			if (!conversation.GetTitle(SetChannelName))
			{
				SetChannelName(string.Empty);
			}
		}
		else
		{
			SetChannelName(ConvertToPlainText(currentChannel.Type));
			List<ChatStruct> chattingList = GameSystem<SocialSystem>.Instance().ChattingList;
			int num2 = KUtility.GetSize(chattingList) - 1;
			for (int j = 0; j < _textListCount; j++)
			{
				if (num2 < 0)
				{
					break;
				}
				if (chattingList[num2].Type == currentChannel.Type)
				{
					_textList.GetNext().Set(chattingList[num2]);
				}
				else
				{
					j--;
				}
				num2--;
			}
		}
		_textList.EndLoad();
	}

	private void AddChat(ChatStruct chat)
	{
		if (!chat.HasTranslatedText)
		{
			if (_textList.Count < _textListCount)
			{
				_textList.Add();
			}
			for (int num = _textList.Count - 1; num >= 1; num--)
			{
				_textList.Swap(num, num - 1);
			}
			_textList[0].Set(chat);
			UIUtility.WidgetsReposition(_textList, _textListContainer, Vector3.up);
		}
	}

	private void OnSubmit()
	{
		string value = _textInput.value;
		_textInput.isSelected = false;
		GameSystem<SocialSystem>.Instance().QuickSay(value);
	}

	private void OnChatAdded(ChatStruct chat)
	{
		if (base.IsOpened)
		{
			SocialSystem.Channel currentChannel = GameSystem<SocialSystem>.Instance().CurrentChannel;
			if (chat.Type == currentChannel.Type)
			{
				AddChat(chat);
			}
		}
	}

	private void OnMessageUpdated(Conversation conv)
	{
		if (base.IsOpened)
		{
			SocialSystem.Channel currentChannel = GameSystem<SocialSystem>.Instance().CurrentChannel;
			if (currentChannel.Type == ChannelType.Conversation && !(currentChannel.Id != conv.Id) && KUtility.GetSize(conv.Messages) != 0)
			{
				AddChat(conv.Messages[conv.Messages.Count - 1]);
			}
		}
	}

	private static string ConvertToPlainText(ChannelType type)
	{
		return type switch
		{
			ChannelType.Region => T._("지역"), 
			ChannelType.Clan => T._("부족"), 
			ChannelType.System => T._("시스템"), 
			ChannelType.ClanWar => T._("부족 전쟁"), 
			ChannelType.Party => T._("파티"), 
			ChannelType.PersonalRegions => T._("개인섬 연합"), 
			_ => string.Empty, 
		};
	}
}
