using System;
using Durango.Logic.Social;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ChatRoomOption_PC : ChatRoomOption
{
	[SerializeField]
	private Selectable _inviteButton;

	protected override void OnAwake()
	{
		base.OnAwake();
		Selectable inviteButton = _inviteButton;
		inviteButton.Clicked = (Action)Delegate.Combine(inviteButton.Clicked, new Action(base.InviteButtonClicked));
	}

	public override void Set([NotNull] Conversation conversation, int height)
	{
		EntityIds.Clear();
		EntityIds.AddRange(conversation.GetEntityIds());
	}

	protected override void UpdateLayout()
	{
		UIUtility.UpdateAnchors(base.transform);
	}
}
