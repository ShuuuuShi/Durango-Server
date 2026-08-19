using L10N;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

namespace ChatData;

public struct ChatStruct
{
	public enum ChatMsgType
	{
		Talk,
		Dictation,
		ChannelUpdated,
		Ping
	}

	public ulong EntityId;

	public ChatableBase Chatter;

	public double Time;

	public float Duration;

	public ChannelType Type;

	public bool IsVolatile;

	public string Name;

	public PortraitEmotion Emotion;

	public object Body;

	private string _cachedText;

	public ChatMsgType MsgType
	{
		get
		{
			if (Body is RadioDictation)
			{
				return ChatMsgType.Dictation;
			}
			if (Body is RadioEntered || Body is RadioLeft)
			{
				return ChatMsgType.ChannelUpdated;
			}
			if (Body is RadioPin)
			{
				return ChatMsgType.Ping;
			}
			return ChatMsgType.Talk;
		}
	}

	public string FindText()
	{
		if (_cachedText != null)
		{
			return _cachedText;
		}
		_cachedText = FindTextInternal();
		return _cachedText;
	}

	private string FindTextInternal()
	{
		if (Body is RadioTalk)
		{
			return ((RadioTalk)Body).Text;
		}
		if (Body is RadioDictation)
		{
			return ((RadioDictation)Body).Text;
		}
		if (Body is RadioText)
		{
			return ((RadioText)Body).Text;
		}
		if (Body is RadioNotice)
		{
			return ((RadioNotice)Body).Text;
		}
		if (Body is RadioPin)
		{
			RadioPin radioPin = (RadioPin)Body;
			return T._("{0} 섬의 위치를 공유합니다.", radioPin.RegionName);
		}
		return string.Empty;
	}

	public Color GetMsgColor(Color defaultColor)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (Body is RadioDictation)
		{
			return Color32.op_Implicit(new Color32((byte)204, (byte)195, (byte)168, byte.MaxValue));
		}
		return (Color)(Type switch
		{
			ChannelType.Clan => Color32.op_Implicit(new Color32((byte)102, (byte)232, (byte)56, byte.MaxValue)), 
			ChannelType.System => Color32.op_Implicit(new Color32((byte)119, byte.MaxValue, (byte)85, byte.MaxValue)), 
			ChannelType.Conversation => Color32.op_Implicit(new Color32(byte.MaxValue, (byte)122, (byte)207, byte.MaxValue)), 
			_ => defaultColor, 
		});
	}

	public Color GetNameColor()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (EntityId == GameManager.PlayerId)
		{
			return Color32.op_Implicit(new Color32((byte)122, (byte)172, byte.MaxValue, byte.MaxValue));
		}
		switch (Type)
		{
		case ChannelType.Region:
		case ChannelType.Clan:
		case ChannelType.Conversation:
			return Color32.op_Implicit(new Color32(byte.MaxValue, (byte)216, (byte)91, byte.MaxValue));
		case ChannelType.System:
			return Color32.op_Implicit(new Color32((byte)184, (byte)184, (byte)184, byte.MaxValue));
		default:
			return Color.white;
		}
	}
}
