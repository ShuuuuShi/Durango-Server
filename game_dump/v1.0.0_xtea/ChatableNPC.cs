using Player;
using UnityEngine;

public class ChatableNPC : ChatableCharacter<NPCActorBehavior>
{
	public override bool IsLocalPlayer => false;

	public override string ChatterName => string.Empty;

	public override Vector3 ChatterPosition
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)base.Owner == (Object)null)
			{
				return ((Component)this).transform.position;
			}
			return base.Owner.CurrentPosition;
		}
	}

	public override bool IsMale
	{
		get
		{
			if ((Object)(object)base.Owner == (Object)null)
			{
				return false;
			}
			return base.Owner.IsMale;
		}
	}

	public override int PortraitType
	{
		get
		{
			ClientActorChat component = ((Component)this).GetComponent<ClientActorChat>();
			if ((Object)(object)component != (Object)null)
			{
				return component.PortraitType;
			}
			return 0;
		}
	}

	public override bool ChatLineAddible => false;

	public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		return PortraitBuilder.MakeArgument(PortraitType, 0, Color.white, IsMale, emotion, base.Owner.GetStoredColor("skin_color"), base.Owner.GetStoredColor("hair_color"), KSingleton<PortraitBuilder>.Instance().DefaultEyeColor, KSingleton<PortraitBuilder>.Instance().DefaultLipColor);
	}
}
