using UnityEngine;

public class ChatableImmovable<T> : ChatableBase where T : ImmovableBase
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
			return Owner.InteractionPosition;
		}
	}

	public override string PortraitName => string.Empty;

	public ChatableImmovable(T owner)
	{
		Owner = owner;
	}
}
