using Player;
using UnityEngine;

public class ChatablePlayer : ChatableCharacter<PlayerBehavior>
{
	public override bool IsLocalPlayer => base.Owner.IsLocalPlayer;

	public override string ChatterName => base.Owner.PlayerName;

	public override Vector3 ChatterPosition => base.Owner.CurrentPosition;

	public override bool IsMale => base.Owner.IsMale;

	public override int PortraitType => base.Owner.PortraitType;

	public override bool ChatLineAddible => true;

	public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		return base.Owner.GetPortraitArgument();
	}
}
