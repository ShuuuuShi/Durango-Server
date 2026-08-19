using UnityEngine;

public abstract class ChatableCharacter<T> : ChatableBase where T : CharacterBehavior
{
	private T _owner;

	public T Owner
	{
		get
		{
			if ((Object)(object)_owner == (Object)null)
			{
				_owner = ((Component)this).GetComponent<T>();
			}
			return _owner;
		}
	}

	public override ulong EntityId
	{
		get
		{
			if ((Object)(object)Owner == (Object)null)
			{
				return 0uL;
			}
			T owner = Owner;
			return owner.EntityId;
		}
	}
}
