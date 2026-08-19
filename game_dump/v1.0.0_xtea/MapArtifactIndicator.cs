using JetBrains.Annotations;
using Shared.Building;
using UnityEngine;

public class MapArtifactIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private Artifact _artifact;

	public void SetArtifact([NotNull] Artifact artifact)
	{
		_artifact = artifact;
		SetTarget(((Component)artifact).gameObject);
		UdpateSprite();
	}

	private void UdpateSprite()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_artifact == (Object)null))
		{
			string spriteName = "icon_map_otherplayer";
			Color color = Color.white;
			int length = 30;
			int depth = 10;
			switch (_artifact.ArtifactId)
			{
			case "bonfire":
			case "tutorial_bonfire":
				spriteName = "icon_map_bonfire";
				color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)106, (byte)0, byte.MaxValue));
				break;
			case "tent":
			case "hut":
			case "temptent":
				spriteName = "icon_map_house";
				color = Color32.op_Implicit(new Color32((byte)188, (byte)185, (byte)183, byte.MaxValue));
				length = 20;
				depth = 10;
				break;
			case "tutorial_boat":
				spriteName = "icon_map_ancora_boat";
				length = 33;
				break;
			case "trap_basket":
			case "trap_damage":
			case "trap_string":
				spriteName = "icon_map_prof_trap";
				length = 24;
				break;
			case "fishtrap":
				spriteName = "icon_map_fishtrap";
				length = 26;
				break;
			}
			_sprite.spriteName = spriteName;
			_sprite.color = color;
			UIUtility.ResizeToSquare(_sprite, length);
			_sprite.depth = depth;
		}
	}

	public static bool HasIndicator(Artifact artifact)
	{
		if ((Object)(object)artifact == (Object)null || artifact.Condition == Condition.Broken || artifact.BuildState < BuildingState.Completed)
		{
			return false;
		}
		switch (artifact.ArtifactId)
		{
		case "bonfire":
		case "tutorial_bonfire":
		case "tent":
		case "hut":
		case "temptent":
		case "tutorial_boat":
		case "trap_basket":
		case "trap_damage":
		case "trap_string":
		case "fishtrap":
			return true;
		default:
			return false;
		}
	}
}
