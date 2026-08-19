using System;
using System.Collections.Generic;
using ChatData;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

public class ChatLineList : MonoBehaviour
{
	private const int MaxChatLineObjectCount = 70;

	public Action<ChatStruct> SharedPointButtonClicked;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UIWidget _scrollViewContainer;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ChattingLineBase _chatLineBase;

	[SerializeField]
	private Color[] _chatLineColors;

	[SerializeField]
	private Color[] _chatTextColors;

	private int _chatLineColorIndex;

	private int _chatTextColorIndex;

	private readonly List<ChattingLineBase> _chattingLines = new List<ChattingLineBase>();

	private readonly Stack<ChattingLineBase> _chattingLinePool = new Stack<ChattingLineBase>();

	private UIWidget _invisibleWidget;

	private IList<ChatStruct> _chats;

	private ChatFilterType _filterType;

	private ulong _filterId;

	private bool _positionUpdated;

	private bool _chatListHided;

	private bool _playersInfoLoaded;

	private bool _isInit;

	public bool ChattingScrollLock { get; set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIScrollView scrollView = _scrollView;
			scrollView.onDragStarted = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView.onDragStarted, new UIScrollView.OnDragNotification(OnFullChatDragStarted));
			((Component)_scrollView).GetComponent<UIPanel>().alpha = 0f;
			_chatLineBase.SetActive(active: false);
		}
	}

	private void OnEnable()
	{
		_invisibleWidget = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleWidget);
		ChattingScrollLock = true;
		UIManager.KeyboardHeightUpdated += OnVisibleKeyboard;
	}

	private void OnDisable()
	{
		UIManager.KeyboardHeightUpdated -= OnVisibleKeyboard;
		((Component)_scrollView).GetComponent<UIPanel>().alpha = 0f;
	}

	private void LateUpdate()
	{
		if (_positionUpdated)
		{
			LateUpdatePosition();
		}
	}

	private void OnVisibleKeyboard(int height)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (height > 0)
		{
			int screenHeight = UIManager.ScreenHeight;
			Transform transform = ((Component)_scrollViewContainer).transform;
			int num = (int)MainCamera.NGUILocalPositionToNGUIPosition(transform.localPosition, transform.parent).y + screenHeight / 2;
			_scrollViewContainer.bottomAnchor.absolute = Mathf.Max(0, height - num);
		}
		else
		{
			_scrollViewContainer.bottomAnchor.absolute = 0;
		}
		_scrollViewContainer.UpdateAnchors();
		_scrollView.panel.UpdateAnchors();
		ChatScrollViewReset();
	}

	private void OnFullChatDragStarted()
	{
		ChattingScrollLock = false;
	}

	private void ChatScrollViewReset()
	{
		ChattingScrollLock = true;
		_scrollView.ResetPosition();
	}

	private ChattingLineBase ChattingLine_Pop()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		ChattingLineBase chattingLineBase;
		if (_chattingLinePool.Count > 0)
		{
			chattingLineBase = _chattingLinePool.Pop();
		}
		else
		{
			chattingLineBase = ((Component)((Component)_chatLineBase).transform.parent).gameObject.AddChild(((Component)_chatLineBase).gameObject).GetComponent<ChattingLineBase>();
			ChattingLineBase chattingLineBase2 = chattingLineBase;
			chattingLineBase2.NameLabelClicked = (Action<ulong>)Delegate.Combine(chattingLineBase2.NameLabelClicked, new Action<ulong>(ChatLineNameLabelClicked));
			ChattingLineBase chattingLineBase3 = chattingLineBase;
			chattingLineBase3.SharedPointButtonClicked = (Action<ChatStruct>)Delegate.Combine(chattingLineBase3.SharedPointButtonClicked, new Action<ChatStruct>(ChatLineSharedPointButtonClicked));
		}
		_chattingLines.Add(chattingLineBase);
		chattingLineBase.Widget.width = (int)((Component)_scrollView).GetComponent<UIPanel>().width;
		ChattingLineBase chattingLineBase4 = chattingLineBase;
		chattingLineBase4.HeightChanged = (Action)Delegate.Combine(chattingLineBase4.HeightChanged, new Action(UpdatePosition));
		chattingLineBase.SetBgColor(_chatLineColors[_chatLineColorIndex]);
		chattingLineBase.SetTextColor(_chatTextColors[_chatTextColorIndex]);
		_chatLineColorIndex = (_chatLineColorIndex + 1) % _chatLineColors.Length;
		_chatTextColorIndex = (_chatTextColorIndex + 1) % _chatTextColors.Length;
		chattingLineBase.SetActive(active: true);
		return chattingLineBase;
	}

	private void ChattingLine_Push(int index)
	{
		ChattingLineBase chattingLineBase = _chattingLines[index];
		_chattingLines.RemoveAt(index);
		_chattingLinePool.Push(chattingLineBase);
		chattingLineBase.SetActive(active: false);
	}

	public void SetTitle(string title)
	{
		_titleLabel.text = title;
	}

	public void Set(IList<ChatStruct> chats, ChatFilterType type, ulong filterId)
	{
		Init();
		_chats = chats;
		_filterType = type;
		_filterId = filterId;
		_chatLineColorIndex = 0;
		_chatTextColorIndex = 0;
		_chatListHided = false;
		PlayersInfoLoad();
		if (_playersInfoLoaded)
		{
			ChatListHideFinished();
			return;
		}
		TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)_scrollView).gameObject, 0.1f, 0f);
		EventDelegate.Add(tweenAlpha.onFinished, ChatListHideFinished, oneShot: true);
	}

	private void PlayersInfoLoad()
	{
		_playersInfoLoaded = false;
		List<ulong> list = new List<ulong>();
		for (int i = 0; i < _chats.Count; i++)
		{
			ChatStruct chatStruct = _chats[i];
			ulong[] array = null;
			if (chatStruct.Body is RadioEntered)
			{
				array = ((RadioEntered)chatStruct.Body).EntityIds;
			}
			else if (chatStruct.Body is RadioLeft)
			{
				array = ((RadioLeft)chatStruct.Body).EntityIds;
			}
			int j = 0;
			for (int num = KUtility.GetSize(array) + 1; j < num; j++)
			{
				ulong num2 = ((j != 0) ? array[j - 1] : chatStruct.EntityId);
				if (num2 != 0L && !list.Contains(num2))
				{
					list.Add(num2);
				}
			}
		}
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(list, PlayersInfoLoaded, useOldCache: true);
	}

	private void PlayersInfoLoaded(Player.PlayerInfo[] playerInfos)
	{
		_playersInfoLoaded = true;
		if (_playersInfoLoaded && _chatListHided)
		{
			DelayedSet();
		}
	}

	private void ChatListHideFinished()
	{
		_chatListHided = true;
		if (_playersInfoLoaded && _chatListHided)
		{
			DelayedSet();
		}
	}

	private void DelayedSet()
	{
		ChatScrollViewReset();
		ChattingLine_Clear();
		int i = 0;
		for (int num = ((_chats != null) ? _chats.Count : 0); i < num; i++)
		{
			if (SocialSystem.IsVisibleFilter(_chats[i], _filterType, _filterId))
			{
				AppendLine(_chats[i]);
			}
		}
		TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)_scrollView).gameObject, 0.1f, 1f);
		tweenAlpha.onFinished.Clear();
	}

	public void Append(ChatStruct chat)
	{
		Init();
		if (ChattingScrollLock)
		{
			ChatScrollViewReset();
		}
		AppendLine(chat);
	}

	public void AppendCurrentMemberLine(ulong[] entityIds)
	{
		ChatStruct chatStruct = default(ChatStruct);
		chatStruct.EntityId = 0uL;
		chatStruct.Name = "#chatting_list_members";
		chatStruct.Time = KUtility.GetTimestamp();
		chatStruct.Type = ChannelType.Conversation;
		chatStruct.Body = new RadioEntered
		{
			EntityIds = entityIds
		};
		ChatStruct chat = chatStruct;
		AppendLine(chat);
	}

	private void AppendLine(ChatStruct chat)
	{
		if (string.IsNullOrEmpty(chat.FindText()))
		{
			return;
		}
		ChattingLineBase chattingLineBase = null;
		if (_chattingLines.Count > 0)
		{
			chattingLineBase = _chattingLines[_chattingLines.Count - 1];
		}
		bool flag = true;
		if ((Object)(object)chattingLineBase != (Object)null)
		{
			if (chat.Type == ChannelType.System || chat.EntityId != chattingLineBase.EntityId || chat.Type != chattingLineBase.Type || chat.MsgType != chattingLineBase.MsgType || chattingLineBase.MsgType == ChatStruct.ChatMsgType.ChannelUpdated || chattingLineBase.MsgType == ChatStruct.ChatMsgType.Ping)
			{
				chattingLineBase = ChattingLine_Pop();
				flag = false;
			}
		}
		else
		{
			chattingLineBase = ChattingLine_Pop();
			flag = false;
		}
		if (flag)
		{
			chattingLineBase.AppendText(chat);
		}
		else
		{
			chattingLineBase.SetText(chat);
		}
		if (_chattingLines.Count > 70)
		{
			ChattingLine_Push(0);
		}
	}

	private void UpdatePosition()
	{
		_positionUpdated = true;
	}

	private void LateUpdatePosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_chattingLines != null)
		{
			Vector3 val = Vector3.zero;
			for (int num = _chattingLines.Count - 1; num >= 0; num--)
			{
				ChattingLineBase chattingLineBase = _chattingLines[num];
				val = (chattingLineBase.Position = val + Vector3.up * (float)chattingLineBase.GetHeight());
			}
		}
	}

	private void ChattingLine_Clear()
	{
		for (int num = _chattingLines.Count - 1; num >= 0; num--)
		{
			ChattingLine_Push(num);
		}
	}

	private void ChatLineNameLabelClicked(ulong entityId)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, ShowChatPlayerProfile);
	}

	private void ChatLineSharedPointButtonClicked(ChatStruct chatStruct)
	{
		if (SharedPointButtonClicked != null)
		{
			SharedPointButtonClicked(chatStruct);
		}
	}

	private void ShowChatPlayerProfile(Player.PlayerInfo playerInfo)
	{
		if (playerInfo.Valid)
		{
			ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
			profileTooltip.Set(playerInfo);
			profileTooltip.Show(3600f);
		}
	}
}
