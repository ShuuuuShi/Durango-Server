using System;
using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.Logic.Social;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ChattingTabList : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private ListObjectPool _tabs;

	[SerializeField]
	private ListObjectPool _rooms;

	[SerializeField]
	private GameObject _makeRoomTab;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	private IList<KeyValuePair<ChatFilterType, uint>> _tabInfos;

	private readonly List<Conversation> _roomInfos = new List<Conversation>();

	private ChatFilterType _selectedType;

	private string _selectedId;

	private bool _needConversationsUpdate;

	public event Action<ChatFilterType> FilterTabClicked;

	public event Action<Conversation> ChatRoomClicked;

	public event Action MakeRoomTabClicked;

	void IUIInitializable.Init()
	{
		_tabs.Init(OnInitTab);
		_rooms.Init(OnInitRoomTab);
		if (GameManager.ClusterMode == Mode.Online)
		{
			SelectableWidget component = _makeRoomTab.GetComponent<SelectableWidget>();
			if (!(component != null))
			{
				return;
			}
			component.SetClickSound(UISound.ClickType.ButtonMedium);
			component.Clicked = (Action)Delegate.Combine(component.Clicked, (Action)delegate
			{
				if (this.MakeRoomTabClicked != null)
				{
					this.MakeRoomTabClicked();
				}
			});
		}
		else
		{
			_makeRoomTab.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		Conversation.MessagesUpdated += OnUpdateConversationMessage;
		_scrollView.ScrollView.movement = ((!UIManager.IsPortraitScreen) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
	}

	private void OnDisable()
	{
		Conversation.MessagesUpdated -= OnUpdateConversationMessage;
	}

	private void OnUpdateConversationMessage(Conversation conv)
	{
		UpdateConversations();
	}

	public void Set(IList<KeyValuePair<ChatFilterType, uint>> tabs, IEnumerable<Conversation> rooms)
	{
		_tabInfos = tabs;
		_roomInfos.Clear();
		_roomInfos.AddRange(rooms);
		_tabs.Set(KUtility.GetSize(_tabInfos));
		_rooms.Clear();
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			ChatFilterType key = _tabInfos[i].Key;
			string tabName = key.GetName();
			uint value = _tabInfos[i].Value;
			string subText = ((value != 0) ? $"[icon=icon_person:0.8] {value}" : string.Empty);
			bool pushOff = SocialSystem.IsKindOfClanChannelFilter(key) && !GameSystem<SocialSystem>.Instance().IsClanPushEnabled(key);
			bool hided = GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(key);
			ChattingTabWidget component = _tabs[i].GetComponent<ChattingTabWidget>();
			component.Set(tabName, subText, pushOff, hided);
		}
		UpdateConversations();
	}

	private void UpdateLayout()
	{
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		Vector3 localPosition = _tabs.BaseObject.transform.localPosition;
		int num = 0;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			UIWidget component = _tabs[i].GetComponent<UIWidget>();
			component.transform.localPosition = localPosition + Vector3.down * num;
			num += component.height;
			widgets.Add(component);
		}
		int j = 0;
		for (int count2 = _rooms.Count; j < count2; j++)
		{
			UIWidget component2 = _rooms[j].GetComponent<UIWidget>();
			component2.transform.localPosition = localPosition + Vector3.down * num;
			num += component2.height;
			widgets.Add(component2);
		}
		_scrollView.Reposition();
	}

	public void Select(ChatFilterType type)
	{
		_selectedId = string.Empty;
		_selectedType = type;
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_tabInfos); i < size; i++)
		{
			if (_tabInfos[i].Key == type)
			{
				num = i;
				break;
			}
		}
		int j = 0;
		for (int count = _tabs.Count; j < count; j++)
		{
			ChattingTabWidget component = _tabs[j].GetComponent<ChattingTabWidget>();
			component.Selected = j == num;
		}
		int k = 0;
		for (int count2 = _rooms.Count; k < count2; k++)
		{
			ChattingRoomTabWidget component2 = _rooms[k].GetComponent<ChattingRoomTabWidget>();
			component2.Selected = false;
		}
	}

	public void Select(string id)
	{
		_selectedId = id;
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_roomInfos); i < size && !(_roomInfos[i].Id == id); i++)
		{
			num++;
		}
		int j = 0;
		for (int count = _tabs.Count; j < count; j++)
		{
			ChattingTabWidget component = _tabs[j].GetComponent<ChattingTabWidget>();
			component.Selected = false;
		}
		int k = 0;
		for (int count2 = _rooms.Count; k < count2; k++)
		{
			ChattingRoomTabWidget component2 = _rooms[k].GetComponent<ChattingRoomTabWidget>();
			component2.Selected = k == num;
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

	private void OnInitTab(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTab));
	}

	private void OnInitRoomTab(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickRoomTab));
	}

	private void OnClickTab(GameObject go)
	{
		int num = -1;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			if (go == _tabs[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			ClickTab(_tabInfos[num].Key);
		}
	}

	private void OnClickRoomTab(GameObject go)
	{
		int num = -1;
		int i = 0;
		for (int count = _rooms.Count; i < count; i++)
		{
			if (go == _rooms[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			ClickRoomTab(_roomInfos[num]);
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

	public void UpdateConversations()
	{
		_needConversationsUpdate = true;
	}

	private void LateUpdateConversations()
	{
		_needConversationsUpdate = false;
		_roomInfos.Sort(SocialSystem.ConversationComparison);
		_rooms.BeginLoad();
		foreach (Conversation roomInfo in _roomInfos)
		{
			ChattingRoomTabWidget component = _rooms.GetNext().GetComponent<ChattingRoomTabWidget>();
			component.Selected = _selectedId == roomInfo.Id;
			component.Set(roomInfo);
		}
		_rooms.EndLoad();
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
	}
}
