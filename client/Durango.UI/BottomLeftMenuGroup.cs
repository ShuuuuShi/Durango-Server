using Durango.Logic.Social;
using UnityEngine;

namespace Durango.UI;

public class BottomLeftMenuGroup : BottomLeftMenuGroupBase
{
	[SerializeField]
	private LineChatWidget _lineChatWidget;

	protected override void Start()
	{
		base.Start();
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

	private void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (chat.Chatter == null || chat.Chatter.ChatLineAddible)
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
