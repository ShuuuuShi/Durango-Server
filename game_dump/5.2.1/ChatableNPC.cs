using Durango.Logic.Social;
using UnityEngine;

public class ChatableNPC : ChatableCharacter<CostumeActorBehavior>
{
	public override bool IsMale
	{
		get
		{
			if (Owner != null)
			{
				return Owner.IsMale;
			}
			return false;
		}
	}

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
		return PortraitBuilder.MakeArgument((component != null) ? component.PortraitType : 0, 0, Color.white, IsMale, emotion, Owner.GetStoredColor("skin_color"), Owner.GetStoredColor("hair_color"), ResourceSingleton<PortraitBuilder>.Instance().DefaultEyeColor, ResourceSingleton<PortraitBuilder>.Instance().DefaultLipColor);
	}
}
