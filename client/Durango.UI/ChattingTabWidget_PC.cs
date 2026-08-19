using System;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ChattingTabWidget_PC : SelectableWidget
{
	public Action<ChattingTabWidget_PC> TabClicked;

	public Action<Vector2> TabDragged;

	public Action<float> TabScrolled;

	public Action<bool> TabPressed;

	public Action IndividualTabCreated;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _notification;

	[SerializeField]
	private int _sidePadding;

	private Conversation _currentConv;

	public bool HasNewChat { get; private set; }

	public bool IsOpened => base.Selected;

	public bool IsMainChannel => _currentConv == null;

	public Conversation CurrentConv
	{
		get
		{
			return _currentConv;
		}
		private set
		{
			if (_currentConv != value)
			{
				Id = ((value != null) ? value.Id : string.Empty);
			}
			_currentConv = value;
		}
	}

	public string Id { get; private set; }

	public ChatFilterType FilterType { get; private set; }

	public string TabName => _nameLabel.text;

	protected override void OnInit()
	{
		base.OnInit();
		ClickSound = UISound.ClickType.ButtonMedium;
		Clicked = (Action)Delegate.Combine(Clicked, (Action)delegate
		{
			if (TabClicked != null)
			{
				TabClicked(this);
			}
		});
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		if (base.IsChangeSelected && base.Selected)
		{
			RefreshNotification();
		}
	}

	[UsedImplicitly]
	protected override void OnPress(bool isPress)
	{
		base.Pressed = isPress;
		if (TabPressed != null)
		{
			TabPressed(isPress);
		}
	}

	[UsedImplicitly]
	private void OnDrag(Vector2 delta)
	{
		if (TabDragged != null)
		{
			TabDragged(delta);
		}
	}

	[UsedImplicitly]
	private void OnScroll(float delta)
	{
		if (TabScrolled != null)
		{
			TabScrolled(delta);
		}
	}

	public void Set(ChatFilterType filterType)
	{
		CurrentConv = null;
		Id = null;
		FilterType = filterType;
		HasNewChat = false;
		UpdateNameLabel();
		RefreshNotification();
		UpdateLayout();
	}

	public void Set(Conversation conversation)
	{
		CurrentConv = conversation;
		HasNewChat = CurrentConv != null && CurrentConv.Notification.Count > 0;
		UpdateNameLabel();
		RefreshNotification();
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		_nameLabel.UpdateAnchors();
		base.Widget.width = _nameLabel.width + _sidePadding;
		UIUtility.UpdateAnchors(base.transform);
	}

	private void UpdateNameLabel()
	{
		if (_currentConv == null)
		{
			_nameLabel.text = FilterType.GetName();
		}
		else if (_currentConv.IsIndividual)
		{
			ChattingGroup_PC.RequestPartnerName(_currentConv, OnResponsePartnerInfo);
		}
		else
		{
			_nameLabel.text = ChattingGroup_PC.GetConversationName(_currentConv);
		}
	}

	public void MarkAsRead()
	{
		if (!(_notification == null))
		{
			HasNewChat = false;
			if (_currentConv != null)
			{
				_currentConv.MarkAsRead();
			}
			_notification.gameObject.SetActive(value: false);
		}
	}

	private void RefreshNotification()
	{
		if (!(_notification == null))
		{
			if (IsOpened)
			{
				MarkAsRead();
			}
			else
			{
				_notification.gameObject.SetActive(HasNewChat);
			}
		}
	}

	public void UpdateNotification(ChatStruct chat, bool isCurrentlyAllChatChannel)
	{
		if (IsMainChannel && !chat.IsVolatile && !IsOpened && FilterType != 0 && FilterType != ChatFilterType.System && SocialSystem.IsVisibleChat(chat, FilterType))
		{
			if (isCurrentlyAllChatChannel && !GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(FilterType))
			{
				MarkAsRead();
				return;
			}
			HasNewChat = true;
			RefreshNotification();
		}
	}

	public void UpdateNotification(Conversation conv, bool isCurrentlyAllChatChannel)
	{
		if (!IsMainChannel && conv != null && !(conv.Id != Id))
		{
			if (isCurrentlyAllChatChannel && !GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(CurrentConv))
			{
				MarkAsRead();
				return;
			}
			HasNewChat = _currentConv.Notification.Count > 0;
			RefreshNotification();
		}
	}

	private void OnResponsePartnerInfo(PlayerInfo info)
	{
		if (CurrentConv != null && CurrentConv.IsIndividual)
		{
			_nameLabel.text = ((!info.Valid) ? T._("알수없음") : info.Name);
			UpdateLayout();
			if (IndividualTabCreated != null)
			{
				IndividualTabCreated();
			}
		}
	}
}
