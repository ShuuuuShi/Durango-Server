using System.Collections.Generic;
using ChatData;
using Messages;
using Player;
using UnityEngine;

public class ChatBubbleGroup : UIBase
{
	private const string BubbleSound = "Sound/Effect/Action/Action_TalkBubble_01.wav";

	[SerializeField]
	private ChatBubble _chatBubble;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private float _bubbleDuration = 5f;

	private List<ChatBubble> _chatBubbles = new List<ChatBubble>();

	private Stack<ChatBubble> _chatBubblePool = new Stack<ChatBubble>();

	private void Awake()
	{
		((Component)_chatBubble).gameObject.SetActive(false);
		SoundManager.Cache("Sound/Effect/Action/Action_TalkBubble_01.wav");
	}

	private void OnEnable()
	{
		GameSystem<SocialSystem>.Instance().ChatAdded += OnChatAdded;
		GameSystem<SocialSystem>.Instance().ChatHided += Hide;
	}

	private void OnDisable()
	{
		GameSystem<SocialSystem>.Instance().ChatAdded -= OnChatAdded;
		GameSystem<SocialSystem>.Instance().ChatHided -= Hide;
	}

	private void OnChatAdded(ChatStruct chat)
	{
		ChatableBase chatableBase = chat.Chatter;
		if ((Object)(object)chatableBase == (Object)null)
		{
			PlayerBehavior playerIncludeLocalPlayer = KSingleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(chat.EntityId);
			if ((Object)(object)playerIncludeLocalPlayer != (Object)null && playerIncludeLocalPlayer.GetRenderEnabled())
			{
				chatableBase = playerIncludeLocalPlayer.ChatableBase;
			}
		}
		if ((Object)(object)chatableBase != (Object)null)
		{
			Show(chatableBase, chatableBase.ChatterName, chat.FindText(), chat.Emotion, chat.Duration, chat.Body is RadioDictation);
		}
	}

	private void Show(ChatableBase chatter, string targetName, string text, PortraitEmotion emotion = PortraitEmotion.None, float duration = 0f, bool showSTTIcon = false)
	{
		Show(chatter, targetName, text, chatter.GetPortraitArgument(emotion), duration, showSTTIcon);
	}

	private void Show(ChatableBase chatter, string targetName, string text, PortraitBuilder.Argument portrait, float duration = 0f, bool showSTTIcon = false)
	{
		if ((Object)(object)chatter == (Object)null || !SocialSystem.HasCharacter(text))
		{
			return;
		}
		ChatBubble chatBubble = Get(chatter.EntityId);
		portrait.Mask = _portraitMask;
		if (portrait.Emotion == PortraitEmotion.None)
		{
			if (GameSystem<SocialSystem>.Instance().IsTextEmotion(text, Emotion.Smile))
			{
				portrait.Emotion = PortraitEmotion.Smile;
			}
			else if (GameSystem<SocialSystem>.Instance().IsTextEmotion(text, Emotion.Sad))
			{
				portrait.Emotion = PortraitEmotion.Sad;
			}
			else
			{
				portrait.Emotion = PortraitEmotion.Normal;
			}
		}
		chatBubble.Set(chatter, targetName, text, portrait, showSTTIcon);
		chatBubble.SetDepth(_chatBubble, GetMaxDepth() + 1);
		chatBubble.Align = (chatter.IsLocalPlayer ? ChatBubble.ChatBubbleAlign.Left : ChatBubble.ChatBubbleAlign.Auto);
		if (duration <= 0f)
		{
			duration = _bubbleDuration;
		}
		chatBubble.Show(duration);
		SoundManager.Play("Sound/Effect/Action/Action_TalkBubble_01.wav");
	}

	public void Hide(ChatableBase chatter)
	{
		ChatBubble chatBubble = Get(chatter.EntityId, make: false);
		if ((Object)(object)chatBubble != (Object)null)
		{
			chatBubble.Hide();
		}
	}

	private ChatBubble Get(ulong entityId, bool make = true)
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
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		ChatBubble chatBubble;
		if (_chatBubblePool.Count > 0)
		{
			chatBubble = _chatBubblePool.Pop();
		}
		else
		{
			ChatBubble component = ((Component)((Component)_chatBubble).transform.parent).gameObject.AddChild(((Component)_chatBubble).gameObject).GetComponent<ChatBubble>();
			component.Disabled = BubblePush;
			((Component)component).transform.rotation = ((Component)_chatBubble).transform.rotation;
			chatBubble = component;
		}
		_chatBubbles.Add(chatBubble);
		return chatBubble;
	}

	private void BubblePush(ChatBubble bubble)
	{
		_chatBubbles.Remove(bubble);
		_chatBubblePool.Push(bubble);
	}

	private int GetMaxDepth()
	{
		int result = -1;
		int i = 0;
		for (int count = _chatBubbles.Count; i < count; i++)
		{
			result = Mathf.Max(new int[1] { _chatBubbles[i].Depth });
		}
		return result;
	}
}
