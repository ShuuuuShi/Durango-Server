using Durango.Logic.Social;

public class ChatablePlayer : ChatableCharacter<PlayerBehavior>
{
	public override bool IsLocalPlayer => Owner.IsLocalPlayer;

	public override string ChatterName => Owner.PlayerName;

	public override bool IsMale => Owner.IsMale;

	public override bool ChatLineAddible => true;

	public ChatablePlayer(PlayerBehavior owner)
		: base(owner)
	{
	}

	public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		return Owner.GetPortraitArgument();
	}
}
