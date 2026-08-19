using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI.Popup;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChatLineList : MonoBehaviour
{
	public Action<ChatStruct> ChatLinkClicked;

	public Action PositionUpdated;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _scrollViewContainer;

	[SerializeField]
	protected UIScrollView ScrollView;

	[SerializeField]
	protected ChattingLineBase _chatLineBase;

	[SerializeField]
	private Color[] _chatLineColors;

	[SerializeField]
	private Color[] _chatTextColors;

	[SerializeField]
	protected int MaxChatLineCount;

	private int _chatLineColorIndex;

	private int _chatTextColorIndex;

	protected readonly List<ChattingLineBase> ChattingLines = new List<ChattingLineBase>();

	private readonly Stack<ChattingLineBase> _chattingLinePool = new Stack<ChattingLineBase>();

	private UIWidget _invisibleWidget;

	private IList<ChatStruct> _chats;

	private ChatFilterType _filterType;

	private string _filterId;

	private bool _positionUpdated;

	private bool _keyboardHeightUpdated;

	private bool _isInit;

	public bool ChattingScrollLock { get; set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIScrollView scrollView = ScrollView;
			scrollView.onDragStarted = (UIScrollView.OnDragNotification)Delegate.Combine(scrollView.onDragStarted, new UIScrollView.OnDragNotification(OnFullChatDragStarted));
			_chatLineBase.SetActive(active: false);
		}
	}

	protected virtual void OnEnable()
	{
		_invisibleWidget = UIUtility.SetScrollViewInvisibleBox(ScrollView, _invisibleWidget);
		ChattingScrollLock = true;
		KeyboardHeightChecker.KeyboardHeightUpdated += OnVisibleKeyboard;
		OnVisibleKeyboard(KeyboardHeightChecker.Height);
	}

	protected virtual void OnDisable()
	{
		KeyboardHeightChecker.KeyboardHeightUpdated -= OnVisibleKeyboard;
	}

	private void LateUpdate()
	{
		if (_positionUpdated)
		{
			LateUpdatePosition();
		}
		if (_keyboardHeightUpdated)
		{
			ApplyKeyboardHeight();
		}
	}

	private void OnVisibleKeyboard(int height)
	{
		_keyboardHeightUpdated = true;
	}

	private void ApplyKeyboardHeight()
	{
		_keyboardHeightUpdated = false;
		int height = KeyboardHeightChecker.Height;
		int absolute = 0;
		UIRoot root = _scrollViewContainer.root;
		if (root != null)
		{
			Vector3 position = _scrollViewContainer.worldCorners[0];
			UIRect component = root.GetComponent<UIRect>();
			position = component.transform.InverseTransformPoint(position);
			Vector3 vector = component.localCorners[0];
			if (position.y - vector.y < (float)height)
			{
				absolute = height - (int)(position.y - vector.y);
			}
		}
		UIPanel uIPanel = ScrollView.panel;
		if (uIPanel == null)
		{
			uIPanel = ScrollView.GetComponent<UIPanel>();
		}
		uIPanel.bottomAnchor.absolute = absolute;
		uIPanel.UpdateAnchors();
		ChatScrollViewReset();
	}

	private void OnFullChatDragStarted()
	{
		ChattingScrollLock = false;
	}

	public virtual void ChatScrollViewReset()
	{
		ChattingScrollLock = true;
		ScrollView.ResetPosition();
	}

	protected ChattingLineBase ChattingLine_Pop(bool initWidth = true)
	{
		ChattingLineBase chattingLineBase;
		if (_chattingLinePool.Count > 0)
		{
			chattingLineBase = _chattingLinePool.Pop();
		}
		else
		{
			chattingLineBase = _chatLineBase.transform.parent.gameObject.AddChild(_chatLineBase.gameObject).GetComponent<ChattingLineBase>();
			chattingLineBase.NameLabelClicked = delegate(string entityId)
			{
				PlayerInfoPopup.RequestShow(entityId);
			};
			chattingLineBase.LinkClicked = OnChatLinkClick;
		}
		ChattingLines.Add(chattingLineBase);
		if (initWidth)
		{
			chattingLineBase.Widget.width = (int)ScrollView.GetComponent<UIPanel>().width;
		}
		chattingLineBase.HeightChanged = UpdatePosition;
		chattingLineBase.SetBgColor(_chatLineColors[_chatLineColorIndex]);
		chattingLineBase.SetTextColor(_chatTextColors[_chatTextColorIndex]);
		_chatLineColorIndex = (_chatLineColorIndex + 1) % _chatLineColors.Length;
		_chatTextColorIndex = (_chatTextColorIndex + 1) % _chatTextColors.Length;
		chattingLineBase.SetActive(active: true);
		return chattingLineBase;
	}

	protected void ChattingLine_Push(int index)
	{
		ChattingLineBase chattingLineBase = ChattingLines[index];
		ChattingLines.RemoveAt(index);
		_chattingLinePool.Push(chattingLineBase);
		chattingLineBase.SetActive(active: false);
	}

	public void SetTitle(string title)
	{
		if (_titleLabel != null)
		{
			_titleLabel.text = title;
		}
	}

	public virtual void Set(IList<ChatStruct> chats, ChatFilterType type, string filterId)
	{
		Init();
		_chats = chats;
		_filterType = type;
		_filterId = filterId;
		_chatLineColorIndex = 0;
		_chatTextColorIndex = 0;
		ChatScrollViewReset();
		ChattingLine_Clear();
		int i = 0;
		for (int size = KUtility.GetSize(_chats); i < size; i++)
		{
			if (SocialSystem.IsVisibleChat(_chats[i], _filterType, _filterId))
			{
				AppendLine(_chats[i]);
			}
		}
		_keyboardHeightUpdated = true;
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

	protected virtual void AppendLine(ChatStruct chat)
	{
		if (string.IsNullOrEmpty(chat.FindText()) && !chat.IsEventMessage())
		{
			return;
		}
		ChattingLineBase chattingLineBase = null;
		if (ChattingLines.Count > 0)
		{
			chattingLineBase = ChattingLines[ChattingLines.Count - 1];
		}
		bool flag = true;
		if (chattingLineBase != null)
		{
			if (chat.Type == ChannelType.System || chat.EntityId != chattingLineBase.EntityId || chat.Name != chattingLineBase.Name || chat.Type != chattingLineBase.Type || chat.MsgType != chattingLineBase.MsgType || chattingLineBase.MsgType == ChatStruct.ChatMsgType.ChannelUpdated || chattingLineBase.MsgType == ChatStruct.ChatMsgType.Link)
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
			chattingLineBase.AppendChat(chat);
		}
		else
		{
			chattingLineBase.SetChat(chat);
			UpdatePosition();
		}
		if (ChattingLines.Count > MaxChatLineCount)
		{
			ChattingLine_Push(0);
		}
	}

	protected void UpdatePosition()
	{
		_positionUpdated = true;
	}

	private void LateUpdatePosition()
	{
		if (ChattingLines != null)
		{
			Vector3 zero = Vector3.zero;
			for (int num = ChattingLines.Count - 1; num >= 0; num--)
			{
				ChattingLineBase chattingLineBase = ChattingLines[num];
				zero += Vector3.up * chattingLineBase.GetHeight();
				chattingLineBase.Position = zero;
			}
			if (PositionUpdated != null)
			{
				PositionUpdated();
			}
			_positionUpdated = false;
		}
	}

	private void ChattingLine_Clear()
	{
		for (int num = ChattingLines.Count - 1; num >= 0; num--)
		{
			ChattingLine_Push(num);
		}
	}

	private void OnChatLinkClick(ChatStruct chatStruct)
	{
		if (ChatLinkClicked != null)
		{
			ChatLinkClicked(chatStruct);
		}
	}
}
