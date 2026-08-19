using System.Collections.Generic;
using Durango.Logic.Social;
using UnityEngine;

namespace Durango.UI;

public class ChatLineList_PC : ChatLineList
{
	private bool _isAllChat;

	public bool IsEmpty
	{
		get
		{
			if (ChattingLines != null)
			{
				return ChattingLines.Count == 0;
			}
			return true;
		}
	}

	protected override void OnEnable()
	{
		base.ChattingScrollLock = true;
	}

	protected override void OnDisable()
	{
	}

	public virtual void Set(IList<ChatStruct> chats, ChatFilterType type, string filterId, bool isAllChat = false)
	{
		_isAllChat = isAllChat;
		base.Set(chats, type, filterId);
	}

	public void EnableChatlineColliders(bool isEnable)
	{
		for (int i = 0; i < ChattingLines.Count; i++)
		{
			ChattingLine_PC chattingLine_PC = (ChattingLine_PC)ChattingLines[i];
			if (chattingLine_PC != null)
			{
				chattingLine_PC.EnableColliders(isEnable);
			}
		}
	}

	public int GetHeight()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return (int)ChattingLines[0].Position.y;
	}

	public int GetHeightOnShrink(int maxLineCount)
	{
		if (IsEmpty)
		{
			return 0;
		}
		float num = 0f;
		int num2 = 0;
		for (int num3 = ChattingLines.Count - 1; num3 >= 0; num3--)
		{
			UILabel textLabel = ChattingLines[num3].GetTextLabel();
			float num4 = (float)textLabel.fontSize + textLabel.floatSpacingY;
			float num5 = textLabel.printedSize.y + textLabel.floatSpacingY;
			int num6 = Mathf.RoundToInt(num5 / num4);
			float num7 = 0f;
			while (num6 > 0 && num2 < maxLineCount)
			{
				num7 += num4;
				num2++;
				num6--;
			}
			num = ((!(num5 < num7)) ? (num + num7) : (num + num5));
			num += (float)ChattingLines[num3].VerticalPadding - textLabel.floatSpacingY;
			if (num2 >= maxLineCount)
			{
				break;
			}
		}
		return (int)num;
	}

	public void ResetScroll()
	{
		ScrollView.currentMomentum = Vector3.zero;
		ScrollView.Press(pressed: false);
		base.ChatScrollViewReset();
	}

	protected override void AppendLine(ChatStruct chat)
	{
		if (string.IsNullOrEmpty(chat.FindText()))
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
			if (chat.EntityId != chattingLineBase.EntityId || chat.Name != chattingLineBase.Name || chat.Type != chattingLineBase.Type || chat.MsgType != chattingLineBase.MsgType || chattingLineBase.MsgType == ChatStruct.ChatMsgType.ChannelUpdated || chattingLineBase.MsgType == ChatStruct.ChatMsgType.Link)
			{
				chattingLineBase = ChattingLine_Pop(initWidth: false);
				flag = false;
			}
		}
		else
		{
			chattingLineBase = ChattingLine_Pop(initWidth: false);
			flag = false;
		}
		if (flag)
		{
			chattingLineBase.AppendChat(chat);
		}
		else
		{
			chattingLineBase.SetChat(chat, _isAllChat);
			UpdatePosition();
		}
		if (ChattingLines.Count > MaxChatLineCount)
		{
			ChattingLine_Push(0);
		}
	}
}
