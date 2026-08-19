using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.Logic.Social;

public class ChatStruct
{
	public enum ChatMsgType
	{
		Talk,
		Dictation,
		ChannelUpdated,
		Link
	}

	private static readonly Color32 ColorMsgRadioDictation = new Color32(byte.MaxValue, 212, 156, byte.MaxValue);

	private static readonly Color32 ColorMsgClan = PresetColor.PlayerClan;

	private static readonly Color32 ColorMsgClanWar = PresetColor.PlayerClan;

	private static readonly Color32 ColorMsgSystem = new Color32(119, byte.MaxValue, 85, byte.MaxValue);

	private static readonly Color32 ColorMsgConversation = new Color32(byte.MaxValue, 122, 207, byte.MaxValue);

	private static readonly Color32 ColorMsgParty = PresetColor.PlayerParty;

	private static readonly Color32 ColorMsgNotice = new Color32(62, 186, 236, byte.MaxValue);

	public static readonly Color32 ColorNameDefault = Color.white;

	public static readonly Color32 ColorNameLocalPlayer = new Color32(122, 172, byte.MaxValue, byte.MaxValue);

	private static readonly Color32 ColorNameNonSystem = new Color32(byte.MaxValue, 216, 91, byte.MaxValue);

	private static readonly Color32 ColorNameSystem = new Color32(184, 184, 184, byte.MaxValue);

	public string EntityId;

	public ChatableBase Chatter;

	public double Time;

	public float Duration;

	public ChannelType Type;

	public bool IsVolatile;

	public string Name;

	public PortraitEmotion Emotion;

	public object Body;

	public bool TranslationOn;

	public bool NoBubble;

	private string _cachedText;

	public string SourceLang
	{
		get
		{
			if (Body is TranslatedRadioTalk)
			{
				return ((TranslatedRadioTalk)Body).SrcLang;
			}
			return string.Empty;
		}
	}

	public bool Translated { get; private set; }

	public bool Translatable
	{
		get
		{
			if (Body is TranslatedRadioTalk)
			{
				TranslatedRadioTalk translatedRadioTalk = (TranslatedRadioTalk)Body;
				return translatedRadioTalk.TranslatedText != null && translatedRadioTalk.SrcLang != LocalizeSystem.LocaleLanguage && EntityId != GameManager.PlayerId;
			}
			return false;
		}
	}

	public bool HasTranslatedText
	{
		get
		{
			if (Body is TranslatedRadioTalk)
			{
				return ((TranslatedRadioTalk)Body).TranslatedText != null;
			}
			return false;
		}
	}

	public ChatMsgType MsgType
	{
		get
		{
			if (Body is RadioDictation)
			{
				return ChatMsgType.Dictation;
			}
			if (IsEventMessage())
			{
				return ChatMsgType.ChannelUpdated;
			}
			if (Body is RadioPin || Body is RadioPinWithText || Body is RadioLink)
			{
				return ChatMsgType.Link;
			}
			return ChatMsgType.Talk;
		}
	}

	public string FindText()
	{
		if (_cachedText != null && TranslationOn == Translated)
		{
			return _cachedText;
		}
		_cachedText = FindTextInternal();
		return _cachedText;
	}

	private string FindTextInternal()
	{
		if (Body is TranslatedRadioTalk)
		{
			Translated = false;
			TranslatedRadioTalk translatedRadioTalk = (TranslatedRadioTalk)Body;
			string value = translatedRadioTalk.Text;
			if (TranslationOn && Translatable && translatedRadioTalk.TranslatedText != null)
			{
				string localeLanguage = LocalizeSystem.LocaleLanguage;
				if (!translatedRadioTalk.TranslatedText.TryGetValue(localeLanguage, out value))
				{
					value = translatedRadioTalk.TranslatedText.Get("en", translatedRadioTalk.Text);
				}
				Translated = true;
				if (string.IsNullOrEmpty(value))
				{
					value = translatedRadioTalk.Text;
				}
			}
			return NGUIText.StripSymbols(value);
		}
		if (Body is RadioTalk)
		{
			return NGUIText.StripSymbols(((RadioTalk)Body).Text);
		}
		if (Body is RadioDictation)
		{
			return ((RadioDictation)Body).Text;
		}
		if (Body is RadioText)
		{
			return NGUIText.StripSymbols(((RadioText)Body).Text);
		}
		if (Body is RadioNotice)
		{
			return ((RadioNotice)Body).Text;
		}
		if (Body is RadioAlert)
		{
			return ((RadioAlert)Body).Text;
		}
		if (Body is RadioPin)
		{
			RadioPin radioPin = (RadioPin)Body;
			return T._("{0} 섬의 위치를 공유합니다.", radioPin.RegionName);
		}
		if (Body is RadioPinWithText)
		{
			return ((RadioPinWithText)Body).Text;
		}
		if (Body is RadioLink)
		{
			return ((RadioLink)Body).Text;
		}
		return string.Empty;
	}

	public Color GetMsgColor(Color defaultColor)
	{
		if (Body is RadioDictation)
		{
			return ColorMsgRadioDictation;
		}
		if (Body is RadioNotice || Body is RadioAlert)
		{
			return ColorMsgNotice;
		}
		return Type switch
		{
			ChannelType.Clan => ColorMsgClan, 
			ChannelType.System => ColorMsgSystem, 
			ChannelType.Conversation => ColorMsgConversation, 
			ChannelType.ClanWar => ColorMsgClanWar, 
			ChannelType.Party => ColorMsgParty, 
			_ => defaultColor, 
		};
	}

	public Color GetNameColor()
	{
		if (EntityId == GameManager.PlayerId)
		{
			return ColorNameLocalPlayer;
		}
		switch (Type)
		{
		case ChannelType.Region:
		case ChannelType.Clan:
		case ChannelType.Conversation:
		case ChannelType.ClanWar:
		case ChannelType.Party:
		case ChannelType.PersonalRegions:
			return ColorNameNonSystem;
		case ChannelType.System:
			return ColorNameSystem;
		default:
			return ColorNameDefault;
		}
	}

	public bool IsEventMessage()
	{
		return Body is RadioEntered || Body is RadioLeft;
	}

	public bool IsNoticeMessage()
	{
		return Body is RadioNotice || Body is RadioAlert;
	}
}
