using Durango.Logic.Social;
using UnityEngine;

public class ChatableNPC : ChatableCharacter<CostumeActorBehavior>
{
	public override bool IsMale => Owner != null && Owner.IsMale;

	public ChatableNPC(CostumeActorBehavior owner)
		: base(owner)
	{
	}

	public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		if (Owner == null)
		{
			return default(PortraitBuilder.Argument);
		}
		ClientActorChat component = Owner.GetComponent<ClientActorChat>();
		int type = ((component != null) ? component.PortraitType : 0);
		return PortraitBuilder.MakeArgument(type, 0, Color.white, IsMale, emotion, Owner.GetStoredColor("skin_color"), Owner.GetStoredColor("hair_color"), ResourceSingleton<PortraitBuilder>.Instance().DefaultEyeColor, ResourceSingleton<PortraitBuilder>.Instance().DefaultLipColor);
	}
}
