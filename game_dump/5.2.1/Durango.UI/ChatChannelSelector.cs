using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.UI.Control;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChatChannelSelector : SelectableWidget
{
	public Action<string> ChannelSelected;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UIWidget _scrollBg;

	[SerializeField]
	private ChattingTabWidget_PC _baseTab;

	private ChattingTabWidget_PC _currentTab;

	private IList<ChatFilterType> _mainChannels;

	private readonly List<Conversation> _conversations = new List<Conversation>();

	private readonly ListObjectPool<ChattingTabWidget_PC> _tabs = new ListObjectPool<ChattingTabWidget_PC>();

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
		base.OnInit();
		Clicked = (Action)Delegate.Combine(Clicked, new Action(OnClickSelector));
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, delegate
		{
			Open(isOpen: false);
		});
		_tabs.BaseObject = _baseTab;
		_tabs.UseBase = false;
		_tabs.Init(delegate(ChattingTabWidget_PC tab)
		{
			tab.TabPressed = (Action<bool>)Delegate.Combine(tab.TabPressed, new Action<bool>(OnPressTab));
			tab.TabDragged = (Action<Vector2>)Delegate.Combine(tab.TabDragged, new Action<Vector2>(OnDragTab));
			tab.TabScrolled = (Action<float>)Delegate.Combine(tab.TabScrolled, new Action<float>(OnScrollTab));
			tab.TabClicked = (Action<ChattingTabWidget_PC>)Delegate.Combine(tab.TabClicked, new Action<ChattingTabWidget_PC>(OnClickTab));
		});
		UIEventListener uIEventListener = UIEventListener.Get(_scrollBg.gameObject);
		uIEventListener.onPress = delegate(GameObject go, bool isPressed)
		{
			OnPressTab(isPressed);
		};
		uIEventListener.onDrag = delegate(GameObject go, Vector2 delta)
		{
			OnDragTab(delta);
		};
		uIEventListener.onScroll = delegate(GameObject go, float delta)
		{
			OnScrollTab(delta);
		};
		_scrollView.gameObject.SetActive(value: false);
	}

	private void OnPressMouse(GameObject go, bool isPressed)
	{
		if (isPressed && IsOpened && !(go == base.gameObject) && !(go == _scrollBg.gameObject) && !_tabs.Any((ChattingTabWidget_PC x) => x.gameObject == go))
		{
			Open(isOpen: false);
		}
	}

	private void OnClickSelector()
	{
		Open(!IsOpened);
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
		if (_currentTab == tabWidget)
		{
			return;
		}
		_currentTab = tabWidget;
		for (int i = 0; i < _tabs.Count; i++)
		{
			ChattingTabWidget_PC chattingTabWidget_PC = _tabs[i];
			chattingTabWidget_PC.Selected = chattingTabWidget_PC == tabWidget;
			if (chattingTabWidget_PC == tabWidget)
			{
				_scrollView.MoveToVisibleArea(i, instant: false);
			}
		}
		if (ChannelSelected != null)
		{
			string tabName = tabWidget.TabName;
			ChannelSelected(tabName);
		}
		Open(isOpen: false);
	}

	public void Open(bool isOpen)
	{
		if (IsOpened != isOpen)
		{
			_scrollView.gameObject.SetActive(isOpen);
			base.Selected = isOpen;
		}
	}

	public string GetSelectedChannelName()
	{
		if (_currentTab == null)
		{
			return string.Empty;
		}
		return _currentTab.TabName;
	}

	public string GetSelectedConversationId()
	{
		if (_currentTab == null)
		{
			return string.Empty;
		}
		return _currentTab.Id;
	}

	public ChannelType GetSelectedChannelType()
	{
		if (_currentTab == null)
		{
			return ChannelType.Invalid;
		}
		if (_currentTab.IsMainChannel)
		{
			return SocialSystem.ConvertToChannelType(_currentTab.FilterType);
		}
		return ChannelType.Conversation;
	}

	public void SelectChannel(ChatFilterType filterType)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			ChattingTabWidget_PC chattingTabWidget_PC = _tabs[i];
			if (chattingTabWidget_PC.FilterType == filterType)
			{
				OnClickTab(chattingTabWidget_PC);
				break;
			}
		}
	}

	public void SelectChannel(string id)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			ChattingTabWidget_PC chattingTabWidget_PC = _tabs[i];
			if (chattingTabWidget_PC.Id == id)
			{
				OnClickTab(chattingTabWidget_PC);
				break;
			}
		}
	}

	public void SetChannelList(IList<ChatFilterType> mainChannels, IEnumerable<Conversation> conversations)
	{
		_mainChannels = mainChannels;
		_conversations.Clear();
		_conversations.AddRange(conversations);
		_conversations.Sort(SocialSystem.ConversationComparison);
		int size = KUtility.GetSize(_mainChannels);
		int size2 = KUtility.GetSize(_conversations);
		_tabs.Set(size + size2);
		for (int i = 0; i < size; i++)
		{
			_tabs[i].Set(_mainChannels[i]);
		}
		for (int j = 0; j < size2; j++)
		{
			_tabs[j + size].Set(_conversations[j]);
		}
		RefreshCurrentTab();
		UpdateLayout();
	}

	private void RefreshCurrentTab()
	{
		if (_currentTab == null)
		{
			SelectChannel(ChatFilterType.Region);
		}
		else if (_currentTab.IsMainChannel)
		{
			SelectChannel(_currentTab.FilterType);
		}
		else
		{
			SelectChannel(_currentTab.Id);
		}
	}

	private void UpdateLayout()
	{
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int i = 0; i < _tabs.Count; i++)
		{
			UIWidget widget = _tabs[i].Widget;
			widgets.Add(widget);
		}
		_scrollView.Reposition();
		_scrollView.UpdateLayout();
		_scrollBg.topAnchor.absolute = _scrollBg.bottomAnchor.absolute + (int)_scrollView.ContentsLength + _scrollView.EndPadding;
		_scrollBg.UpdateAnchors();
	}
}
