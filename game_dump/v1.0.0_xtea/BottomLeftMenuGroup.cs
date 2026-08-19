using ChatData;
using UnityEngine;

public class BottomLeftMenuGroup : UIBase
{
	[SerializeField]
	private BottomMenuWidget _bottomMenuWidget;

	[SerializeField]
	private LineChatWidget _lineChatWidget;

	private void Start()
	{
		base.OnVisible += BaseUI_OnVisible;
		GameSystem<SocialSystem>.Instance().ChatAdded += SocialSystem_ChatAdded;
	}

	private void OnEnable()
	{
		Conversation.MessagesUpdated += Conversation_MessageUpdated;
	}

	private void OnDisable()
	{
		Conversation.MessagesUpdated -= Conversation_MessageUpdated;
	}

	private void BaseUI_OnVisible(bool visible)
	{
		KSingleton<PlayerController>.Instance().DrawMode = false;
	}

	private void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (!((Object)(object)chat.Chatter != (Object)null) || chat.Chatter.ChatLineAddible)
		{
			_lineChatWidget.Add(chat);
		}
	}

	private void Conversation_MessageUpdated(Conversation conv)
	{
		if (conv.Messages.Count != 0)
		{
			ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
			_lineChatWidget.Add(chat, conv);
		}
	}
}
