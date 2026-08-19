using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ChatBubbleGroup : UIBase
{
	public static bool On = true;

	[SerializeField]
	private ChatBubble _chatBubble;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private SoundEventType _bubbleSound;

	[SerializeField]
	private float _bubbleDuration = 5f;

	private readonly List<ChatBubble> _chatBubbles = new List<ChatBubble>();

	private readonly Stack<ChatBubble> _chatBubblePool = new Stack<ChatBubble>();

	private void Start()
	{
		_chatBubble.gameObject.SetActive(value: false);
		SoundManager.PrepareEvent(_bubbleSound);
		GameSystem<SocialSystem>.Instance().ChatAdded += OnChatAdded;
		GameSystem<SocialSystem>.Instance().ChatHided += delegate(ChatableBase chatable)
		{
			if (chatable != null)
			{
				Hide(chatable.EntityId);
			}
		};
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _chatBubbles.Count; i++)
		{
			_chatBubbles[i].Refresh();
		}
	}

	private void OnChatAdded(ChatStruct chat)
	{
		if (chat.HasTranslatedText || chat.NoBubble)
		{
			return;
		}
		ChatableBase chatableBase = chat.Chatter;
		if (chatableBase == null)
		{
			PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(chat.EntityId);
			if (playerIncludeLocalPlayer != null && playerIncludeLocalPlayer.GetVisible())
			{
				chatableBase = playerIncludeLocalPlayer.ChatableBase;
			}
		}
		if (chatableBase != null)
		{
			Show(chatableBase, chat.FindText(), chat.Emotion, chat.Duration, chat.Body is RadioDictation);
		}
	}

	public void Show(ChatableBase chatter, string text, PortraitEmotion emotion = PortraitEmotion.None, float? duration = 0f, bool showSttIcon = false)
	{
		PortraitBuilder.Argument? portraitArgs = null;
		string portraitName = chatter.PortraitName;
		if (string.IsNullOrEmpty(portraitName))
		{
			PortraitBuilder.Argument portraitArgument = chatter.GetPortraitArgument(emotion);
			portraitArgument.Mask = _portraitMask;
			if (portraitArgument.Emotion == PortraitEmotion.None)
			{
				if (GameSystem<SocialSystem>.Instance().IsTextEmotion(text, Emotion.Smile))
				{
					portraitArgument.Emotion = PortraitEmotion.Smile;
				}
				else if (GameSystem<SocialSystem>.Instance().IsTextEmotion(text, Emotion.Sad))
				{
					portraitArgument.Emotion = PortraitEmotion.Sad;
				}
				else
				{
					portraitArgument.Emotion = PortraitEmotion.Normal;
				}
			}
			portraitArgs = portraitArgument;
		}
		Show(chatter, text, portraitArgs, portraitName, Color.white, null, null, alwaysInScreen: true, duration, showSttIcon);
	}

	public void Show(ChatableBase chatter, string text, PortraitBuilder.Argument? portraitArgs, string portraitIcon, Color portraitColor, ChatBubble.TargetPivot? direction = null, Vector3? offset = null, bool alwaysInScreen = true, float? duration = 0f, bool showSttIcon = false)
	{
		if (chatter != null && On && (string.IsNullOrEmpty(text) || UISpriteLabel.HasCharacter(text)))
		{
			ChatBubble chatBubble = Get(chatter.EntityId);
			chatBubble.AlwaysInScreen = alwaysInScreen;
			chatBubble.Set(chatter, text, portraitArgs, portraitIcon, portraitColor, direction, offset, showSttIcon);
			chatBubble.Align = (chatter.IsLocalPlayer ? ChatBubble.ChatBubbleAlign.Left : ChatBubble.ChatBubbleAlign.Auto);
			if (duration.HasValue && duration.Value <= 0f)
			{
				duration = _bubbleDuration;
			}
			chatBubble.Show(duration);
			SoundManager.PlayEvent(_bubbleSound);
		}
	}

	public void Hide(string entityId)
	{
		ChatBubble chatBubble = Get(entityId, make: false);
		if (chatBubble != null)
		{
			chatBubble.Hide();
		}
	}

	private ChatBubble Get(string entityId, bool make = true)
	{
		int num = -1;
		int i = 0;
		for (int count = _chatBubbles.Count; i < count; i++)
		{
			if (_chatBubbles[i].Id == entityId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			if (make)
			{
				return BubblePop();
			}
			return null;
		}
		return _chatBubbles[num];
	}

	private ChatBubble BubblePop()
	{
		ChatBubble chatBubble;
		if (_chatBubblePool.Count > 0)
		{
			chatBubble = _chatBubblePool.Pop();
		}
		else
		{
			ChatBubble component = _chatBubble.transform.parent.gameObject.AddChild(_chatBubble.gameObject).GetComponent<ChatBubble>();
			component.Disabled = BubblePush;
			component.transform.rotation = _chatBubble.transform.rotation;
			chatBubble = component;
		}
		_chatBubbles.Add(chatBubble);
		base.enabled = true;
		return chatBubble;
	}

	private void BubblePush(ChatBubble bubble)
	{
		_chatBubbles.Remove(bubble);
		_chatBubblePool.Push(bubble);
		base.enabled = _chatBubbles.Count > 0;
	}
}
