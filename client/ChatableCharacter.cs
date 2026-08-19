using Shared.Battle;
using UnityEngine;

public class ChatableCharacter<T> : ChatableBase where T : CharacterBehavior
{
	protected T Owner;

	public override string EntityId => (!(Owner == null)) ? Owner.EntityId : string.Empty;

	public override Vector3 ChatterPosition
	{
		get
		{
			if (Owner == null)
			{
				return Vector3.zero;
			}
			Transform bodyPartTransform = Owner.GetBodyPartTransform(BodyPart.Body);
			return (!(bodyPartTransform == null)) ? bodyPartTransform.position : Owner.CurrentPosition;
		}
	}

	public override string PortraitName
	{
		get
		{
			ClientActorChat clientActorChat = ((!(Owner != null)) ? null : Owner.GetComponent<ClientActorChat>());
			return (!(clientActorChat != null)) ? string.Empty : clientActorChat.PortraitName;
		}
	}

	public ChatableCharacter(T owner)
	{
		Owner = owner;
	}
}
