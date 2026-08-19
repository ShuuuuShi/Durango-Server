using Durango.Logic.Item;
using Shared.Etc;
using UnityEngine;

public class ModularAddon
{
	public int Index;

	public string ModelKey;

	public string Type;

	public ItemData Item;

	public const string InnerShadowPrefab = "Prop/module/shadow/shadow_01_inner.prefab";

	public static string GetAddOnType(ItemData item)
	{
		if (item.HasTag("window"))
		{
			return "window";
		}
		if (item.HasTag("door"))
		{
			return "door";
		}
		if (item.HasTag("wall_deco"))
		{
			return "wall_deco";
		}
		if (item.HasTag("empty_door"))
		{
			return "empty_door";
		}
		return null;
	}

	public bool IsEmptyDoor()
	{
		return Type == "empty_door";
	}

	public string GetWallPostfix()
	{
		return Type switch
		{
			"door" => "door", 
			"window" => "window", 
			_ => null, 
		};
	}

	public bool NeedInnerShadow()
	{
		return Type switch
		{
			"door" => true, 
			"window" => true, 
			"empty_door" => true, 
			_ => false, 
		};
	}

	public Vector3 GetAngle(Direction dir)
	{
		if (Type == "wall_deco")
		{
			return ArtifactUtil.DirectionToForwardAngle(dir);
		}
		return ArtifactUtil.DirectionToAngle(dir);
	}
}
