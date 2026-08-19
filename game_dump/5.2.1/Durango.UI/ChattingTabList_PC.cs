using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ChattingTabList_PC : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private SelectableWidget _prevButton;

	[SerializeField]
	private SelectableWidget _nextButton;

	[SerializeField]
	private ChattingTabWidget_PC _baseTab;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	private IList<KeyValuePair<ChatFilterType, uint>> _tabInfos;

	private readonly List<Conversation> _roomInfos = new List<Conversation>();

	private readonly ListObjectPool<ChattingTabWidget_PC> _tabs = new ListObjectPool<ChattingTabWidget_PC>();

	private ChatFilterType _selectedType;

	private string _selectedId;

	private bool _needReposition;

	private bool _needConversationsUpdate;

	private bool _needCheckNotifications;

	private bool _hasUnreadTab;

	public event Action<ChatFilterType> FilterTabClicked;

	public event Action<Conversation> ChatRoomClicked;

	public event Action<bool> NotificationStateChanged;

	void IUIInitializable.Init()
	{
		_tabs.BaseObject = _baseTab;
		_tabs.UseBase = false;
		_tabs.Init(delegate(ChattingTabWidget_PC tab)
		{
			tab.TabPressed = (Action<bool>)Delegate.Combine(tab.TabPressed, new Action<bool>(OnPressTab));
			tab.TabDragged = (Action<Vector2>)Delegate.Combine(tab.TabDragged, new Action<Vector2>(OnDragTab));
			tab.TabScrolled = (Action<float>)Delegate.Combine(tab.TabScrolled, new Action<float>(OnScrollTab));
			tab.TabClicked = (Action<ChattingTabWidget_PC>)Delegate.Combine(tab.TabClicked, new Action<ChattingTabWidget_PC>(OnClickTab));
			tab.IndividualTabCreated = (Action)Delegate.Combine(tab.IndividualTabCreated, new Action(NeedReposition));
		});
		SelectableWidget prevButton = _prevButton;
		prevButton.Clicked = (Action)Delegate.Combine(prevButton.Clicked, (Action)delegate
		{
			OnClickArrow(isNext: false);
		});
		SelectableWidget nextButton = _nextButton;
		nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, (Action)delegate
		{
			OnClickArrow(isNext: true);
		});
	}

	private void OnPressTab(bool isPressed)
	{
		_scrollView.ScrollView.Press(isPressed);
	}

	private void OnDragTab(Vector2 delta)
	{
		_scrollView.ScrollView.Drag();
	}

	private void OnScrollTab(float delta)
	{
		_scrollView.ScrollView.Scroll(delta);
	}

	private void OnClickTab(ChattingTabWidget_PC tabWidget)
	{
		if (tabWidget == null)
		{
			return;
		}
		if (tabWidget.IsMainChannel)
		{
			ClickTab(tabWidget.FilterType);
		}
		else
		{
			ClickRoomTab(tabWidget.CurrentConv);
		}
		for (int i = 0; i < _tabs.Count; i++)
		{
			ChattingTabWidget_PC chattingTabWidget_PC = _tabs[i];
			chattingTabWidget_PC.Selected = chattingTabWidget_PC == tabWidget;
			if (chattingTabWidget_PC.Selected)
			{
				_scrollView.MoveToVisibleArea(i, instant: false);
			}
		}
		NeedCheckNotifications();
	}

	public void OnClickArrow(bool isNext)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			if (_tabs[i].Selected)
			{
				int num = ((!isNext) ? (i - 1) : (i + 1));
				num = (int)Mathf.Repeat(num, _tabs.Count);
				OnClickTab(_tabs[num]);
				break;
			}
		}
	}

	private void ClickTab(ChatFilterType filter)
	{
		if (this.FilterTabClicked != null)
		{
			this.FilterTabClicked(filter);
		}
	}

	private void ClickRoomTab(Conversation conv)
	{
		if (this.ChatRoomClicked != null)
		{
			this.ChatRoomClicked(conv);
		}
	}

	public void Set(IList<KeyValuePair<ChatFilterType, uint>> tabs, IEnumerable<Conversation> rooms)
	{
		_tabInfos = tabs;
		_roomInfos.Clear();
		_roomInfos.AddRange(rooms);
		_tabs.Set(KUtility.GetSize(_tabInfos));
		for (int i = 0; i < _tabs.Count; i++)
		{
			_tabs[i].Set(_tabInfos[i].Key);
		}
		UpdateConversations();
	}

	private void UpdateLayout()
	{
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int i = 0; i < _tabs.Count; i++)
		{
			widgets.Add(_tabs[i].Widget);
		}
		NeedReposition();
		NeedCheckNotifications();
	}

	public void Select(ChatFilterType type)
	{
		_selectedId = string.Empty;
		_selectedType = type;
		foreach (ChattingTabWidget_PC tab in _tabs)
		{
			if (tab.IsMainChannel)
			{
				tab.Selected = tab.FilterType == type;
			}
			else
			{
				tab.Selected = false;
			}
		}
	}

	public void Select(string id)
	{
		_selectedId = id;
		foreach (ChattingTabWidget_PC tab in _tabs)
		{
			if (tab.IsMainChannel)
			{
				tab.Selected = false;
			}
			else
			{
				tab.Selected = tab.Id == id;
			}
		}
		foreach (Conversation roomInfo in _roomInfos)
		{
			if (roomInfo.Id == id)
			{
				roomInfo.MarkAsRead();
				break;
			}
		}
	}

	public void UpdateNotifications(ChatStruct chat, bool isAllChannel)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			_tabs[i].UpdateNotification(chat, isAllChannel);
		}
		NeedCheckNotifications();
	}

	public void UpdateNotifications(Conversation conv, bool isAllChannel)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			_tabs[i].UpdateNotification(conv, isAllChannel);
		}
		NeedCheckNotifications();
	}

	public void MarkUnHiddenChannelsAsRead()
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			ChattingTabWidget_PC chattingTabWidget_PC = _tabs[i];
			if (chattingTabWidget_PC.IsMainChannel)
			{
				if (!GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(chattingTabWidget_PC.FilterType))
				{
					chattingTabWidget_PC.MarkAsRead();
				}
			}
			else if (chattingTabWidget_PC.CurrentConv != null && !GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(chattingTabWidget_PC.CurrentConv))
			{
				chattingTabWidget_PC.MarkAsRead();
			}
		}
		NeedCheckNotifications();
	}

	private void NeedReposition()
	{
		_needReposition = true;
	}

	private void NeedCheckNotifications()
	{
		_needCheckNotifications = true;
	}

	public void UpdateConversations()
	{
		_needConversationsUpdate = true;
	}

	private void LateUpdateConversations()
	{
		_needConversationsUpdate = false;
		_roomInfos.Sort(SocialSystem.ConversationComparison);
		int size = KUtility.GetSize(_tabInfos);
		int size2 = KUtility.GetSize(_roomInfos);
		_tabs.Set(size + size2);
		for (int i = 0; i < size2; i++)
		{
			_tabs[i + size].Set(_roomInfos[i]);
		}
		UpdateLayout();
		if (string.IsNullOrEmpty(_selectedId))
		{
			Select(_selectedType);
		}
		else
		{
			Select(_selectedId);
		}
	}

	private void LateUpdate()
	{
		if (_needConversationsUpdate)
		{
			LateUpdateConversations();
		}
		if (_needReposition)
		{
			_needReposition = false;
			_scrollView.Reposition();
			for (int i = 0; i < _tabs.Count; i++)
			{
				if (_tabs[i].Selected)
				{
					_scrollView.MoveToVisibleArea(i, instant: false);
					break;
				}
			}
		}
		if (!_needCheckNotifications)
		{
			return;
		}
		_needCheckNotifications = false;
		bool flag = false;
		for (int j = 0; j < _tabs.Count; j++)
		{
			if (_tabs[j].HasNewChat)
			{
				flag = true;
				break;
			}
		}
		if (_hasUnreadTab != flag)
		{
			_hasUnreadTab = flag;
			if (this.NotificationStateChanged != null)
			{
				this.NotificationStateChanged(_hasUnreadTab);
			}
		}
	}
}
