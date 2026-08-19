using Durango.Logic.Social;
using UnityEngine;

public abstract class ChatableBase
{
	public abstract string EntityId { get; }

	public abstract Vector3 ChatterPosition { get; }

	public abstract string PortraitName { get; }

	public virtual bool IsLocalPlayer => false;

	public virtual string ChatterName => string.Empty;

	public virtual bool IsMale => false;

	public virtual bool ChatLineAddible => false;

	public virtual PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		return default(PortraitBuilder.Argument);
	}
}
