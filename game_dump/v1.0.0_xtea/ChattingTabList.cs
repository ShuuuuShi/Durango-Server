using System;
using System.Collections.Generic;
using ChatData;
using UnityEngine;

public class ChattingTabList : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _tabs;

	[SerializeField]
	private ListObjectPool _rooms;

	[SerializeField]
	private GameObject _makeRoomTab;

	private IList<KeyValuePair<ChatFilterType, uint>> _tabInfos;

	private List<Conversation> _roomInfos;

	private UIWidget _invisibleBox;

	private bool _needConversationsUpdate;

	private ChatFilterType _selectedType;

	private ulong _selectedId;

	private bool _isInit;

	public event Action<ChatFilterType> FilterTabClicked;

	public event Action<Conversation> ChatRoomClicked;

	public event Action MakeRoomTabClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_tabs.Init(OnInitTab);
		_rooms.Init(OnInitRoomTab);
		UIEventListener.Get(_makeRoomTab).onClick = delegate
		{
			if (this.MakeRoomTabClicked != null)
			{
				this.MakeRoomTabClicked();
			}
		};
	}

	private void OnEnable()
	{
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleBox);
		Conversation.MessagesUpdated += OnUpdateConversationMessage;
	}

	private void OnDisable()
	{
		Conversation.MessagesUpdated -= OnUpdateConversationMessage;
	}

	private void LateUpdate()
	{
		if (_needConversationsUpdate)
		{
			LateUpdateConversations();
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
			if ((Object)(object)go == (Object)(object)_tabs[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1 && this.FilterTabClicked != null)
		{
			this.FilterTabClicked(_tabInfos[num].Key);
		}
	}

	private void OnClickRoomTab(GameObject go)
	{
		int num = -1;
		int i = 0;
		for (int count = _rooms.Count; i < count; i++)
		{
			if ((Object)(object)go == (Object)(object)_rooms[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1 && this.ChatRoomClicked != null)
		{
			this.ChatRoomClicked(_roomInfos[num]);
		}
	}

	public void Set(IList<KeyValuePair<ChatFilterType, uint>> tabs, IList<Conversation> conversations)
	{
		Init();
		_tabInfos = tabs;
		_roomInfos = new List<Conversation>();
		int i = 0;
		for (int count = conversations.Count; i < count; i++)
		{
			_roomInfos.Add(conversations[i]);
		}
		_tabs.Set(tabs?.Count ?? 0);
		_rooms.Clear();
		int j = 0;
		for (int count2 = _tabs.Count; j < count2; j++)
		{
			string tabName = LocalizeUtil.Get(tabs[j].Key);
			uint value = tabs[j].Value;
			string subText = ((value != 0) ? LocalizeSystem.Format("#chatting_tab_member_count", value.ToString()) : string.Empty);
			ChattingTabWidget component = _tabs[j].GetComponent<ChattingTabWidget>();
			component.Set(tabName, subText);
		}
		UpdateConversations();
	}

	private int ConversationComparison(Conversation c1, Conversation c2)
	{
		double lastestUpdateTime = c1.GetLastestUpdateTime();
		double lastestUpdateTime2 = c2.GetLastestUpdateTime();
		if (lastestUpdateTime > lastestUpdateTime2)
		{
			return -1;
		}
		if (lastestUpdateTime < lastestUpdateTime2)
		{
			return 1;
		}
		return 0;
	}

	private void OnUpdateConversationMessage(Conversation conv)
	{
		UpdateConversations();
	}

	private void UpdateConversations()
	{
		_needConversationsUpdate = true;
	}

	private void LateUpdateConversations()
	{
		_needConversationsUpdate = false;
		_roomInfos.Sort(ConversationComparison);
		int num = 0;
		int i = 0;
		for (int count = _roomInfos.Count; i < count; i++)
		{
			ChattingRoomTabWidget chattingRoomTabWidget = ((num >= _rooms.Count) ? ((ListObjectPoolBase<GameObject>)_rooms).Add<ChattingRoomTabWidget>() : _rooms[num].GetComponent<ChattingRoomTabWidget>());
			chattingRoomTabWidget.Set(_roomInfos[i]);
			num++;
		}
		_rooms.Set(num);
		UpdateLayout();
		if (_selectedId == 0L)
		{
			Select(_selectedType);
		}
		else
		{
			Select(_selectedId);
		}
	}

	public void UpdateLayout()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _tabs.BaseObject.transform.localPosition;
		int num = 0;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			UIWidget component = _tabs[i].GetComponent<UIWidget>();
			((Component)component).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += component.height;
		}
		int j = 0;
		for (int count2 = _rooms.Count; j < count2; j++)
		{
			UIWidget component2 = _rooms[j].GetComponent<UIWidget>();
			((Component)component2).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += component2.height;
		}
		_makeRoomTab.transform.localPosition = localPosition + Vector3.down * (float)num;
	}

	public void Select(ChatFilterType type)
	{
		_selectedId = 0uL;
		_selectedType = type;
		int num = -1;
		int i = 0;
		for (int num2 = ((_tabInfos != null) ? _tabInfos.Count : 0); i < num2; i++)
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
			component.Select(j == num);
		}
		int k = 0;
		for (int count2 = _rooms.Count; k < count2; k++)
		{
			ChattingRoomTabWidget component2 = _rooms[k].GetComponent<ChattingRoomTabWidget>();
			component2.Select(select: false);
		}
	}

	public void Select(ulong id)
	{
		_selectedId = id;
		int num = 0;
		int i = 0;
		for (int num2 = ((_roomInfos != null) ? _roomInfos.Count : 0); i < num2 && _roomInfos[i].Id != id; i++)
		{
			num++;
		}
		int j = 0;
		for (int count = _tabs.Count; j < count; j++)
		{
			ChattingTabWidget component = _tabs[j].GetComponent<ChattingTabWidget>();
			component.Select(select: false);
		}
		int k = 0;
		for (int count2 = _rooms.Count; k < count2; k++)
		{
			ChattingRoomTabWidget component2 = _rooms[k].GetComponent<ChattingRoomTabWidget>();
			component2.Select(k == num);
		}
	}
}
