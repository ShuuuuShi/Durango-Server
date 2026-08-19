using Player;
using UnityEngine;

public abstract class ChatableBase : MonoBehaviour
{
	public abstract ulong EntityId { get; }

	public abstract bool IsLocalPlayer { get; }

	public abstract string ChatterName { get; }

	public abstract Vector3 ChatterPosition { get; }

	public abstract int PortraitType { get; }

	public abstract bool IsMale { get; }

	public abstract bool ChatLineAddible { get; }

	public abstract PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None);
}
