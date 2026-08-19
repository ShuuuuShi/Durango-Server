using Shared.Battle;
using UnityEngine;

public class ChatableCharacter<T> : ChatableBase where T : CharacterBehavior
{
	protected T Owner;

	public override string EntityId
	{
		get
		{
			if (Owner == null)
			{
				return string.Empty;
			}
			return Owner.EntityId;
		}
	}

	public override Vector3 ChatterPosition
	{
		get
		{
			if (Owner == null)
			{
				return Vector3.zero;
			}
			Transform bodyPartTransform = Owner.GetBodyPartTransform(BodyPart.Body);
			if (bodyPartTransform == null)
			{
				return Owner.CurrentPosition;
			}
			return bodyPartTransform.position;
		}
	}

	public override string PortraitName
	{
		get
		{
			ClientActorChat clientActorChat = ((!(Owner != null)) ? null : Owner.GetComponent<ClientActorChat>());
			if (clientActorChat != null)
			{
				return clientActorChat.PortraitName;
			}
			return string.Empty;
		}
	}

	public ChatableCharacter(T owner)
	{
		Owner = owner;
	}
}
